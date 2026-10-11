# Contexto del Proyecto

Este documento es el contexto técnico permanente del repositorio. Describe la arquitectura, las
convenciones y las restricciones que deben respetarse al trabajar en él. Todo lo que aparece aquí
fue verificado directamente contra el código; cuando algo no puede determinarse con certeza se
indica explícitamente.

## Tipo de Proyecto

API web ASP.NET Core (`net10.0`) estructurada en **Clean Architecture**. El producto es una **red
social para deportistas y escuelas de deporte**: el backend de este repositorio es el **núcleo de la
solución** y la app móvil y la web son clientes que lo consumen.

El repositorio nace de una plantilla arquitectónica, pero su **dominio de ejemplo (partidos y
árbitros) ya ha sido eliminado**: no queda ninguna referencia a él en `src/`. De la plantilla se
conservan las **decisiones arquitectónicas** y las **primitivas de dominio**, no su dominio.

Consecuencias prácticas:

- Los **requisitos** están en `openspec/specs/` (ver *Notas Importantes*): es la fuente de verdad.
- El **modelo de dominio ya está implementado** en `Domain/Entities/` y `Domain/Enumerations/`, y
  **ya se persiste** (configuraciones EF Core, repositorios y migración inicial). Todavía **no** hay
  casos de uso.
- No deben deducirse requisitos del código ni inventarse reglas: el código implementa lo que dicen
  los specs, no al contrario.
- Al añadir funcionalidad nueva hay que distinguir entre **decisiones arquitectónicas reales**
  (obligatorias) y **detalles de implementación** (reemplazables).

Estado actual del repositorio, relevante para planificar trabajo:

- La solución es `HSoft.SocialSchool.slnx`, en la raíz (un nivel por encima de `src/`).
- **No existe** ningún proyecto de tests.
- Persistence ya tiene **18 configuraciones** EF Core, **13 repositorios** y la carpeta
  **`Migrations/`** con la migración inicial `InitialCreate`, **sin aplicar** a ninguna base de datos.
- **No hay repositorio git** inicializado en esta carpeta (no hay historial que consultar).
- No existe `.template.config/` en el repositorio.
- El dominio tiene **18 entidades y 10 enumeraciones** implementadas. `Domain/ValueObjects/` y
  `Domain/DomainEvents/` **no existen**, porque el dominio actual no tiene value objects ni eventos
  de dominio propios.
- `Application/Commands`, `Application/Queries`, `Application/Models`, `Application/Repositories`,
  `Application/Validators/Commands`, `Application/Validators/Models` y
  `Persistence/Configurations` están **vacíos**: eran las carpetas del ejemplo eliminado.

---

## Arquitectura

Clean Architecture con cuatro proyectos y un flujo de dependencias estricto y unidireccional:

```
Api ──► Application ──► Domain
  └───► Persistence ──► Application, Domain
```

Trazas arquitectónicas concretas del sistema:

- **CQRS ligero sobre Mediator** (no MediatR): los casos de uso son `IRequest`/`IRequestHandler`
  despachados por `IMediator`.
- **Result pattern funcional** (`CSharpFunctionalExtensions`): los handlers devuelven
  `Result` / `Result<T>` en lugar de lanzar excepciones para fallos esperados.
- **DDD táctico**: entidades con factory estático, value objects, agregados, eventos de dominio
  publicados en dos fases (pre y post commit).
- **Validación en dos capas**: filtro MVC para la forma de los DTOs y behavior de Mediator para
  reglas de negocio.
- **Contenedor DI híbrido**: Autofac como `ServiceProviderFactory`, con registros repartidos entre
  el `IServiceCollection` nativo y módulos Autofac.

---

## Estructura de la Solución

Proyectos bajo `src/`, definidos en `HSoft.SocialSchool.slnx`:

| Proyecto | Tipo | Responsabilidad |
|---|---|---|
| `HSoft.SocialSchool.Domain` | Class library | Modelo de dominio y primitivas. Núcleo sin dependencias externas de infraestructura. |
| `HSoft.SocialSchool.Application` | Class library | Casos de uso (Commands/Queries), behaviors, validadores, DTOs, opciones y contratos de repositorio/servicios. |
| `HSoft.SocialSchool.Persistence` | Class library | Implementación de persistencia con EF Core, repositorios, configuraciones, módulo Autofac y servicio de migraciones. Este proyecto **hace de capa de infraestructura**; no existe un proyecto `Infrastructure` separado. |
| `HSoft.SocialSchool.Api` | Web SDK | Composición de la aplicación: arranque, DI, pipeline HTTP, controladores, filtros y autenticación. Único punto de entrada ejecutable. |

Carpetas y estructura interna, por proyecto:

- **Domain**: `Entities/<Feature>/` (hoy `Users`, `Athletes`, `Schools`, `Sports`, `Ratings`,
  `Attendances`, `Friendships`, `Blocks`, `Posts`, `Reports`, `Notifications`), `Enumerations/`,
  `Primitives/` y `Constants.cs`.
- **Application**: `Commands/<Feature>/`, `Queries/<Feature>/`, `Behaviors/`,
  `Validators/Commands|Models/`, `Models/<Feature>/`, `Repositories/`, `Services/`, `Options/`,
  `IoC/Services/`. Hoy las carpetas de casos de uso, DTOs, repositorios y validadores están vacías.
- **Persistence**: `Configurations/`, `Repositories/`, `Services/`, `Extensions/`, `IoC/Services/`,
  `IoC/Autofac/`, más `MyCompanyDbContext.cs` en la raíz del proyecto.
- **Api**: `Controllers/`, `Filters/`, `Core/`, `Extensions/`, `Infrastructure/ExceptionHandlers/`,
  `Infrastructure/Security/` (vacía), `Properties/` y `wwwroot/` (vacía).

---

## Responsabilidades de cada Capa

### Domain

