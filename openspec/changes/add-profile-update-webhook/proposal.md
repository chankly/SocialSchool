# Proposal: Sincronizar los cambios de perfil que llegan de Keycloak

## Why

El webhook solo atiende al alta. Keycloak también lanza `access.UPDATE_PROFILE` cuando el usuario edita sus
datos de cuenta, y hoy esos eventos se acusan y se tiran: si cambia el correo o el idioma, la cuenta se
queda con el dato viejo. Los dos ejemplos recibidos lo demuestran y traen cosas distintas: uno un cambio de
idioma (`updated_locale`), otro un cambio de nombre y apellidos.

## What Changes

- **El webhook atiende `access.UPDATE_PROFILE`**: sincroniza en la cuenta el **correo** y el **idioma
  preferido** que el evento indique como modificados.
- **El tipo de cuenta no se toca nunca**: el cambio de tipo lo hace un técnico a mano, y `User` no expone
  ningún mutador para él.
- **Los cambios de nombre y apellidos se ignoran**: son datos de nuestro perfil, no de la cuenta, y
  `project.md` §4 ya dice que Keycloak no captura los datos del perfil.
- **Dominio**: aparece `ChangeEmail`, que es lo único que faltaba; `ChangePreferredLanguage` ya existía.
- **Modelo del evento**: se añade `details` con los dos campos que se usan, sin modelar el cuerpo entero.
- **Idempotencia**: si el dato que llega es el que ya está guardado, no se escribe nada.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- **`registration`**: se le añade el requisito de sincronizar los cambios de perfil, con sus escenarios.
  Es la misma capacidad porque es el mismo webhook y los mismos eventos de Keycloak.

## Impact

- **Nuevo**: `SyncUserProfileCommand` + handler + validador del mensaje.
- **Modificado**: `User` (`ChangeEmail`), `KeycloakEvent`/`KeycloakEventDetails` (el `details` del evento),
  `WebHooksController` (reparto por tipo de evento) y la spec de `registration`.
- **Sin cambios de esquema**: no hay migración; la columna del correo ya existe.
- **Fuera de alcance**: materializar la cuenta desde un evento de actualización (eso sigue siendo del alta)
  y cualquier sincronización en la dirección contraria, de nuestra cuenta hacia Keycloak.
