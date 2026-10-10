# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Qué es este proyecto

API web ASP.NET Core (`net10.0`) estructurada en **Clean Architecture**. Es la materialización de una plantilla `dotnet new` (`.template.config/template.json`, shortName `clean-arch`, `sourceName` = `HSoft.SocialSchool`): al instanciarla se renombran namespaces y carpetas. El dominio de ejemplo incluido es un **Referee Evaluator** (partidos, árbitros, equipos).

La solución (`HSoft.SocialSchool.slnx`, en la carpeta padre de `src/`) tiene 4 proyectos con flujo de dependencias estricto:

```
Api ──► Application ──► Domain
  └───► Persistence ──► Application, Domain
```

- **Domain** — entidades, value objects, eventos de dominio, enums y primitivas (`Entity`, `ValueObject`, `IAggregateRoot`, `IBaseRepository`). Sin dependencias salvo `Mediator.Abstractions` (para `INotification`).
- **Application** — casos de uso vía Mediator (Commands/Queries + Handlers), behaviors de pipeline, validadores FluentValidation, modelos de respuesta, opciones. No conoce a Persistence.
- **Persistence** — `MyCompanyDbContext` (EF Core + SQL Server), configuraciones `IEntityTypeConfiguration`, repositorios, módulo Autofac, servicio de migraciones.
- **Api** — composición (DI, middleware, auth), controladores, filtros. Único punto de arranque.

## Comandos

Ejecutar desde la carpeta `src/` (o usar `-p`/`-s` con rutas explícitas). No hay scripts ni Makefile.

```bash
# Restaurar y compilar toda la solución
dotnet build ../HSoft.SocialSchool.slnx

# Ejecutar la API (perfil http: http://localhost:5094 ; https: https://localhost:7146)
dotnet run --project HSoft.SocialSchool.Api
dotnet run --project HSoft.SocialSchool.Api --launch-profile https

# Swagger UI disponible en /swagger una vez arrancada
```

### Migraciones EF Core

El `DbContext` vive en **Persistence** pero la API es el startup project. Hay que indicar ambos:

```bash
dotnet ef migrations add <Nombre> \
  --project HSoft.SocialSchool.Persistence \
  --startup-project HSoft.SocialSchool.Api

dotnet ef database update \
  --project HSoft.SocialSchool.Persistence \
  --startup-project HSoft.SocialSchool.Api
```

En runtime, las migraciones pendientes se aplican vía `PUT /api/management/migration/apply` (requiere policy `user.admin`).

### Pruebas

**No existe ningún proyecto de tests** en la solución todavía. Si añades uno (xUnit/NUnit), créalo fuera de `src/` o como proyecto adicional y agrégalo al `.slnx`; luego `dotnet test`.

## Arquitectura: lo que hay que entender antes de tocar código

### Mediator (NO es MediatR)
Se usa **`Mediator`** de martinothamar (`Mediator.SourceGenerator` 3.0.2), que genera el dispatcher en tiempo de compilación. Implicaciones:
- Los handlers devuelven `ValueTask<T>`, no `Task<T>`.
- El registro está en `Application/IoC/Services/ServicesExtensions.cs` (`RegisterMediator`). El generador escanea las **assemblies declaradas explícitamente** en `options.Assemblies` — incluye la de Application **y la de Domain** (para que los `INotificationHandler<T>` de eventos de dominio se vinculen). Si añades handlers en una assembly nueva, agrégala ahí.
- Orden de los pipeline behaviors: `LoggingBehavior` → `ValidationBehavior` (`Application/Behaviors/`).
- Tipos generados como `internal`; los handlers se escriben `internal`.

### Eventos de dominio en dos fases (clave)
`MyCompanyDbContext.SaveChangesAsync` orquesta los eventos alrededor del commit (ver `MyCompanyDbContext.cs` + `Persistence/Extensions/MediatorExtensions.cs`):
1. **Pre-commit (transaccionales)**: `DispatchDomainEventsAsync` se publica *dentro* de la execution strategy, antes de `base.SaveChangesAsync`. El bucle reevalúa el ChangeTracker en cada iteración, de modo que eventos encolados por otros handlers también se procesan.
2. **Post-commit**: los eventos que implementan `IPostCommitNotification` se acumulan y se publican **después** del commit (pueden leer datos ya persistidos).

Las entidades acumulan eventos con `Entity.AddDomainEvent`. Eventos marcados con `IUniqueDomainEvent` se deduplican por valor (usa `record`s).

### Resultado funcional, no excepciones
Los handlers devuelven `Result` / `Result<T>` de **CSharpFunctionalExtensions**. Los controladores mapean `response.IsFailure` → `Problem(...)` (ver `ManagementController`). Sigue ese patrón en handlers y controladores nuevos.

### Validación por partida doble
- `FluentValidationFilter` (filtro MVC): valida los argumentos de acción del controlador y devuelve `ValidationProblemDetails` (RFC 7807) con 400.
- `ValidationBehavior` (pipeline Mediator): valida los requests de Mediator y lanza `ValidationException`.
Ambos filtros se registran globalmente en `AddConfiguredControllers`. Los validadores se descubren con `AddValidatorsFromAssemblyContaining<GetMatchQuery>`. Para crear validadores existe el skill `fluent-validation`.

