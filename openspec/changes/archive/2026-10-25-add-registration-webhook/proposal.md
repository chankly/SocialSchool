# Proposal: Alta de la cuenta desde el registro de Keycloak

## Why

Hoy nadie crea la cuenta de negocio. `WebHooksController` recibe el evento de Keycloak pero solo escribe
el cuerpo en consola, así que la tabla `Users` está vacía: `GET /api/user/profile/me` responde siempre
`204` y la web no puede distinguir "acabo de registrarme" de "tengo el perfil sin rellenar". Sin esta
pieza, el flujo que quedó escrito en `project.md` §4 no funciona de punta a punta.

El evento de registro trae la identidad (`authDetails.userId`) pero **no** los datos de la cuenta, así que
hay que pedirlos a la Admin API de Keycloak con un token de `client_credentials`.

## What Changes

- **El webhook materializa la cuenta**: al recibir un evento `access.REGISTER`, consulta
  `GET /admin/realms/{realm}/users/{userId}` y crea el `User` con el correo, el tipo de cuenta
  (`attributes.userType`), el proveedor de identidad (`federatedIdentities`) y el idioma (`locale`),
  guardando el identificador de Keycloak como identidad externa.
- **Cliente de la Admin API**: token por `client_credentials` y lectura del usuario, con las credenciales
  en configuración (User Secrets en local, **Key Vault → variables de entorno** en Azure).
- **El nick deja de pedirse al crear la cuenta**: `User.Create` ya no lo exige, la columna admite nulos y
  aparece `SetNickname`, que solo se puede llamar una vez. Es lo que permite crear la cuenta antes de que
  el usuario elija su nick, y encaja con la regla de que el nick es suyo, único e inmutable.
- **Idempotencia**: un registro repetido no crea una segunda cuenta.
- **Infraestructura**: dos secretos nuevos en Key Vault y dos variables de entorno en la Container App,
  más la documentación del cliente confidencial de Keycloak y el rol `view-users`.
- **Specs**: la regla del nick en `domain.md` y `project.md`, y una capacidad nueva, `registration`.

## Capabilities

### New Capabilities

- **`registration`**: alta de la cuenta de negocio a partir del evento de registro de Keycloak.

### Modified Capabilities

Ninguna. `profile` ya describe que la consulta devuelve `204` mientras no haya perfil; esto es lo que
crea la cuenta que esa consulta busca.

## Impact

- **Nuevo**: `Application/Queries|Commands`... no: `Application/Commands/Registration/` con el mensaje y
  el handler, `Application/Services/IKeycloakUsersApi`, `Persistence/Services/KeycloakUsersApi`, el modelo
  del perfil de Keycloak y la spec `registration`.
- **Modificado**: `User` (Create y Nickname), `UserConfiguration`, una **migración** que hace nullable la
  columna, `WebHooksController`, los módulos `key-vault` y `container-apps/api` de Terraform, la raíz de
  Terraform y los dos README de `devops`, `domain.md` y `project.md`.
- **Sin cambios en la autenticación**: el webhook sigue siendo anónimo por decisión del propietario.
- **Pasos manuales**: crear en Keycloak el cliente confidencial con service account y el rol
  `view-users`, y rellenar `TF_VAR_keycloak_admin_client_id` y `TF_VAR_keycloak_admin_client_secret`.
