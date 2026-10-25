# Tasks

> Las decisiones abiertas del diseño (grafía del realm, aviso al técnico, nombre y apellidos como pista)
> no cambian lo que hay que construir aquí.

## 1. Dominio

- [x] 1.1 Quitar el nick de `User.Create`, dejar `Nickname` como `string?` y añadir `SetNickname`, que valida el formato y falla si el nick ya estaba fijado. Verificación: el resto de métodos de `User` no cambian y `ValidateNickname` sigue siendo el único sitio con la regla del formato.
- [x] 1.2 Hacer nullable la columna en `UserConfiguration` (quitar `IsRequired`, mantener longitud e índice único) y generar la migración. Verificación: la migración (`20261025125346_MakeNicknameNullable`) solo cambia la nulabilidad de `Nickname`; **aplicada** a `api` del entorno `dv`.

## 2. Application

- [x] 2.1 Añadir `IKeycloakUsersApi` en `Services/` con un método que devuelva el perfil de un usuario de Keycloak (identificador, correo, tipo de cuenta, proveedor de identidad y locale), más el modelo de ese perfil en `Models/Registration/`. Verificación: la interfaz no menciona HTTP ni tokens; eso es de la implementación.
- [x] 2.2 Crear `RegisterUserCommand` + handler: si ya existe cuenta con esa identidad externa no hace nada, si no pide el perfil y crea el `User` sin nick. Verificación: no crea `Athlete`/`School`, y un `userType` ausente o desconocido devuelve fallo en lugar de inventarse el tipo.
- [x] 2.3 *(petición posterior del propietario)* Mover al pipeline la validación del mensaje: `RegisterUserCommandValidator` en `Validators/Commands` comprueba que el `KeycloakUserId` no viene vacío y que es un GUID, y el `Handle` queda como orquestación (idempotencia → consulta → mapeo → alta). Lo que depende del perfil que devuelve Keycloak **no** puede ir al validador sin pedirlo dos veces, así que sigue en un método privado con su motivo escrito. Verificación: tres tests del validador y cuatro del handler.

## 3. Persistence

- [x] 3.1 Implementar `KeycloakUsersApi` con `HttpClient`: token por `client_credentials` contra `{Authority}/protocol/openid-connect/token` y `GET` del usuario en la Admin API derivada de `Authority`. Verificación: no hay ninguna URL de Keycloak escrita a mano ni el realm duplicado, y las credenciales vienen de configuración.
- [x] 3.2 Registrar el cliente por interfaz. *Corrección respecto al enunciado*: el registro va en `Program.cs` (proyecto **Api**) con `AddHttpClient`, y no en la extensión de Persistence, porque `Microsoft.Extensions.Http` no está disponible en un proyecto de clase y añadirlo sería una dependencia nueva. `PersistenceModule` sigue registrando repositorios por convención y no se toca Autofac.

## 4. Api

- [x] 4.1 Sustituir el esbozo de `WebHooksController`: leer el evento, ignorar lo que no sea `access.REGISTER` acusando recibo, y enviar el comando con el `userId` cuando lo sea. Verificación: el controlador no tiene lógica de negocio (ni mapeos ni comprobaciones de existencia) y sigue siendo anónimo.

## 5. Infraestructura

- [x] 5.1 Añadir los dos secretos al módulo `key-vault` y las dos variables de entorno a `container-apps/api`, con sus variables y el cableado en la raíz de Terraform (el secreto, sensible). Verificación: `terraform validate` pasa y los nombres coinciden con las claves de configuración que lee la API.
- [x] 5.2 Documentar el cliente confidencial de Keycloak (service account, rol `view-users`) y las dos variables `TF_VAR_...`. *Corrección respecto al enunciado*: va en `devops/iac/README.md`, junto al bloque de `apply` donde se pasan esas variables, y no en `devops/README.md`, que describe las pipelines.

## 6. Spec y documentación

- [x] 6.1 Actualizar `domain.md` (la regla del nick y el invariante 2) y `project.md` (§4: el nick entra en los datos que completa el usuario). El webhook materializa la cuenta con identidad y tipo, ya escrito en §4.
- [x] 6.2 Anotar en `QWEN.md` lo que cambia en la capa de infraestructura: hay un cliente HTTP de Keycloak, dónde se registra y de dónde salen sus credenciales.

## 7. Verificación

- [x] 7.1 `dotnet build` sin errores nuevos.
- [x] 7.2 Tests del handler con `IKeycloakUsersApi` y repositorios simulados: alta de deportista, alta de escuela, evento repetido sin duplicar, `userType` ausente → fallo, evento sin usuario → fallo. *(Resultado real: los cinco pasan; la suite queda en 21/22 por el fallo preexistente de audiencia.)*
- [x] 7.3 `terraform validate` (offline) y revisión del diff: dominio, Application, Persistence, Api, Terraform, specs y documentación; sin tocar la configuración de autenticación ni la consulta del perfil.
