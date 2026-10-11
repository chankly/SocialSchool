# Tasks

## 1. Configuración

- [ ] 1.1 Añadir la sección `Keycloak` (`Authority` y `Audience`) a `src/HSoft.SocialSchool.Api/appsettings.json` con un valor de ejemplo claramente identificable (`https://<keycloak-host>/realms/<realm>` y `SocialSchool-Api`) y dejar constancia de que el valor real va en User Secrets o variables de entorno; verificar arrancando la API (`dotnet run`) que arranca sin error de configuración, lo que además valida que el JSON sigue siendo correcto.

## 2. Autenticación

- [ ] 2.1 Reescribir `AddAuthentication` en `src/HSoft.SocialSchool.Api/Core/ServicesCollectionExtensions.cs`: esquema por defecto `JwtBearerDefaults.AuthenticationScheme`, `Authority` desde `Keycloak:Authority`, `Audience` desde `Keycloak:Audience`, `MapInboundClaims = false`, `RequireHttpsMetadata = true`, y **sin** `ValidTypes` ni `ForwardDefaultSelector`; verificar que `dotnet build HSoft.SocialSchool.slnx` compila con 0 errores.
- [ ] 2.2 Eliminar el registro del esquema `OAuth2Introspection` y, tras comprobar con una búsqueda que no los usa nadie más, borrar `src/HSoft.SocialSchool.Api/Extensions/AuthenticationForwardSelector.cs` y `src/HSoft.SocialSchool.Api/Extensions/HttpContextAuthExtensions.cs`; verificar que `dotnet build` compila y que buscar `AuthenticationForwardSelector` e `IsReferenceBearerToken` en `src/` no devuelve referencias.
- [ ] 2.3 Quitar `Duende.AspNetCore.Authentication.OAuth2Introspection` de `src/HSoft.SocialSchool.Api/HSoft.SocialSchool.Api.csproj`; verificar que `dotnet build` compila y que no queda ningún `using Duende` en `src/`.

## 3. Autorización

- [ ] 3.1 En `AddAuthentication`, fijar `options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()` y eliminar la policy placeholder `assessment.list`; verificar que `dotnet build` compila y que buscar `assessment.list` en `src/` no devuelve referencias.

## 4. Identidad del usuario

- [ ] 4.1 Crear `src/HSoft.SocialSchool.Application/Services/ICurrentUser.cs` (contrato con `ExternalId` e `IsAuthenticated`) y `src/HSoft.SocialSchool.Api/Infrastructure/Security/CurrentUser.cs` (implementación sobre `IHttpContextAccessor` que lee el claim `sub`), y registrar la implementación en `AddAuthorizationHandlers()`; verificar que `dotnet build` compila.
- [ ] 4.2 Actualizar `src/HSoft.SocialSchool.Api/Filters/PerformanceAuditFilter.cs` para leer la identidad del `ClaimsPrincipal` (`sub`) y el cliente desde `azp` con `client_id` como respaldo, eliminando la decodificación manual del JWT; verificar que `dotnet build` compila y que el archivo ya no contiene `Convert.FromBase64String`.

## 5. Comprobación con Keycloak (manual)

- [ ] 5.1 Obtener un access token real del realm y comprobar el `typ` del header y el contenido del claim `aud`; verificar anotando ambos valores y, si `aud` no contiene `SocialSchool-Api`, crear el *audience mapper* en el cliente `SocialSchool-Api`. Si el `typ` del header no fuese `JWT`, ajustar `ValidTypes` en consecuencia y dejar constancia del valor real.
- [ ] 5.2 Llamar con ese token a un endpoint protegido y comprobar que no devuelve `401`; verificar que la llamada se atiende (código de éxito o de negocio, nunca de autenticación).

## 6. Pruebas

- [ ] 6.1 Crear `tests/HSoft.SocialSchool.Api.IntegrationTests/` (xUnit + `Microsoft.AspNetCore.Mvc.Testing`), añadirlo a `HSoft.SocialSchool.slnx`, dotarlo de su propio `appsettings.json` copiado a la salida y añadir `public partial class Program;` al final de `src/HSoft.SocialSchool.Api/Program.cs`; verificar que `dotnet test` ejecuta el proyecto sin fallo de arranque del host.
- [ ] 6.2 Añadir al proyecto de tests un test que fija la configuración real de validación: `Authority` y `Audience` tomados de `Keycloak:*`, `MapInboundClaims = false`, `RequireHttpsMetadata = true` y ausencia de un esquema de introspección; verificar que `dotnet test` pasa.
- [ ] 6.3 Añadir los tests de la canalización con un token firmado por una clave de test y las opciones de `JwtBearer` sustituidas en el factory: `401` sin token, éxito (`200`/`204`) con token válido, `403` con token válido que no satisface una policy registrada sólo en el factory, y `401` en `PUT /api/management/migration/apply` sin token (consecuencia de la `FallbackPolicy`); verificar que `dotnet test` pasa.

## 7. Documentación

- [ ] 7.1 Actualizar `openspec/specs/project.md` §4 y §11: Keycloak como proveedor de identidad de la API, con Google federado dentro de Keycloak, y el *audience mapper* como requisito de configuración; verificar leyendo las dos secciones que ya no afirman que la API valide tokens de Google o Facebook directamente.
- [ ] 7.2 Actualizar la sección de autenticación de `QWEN.md`: esquema único JWT Bearer contra Keycloak, claves `Keycloak:Authority`/`Keycloak:Audience`, desaparición de `securityServer:*`, de Duende y del reenvío de *reference tokens*, la `FallbackPolicy` y `ICurrentUser`; verificar leyendo la sección que no menciona introspección ni `securityServer`.

## 8. Verificación final

- [ ] 8.1 Ejecutar `dotnet build HSoft.SocialSchool.slnx` (0 errores) y `dotnet test` (todo en verde sobre la solución completa) y comprobar que no hay regresiones en el resto de proyectos.
- [ ] 8.2 Ejecutar `openspec validate add-keycloak-authentication --type change` y `openspec status --change add-keycloak-authentication`; verificar que la validación pasa y que los cuatro artefactos quedan `done` y ninguno `blocked`.
