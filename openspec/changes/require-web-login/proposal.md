# Proposal

## Why

El API ya deniega por defecto todo lo que no lleve token, pero el cliente web no: `App.razor` solo
comprueba que haya configuración y cualquiera que abra la URL ve la interfaz, incluida la portada. La
protección depende hoy de que cada pantalla se acuerde de pintar su tarjeta de «sin sesión», y la
portada ni siquiera lo hace, así que la aplicación muestra una aplicación vacía a quien no se ha
autenticado y descubre el problema cuando las llamadas responden `401`.

## What Changes

- **BREAKING**: dejará de poderse navegar por el cliente sin sesión. Cualquier ruta distinta de las
  de acceso arranca el login de Keycloak en cuanto se abre, sin portada intermedia ni contenido.
- La protección pasa a ser **por defecto**: toda página exige sesión sin que haya que acordarse de
  marcarla, y las pantallas de acceso (`/authentication/*`) se declaran explícitas como anónimas.
- Al volver del login se aterriza en la ruta que se estaba pidiendo, no siempre en el perfil.
- Una respuesta `401` con la aplicación abierta devuelve al login conservando la ruta actual.
- Se eliminan las tarjetas de «sin sesión» y los botones de acceso repartidos por las pantallas y por
  el layout, que quedan inalcanzables.
- El aviso de configuración incompleta se mantiene alcanzable y **no** provoca un bucle de login.

## Capabilities

### New Capabilities

- `web-access`: reglas de acceso del cliente web: qué puede ver quien no ha iniciado sesión, cómo se
  inicia el login, cómo se conserva la ruta de destino y qué ocurre cuando la sesión se rechaza.

### Modified Capabilities

- Ninguna. La capability `authentication` describe la validación de tokens y la denegación por
  defecto **del API**, que no cambia; el acceso del cliente es una responsabilidad distinta.

## Impact

- **Cliente web** (`src/HSoft.SocialSchool.Web`): `App.razor`, `_Imports.razor`,
  `Pages/Authentication/Authentication.razor`, `Extensions/ServiceCollectionExtensions.cs`,
  `Components/Layout/MainLayout.razor`, y las pantallas `Home`, `MyProfile`, `FriendsPage`,
  `SchoolsPage` y `SchoolDetailPage`. Un componente nuevo para redirigir al login y un handler nuevo
  para el `401`.
- **Sin cambios** en el API, en `Domain`/`Application`/`Persistence`, en las migraciones, en Terraform
  ni en la configuración del realm. El webhook de Keycloak sigue anónimo.
- **Configuración externa**: el cliente público de Keycloak ya tiene que admitir
  `https://localhost:7200/authentication/login-callback` y el *web origin*; sin eso el login falla
  igual que hoy.
- **Documentación**: `README-frontend.md` pasa a describir la regla de acceso.
- **Verificación**: no hay proyecto de tests para la web; la comprobación es compilación más la
  matriz manual de escenarios.
