# Design: Sincronizar los cambios de perfil que llegan de Keycloak

## Context

Ver `proposal.md` — Why. Lo verificado en el código:

- `WebHooksController` solo distingue `access.REGISTER`; todo lo demás lo acusa y termina.
- `KeycloakEvent` modela `type` y `authDetails` (con `userId`). Los eventos de actualización traen además
  `details`, y el contenido de ese objeto **cambia según lo que se haya editado**: `updated_locale` en uno,
  `updated_first_name`/`updated_last_name` en el otro.
- `User` tiene `ChangePreferredLanguage` y **no** tiene ningún mutador del correo; `Email` solo se fija al
  crear la cuenta.
- `User.AccountType` no tiene mutador: la inmutabilidad ya la garantiza el dominio.
- `IUserRepository` ya permite buscar por identidad externa y `BaseRepository.Update` persiste.
- El webhook sigue siendo anónimo, por decisión del propietario.

## Goals / Non-Goals

**Goals:**

- Que un cambio de correo o de idioma en Keycloak llegue a la cuenta.
- Que el tipo de cuenta no se pueda cambiar por esta vía ni por accidente.
- No escribir en la base si el dato recibido es el que ya está.

**Non-Goals:**

- Crear cuentas desde un evento de actualización.
- Sincronizar nombres, apellidos ni el resto del perfil: son datos de nuestra aplicación.
- Sincronizar en la dirección contraria (de nuestra cuenta hacia Keycloak).

## Decisions

**1. Los datos salen del `details` del evento, no de una segunda llamada a la Admin API.** El evento dice
exactamente qué ha cambiado (`updated_email`, `updated_locale`); volver a leer el usuario costaría una
llamada más por evento y abriría una carrera entre lo que llega y lo que ya está guardado. *Alternativa
descartada*: releer el perfil completo, que además traería datos que no nos toca copiar.

**2. Un comando propio, `SyncUserProfileCommand`, y no reutilizar el del alta.** El alta crea y necesita el
perfil de Keycloak; la sincronización solo mueve dos campos y no crea nada. Mezclarlos dejaría un comando con
dos modos y un handler con dos caminos que no comparten casi nada.

**3. Si la cuenta no existe, no se hace nada y se responde bien.** Crear la cuenta desde aquí duplicaría la
responsabilidad del alta y obligaría a decidir el tipo de cuenta a partir de un evento que no lo trae. Se
deja anotado como pregunta abierta, porque significa que un alta perdida no se recupera sola.

**4. Solo se persiste si algo cambia.** El handler compara el correo y el idioma con los que ya tiene la
cuenta y solo llama a `Update` cuando hay diferencia. Sin eso, un evento repetido escribiría la fila para
nada y podría pisar una escritura concurrente.

**5. Los nombres y apellidos se ignoran a propósito.** `User` no los tiene y, desde el 2026-10-25, el perfil
lo completa el usuario en nuestra aplicación: copiarlos desde Keycloak sería reintroducir la duplicidad que
`project.md` §4 eliminó.

**6. El reparto por tipo de evento se queda en el controlador y la lógica en el handler.** El controlador
solo traduce: `access.REGISTER` → comando de alta; `access.UPDATE_PROFILE` → comando de sincronización;
cualquier otro → acuse. El handler de sincronización decide si hay algo que aplicar.

**7. `ChangeEmail` valida que el correo no venga vacío y se compara sin distinguir mayúsculas para decidir si
hay cambio.** El correo es el de la cuenta y su origen es el proveedor de identidad; aquí solo se acepta el
que Keycloak manda.

## Risks / Trade-offs

- **Un alta perdida no se recupera**: si el evento de registro no llegó, los de actualización no crean la
  cuenta y el usuario verá `204` indefinidamente. Anotado como pregunta abierta.
- **El correo de Keycloak manda**: si alguien cambia el correo allí, aquí se sobrescribe el que hubiera. Es
  la dirección correcta (Keycloak es el origen de la identidad), pero no se guarda el anterior.
- **Los eventos repetidos son inocuos** gracias a la comparación previa, pero no hay control de concurrencia
  más allá de eso: dos eventos simultáneos pueden escribir el mismo valor dos veces.
- **El webhook sigue sin autenticar**: cualquiera que conozca la URL y un `userId` puede provocar que se
  sincronice basura. Riesgo aceptado por el propietario y ya documentado en la capacidad `registration`.

## Migration Plan

1. Sin migración: no cambia el esquema.
2. Desplegar la API con el webhook nuevo; Keycloak ya emite estos eventos si el webhook está configurado.
3. Comprobar con un cambio real de idioma: `UpdatedLocale` en el log y la columna actualizada.
4. **Reversión**: quitar el caso del controlador; los eventos vuelven a acusarse sin efecto.

## Open Questions

- ¿Debería un evento de actualización materializar la cuenta cuando no existe, como red de seguridad del alta?
- ¿Conviene guardar el correo anterior en algún sitio para poder auditar el cambio?