Contiene el modelo y sus reglas. No conoce EF Core, ASP.NET ni serialización. Su única dependencia
externa es `Mediator.Abstractions`, necesaria porque los eventos de dominio implementan
`INotification`.

Responsabilidades: entidades y agregados, value objects, enumeraciones, eventos de dominio e
interfaces de primitivas (`IEntity`, `IAggregateRoot`, `Entity`, `ValueObject`, `IBaseRepository<T>`,
`IPostCommitNotification`, `IUniqueDomainEvent`).

### Application

Orquesta casos de uso. No conoce Persistence ni ASP.NET; define **contratos** (hoy únicamente
`IMigrationService`) que Persistence implementa.

Responsabilidades: Commands y Queries con sus handlers, validadores, pipeline behaviors, DTOs de
entrada/salida, opciones de configuración y el registro de Mediator y validadores.

### Persistence

Implementa la infraestructura de datos: `DbContext`, `IEntityTypeConfiguration<T>`, repositorios
concretos, servicio de migraciones y el registro por convención en Autofac.

### Api

Compone y expone. Traduce HTTP a mensajes de Mediator y mapea el resultado a respuestas HTTP. Es el
único proyecto con `Program.cs` y el único que conoce el pipeline ASP.NET Core (autenticación,
Swagger, filtros, manejo de excepciones).

---

## Reglas de Dependencia

Direcciones permitidas (derivadas de los `ProjectReference` reales):

- `Api` → `Application`, `Persistence`
- `Application` → `Domain`
- `Persistence` → `Application`, `Domain`
- `Domain` → (nada del proyecto)

Reglas que **no** deben romperse:

- `Domain` no referencia ninguna otra capa del proyecto ni infraestructura.
- `Application` **no** puede referenciar `Persistence` ni `Api`. Si un caso de uso necesita datos,
  define el contrato en `Application/Repositories` o `Application/Services` y lo implementa
  Persistence.
- `Api` no debe contener lógica de negocio: sólo composición, traducción HTTP y mapeo de resultados.
- Las entidades y value objects de `Domain` no deben contaminarse con atributos de EF Core,
  serialización o ASP.NET. El mapeo a base de datos vive en `Persistence/Configurations`.

Detalle frágil a tener en cuenta: `Api` y `Persistence` usan tipos de **Autofac**
(`AutofacServiceProviderFactory`, `ContainerBuilder`, `Module`) sin declarar el paquete
directamente; lo reciben de forma transitiva a través de
`Application` (que referencia `Autofac.Extensions.DependencyInjection`). Si se rompe esa cadena,
habrá que añadir la referencia explícita donde corresponda.

---

## Dominio

### Primitivas disponibles

- `Entity` (`Primitives/Entity.cs`): clase base abstracta.
  - `Id` es `virtual Guid` con setter privado (virtual por los proxies de EF Core).
  - Mantiene la colección de eventos de dominio (`List<INotification>`) y expone
    `AddDomainEvent`, `RemoveDomainEvent`, `ClearDomainEvents`.
  - `AddDomainEvent` deduplica los eventos que implementan `IUniqueDomainEvent`.
  - La igualdad se basa en el tipo real (desenvolviendo `Castle.Proxies.*`) más el `Id`; dos
    entidades con `Id == Guid.Empty` nunca son iguales.
- `ValueObject`: igualdad estructural mediante `GetEqualityComponents()`, con
  `==`/`!=`/`Equals`/`GetHashCode` ya implementados y también conscientes de proxies.
- `IEntity`, `IAggregateRoot` (marca la raíz de agregado), `IBaseRepository<T>`.
- `IPostCommitNotification`: evento que se publica **después** del commit.
- `IUniqueDomainEvent`: evento idempotente dentro de la misma unidad de trabajo.

### Convenciones de modelado (obligatorias)

- **Entidad**: `sealed` cuando no se hereda de ella, con **constructor privado** (EF lo necesita) y
  un **factory estático `Create(...)`** que construye la instancia válida. Setters `private`.
- **Agregado**: hereda de `Entity` e implementa `IAggregateRoot`.
- **Colecciones internas**: `private readonly List<T> _items = [];` expuestas como
  `IReadOnlyCollection<T>` mediante `AsReadOnly()`.
- **Invariantes**: se protegen en los **métodos de comportamiento** de la entidad (no en los
  setters) lanzando `ArgumentException` / `InvalidOperationException`. Ejemplos canónicos del
  dominio actual: `Athlete.DeclareSkill` (sólo maniobras de deportes que practica, sin duplicados),
  `SchoolAttendance.Accept`/`Reject`/`Withdraw` (transiciones de estado) y `Rating.UpdateValue`
  (rango 1-5).
- **Parentesco**: las entidades hijas se crean con un factory `internal static Create(<padre>, ...)`
  al que sólo se accede desde un método del agregado (ver `Athlete.DeclareSkill` +
  `AthleteSkill.Create`, o `School.AddService` + `SchoolService.Create`). Las operaciones de las
  hijas se exponen como `internal` para que sólo las use su agregado.
- **Eventos de dominio**: `sealed record <Algo>DomainEvent(...) : INotification`. Se añaden desde los
  métodos de comportamiento o desde el handler, con `AddDomainEvent(...)`. Marcarlos con
  `IUniqueDomainEvent` o `IPostCommitNotification` según corresponda. **El dominio actual no define
  ningún evento todavía.**
- **Value objects**: heredan de `ValueObject`, exponen `Create(...)` estático e implementan
  `GetEqualityComponents()`. Para mutaciones inmutables se usan métodos `With...` que devuelven una
  nueva instancia. **El dominio actual no tiene value objects**: los perfiles usan propiedades
  primitivas.
- **Enumeraciones**: `enum` con valores explícitos y documentación XML por miembro.

---

## Application

### Casos de uso

- Estructura por feature: `Commands/<Feature>/` y `Queries/<Feature>/`.
- El **mensaje** es un `record` (o clase) que implementa `IRequest<TResponse>`; se declara `sealed`
  cuando es un record de datos.
