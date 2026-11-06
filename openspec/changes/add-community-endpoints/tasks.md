# Tasks

## 1. Escuelas

- [ ] 1.1 Consulta del directorio: `GetSchoolsQuery(sportId?, city?, search?)` con su handler, que devuelve solo cuentas de tipo `School` con estado `Active` o `PendingValidation`, ordenadas por nombre y como máximo 50. Necesita un método nuevo en `ISchoolRepository` con los filtros y el `Include` del deporte para el nombre. Verificación: los filtros son opcionales y la búsqueda por texto no distingue mayúsculas.
- [ ] 1.2 Consulta de la ficha: `GetSchoolQuery(id)` con servicios y asistentes aceptados (nick y foto), o `null` si no existe. Verificación: no carga colecciones de otros agregados y los asistentes salen solo de `SchoolAttendance` aceptadas.
- [ ] 1.3 `SchoolsController` con `GET api/schools` y `GET api/schools/{id}` (404 cuando no existe), DTOs en `Models/Schools/` y XML docs.

## 2. Amistades

- [ ] 2.1 Consulta `GetFriendsQuery` que devuelva amigos, solicitudes recibidas, enviadas y descubribles según las reglas del diseño. Necesita métodos de lista en los repositorios de `Friendship`, `FriendRequest` y `User`. Verificación: uno mismo, los amigos y quien tiene solicitud pendiente no aparecen como descubrible.
- [ ] 2.2 Comando para enviar solicitud, con validador del mensaje (destinatario obligatorio y GUID) y las reglas de negocio en el handler: no a uno mismo, no entre dos escuelas, no si ya son amigos, no si hay una pendiente en cualquier dirección. Devuelve el identificador de la solicitud. Verificación: cada rechazo es un `Result.Failure` con su motivo y deja la base intacta.
- [ ] 2.3 Comandos de aceptar, rechazar y cancelar: solo el receptor acepta o rechaza, solo el emisor cancela, y solo si está pendiente. Aceptar crea la `Friendship` y guarda las dos cosas; rechazar no genera nada más. Verificación: un tercero no puede resolver una solicitud ajena.
- [ ] 2.4 Comando para deshacer una amistad, por cualquiera de las dos partes. Verificación: deja de aparecer en las dos listas.
- [ ] 2.5 `FriendsController` con las cinco rutas del contrato, DTOs en `Models/Friends/` y XML docs con `[ProducesResponseType]`.

## 3. Verificación

- [ ] 3.1 Tests de handler con repositorios simulados: directorio con y sin filtros; ficha con servicios y asistentes; descubribles excluyendo uno mismo, amigos y pendientes; envío rechazado por uno mismo, por dos escuelas, por amistad existente y por solicitud pendiente; aceptar crea la amistad; aceptar por un tercero falla; deshacer amistad.
- [ ] 3.2 `dotnet build` y `dotnet test` en verde (salvo el fallo preexistente de audiencia).
