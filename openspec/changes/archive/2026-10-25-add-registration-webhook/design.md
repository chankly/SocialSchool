# Design: Alta de la cuenta desde el registro de Keycloak

## Context

Ver `proposal.md` — Why. Lo verificado en el código que condiciona el enfoque:

- `WebHooksController` es un esbozo `[AllowAnonymous]` que lee el cuerpo y lo imprime.
- `User.Create` exige el nick y `ValidateNickname` solo admite letras y dígitos; `User` **no tiene** ningún
  método para cambiar el nick. El registro de Keycloak trae `username` = un correo, y `attributes` solo
  lleva `userType` y `locale`.
- `User.Nickname` está configurado `IsRequired().HasMaxLength(50)` con índice único; en MySQL un índice
  único admite varios `NULL`.
- `User.IdentityProvider` es un **string**, no una enumeración: `"google"` entra tal cual.
- `UserConfiguration` ya tiene el índice único de `ExternalIdentityId` que usa la consulta del perfil, así
  que la cuenta se puede buscar por la identidad externa antes de crearla.
- **No hay ningún `HttpClient` en `src`**: ni registro ni cliente. La integración se monta desde cero.
- `Keycloak:Authority` (por ejemplo `https://host/realms/<realm>`) ya es lo que la API usa para validar
  tokens; existe además `keycloak_realm` en Terraform con el valor `socialschool`.
- El módulo `key-vault` declara los secretos con `locals` y variables; el módulo `container-apps/api`
  repite el patrón `secret { key_vault_secret_id }` + `env { secret_name }` para cada secreto.

## Goals / Non-Goals

**Goals:**

- Que registrarse en Keycloak acabe en un `User` con la identidad, el tipo de cuenta y el idioma.
- Que repetir el evento no duplique nada.
- Que las credenciales de la Admin API no estén en el repositorio ni en los ficheros de configuración.

**Non-Goals:**

- Crear el perfil (`Athlete`/`School`): lo completa el usuario después.
- Autenticar la llamada al webhook (decisión del propietario, ver riesgos).
- Sincronizar cambios posteriores del usuario en Keycloak (correo, borrado): este cambio solo cubre el alta.

## Decisions

**1. El nick deja de ser obligatorio al crear la cuenta.** `User.Create` pierde el parámetro, `Nickname`
pasa a `string?` y nace `SetNickname`, que valida el formato y **solo funciona una vez**. Es la única
forma coherente con el flujo: la cuenta existe antes de que el usuario elija su nick, y después el nick
sigue siendo único e inmutable. *Alternativas descartadas*: derivarlo del correo (el usuario quedaría con
un nick que no eligió y no podría cambiarlo) y pedirlo en Keycloak (hoy el registro no recoge atributos).
La columna pasa a admitir `NULL`; el índice único sigue siendo válido porque MySQL permite varios nulos.

**2. Las dos URLs de Keycloak se derivan de `Keycloak:Authority`.** El token sale de
`{Authority}/protocol/openid-connect/token` y el usuario de `{Authority}` con `/realms/` sustituido por
`/admin/realms/`. Así no se duplica el nombre del realm —que hoy está en Terraform como `socialschool`
mientras el evento dice `SocialSchool`, y el path distingue mayúsculas— y la Admin API usa exactamente el
mismo realm con el que la API valida los tokens. *Alternativa descartada*: dos claves nuevas de
configuración, que se pueden desincronizar en silencio.

**3. La interfaz vive en `Application` y el cliente HTTP en `Persistence`.** `IKeycloakUsersApi` describe
"dame el perfil de este usuario"; la implementación usa `IHttpClientFactory`, pide el token con
`client_credentials` y hace el `GET`. `Persistence` es la capa de infraestructura de este repositorio (ya
implementa `IMigrationService`), así que no hace falta un proyecto nuevo. Se registra con
`AddHttpClient` en una extensión de Persistence.

**4. Un `RegisterUserCommand` idempotente, no lógica en el controlador.** El controlador solo traduce el
evento HTTP: si no es `access.REGISTER`, acusa recibo y termina; si lo es, envía el comando con el
`userId`. El handler comprueba si ya existe la cuenta por identidad externa, y si no, pide el perfil y
crea el `User`. El índice único de `ExternalIdentityId` es la red por debajo: si dos entregas llegan a la
vez, la segunda choca contra la base y no duplica.

**5. Códigos de respuesta con una regla simple.** `2xx` solo cuando la cuenta existe (creada ahora o
antes) o el evento se ha ignorado; `500` cuando la cuenta no se ha podido crear (fallo del token, de la
Admin API o `userType` ausente). Así "no 2xx" significa siempre lo mismo y Keycloak puede reintentar. El
caso de `userType` ausente se registra en el log con el identificador del usuario.

**6. El webhook sigue anónimo, y se deja escrito lo que cuesta.** Decisión del propietario. Consecuencia:
cualquiera que conozca la URL puede enviar un `userId` y provocar el alta de esa cuenta —no puede leer
nada ni modificarla después—, y la única defensa es que los identificadores de Keycloak son GUID. Queda
como riesgo aceptado, no como olvido.

**7. Los secretos, por el mismo camino que los demás.** Dos secretos en Key Vault
(`keycloak-admin-client-id`, `keycloak-admin-client-secret`) y dos variables de entorno en la Container
App (`Keycloak__Admin__ClientId`, `Keycloak__Admin__ClientSecret`) con `secret_name`. El cliente
confidencial de Keycloak y su rol `view-users` son pasos manuales documentados: no se pueden crear desde
Terraform con el proveedor actual.

## Risks / Trade-offs

- **El webhook es un endpoint público que escribe**: sin secreto compartido, un tercero puede disparar
  altas de cuentas cuyo `userId` conozca. No puede leer ni borrar, pero sí crear. Aceptado por el
  propietario.
- **El realm tiene dos grafías en el repositorio** (`socialschool` en Terraform, `SocialSchool` en el
  evento). Derivar de `Authority` evita duplicarlo, pero si `Authority` está mal escrita toda la
  autenticación falla, no solo esto. Hay que confirmar cuál es la real.
- **La migración del nick nullable** deja cuentas sin nick hasta que el usuario complete el perfil: si
  alguien publica o busca antes, habrá que decidir qué se muestra.
- **El cliente HTTP no se puede probar contra Keycloak desde aquí**: los tests usan una implementación
  simulada de `IKeycloakUsersApi`; la llamada real es prueba del usuario.
- **Una cuenta creada con `userType` incorrecto** (por ejemplo, alguien que elige "School" sin serlo) se
  queda con ese tipo, porque es inmutable. La validación de escuelas es del técnico.

## Migration Plan

1. Migración: `Nickname` nullable (sin pérdida de datos; las cuentas existentes no se tocan).
2. Crear en Keycloak el cliente confidencial con service account y `view-users`, y pasar sus credenciales
   como `TF_VAR_...` al `apply`.
3. Desplegar: Terraform crea los secretos y las variables de entorno; la API los lee al arrancar.
4. Comprobar con un registro real: el evento llega, la cuenta aparece y `/api/user/profile/me` pasa de
   `204` a `200` cuando el perfil se complete.
5. **Reversión**: quitar las variables de entorno y los secretos; el código sigue funcionando sin ellos
   hasta que llegue un evento (fallará con 500 y lo dirá el log).

## Open Questions

- Cuál es la grafía real del realm (`socialschool` o `SocialSchool`).
- Si el alta de una escuela debería avisar al técnico (hoy solo queda pendiente de validación).
- Si conviene guardar también `firstName`/`lastName` de Keycloak como pista para el formulario.