- El **handler** implementa `IRequestHandler<TRequest, TResponse>` y se declara **`internal sealed`**
  (obligatorio: Mediator genera los tipos como `internal`).
- La firma de `Handle` devuelve **`ValueTask<TResponse>`**, nunca `Task<T>`.
- Los handlers **no** lanzan excepciones para fallos esperados: devuelven `Result` / `Result<T>` de
  `CSharpFunctionalExtensions`. Sólo se lanza para errores irrecuperables.
- Un handler no guarda explícitamente: los métodos del repositorio ya persisten (ver Persistencia).
- Los eventos de dominio se añaden a la entidad antes de guardar, o directamente en el handler, y
  los despacha el `DbContext`.

### Behaviors de pipeline

Registrados en orden en `ApplicationServiceExtensions.RegisterMediator`:

1. `LoggingBehavior<,>` — traza entrada/salida y tiempo de cada mensaje.
2. `ValidationBehavior<,>` — ejecuta todos los `IValidator<TRequest>` registrados; si hay errores
   lanza `ValidationException`.

Ambos usan `Serilog.ILogger` inyectado.

### Configuración de Mediator

En `Application/IoC/Services/ApplicationServiceExtensions.cs`:

- `options.Namespace = "HSoft.SocialSchool.Application"` — coincide con el namespace raíz del
  proyecto.
- `ServiceLifetime.Scoped`, `GenerateTypesAsInternal = true`, `ForeachAwaitPublisher`.
- `options.Assemblies` incluye explícitamente la assembly de Application **y la de Domain** (anclada
  en `typeof(Entity)`), porque el generador debe encontrar los `INotificationHandler<T>` de eventos
  de dominio. **Si se añaden handlers en una assembly nueva, hay que agregarla aquí.**
- `AddValidators()` usa `AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly)`
  para descubrir validadores por escaneo de assembly. **Hoy no existe ningún validador**, así que la
  llamada no registra nada todavía.

### Modelos (DTOs)

Viven en `Models/<Feature>/` y **hoy no existe ninguno**. Los de entrada son `record` con
propiedades `init`, valores por defecto seguros (`string.Empty`, `null!` para los requeridos) y
documentación XML. Los de salida son records planos. No se usan entidades de dominio como contrato
de la API.

### Contratos

- `Repositories/`: interfaces de repositorio propias de la capa de aplicación, que extienden
  `IBaseRepository<T>` del dominio. Hay una por agregado (**13**), todas sin métodos adicionales.
- `Services/`: interfaces de servicios de aplicación (ver `IMigrationService`).
- `Options/`: clases de opciones ligadas a configuración (ver `ApplicationOptions`).
- `DomainEventHandlers/`: **no existe todavía**. Si se añaden handlers de eventos de dominio, este es
  su sitio.

---

## Infraestructura

La capa de infraestructura es el proyecto **Persistence**.

### `MyCompanyDbContext`

Ubicado en la raíz de Persistence. Recibe `DbContextOptions<MyCompanyDbContext>` e `IMediator` por
constructor.

- `OnModelCreating` aplica automáticamente todas las `IEntityTypeConfiguration<T>` del ensamblado
  con `ApplyConfigurationsFromAssembly`. **No hay que registrar cada configuración a mano**: basta
  con crear la clase en `Configurations/`.
- Sobrescribe `SaveChangesAsync` para orquestar los eventos de dominio en dos fases (ver Persistencia).

### Repositorios

- `BaseRepository<T>` (`Repositories/BaseRepository.cs`) implementa `IBaseRepository<T>` con CRUD
  genérico. **Cada método llama internamente a `SaveChangesAsync`**: no hay una unidad de trabajo
  explícita en la capa de aplicación.
  - `GetById` usa `FindAsync` (rastreado); `GetByIdReadOnly` y `ExistsAsync` usan `AsNoTracking()`.
  - `Update` resuelve el conflicto de instancias desconectadas ya rastreadas copiando valores
    (`CurrentValues.SetValues`) en lugar de lanzar.
  - `Save` decide entre `Add` y `Modified` según `Id == Guid.Empty`.
- Los repositorios concretos son mínimos: heredan de `BaseRepository<T>` e implementan la interfaz
  de Application. Hay uno por agregado (**13**) y ninguno declara métodos propios todavía.

### Módulo Autofac

`PersistenceModule` (`IoC/Autofac/PersistenceModule.cs`):

- Registra por **convención** todos los tipos del ensamblado cuyo nombre termina en `Repository` y
  no empieza por `Caching`, como `InstancePerLifetimeScope` y `AsImplementedInterfaces()`. Es decir:
  crear `XyzRepository : BaseRepository<Xyz>, IXyzRepository` es suficiente para que se inyecte
  `IXyzRepository`.
- Registra `MigrationService` como `IMigrationService`.

### Servicio de migraciones

`MigrationService` aplica las migraciones pendientes con `Database.GetPendingMigrationsAsync()` +
`Database.MigrateAsync()`, con log vía Serilog y re-lanzado de la excepción.

---

## API

### Composición y arranque

`Api/Program.cs` usa top-level statements y es el único punto de composición:

```
WebApplication.CreateBuilder(args)
  .ConfigureWebApplicationBuilder(ConfigureAutofacContainer, ConfigureServices)
→ builder.Build()
→ app.Configure()          // pipeline HTTP (WebApplicationExtensions)
→ app.LogRuntimeInformation()
→ app.RunAsync()
```

- `ConfigureAutofacContainer(ContainerBuilder)`: registra los módulos Autofac. **Actualmente sólo
  `PersistenceModule`**; los demás registros (`MainModule`, `ApplicationModule`,
  `AzureCommunicationModule`, `AzureDataLakeModule`, `CachingStackExchangeModule`, `SmtpClientFake`)
  están comentados y hacen referencia a tipos que no existen en este repositorio.
