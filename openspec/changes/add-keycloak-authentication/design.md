# Design: Autenticación con Keycloak

## Context

Ver `proposal.md` para la motivación. Estado actual que condiciona el enfoque (verificado en el
código):

- El pipeline ya monta `UseRouting() → UseAuthentication() → UseAuthorization() → MapControllers()`
  en `Api/Extensions/WebApplicationExtensions.cs`, y `UseSwagger`/`UseSwaggerUI` van **antes**, así
  que Swagger se sirve sin pasar por autorización.
- `Api/Core/ServicesCollectionExtensions.cs` → `AddAuthentication` registra hoy un esquema por defecto
  llamado `"token"` (JwtBearer con `securityServer:authority`, `ValidTypes = ["at+jwt"]`,
  `ValidateAudience = false` y reenvío de *reference tokens*) y un segundo esquema
  `"Introspection"` (Duende OAuth2). El único uso de `Duende` está ahí.
- Existe una sola policy, `"assessment.list"`, con `RequireClaim("")`; **ningún endpoint la usa** y no
  hay ningún `[Authorize]` en `src/`.
- `AddAuthorizationHandlers()` sólo hace `AddHttpContextAccessor()`.
- `Api/Infrastructure/Security/` está vacía.
- `Api/Filters/PerformanceAuditFilter.cs` (filtro global) decodifica el JWT a mano en base64 para leer
  `sub` y `client_id`, e ignora el `ClaimsPrincipal`.
- El host se configura con Autofac como `ServiceProviderFactory` y `HostBuilderExtensions` exige
  `appsettings.json` en el **directorio de trabajo** (`optional: false`).

## Goals / Non-Goals

**Goals:**

- Que la API valide tokens de Keycloak con emisor, firma, expiración y audiencia, sin atajos.
- Que la postura por defecto sea segura (denegar) y que lo público sea una decisión explícita.
- Dejar una forma única y reutilizable de obtener la identidad externa, usable desde Application.
- Poder verificar 401/403/200 sin depender de un Keycloak disponible.

**Non-Goals:**

- Endpoints de cuenta y tabla de usuarios (los prohíbe el alcance de la petición).
- CORS: se mantiene separado de la autenticación y fuera de este cambio.
- Roles y permisos de negocio: no existen todavía y no se inventan.
- Alinear el paquete `Microsoft.AspNetCore.Authentication.JwtBearer` (sigue en 8.0.25).
- Conectar `UseExceptionHandler` (401 y 403 los produce el middleware de autorización, no excepciones).

## Decisions

**1. Un único esquema, `JwtBearerDefaults.AuthenticationScheme`.** El nombre `"token"` sólo tenía
sentido para convivir con el segundo esquema. *Alternativa descartada*: conservar el nombre `"token"`
para no tocar el arranque; se descarta porque mantiene un nombre engañoso sin contrapartida.

