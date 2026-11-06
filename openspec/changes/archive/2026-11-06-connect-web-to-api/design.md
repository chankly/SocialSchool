# Design: Conectar el cliente web al API

## Context

- `src/HSoft.SocialSchool.Web` ya tiene `IProfileService`, `ISportsService`, `ISessionService` y sus mocks,
  con DTOs que copian el contrato real (`ProfileResponse` con `athlete`/`school`, `SportSummary`…).
- La página `Pages/Profile/MyProfile.razor` resuelve los tres estados: `200` perfil, `204` formulario y
  `401` sin sesión, y al guardar pasa de `204` a `200` con el mock.
- El backend expone `GET` y `POST /api/user/profile/me`, `GET /api/sports`, y todo cuelga de la política de
  denegación por defecto: sin token, `401`.
- El `clientId` de la API en Keycloak (la audiencia) es un GUID y ya está en la configuración del boceto.

## Decisions

**1. Mocks y HTTP conviven detrás de la misma interfaz, y se elige por configuración.** Si
`WebAppSettings` trae `ApiBaseUrl` y `Oidc` completos, se registran los servicios HTTP; si no, los mocks.
Así el boceto sigue funcionando sin Keycloak (y se puede revisar en esta máquina) sin mantener dos páginas.

**2. Autenticación con `AddOidcAuthentication` y `AuthorizationMessageHandler`.** Es el mecanismo estándar de
Blazor WASM: el `HttpClient` del perfil y del catálogo sale con el bearer puesto, y el `401` se traduce en
redirigir al login. Nada de tokens en `localStorage` a mano.

**3. Los DTOs de respuesta no se tocan.** Ya coinciden con `MyProfileResponse` y con el catálogo; el trabajo
es sustituir la implementación, no el contrato.

**4. El alta envía solo el bloque del tipo de la cuenta.** El cliente conoce su tipo por el token (con la
normalización `Athlete`/`Deportista` que ya existe) y manda `nickname` + el bloque correspondiente; el
servidor es el que decide si eso cuadra con la cuenta.

**5. Amistades y escuelas se quedan en mock.** Su backend no existe todavía; cambiarlas ahora sería inventar
contratos.

## Risks / Trade-offs

- **Sin Keycloak configurado no se prueba el camino real**: la sesión de demo cubre la UI, pero el `POST`
  contra el API necesita token. La prueba de punta a punta es del propietario.
- **El cliente público de Keycloak es manual**: hasta que exista, `Oidc` va vacío y se usa el respaldo.
- **Una dependencia NuGet nueva** (`Microsoft.AspNetCore.Components.WebAssembly.Authentication`), justificada
  por la autenticación.

## Migration Plan

1. Crear el cliente público de la web en Keycloak (PKCE, sin secreto) y poner `Authority` y `ClientId` en
   `wwwroot/appsettings.json`.
2. Desplegar la web (o servirla en local) y completar un perfil real.
3. **Reversión**: vaciar `Oidc`/`ApiBaseUrl` vuelve al modo demo.

## Open Questions

- Si el catálogo de deportes acabará cacheado en el cliente.
- Si las amistades y las escuelas se conectan en el siguiente cambio o se prioriza la edición del perfil.