- `ConfigureServices(IServiceCollection, IConfiguration)`: `AddMemoryCache` + `AddDistributedMemoryCache`,
  `RegisterMediator()`, `AddValidators()`, `AddDbContextPool(configuration)`. Otras llamadas están
  comentadas.

### Controladores

- Heredan de `ControllerBase`, decorados con `[ApiController]`.
- Inyectan `IMediator` (se usa **constructor primario** en los controladores existentes).
- Envían el mensaje con `_mediator.Send(...)` pasando siempre el `CancellationToken`.
- Mapean el resultado con el patrón: `if (result.IsFailure) return Problem(detail: result.Error);`
  y en caso de éxito devuelven el resultado tipado (`Created(...)`, `Ok()`, ...).
- Rutas: `ManagementController` es hoy el único controlador y usa `[Route("api/[controller]")]`; las
  URLs en minúscula están configuradas globalmente (`LowercaseUrls = true`).

### Pipeline HTTP

`WebApplicationExtensions.Configure` monta, en orden: `UseStaticFiles`, `UseSwagger`,
`UseSwaggerUI` (con `custom.css`), un middleware que activa el buffering del body,
`UseHttpsRedirection`, `UseRouting`, `UseAuthentication`, `UseAuthorization`, `MapControllers`.

Punto a tener en cuenta: **no se invoca `UseExceptionHandler()`** en el pipeline. El
`GlobalExceptionHandler` está registrado en DI (`AddExceptionHandler<GlobalExceptionHandler>()`),
pero sin ese middleware el handler no se conecta al pipeline, por lo que las `ValidationException`
que lanza el `ValidationBehavior` no tienen garantizada su conversión a `400 ProblemDetails`.

### Autenticación y autorización

Configurada en `ServicesCollectionExtensions.AddAuthentication`:

- Esquema por defecto: `JwtBearerDefaults.AuthenticationScheme` (`"Bearer"`).
- **Un único esquema JWT Bearer contra Keycloak**: `Authority = configuration["Keycloak:Authority"]`,
  `Audience = configuration["Keycloak:Audience"]` (lo que activa la validación de emisor y de
  audiencia), `RequireHttpsMetadata = true` y `MapInboundClaims = false` para leer los claims
  originales (el `sub` se llama literalmente `sub`).
- **No se restringe `ValidTypes`**: el `at+jwt` de la plantilla rechazaba los tokens de Keycloak, que
  no usan RFC 9068. La distinción entre access token e ID token la garantiza la **audiencia** (el ID
  token va destinado al cliente web, no a `SocialSchool-Api`). Si el realm emitiese `at+jwt`, habría
  que volver a fijarlo.
- **Denegación por defecto**: `options.FallbackPolicy = RequireAuthenticatedUser()`. Todo endpoint
  exige token salvo los marcados `[AllowAnonymous]`. Consecuencia: `ManagementController` (incluido
  `PUT /api/management/migration/apply`) requiere token. **Swagger no se ve afectado**, porque
  `UseSwagger`/`UseSwaggerUI` se sirven antes del middleware de autenticación.
- **`ICurrentUser`**: contrato en `Application/Services/ICurrentUser.cs` (`ExternalId`, `IsAuthenticated`)
  e implementación en `Api/Infrastructure/Security/CurrentUser.cs`, registrada como `Scoped` en
  `AddAuthorizationHandlers`. Permite a los casos de uso obtener el `sub` sin conocer `HttpContext`.
- **No hay introspección**: se eliminaron el esquema `OAuth2Introspection` de Duende, el reenvío de
  *reference tokens* (`AuthenticationForwardSelector`, `HttpContextAuthExtensions`) y la dependencia
  `Duende.AspNetCore.Authentication.OAuth2Introspection`. La API solo acepta JWT.
- Las claves `securityServer:*` / `SecurityServer:*` ya no se usan.

### Swagger

`AddSwaggerDocumentation` registra Swagger con versión derivada de `Domain.Constants.Application.Version`
(`v1`), security definition `Bearer`, `CustomSchemaIds(x => x.FullName)`,
`DescribeAllParametersInCamelCase()` e inclusión de comentarios XML (el proyecto Api tiene
`GenerateDocumentationFile=true`, así que **los comentarios XML son la fuente de la documentación de
la API**).

### JSON y rutas

`AddConfiguredControllers` configura `JsonStringEnumConverter` y
`PropertyNameCaseInsensitive = true`, y fuerza `LowercaseUrls = true` en las rutas.

---

## Inyección de Dependencias

Sistema **híbrido**: el `IServiceCollection` nativo se pasa a Autofac, que actúa como
`ServiceProviderFactory` (`HostBuilderExtensions.ConfigureHost` con `AutofacServiceProviderFactory`).

- **Registro nativo** (dentro de `ConfigureServices` en `Program.cs`, más las extensiones que se
  llaman desde `configureServices`): Mediator, validadores y `DbContextPool`. También todo lo que
  registran `WebApplicationBuilderExtensions` (controllers, opciones, autenticación, exception
  handler, Swagger).
- **Registro Autofac** (`ConfigureAutofacContainer` en `Program.cs`): módulos de Autofac. Hoy
  únicamente `PersistenceModule`.

Reglas a seguir al registrar servicios nuevos:

- Preferir el registro en la extensión de la capa que posee el servicio (patrón existente:
  `ApplicationServiceExtensions.RegisterMediator` / `AddValidators`,
  `PersistenceServiceExtensions.AddDbContextPool`).
- Los repositorios concretos **no se registran a mano**: los descubre `PersistenceModule` por
  convención de nombre.
- Un tipo nuevo que no acabe en `Repository` y deba resolverse por interfaz necesita registro
  explícito (en un `IServiceCollection` o en un módulo Autofac, según corresponda).
- Los behaviors añadidos al pipeline se declaran en `options.PipelineBehaviors` dentro de
  `RegisterMediator`, respetando el orden deseado.