**2. Denegar por defecto con `FallbackPolicy`.** `options.FallbackPolicy` exige usuario autenticado, y
lo público se marca `[AllowAnonymous]`. Es lo que pide la petición ("los endpoints públicos puedan
declararse explícitamente como anónimos") y evita que un endpoint nuevo nazca público por olvido.
*Alternativa descartada*: `[Authorize]` controlador a controlador; deja la API abierta por defecto y
depende de que nadie se olvide. Consecuencia asumida: `ManagementController` pasa a requerir token.

**3. Se elimina la introspección.** La API sólo acepta JWT Bearer. *Alternativa descartada*:
conservar Duende apuntando a Keycloak para tokens opacos; implicaría mantener una dependencia, un
client secret en la API y un camino de autenticación que nadie va a usar.

**4. `MapInboundClaims = false`.** Deja los claims con su nombre original, de modo que `sub` se lee
como `sub`. *Alternativa descartada*: dejar el mapeo por defecto (renombra `sub` a una URI de
`schemas.xmlsoap.org`), que obliga a leer claims con nombres largos y dificulta razonar sobre los
tokens de Keycloak.

**5. La distinción access token / ID token se resuelve por audiencia, no por `ValidTypes`.** El ID
token va destinado al cliente web, así que exigir `aud = SocialSchool-Api` lo rechaza. Sobre
`ValidTypes` hay una incógnita real: valida el `typ` del **header** del JWT (su valor por defecto es
`"JWT"`), mientras que Keycloak distingue access e ID token con un `typ` en el **payload**. Por eso la
decisión por defecto es **quitar la línea `ValidTypes`** y **verificarla con un token real** antes de
darla por buena; la alternativa (fijarla a `"at+jwt"`) sólo vale si el realm emite RFC 9068.

**6. `ICurrentUser` en Application, implementación en Api.** El contrato (`ExternalId`, `IsAuthenticated`)
vive en `Application/Services/` para que los futuros handlers lo consuman sin conocer `HttpContext`, y
la implementación en la carpeta reservada `Api/Infrastructure/Security/`, leyendo el claim `sub`.
*Alternativas descartadas*: exponer `ClaimsPrincipal` a Application (acopla el dominio de la aplicación
a ASP.NET) y dejarlo sólo en Api (los casos de uso no podrían usarlo).

**7. La configuración usa claves nuevas `Keycloak:Authority` y `Keycloak:Audience`.** Nombran lo que
son, y `Audience` activa de paso la validación de audiencia. *Alternativa descartada*: reutilizar
`securityServer:*`, que describe a otro proveedor y mezclaría conceptos. No se añade ningún secreto de
cliente: para validar JWT no hace falta.

**8. Tests de integración con un token firmado localmente.** El host de test sustituye las opciones de
`JwtBearer` por una clave de firma de test, y se emiten tokens con ella; así se ejercita **nuestra**
canalización (401/403/200, lectura de `sub`) sin Keycloak. Para que eso no convierta el test en una
prueba de mentira, se añade además un test que fija la **configuración real** (`Authority`, `Audience`,
`MapInboundClaims = false`, `RequireHttpsMetadata`, y la ausencia del esquema de introspección). El
403 se prueba con una **policy registrada sólo en el factory de test**, para no inventar permisos de
negocio en producción. *Alternativa descartada*: levantar Keycloak en un
contenedor; más fiel, pero añade una dependencia de infraestructura a un cambio ya grande.

**9. El filtro de auditoría lee del `ClaimsPrincipal`.** Se elimina la decodificación manual del JWT y
se lee el identificador del cliente desde `azp` (lo que emite Keycloak) además de `client_id`, para no
degradar en silencio a `unknown`. *Alternativa descartada*: dejarlo como está; el log de auditoría
quedaría ciego precisamente con el proveedor nuevo.

## Risks / Trade-offs

- **Sin *audience mapper* en Keycloak, `aud` no contendrá `SocialSchool-Api` y se rechazarán todos los
  tokens** → paso manual documentado y comprobación manual con un token real.
- **El `typ` del token es desconocido** → se quita `ValidTypes` por defecto y hay una tarea explícita
  de verificación con un token del realm antes de considerar cerrado el cambio.
- **Los tests no prueban la integración real con Keycloak** → se documenta la limitación y la
  comprobación manual con token real forma parte de la verificación.
- **La `FallbackPolicy` cambia el comportamiento de endpoints existentes** (`ManagementController`
  pasa a exigir token) → es la opción elegida de forma consciente; si se quisiera conservar público,
  bastaría `[AllowAnonymous]`.
- **`appsettings.json` es obligatorio en el directorio de trabajo**, y el host usa Autofac → el
  proyecto de test necesita su propio `appsettings.json` copiado a la salida; sin él, el host de test
  no arranca.
- **`UseHttpsRedirection` en el host de test** podría producir redirecciones en lugar del código
  esperado → sin puerto HTTPS configurado no redirige; si apareciera, se ajusta el host de test.

## Migration Plan

1. Añadir las claves `Keycloak:Authority` y `Keycloak:Audience` (User Secrets o variables de entorno)
   antes de desplegar el código.
2. En Keycloak: crear el *audience mapper* en el cliente `SocialSchool-Api` para que `aud` contenga
   `SocialSchool-Api`.
3. Desplegar el código (validación, política por defecto e identidad).
4. Verificar manualmente con un token real del realm.

**Reversión**: no hay cambios de datos ni migraciones; revertir el despliegue del código devuelve el
comportamiento anterior. En Keycloak no hace falta deshacer nada (el mapper es inocuo).

## Open Questions

Ninguna. Lo único pendiente (el valor de `ValidTypes`) no cambia el enfoque ni el plan de tareas:
tiene una tarea de verificación propia y un valor por defecto documentado.
