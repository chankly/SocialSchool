# Proposal: Completar el perfil propio

## Why

El perfil se puede consultar pero no crear. `GET /api/user/profile/me` contesta `204` y la web muestra el
formulario, pero **no hay endpoint que lo guarde**: en cuanto el usuario envía el formulario no existe nada
del lado del servidor. Además el formulario necesita el catálogo de deportes, que tampoco está publicado.

Con esto se cierra el camino: registrarse en Keycloak → rellenar el perfil → verlo.

## What Changes

- **`POST /api/user/profile/me`**: crea el perfil del usuario autenticado, sea deportista o escuela, y
  devuelve el **mismo `MyProfileResponse`** que la consulta, para que el cliente no tenga que volver a
  preguntar.
- **Cuerpo**: `nickname` más **exactamente uno** de los bloques `athlete` o `school`; el bloque tiene que
  corresponder al tipo de la cuenta, que manda sobre lo que diga el cliente.
- **Nick**: es el momento de fijarlo (`User.SetNickname`, una sola vez). Si está ocupado, `400` con un
  detalle que el cliente puede enseñar junto al campo.
- **`GET /api/sports`**: catálogo de deportes activos (`[{ id, name }]`) para el selector del formulario.
- **Catálogo con datos**: se siembran seis deportes en una migración, porque sin filas el formulario no
  tiene nada que ofrecer. Es dato de demostración, sustituible por el catálogo del técnico.
- **Sin cambios de dominio**: `User.SetNickname`, `Athlete.Create`/`UpdateProfile`/`AddPracticedSport` y
  `School.Create`/`UpdateProfile` ya cubren todo.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- **`profile`**: se le añade el requisito de completar el perfil propio.

## Impact

- **Nuevo**: comando con su handler y validadores, consulta del catálogo, un método en `IUserRepository`
  (`ExistsByNicknameAsync`) y un `POST`/`GET` en `UserProfileController` + `SportsController`.
- **Modificado**: `SportConfiguration` (semilla), una **migración**, y la spec de `profile`.
- **Fuera de alcance**: editar el perfil (`PUT`) y las maniobras declaradas.