### Fuentes de configuración

`ConfigureHost` declara las fuentes sobre `builder.Configuration` (las APIs de `IHostBuilder` no están
soportadas en `WebApplicationBuilder`). El content root es el directorio de trabajo, que
`WebApplicationBuilder` fija por defecto; ya no se llama a `UseContentRoot`. Orden (la última fuente
gana):

1. `appsettings.json` (**obligatorio**).
2. `appsettings.{Environment}.json` (opcional).
3. `Config/security.json` — **comentado actualmente**; no se carga.
4. `secrets/akv_secrets.json` (opcional, si existe el archivo).
5. User Secrets (`AddUserSecrets<Program>`).
6. Variables de entorno.

Claves consumidas por el código: `ConnectionStrings:MyCompany` (SQL Server), `Keycloak:Authority` y
`Keycloak:Audience` (identidad), `AuditingEnabled`, y la sección `Serilog`.

---

## Validación

Se realiza en **dos capas complementarias**, y la separación es intencionada:

1. **Forma del DTO** — `FluentValidationFilter` (filtro MVC global, registrado en
   `AddConfiguredControllers`). Resuelve el `IValidator<>` del argumento de la acción y, si falla,
   devuelve `ValidationProblemDetails` (RFC 7807) con `400`. Validadores en `Validators/Models/`.
2. **Reglas de negocio** — `ValidationBehavior` (pipeline de Mediator). Ejecuta todos los
   `IValidator<TRequest>` del mensaje y lanza `ValidationException`. Validadores en
   `Validators/Commands/` (y `Validators/Queries/` si hiciera falta). Pueden depender de un contrato
   de `Repositories/` cuando la regla exige consultar datos.

**Hoy no existe ningún validador**: los del ejemplo se eliminaron con él.

Al crear un caso de uso nuevo: valida la forma en el DTO (`Validators/Models/`) y las reglas de
negocio en el mensaje (`Validators/Commands/` o `Validators/Queries/`). Los validadores se descubren
por assembly, así que basta con colocarlos en la carpeta correcta.

---

## Tratamiento de Errores

- **Fallos esperados** → `Result` / `Result<T>`. El handler devuelve `Result.Failure(...)` y el
  controlador lo traduce con `Problem(detail: response.Error)`.
- **Errores de validación** → `ValidationException` lanzada por `ValidationBehavior`
  (o `ValidationProblemDetails` devuelto por `FluentValidationFilter`).
- **Excepciones no controladas** → `GlobalExceptionHandler` (`IExceptionHandler`), que construye un
  `ProblemDetails` con `traceId`; mapea `ValidationException` a `400` con extensión `errors`
  agrupada por propiedad, y cualquier otra excepción a `500` con título genérico. En entorno
  `Development` añade una extensión `debugInfo` con mensaje, tipo, stack trace e inner exception.
- El registro de `AddProblemDetails()` también existe, junto con un bloque comentado que mapeaba
  `ValidationException` a problem details. **Recordatorio:** el pipeline no llama a
  `UseExceptionHandler()`, por lo que hoy el `GlobalExceptionHandler` no está efectivamente
  conectado.

No se usan excepciones de dominio personalizadas: el dominio lanza `ArgumentException`,
`InvalidOperationException` u otras excepciones de BCL.

---

## Persistencia

- **Proveedor**: SQL Server vía `Microsoft.EntityFrameworkCore.SqlServer` (10.0.9).
- **Registro**: `AddDbContextPool<MyCompanyDbContext>` con la cadena `ConnectionStrings:MyCompany`,
  `MigrationsAssembly` apuntando al propio ensamblado de Persistence y `EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: 5s)`.
- **Configuraciones**: clases `IEntityTypeConfiguration<T>` en `Configurations/`, aplicadas
  automáticamente, así que **no hay que registrarlas a mano**. Se definen `ToTable`, clave primaria,
  longitudes máximas, `IsRequired`, índices con nombre explícito (`IX_...`) y relaciones. Las 18
  entidades del dominio ya están mapeadas (`internal sealed`, una por entidad) y las que tienen
  colección hija declaran la relación desde el lado principal, para que EF use el backing field.
- **Value objects**: se mapean como **owned types** (`OwnsOne`), aplanando sus propiedades en
  columnas del propietario. El dominio actual **no tiene value objects**, así que hoy no hay ningún
  caso al que aplicarlo; la convención se mantiene para cuando se añadan.
- **Proxies de lazy loading**: el paquete `Microsoft.EntityFrameworkCore.Proxies` está referenciado
  y `Entity`/`ValueObject` están preparados para proxies (`Castle.Proxies.*`), pero
  `UseLazyLoadingProxies()` **no se invoca** en `PersistenceServiceExtensions.AddDbContextPool`. Hoy
  la carga no es lazy; hay que usar `Include` o consultas explícitas.
- `LinqKit` está referenciado en el `.csproj` pero **no se usa** en ningún archivo del repositorio.

### Eventos de dominio en dos fases

`MyCompanyDbContext.SaveChangesAsync` + `Extensions/MediatorExtensions.DispatchDomainEventsAsync`:

1. **Pre-commit (transaccional)**: dentro de `Database.CreateExecutionStrategy().ExecuteAsync(...)`, se
   recogen los eventos de las entidades rastreadas y se publican con `mediator.Publish`. El bucle
   reevalúa el `ChangeTracker` en cada iteración y limpia los eventos con `ClearDomainEvents()`, de
   modo que los eventos añadidos por otros handlers también se procesan y el bucle termina.
2. **Post-commit**: los eventos que implementan `IPostCommitNotification` **no** se publican en la
   transacción; se acumulan y se publican después del commit, fuera de la execution strategy, para
   que sus handlers puedan leer datos ya persistidos.

Los eventos marcados con `IUniqueDomainEvent` se deduplican por valor (de ahí que los eventos sean
`record`).

### Migraciones