### DI híbrida: Autofac + contenedor nativo
`HostBuilderExtensions` instala `AutofacServiceProviderFactory`. El registro está repartido:
- **Nativo** (`ConfigureServices` en `Program.cs`): Mediator, validators, `DbContextPool`, caches.
- **Autofac** (`ConfigureAutofacContainer` en `Program.cs`): **actualmente todos los `RegisterModule` están comentados.** El `PersistenceModule` (que auto-registra por convención todo tipo cuyo nombre termine en `Repository` —excepto los `Caching*`— y el `MigrationService`) **no está activo**: para que se inyecten repositorios e `IMigrationService` hay que descomentar su registro en `Program.cs`.

### Persistencia (EF Core)
- `AddDbContextPool` con SQL Server, `EnableRetryOnFailure` (3 reintentos) y **lazy-loading proxies** (`Microsoft.EntityFrameworkCore.Proxies`) — por eso `Entity.Id` y navegaciones son `virtual` y `Equals` desenvuelve los proxies `Castle.Proxies.*`.
- Configuraciones vía `IEntityTypeConfiguration<T>` auto-aplicadas con `ApplyConfigurationsFromAssembly`. Los value objects se mapean como **owned types** (ej. `TeamInfo` → columnas `HomeTeam*`/`AwayTeam*` en `MatchConfiguration`).
- La migrations assembly es la propia Persistence.

### Convenciones de modelado de dominio
- Entidades: constructor privado + **factory estático `Create(...)`**, setters `private`, invariantes validadas en métodos de comportamiento (ej. `Match.UpdateStatus`, `UpdateScore`). Heredan de `Entity` e implementan `IAggregateRoot` si son raíz.
- Value objects heredan de `ValueObject`; se crean/mutan con métodos `With...` que devuelven nuevas instancias.
- Estructura por feature dentro de cada capa: `Commands/<Feature>/`, `Queries/<Feature>/`, `Entities/<Feature>/`, `Configurations/`.

### Logging y observabilidad
- **Serilog** configurado en `WebApplicationBuilderExtensions` (enrichers: environment, threadId; lee `ReadFrom.Configuration`). Inyectar `Serilog.ILogger` directamente.
- `PerformanceAuditFilter`: loguea cada request/response con timings; si `AuditingEnabled=true` emite líneas `AUDIT` con sub/client extraídos del JWT.

## Autenticación

Esquema por defecto `"token"` (`ServicesCollectionExtensions.AddAuthentication`):
- **JWT Bearer** para tokens `at+jwt`.
- **OAuth2 Introspection** (Duende) para reference tokens — `AuthenticationForwardSelector` reenvía al esquema `"Introspection"` cuando el token no es un JWT (no contiene puntos).

## Configuración y variables de entorno

`HostBuilderExtensions.ConfigureHost` carga la configuración desde una carpeta **`Config/`** (relativa al content root), no desde la raíz del proyecto:
- `Config/appsettings.json` — **requerido**
- `Config/appsettings.{Environment}.json` — opcional
- `Config/security.json` — **requerido** (no incluido en el repo; sin él la app no arranca)
- `secrets/akv_secrets.json` — opcional (secretos de Azure Key Vault)
- User Secrets (`AddUserSecrets<Program>`) y variables de entorno

> Nota: los `appsettings.json` actuales están en la **raíz del proyecto Api**, no en `Config/`. Al partir de la plantilla hay que crear `Config/appsettings.json` y `Config/security.json` (o ajustar las rutas) antes de ejecutar.

Claves de configuración que el código consume:
- `ConnectionStrings:MyCompany` — cadena de conexión SQL Server.
- `SecurityServer:Authority` / `securityServer:authority` — autoridad del servidor de seguridad (JWT/introspection).
- `SecurityServer:ApiName`, `SecurityServer:ApiSecret` — credenciales para introspection.
- `AuditingEnabled` — `"true"`/`"false"`, activa las trazas AUDIT.
- `ASPNETCORE_ENVIRONMENT` — `Development` por defecto en `launchSettings.json`.

## Puntos de entrada

- `Api/Program.cs` — top-level statements; `ConfigureWebApplicationBuilder` (servicios y contenedor) → `app.Configure()` (pipeline HTTP en `WebApplicationExtensions`) → `RunAsync`.
- `Api/Controllers/ManagementController.cs` — `PUT /api/management/migration/apply` (aplica migraciones; policy `user.admin`).
- `Api/Controllers/MatchesController.cs` — `[Route("[controller]")]`, actualmente vacío (esqueleto para endpoints del dominio Match).
- `Api/HSoft.SocialSchool.Api.http` — requests de ejemplo para probar la API.

## Avisos para edición

- **Mismatch de versiones de paquetes**: Persistence usa EF Core `10.0.9` pero `Api.csproj` referencia `Microsoft.EntityFrameworkCore.Design 8.0.22`, `Serilog.AspNetCore 8.x` y `JwtBearer 8.x` mientras el target es `net10.0`. Al alinear versiones, hazlo de forma consistente en toda la solución.
- Hay carpetas placeholder vacías que indican dónde colocar código nuevo: `Application/Validators|Repositories|DomainEventHandlers`, `Api/Security`.
- `GetMatchQueryHandler` lanza `NotImplementedException` (scaffold del ejemplo); el namespace de `GetMatchQuery`/handler es `...Queries.Customers` aunque viven en `Queries/Matches/`.
