# Proposal: Autenticación con Keycloak

## Why

La autenticación actual es la que traía la plantilla y no sirve para el producto: valida un emisor
distinto (`securityServer:authority`), tiene la **validación de audiencia desactivada**
(`ValidateAudience = false`), exige un tipo de token (`at+jwt`, RFC 9068) que Keycloak no emite por
defecto, y admite un segundo mecanismo (introspección de tokens opacos) que el producto no necesita.
Consecuencia directa: **hoy ningún endpoint está protegido** — ni siquiera
`PUT /api/management/migration/apply`. Mientras esto no se corrija, cualquier endpoint que
construyamos nace público y sin forma fiable de saber quién llama.

Además, la operación será Keycloak (federando Google), mientras que `project.md` §4 y §11 siguen
describiendo Google y Facebook como integraciones directas de la API.

## What Changes

- **Validación de access tokens de Keycloak**: autoridad, audiencia `SocialSchool-Api`, firma, emisor
  y expiración. Se activa la validación de audiencia (hoy desactivada) y se fija
  `MapInboundClaims = false` para leer los claims originales (`sub`).
- **BREAKING** — **Se elimina el esquema de introspección** (Duende `OAuth2Introspection`) y el
  reenvío de *reference tokens*: la API solo acepta JWT Bearer. Desaparecen la dependencia
  `Duende.AspNetCore.Authentication.OAuth2Introspection`, los helpers `AuthenticationForwardSelector`
  y `HttpContextAuthExtensions`, y la necesidad de un client secret en la API.
- **BREAKING** — **Denegar por defecto**: `FallbackPolicy = RequireAuthenticatedUser()`. Todos los
  endpoints MVC pasan a exigir token, **incluido el de migraciones**, que era público. Lo que deba ser
  público se marca `[AllowAnonymous]` de forma explícita. Swagger no se ve afectado, porque
  `UseSwagger`/`UseSwaggerUI` se sirven antes del middleware de autenticación.
- Se elimina la policy placeholder `"assessment.list"` (`RequireClaim("")`, sin ningún uso).
- **Identidad reutilizable**: nueva abstracción `ICurrentUser`, con el contrato en Application y la
  implementación en Api, para que los futuros casos de uso obtengan el `sub` sin tocar `HttpContext`.
- **Configuración**: sección `Keycloak: { Authority, Audience }`, sin secretos de cliente. El valor
  real va en User Secrets o variables de entorno.
- **Auditoría**: `PerformanceAuditFilter` deja de leer `client_id` (que Keycloak no emite; emite `azp`)
  y deja de decodificar el JWT a mano.
- **Pruebas**: proyecto de tests de integración que verifica 401/403/200 y fija la configuración real
  de validación.
- **Documentación**: `project.md` §4 y §11 (Keycloak con Google federado) y la sección de
  autenticación de `QWEN.md`, que quedaría obsoleta.

## Capabilities

### New Capabilities

- `authentication`: la API valida access tokens JWT emitidos por Keycloak, exige usuario autenticado
  por defecto, permite declarar endpoints públicos de forma explícita y expone la identidad externa
  (`sub`) del usuario autenticado a la capa de aplicación.

No hay capacidades existentes: `openspec list --specs` está vacío, porque `project.md` y `domain.md`
son documentos de nivel de proyecto y no capacidades del CLI.

### Modified Capabilities

Ninguna.

## Impact

- **Api**: `Core/ServicesCollectionExtensions.cs` (se rehace la sección de autenticación),
  `Infrastructure/Security/` (carpeta hoy vacía: aloja la implementación de `ICurrentUser`),
  `Filters/PerformanceAuditFilter.cs`, `Program.cs` (hacer accesible `Program` para el host de test),
  `appsettings.json` y `.csproj` (quitar Duende). **No** se toca el orden del pipeline.
- **Application**: nuevo contrato `Services/ICurrentUser.cs`.
- **Nuevo proyecto**: tests de integración, más su alta en `HSoft.SocialSchool.slnx`.
- **Configuración externa**: claves `Keycloak:Authority` y `Keycloak:Audience`. Dejan de usarse las
  claves `securityServer:*` / `SecurityServer:*`.
- **Pasos manuales en Keycloak** (documentados en el cambio, no automatizables desde el repositorio):
  un *audience mapper* en el cliente `SocialSchool-Api` para que el claim `aud` contenga esa
  audiencia; sin él, todos los tokens serán rechazados. Y comprobar el `typ` real de un access token
  del realm, que decide el valor de `ValidTypes`.
- **Fuera de alcance**: endpoints de cuenta y tabla `Users`, CORS, roles y permisos de negocio,
  alineación del paquete `JwtBearer` (sigue en 8.0.25) y la conexión de `UseExceptionHandler`.