Hay una migración generada, `InitialCreate` (con su snapshot), en `Persistence/Migrations/`. **No se
ha aplicado** a ninguna base de datos: `ConnectionStrings:MyCompany` sigue siendo la cadena de
ejemplo inválida. El `DbContext` vive en Persistence pero el startup project es Api, así que al
generar migraciones hay que indicar ambos proyectos (ver sección de comandos más abajo).

---

## Testing

**No existe ningún proyecto de tests en la solución.** Por tanto no hay framework, ni convenciones,
ni patrones de test que respetar todavía: cualquier afirmación sobre cómo se testea este proyecto
sería inventada.

Si se añade un proyecto de tests, lo coherente con la arquitectura actual es:

- Crearlo como proyecto adicional (por ejemplo `tests/HSoft.SocialSchool.UnitTests`), fuera de `src/`.
- Añadirlo al `HSoft.SocialSchool.slnx` y ejecutar con `dotnet test`.
- Mantener el flujo de dependencias: los tests pueden referenciar cualquier capa, pero **no** deben
  introducir dependencias nuevas en `Domain` o `Application`.
- Al testear handlers de Mediator hay que tener en cuenta que los tipos generados son `internal`
  (posible necesidad de `InternalsVisibleTo` desde Application hacia el proyecto de tests).

Hasta que exista, la verificación de cambios se limita a la compilación.

---

## Convenciones de Código

- **Idioma de los identificadores**: los nombres de clases, propiedades, enumeraciones y miembros de
  enumeración se escriben siempre en **inglés**. La documentación del modelo de negocio se mantiene
  en **español** (ver `openspec/specs/domain.md`).
- **Namespaces**: con bloques (`namespace X { ... }`), no file-scoped. El namespace raíz es
  `HSoft.SocialSchool.<Capa>` y continúa la ruta de carpetas (p. ej.
  `HSoft.SocialSchool.Domain.Entities.Athletes`).
- **Usings**: explícitos al principio del archivo (no se usan `global using` propios). El
  `<ImplicitUsings>enable</ImplicitUsings>` está activo en los cuatro proyectos, y `Nullable` también.
- **Formato**: llaves en línea nueva (Allman); indentación de 4 espacios; `_camelCase` para campos
  privados; propiedades y métodos `PascalCase`.
- **Clases**: `sealed` por defecto cuando no se hereda; los handlers de Mediator son
  `internal sealed`; los tipos de configuración de EF pueden ser `internal`.
- **Constructores**: se usa **constructor primario** en controladores, repositorios concretos,
  filtros y extensiones breves (ver `ManagementController`, `BaseRepository`,
  `FluentValidationFilter`). En clases con más lógica (handlers con lógica, servicios) se usa
  constructor explícito con campos.
- **Records**: para mensajes de Mediator, DTOs y eventos de dominio.
- **Documentación XML**: presente en miembros públicos de Domain y Application y obligatoria en Api
  (`GenerateDocumentationFile=true`); alimenta Swagger. En Domain está escrita **en español**,
  coherente con la convención de idioma de arriba.
- **Organización por feature** dentro de cada capa: `<Tipo>/<Feature>/`.
- **Async**: `ValueTask` en handlers por exigencia de Mediator; `Task` en repositorios y servicios.
  Se propaga siempre el `CancellationToken` en los métodos de repositorio.
- **Colecciones**: se usa sintaxis de expresión de colección (`[]`, `[typeof(X).Assembly, ...]`).
- **Formato de mensajes de log**: plantillas estructuradas con `{Placeholder}`, en inglés
  (ver `PerformanceAuditFilter`, `LoggingBehavior`).
- **Comentarios**: en el código existente aparecen comentarios en español e inglés; los comentarios
  que explican *por qué* son los más valiosos (ver `BaseRepository.Update`, `MediatorExtensions`).

### Logging

- **Serilog** configurado en `WebApplicationBuilderExtensions` con `ReadFrom.Configuration(context.Configuration)`,
  enrichers `FromLogContext`, `WithEnvironmentName`, `WithThreadId` y `writeToProviders: false`.
  La configuración (sink Console, niveles) está en `appsettings.json` y por defecto el nivel mínimo
  global es `Warning`.
- Se inyecta `Serilog.ILogger` (no `ILogger<T>`) en Application y Persistence. En Api, los filtros
  usan `ILoggerFactory.CreateLogger("Audit")`.
- `PerformanceAuditFilter` (filtro global) registra cada request con su código de estado y tiempo en
  `OnCompleted`, y si `AuditingEnabled=true` emite además una línea `AUDIT` con `sub` y `client_id`
  extraídos manualmente del JWT.

---

## Guías de Implementación

Procedimiento para añadir una funcionalidad nueva respetando la arquitectura (ejemplo: un agregado
`Xyz`):

1. **Domain**
   - `Entities/<Feature>/Xyz.cs`: `sealed class Xyz : Entity, IAggregateRoot`, constructor privado,
     factory estático `Create(...)`, setters privados, invariantes en métodos de comportamiento.
   - Entidades hijas con factory `internal static Create(<padre>, ...)` y método en el agregado para
     añadirlas.
   - Value objects nuevos en `ValueObjects/` heredando de `ValueObject`.
   - Eventos en `DomainEvents/` como `sealed record ... : INotification` (marcar
     `IUniqueDomainEvent` / `IPostCommitNotification` si aplica).
   - Enumeraciones nuevas en `Enumerations/` con valores explícitos.

2. **Persistence**
   - `Configurations/XyzConfiguration.cs` con `IEntityTypeConfiguration<Xyz>` → se aplica
     automáticamente, no hay que registrarla.
   - `Repositories/XyzRepository.cs`: `class XyzRepository(MyCompanyDbContext db) : BaseRepository<Xyz>(db), IXyzRepository { }`
     → el nombre terminado en `Repository` hace que `PersistenceModule` lo registre solo.
   - Si hay cambios de esquema, generar migración (ver comandos).

