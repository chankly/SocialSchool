# Proposal: La web contra datos reales, sin mocks

## Why

La web funciona con datos de ejemplo en amistades y escuelas, y con una sesión de demostración que permite
elegir el estado del perfil. Eso sirve para un boceto, no para una aplicación: enseñar gente y escuelas que
no existen es peor que no enseñar nada. El objetivo es que **todo** lo que se ve venga del API.

## What Changes

- **Fuera los datos de ejemplo**: se eliminan los servicios simulados, el almacén de datos de demo y la
  sesión de demostración. La aplicación solo funciona con API y Keycloak configurados.
- **Sin configuración no hay datos falsos**: si faltan `ApiBaseUrl` u `Oidc`, la aplicación muestra un aviso
  de configuración incompleta en vez de un perfil inventado.
- **Amistades y escuelas reales**: los servicios de ambas pasan a HTTP contra los endpoints nuevos del API
  (listar amigos y solicitudes, enviar/aceptar/rechazar/cancelar/deshacer, directorio y ficha de escuela).
- **Inicio y cabecera** dejan de usar la sesión de demo: muestran lo que diga la sesión real.

## Capabilities

### New Capabilities

Ninguna: el comportamiento del sistema no cambia, solo deja de haber datos falsos en el cliente.

### Modified Capabilities

Ninguna. El cambio se declara `skip_specs: true`.

## Impact

- **Modificado**: `src/HSoft.SocialSchool.Web` — servicios de amistades y escuelas, sesión, estado, páginas
  de inicio, amistades, escuelas y perfil, y `README-frontend.md`.
- **Eliminado**: `Services/Demo/*`, `MockSessionService`, `IDemoSessionService`, `MockFriendsService`,
  `MockSchoolsService` y el selector de sesión de la cabecera.
- **Sin cambios en el backend**, salvo los endpoints nuevos del cambio `add-community-endpoints`.
- **Consecuencia**: sin Keycloak configurado la web ya no se puede navegar con datos falsos; es intencionado.
