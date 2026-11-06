# Proposal: Conectar el cliente web al API

## Why

El cliente Blazor funciona entero sobre mocks: el formulario del perfil se guarda en memoria y el catálogo
de deportes es una lista local. Con el `POST /api/user/profile/me` y el `GET /api/sports` ya en el backend,
el siguiente paso es que la web hable con el API de verdad, que es lo que convierte el boceto en una primera
versión usable.

## What Changes

- **Servicios reales**: `IProfileService` y `ISportsService` pasan a hacer HTTP contra el API (consulta del
  perfil, alta del perfil y catálogo), manteniendo los DTOs que ya reflejan el contrato.
- **Autenticación OIDC**: configuración de Keycloak para el cliente (`AddOidcAuthentication` +
  `AuthorizationMessageHandler`) con la autoridad y el `clientId` en `wwwroot/appsettings.json`, sin secretos.
- **Respaldo de demostración**: si el OIDC no está configurado, se mantiene la sesión de demo del boceto,
  para poder seguir viendo los tres estados del perfil sin depender de Keycloak.
- **Fuera de alcance**: amistades y escuelas siguen con mocks; no hay edición de perfil.

## Capabilities

### New Capabilities

Ninguna: no cambia el comportamiento del sistema, solo se implementa el consumo del API ya especificado.

### Modified Capabilities

Ninguna. El cambio se declara `skip_specs: true`.

## Impact

- **Modificado**: `src/HSoft.SocialSchool.Web` (servicios, `Program.cs`, `appsettings.json`, estado de
  sesión y la página del perfil), más una dependencia NuGet de autenticación de Blazor WASM.
- **Sin cambios en el backend**.
- **Paso manual**: crear en Keycloak el cliente público de la web (PKCE) y poner su `clientId` en la
  configuración; hasta entonces la web funciona con la sesión de demo.