3. **Application**
   - `Repositories/IXyzRepository.cs` extendiendo `IBaseRepository<Xyz>` (sólo añadir métodos extra).
   - `Commands/<Feature>/CreateXyzCommand.cs`: `sealed record CreateXyzCommand(XyzRequest Request) : IRequest<Result<Guid>>`.
   - `Commands/<Feature>/CreateXyzCommandHandler.cs`: `internal sealed class ... : IRequestHandler<CreateXyzCommand, Result<Guid>>`
     con `ValueTask`, devolviendo `Result.Success(...)` / `Result.Failure(...)`.
   - `Models/<Feature>/XyzRequest.cs` (record de entrada) y `XyzResponse.cs` (record de salida).
   - `Validators/Models/XyzRequestValidator.cs` (forma) y `Validators/Commands/CreateXyzCommandValidator.cs`
     (reglas de negocio).
   - Queries análogas en `Queries/<Feature>/`.
   - Si el caso de uso necesita una integración externa, definir la interfaz en `Services/` e
     implementarla en Persistence (o en otro proyecto de infraestructura, que habría que crear).

4. **Api**
   - `Controllers/XyzController.cs` con `[ApiController]` y `[Route("[controller]")]`, constructor
     primario con `IMediator`, `Send(..., cancellationToken)` y mapeo `IsFailure → Problem(...)`.
   - Documentar con XML los endpoints públicos (Swagger) y añadir `[ProducesResponseType]`.
   - No añadir lógica de negocio aquí.

5. **DI**: normalmente **no hay que tocar nada**. Los validadores se descubren por assembly y los
   repositorios por convención de nombre. Sólo hay que registrar explícitamente si se crea una
   assembly nueva de handlers (añadirla a `options.Assemblies`) o un servicio que no siga la
   convención `*Repository`.

6. **Verificar**: compilar la solución (`dotnet build ../HSoft.SocialSchool.slnx` desde `src/`) y, si
   existe, ejecutar los tests.

### Comandos útiles

Se ejecutan desde `src/` (o con rutas explícitas). No hay scripts ni Makefile.

```bash
dotnet build ../HSoft.SocialSchool.slnx
dotnet run --project HSoft.SocialSchool.Api                    # http://localhost:5094
dotnet run --project HSoft.SocialSchool.Api --launch-profile https   # https://localhost:7146
```

Migraciones EF Core (DbContext en Persistence, startup project Api):

```bash
dotnet ef migrations add <Nombre> --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api
dotnet ef database update       --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api
```

En runtime, las migraciones pendientes se aplican con `PUT /api/management/migration/apply`, que
**ahora requiere autenticación** por la `FallbackPolicy` (hay que presentar un token de Keycloak).

---

## Restricciones Arquitectónicas

Reglas que **no** deben romperse:

1. `Domain` no depende de ninguna otra capa ni de infraestructura. Su única dependencia externa
   permitida es `Mediator.Abstractions`.
2. `Application` no referencia `Persistence` ni `Api`. Los accesos a datos se declaran como
   interfaces en `Application` y se implementan en `Persistence`.
3. `Api` no contiene lógica de negocio; sólo composición, traducción HTTP y mapeo de resultados.
4. Las entidades y value objects de dominio no llevan atributos de EF Core ni de ASP.NET; el mapeo
   vive en `Persistence/Configurations`.
5. Los handlers de Mediator son `internal` (el generador produce tipos `internal`), devuelven
   `ValueTask<T>` y no lanzan excepciones para fallos esperados: devuelven `Result`/`Result<T>`.
6. Los fallos de negocio se comunican con `Result`, no con excepciones; los controladores usan
   `Problem(detail: ...)` cuando `IsFailure`.
7. Toda la persistencia pasa por el `DbContext` y sus repositorios: nada de SQL crudo disperso ni
   de `DbContext` inyectado en `Application` o `Api`.
8. No se instancian ni se registran repositorios a mano: la convención de `PersistenceModule`
   (`*Repository`, excepto `Caching*`, registrado por interfaz) es la fuente de verdad.
9. Al añadir handlers en una assembly nueva hay que incluirla en `options.Assemblies` de
   `RegisterMediator`, o no se descubrirán.
10. No introducir dependencias nuevas en `Domain` o `Application` sin justificarlo: cada paquete ahí
    afecta a todas las capas.
11. No cambiar el orden de los pipeline behaviors sin motivo: hoy es Logging → Validation, y
    Validation es el que corta la ejecución ante errores.
12. No modificar los paquetes ni las versiones de forma aislada: hay desalineaciones conocidas (ver
    Notas Importantes) y cualquier alineación debe hacerse de forma coherente en toda la solución.

---

## Dominio Implementado

El dominio de ejemplo de la plantilla (partidos y árbitros) **fue eliminado** junto con sus casos de
uso, DTOs, validadores, configuraciones EF, repositorio y controlador. No queda ninguna referencia a
él en `src/`.

El dominio actual (fuente de verdad: `openspec/specs/domain.md`) vive en `Domain/Entities/`:

| Carpeta | Entidades |
|---|---|
| `Users` | `User` (agregado), `NotificationPreference` (hija) |
| `Athletes` | `Athlete` (agregado), `AthleteSkill` (hija) |
| `Schools` | `School` (agregado), `SchoolService` (hija) |
| `Sports` | `Sport` y `Skill` (catálogo) |
| `Ratings` | `Rating` |
| `Attendances` | `SchoolAttendance` |
| `Friendships` | `FriendRequest`, `Friendship` |
| `Blocks` | `Block` |
| `Posts` | `Post` (agregado), `Comment` y `Like` (hijas) |
| `Reports` | `Report` |
| `Notifications` | `Notification` |

Enumeraciones en `Domain/Enumerations/`: `AccountType`, `AccountStatus`, `RequestStatus`,
`AttendanceStatus`, `ReportCategory`, `ReportStatus`, `ReportableContentType`, `PostMediaType`,
`NotificationType` y `NotificationChannel`.

