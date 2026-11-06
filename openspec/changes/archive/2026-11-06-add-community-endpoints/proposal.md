# Proposal: Escuelas y amistades reales

## Why

La web ya tiene perfil real, pero **amistades y escuelas siguen sobre datos de ejemplo**: no existen
endpoints para ellas. Mientras eso no cambie, la aplicación no es funcional: enseña gente, escuelas y
solicitudes que no existen.

## What Changes

- **Directorio de escuelas**: `GET /api/schools` con filtros por deporte, ciudad y texto, y
  `GET /api/schools/{id}` con servicios y asistentes.
- **Amistades**: `GET /api/friends` (amigos, solicitudes recibidas y enviadas, y gente a la que se puede
  invitar) y las acciones de enviar, aceptar, rechazar, cancelar y deshacer.
- **Reglas del producto**: nada de amistad entre dos escuelas, ni con uno mismo, ni duplicada; el rechazo
  es silencioso; aceptar crea la amistad.
- **Sin datos de ejemplo**: el catálogo no se siembra aquí; escuelas y deportistas tendrán los que existan
  en el entorno.

## Capabilities

### New Capabilities

- **`schools`**: directorio y ficha pública de las escuelas.
- **`friends`**: solicitudes de amistad y lista de amigos.

### Modified Capabilities

Ninguna.

## Impact

- **Nuevo**: consultas y comandos de amistad, la consulta del directorio y la ficha, y los controladores
  `SchoolsController` y `FriendsController`.
- **Modificado**: `IFriendshipRepository`/`IFriendRequestRepository`/`ISchoolRepository` si necesitan
  consultas nuevas.
- **Sin migración**: el esquema ya tiene todo (`Friendships`, `FriendRequests`, `Schools`, `SchoolServices`,
  `SchoolAttendances`).
- **Fuera de alcance**: bloquear usuarios, puntuar y denunciar (no hay endpoints para crearlos todavía).