Decisiones de modelado vigentes:

- **Identidad compartida**: `Athlete` y `School` son agregados distintos de `User` que comparten su
  `Id`. No hay herencia, por eso todas las entidades pueden ser `sealed`.
- **Agregados mixtos**: las entidades hijas (`NotificationPreference`, `AthleteSkill`,
  `SchoolService`, `Comment`, `Like`) se crean y se modifican a través de su agregado y exponen sus
  operaciones como `internal`.
- **Reglas que no viven en la entidad**: lo que exige consultar otro agregado (tipos de cuenta en una
  solicitud de amistad, ausencia de bloqueo, unicidad del nick, quién puede ver qué) corresponde al
  caso de uso; las entidades sólo protegen sus propios invariantes. Está anotado en las XML docs de
  `FriendRequest`.
- **Pendiente de construir**: casos de uso, migración aplicada a una base de datos y —si se
  quieren— eventos de dominio (hoy no hay ninguno definido).

### Restos de plantilla que siguen presentes

No son decisiones de negocio, pero tampoco hay que tocarlos sin motivo:

- `ServicesCollectionExtensions`: policy `"assessment.list"` con `RequireClaim("")`.
- `Program.cs`: módulos Autofac comentados que referencian tipos inexistentes (`MainModule`,
  `ApplicationModule`, `AzureCommunicationModule`, `AzureDataLakeModule`,
  `CachingStackExchangeModule`, `SmtpClientFake`).
- `Api/HSoft.SocialSchool.Api.http` apunta a un `weatherforecast` que no existe.

Y esto, que **no** es código de ejemplo, debe mantenerse: las primitivas de dominio (`Entity`,
`ValueObject`, `IAggregateRoot`, `IBaseRepository`, `IPostCommitNotification`, `IUniqueDomainEvent`),
los behaviors de pipeline, el `DbContext` con la publicación de eventos en dos fases,
`BaseRepository<T>`, `PersistenceModule`, `GlobalExceptionHandler`, `FluentValidationFilter`,
`PerformanceAuditFilter`, `MigrationService` y toda la composición de `Program.cs`/`Core`/`Extensions`.

---

## Notas Importantes

- **Configuración de ejemplo inválida**: `appsettings.json` tiene
  `ConnectionStrings:MyCompany = "MyCompany"`, que no es una cadena de conexión válida. Hay que
  sustituirla para poder ejecutar la API contra SQL Server.
- **`security.json` no se carga**: la línea que lo añadía está comentada. Las claves que espera el
  código (`Keycloak:Authority` y `Keycloak:Audience`) deben venir de User Secrets o de variables de
  entorno; `appsettings.json` solo lleva valores de ejemplo.
- **Desalineación de versiones de paquetes** (target `net10.0`):
  - `Microsoft.EntityFrameworkCore.Design` de `Api` está **alineado** con `Persistence` en `10.0.9`
    (y la herramienta `dotnet ef` está en `10.0.9`); ya no es una desalineación.
  - `Api` referencia `Serilog.AspNetCore 8.0.3` y `Microsoft.AspNetCore.Authentication.JwtBearer
    8.0.25`; `Application` usa `Serilog 4.3.1`.
  - `Swashbuckle.AspNetCore 6.5.0`.
  - `Microsoft.AspNetCore.OpenApi` está comentado en `Api.csproj`.
  Cualquier alineación debe hacerse de forma consistente en toda la solución.
- **`MyCompanyDbContext` se registra con `AddDbContextPool` y recibe `IMediator` (scoped) por
  constructor.** Es un detalle a tener presente si se cambian los tiempos de vida de los servicios,
  ya que el pool reutiliza instancias del contexto.
- **`wwwroot/` está vacía**, pero el pipeline llama a `UseStaticFiles()` y Swagger inyecta
  `/swagger/custom.css`: ese archivo no existe hoy.
- **`Api/Infrastructure/Security/` está vacía**: es la carpeta reservada para la seguridad.
  (`Application/DomainEventHandlers/` y `Application/Validators/Queries/` **no existen**; si se
  necesitan, hay que crearlas.)
- **`ApplicationOptions` está vacío** y se liga a configuración sin secciones propias; sirve de
  punto de extensión.
- **OpenSpec**: el repositorio tiene un flujo de trabajo *spec-driven* (`openspec/config.yaml` con
  `schema: spec-driven`, más `openspec/changes/`, `openspec/specs/` y el conjunto de skills/comandos
  `opsx-*` en `.qwen/`). `openspec/changes/` sigue vacío (no hay cambios registrados), pero
  `openspec/specs/` contiene **dos documentos de nivel de proyecto**, escritos a partir de una
  entrevista de requisitos (septiembre de 2026) y que son la **fuente de verdad del producto**:
  - `openspec/specs/project.md` — definición completa del producto: usuarios y tipos de cuenta,
    relaciones (amistad, asistencia, bloqueo), visibilidad, funcionalidades, reglas transversales,
    ciclos de vida, integraciones, alcance del MVP, fuera de alcance y una sección de **decisiones
    abiertas**.
  - `openspec/specs/domain.md` — modelo de negocio: entidades con responsabilidades, propiedades y
    reglas de negocio, enumeraciones, relaciones entre entidades e invariantes globales.
  No siguen el formato de capacidad (`specs/<capacidad>/spec.md`): son documentos sueltos de nivel
  de proyecto.
- **El dominio real del proyecto (`SocialSchool`) está definido en `openspec/specs/`
  (`project.md` + `domain.md`) y ya implementado en `Domain/Entities/`**: es una **red social para
  deportistas y escuelas de deporte**. Los requisitos se toman de esos documentos, no del código ni
  de suposiciones. Las **decisiones abiertas** de `project.md` no deben resolverse por iniciativa
  propia: hay que preguntar.
