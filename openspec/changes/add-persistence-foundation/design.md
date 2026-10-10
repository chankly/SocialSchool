# Design: Cimientos de persistencia

## Context

Ver `proposal.md` para la motivación. Estado actual que condiciona el enfoque:

- El dominio está implementado en `Domain/Entities/` (18 entidades, 10 enumeraciones). **Ninguna
  entidad lleva atributos de EF**; todo el mapeo debe vivir en `Persistence/Configurations/`.
- `MyCompanyDbContext.OnModelCreating` ya llama a `ApplyConfigurationsFromAssembly`, así que **no hay
  que registrar configuraciones** ni declarar `DbSet`: basta con crear la clase en `Configurations/`.
- `BaseRepository<T>` ya implementa el CRUD genérico y **cada método guarda** (`SaveChangesAsync`).
  `IBaseRepository<T>` exige `T : class, IAggregateRoot`.
- `PersistenceModule` registra por convención todo tipo cuyo nombre termina en `Repository` (salvo
  `Caching*`) como `InstancePerLifetimeScope` y `AsImplementedInterfaces()`.
- El proveedor es SQL Server y `MigrationsAssembly` apunta al propio ensamblado de Persistence.
- `UseLazyLoadingProxies()` **no** se invoca: la carga no es perezosa.
- `Api.csproj` referencia `Microsoft.EntityFrameworkCore.Design 8.0.22` (con `PrivateAssets=all`)
  mientras Persistence usa EF Core `10.0.9`.

## Goals / Non-Goals

**Goals:**

- Persistir las 18 entidades con un esquema explícito y legible (tablas, claves, longitudes,
  índices y relaciones), evitando que EF adivine.
- Fijar de una vez las convenciones de mapeo que copiarán los cambios siguientes.
- Dar a cada raíz de agregado su repositorio, listo para el primer caso de uso.
- Producir una migración inicial fiel al modelo, generable **sin** base de datos.

**Non-Goals:**

- Endpoints, casos de uso, validadores y DTOs.
- Aplicar la migración a una base de datos (`dotnet ef database update`): la cadena de conexión
  sigue siendo la inválida de la plantilla.
- Alinear el resto de paquetes desalineados (Serilog, JwtBearer, Swashbuckle): fuera de alcance.
- Eventos de dominio (no hay ninguno definido) y value objects (no existen).
- Filtro global de borrado suave.
- Decidir la unicidad de `Email` y la vinculación de identidades Google/Facebook
  (`project.md` §15.7).

## Decisions

**1. Enumeraciones como `int`.** Es el valor por defecto de EF y las enumeraciones ya tienen valores
explícitos, así que la numeración es estable. El JSON de la API ya expone los nombres mediante
`JsonStringEnumConverter`, de modo que la legibilidad para el cliente no depende del almacenamiento.
*Alternativa descartada*: guardarlas como texto (más legible en base de datos, pero exige conversión
explícita en cada propiedad, ocupa más y convertiría cualquier renombrado en una migración de datos).

**2. Clave primaria compartida para los perfiles.** `Athlete.Id` y `School.Id` son PK y, a la vez, FK
a `Users.Id` (`HasOne<User>().WithOne().HasForeignKey<Athlete>(a => a.Id)`), con borrado en cascada.
Las entidades no declaran navegación a `User`, así que la relación se configura sin navegación.
*Alternativas descartadas*: mismo `Id` sin FK (la base de datos no impediría un perfil huérfano) y
herencia TPH/TPT (impediría clases `sealed` y mezclaría datos de dos tipos de cuenta, contradiciendo
la decisión de identidad compartida ya tomada al modelar el dominio).

**3. Las entidades hijas son entidades normales con FK explícita**, no owned types:
`AthleteSkill.AthleteId`, `SchoolService.SchoolId`, `Comment.PostId`, `Like.PostId` y
`NotificationPreference.UserId`, mapeadas con `HasMany(x => x.Colección).WithOne().HasForeignKey(...)`.
EF debe poblar las colecciones de solo lectura a través de los backing fields `_declaredSkills`,
`_services`, `_comments`, `_likes` y `_notificationPreferences` (convención `_camelCase`).
*Alternativa descartada*: `OwnsMany`, que impide que otra entidad referencie la hija — y `Rating`
referencia a `AthleteSkill`.

**4. Cascada solo dentro del agregado; `Restrict` al cruzar agregados.** En cascada: `User`→perfil,
`User`→`NotificationPreference`, `User`→`Notification`, `Athlete`→`AthleteSkill`,
`AthleteSkill`→`Rating`, `School`→`SchoolService`, `Post`→`Comment`, `Post`→`Like`. Con
`DeleteBehavior.Restrict`: `Rating.AuthorId`, `FriendRequest.*`, `Friendship.*`, `Block.*`,
`SchoolAttendance.*`, `Post.AuthorId`, `Report.*`, `AthleteSkill.SkillId`, `School.SportId`,
`Post.SportId`, `Skill.SportId`. Motivo doble: SQL Server rechaza *multiple cascade paths* (por
ejemplo `Rating` entra en el árbol de `User` por dos caminos: su autor y su maniobra declarada) y una
cascada entre agregados borraría datos ajenos sin que ningún caso de uso lo pida.

**5. N:M `Athlete`↔`Sport` con tabla de unión `AthleteSports`** y PK compuesta (`AthleteId`,
`SportId`), configurada dentro de `AthleteConfiguration`.

**6. Claves primarias `Guid` con `ValueGeneratedNever()`.** Los identificadores los genera el dominio
(`Guid.NewGuid()` en las hijas, `Guid` recibido en los factory), no la base de datos.

**7. Sin filtro global de borrado suave.** `AccountStatus.Deleted` es un estado de dominio y la
visibilidad se resuelve en los casos de uso. Un `HasQueryFilter` exigiría unir `Athlete`/`School` con
`User`, algo que el filtro no puede expresar sin navegaciones que el modelo no tiene.

**8. Sin carga perezosa.** Las consultas usarán `Include` explícito; el paquete de proxies sigue
referenciado pero `UseLazyLoadingProxies()` no se invoca.

**9. `Email` no es único.** La vinculación de identidades Google/Facebook está abierta
(`project.md` §15.7). `Nickname` sí es único: es una regla de dominio (`domain.md`).

**10. Tipos y longitudes.**

| Tipo | Columna | Longitud |
|---|---|---|
| `Guid` (PK y FK) | `uniqueidentifier` | — |
| `DateOnly` (`DateOfBirth`) | `date` | — |
| `DateTime` | `datetime2` | — |
| `decimal` (`Price`) | `decimal(18,2)` | — |
| `bool` | `bit` | — |
| enumeraciones | `int` | — |
| `Email`, `ContactEmail` | `nvarchar` | 320 |
| `Nickname` | `nvarchar` | 50 |
| `FirstName`, `LastName`, `Name` | `nvarchar` | 200 |
| `Country`, `City` | `nvarchar` | 100 |
| `Phone` | `nvarchar` | 30 |
| `Currency` | `nvarchar` | 3 |
| `MediaUrl`, `ProfilePictureUrl`, `LogoUrl`, `Website` | `nvarchar` | 2048 |
| `Biography`, `Description`, `Caption`, `Comment.Text` | `nvarchar` | 1000 |
| `FreeText`, `ContentReference` | `nvarchar` | 200 |
| `IdentityProvider` | `nvarchar` | 50 |
| `ExternalIdentityId` | `nvarchar` | 200 |
| `PreferredLanguage` | `nvarchar` | 10 |

**11. Índices.** Únicos: `Users.Nickname`; `Sports.Name`; `Skills(SportId, Name)`;
`AthleteSkills(AthleteId, SkillId)`; `Ratings(AthleteSkillId, AuthorId)`;
`Friendships(UserAId, UserBId)`; `Blocks(BlockerId, BlockedId)`;
`SchoolAttendances(AthleteId, SchoolId)`; `Likes(PostId, UserId)`;
`NotificationPreferences(UserId, Channel)`. De consulta: `FriendRequests(ReceiverId, Status)` y
`FriendRequests(RequesterId)`; `Notifications(RecipientId, IsRead)`; `Posts(AuthorId, PublishedAt)` y
`Posts(SportId)`; `Reports(Status)`; `Friendships(UserBId)`. Todos con nombre explícito `IX_...`.

**12. Repositorios.** Interfaz `I<X>Repository : IBaseRepository<X>` en `Application/Repositories/`
(sin métodos adicionales por ahora) e implementación
`public sealed class <X>Repository(MyCompanyDbContext dbContext) : BaseRepository<X>(dbContext), I<X>Repository { }`
en `Persistence/Repositories/`, para los 13 agregados: `User`, `Athlete`, `School`, `Sport`, `Skill`,
`Rating`, `FriendRequest`, `Friendship`, `Block`, `SchoolAttendance`, `Post`, `Report` y
`Notification`. `public sealed` (y no `internal`) para que `PersistenceModule` los resuelva sin
depender de que el escáner de Autofac incluya tipos no públicos.

**13. Alinear `Microsoft.EntityFrameworkCore.Design` de `Api.csproj` a `10.0.9`.** Como el paquete
va con `PrivateAssets=all`, la versión de 8.0.22 del proyecto de arranque no se sustituye por la de
Persistence: las herramientas de EF cargarían diseño 8 contra runtime 10. Es prerrequisito de la
migración, no una limpieza opcional.

**14. Configuraciones `internal sealed`** (siguiendo la convención de `QWEN.md` para tipos de
configuración de EF), una por entidad, en `Persistence/Configurations/`.

## Risks / Trade-offs

- **Colecciones de solo lectura no enlazadas por convención** → si EF no descubre los backing fields,
  fijarlos explícitamente con `Navigation(...).HasField("_declaredSkills")` y equivalentes.
- **Host de diseño que no arranca** (`Program.cs` exige claves como `securityServer:authority` y el
  `DbContext` recibe `IMediator`) → si `dotnet ef migrations add` no puede construir el contexto,
  añadir un `IDesignTimeDbContextFactory<MyCompanyDbContext>` en Persistence, que evita montar el
  host.
- **Mapear 18 entidades de golpe amplifica cualquier convención equivocada** → mitigado fijando aquí
  las convenciones y aplicándolas uniformemente; el primer cambio de comportamiento podrá ajustarlas
  con una migración incremental.
- **La migración se genera pero no se aplica** → no hay verificación contra SQL Server real; el SQL
  se revisa con `dotnet ef migrations script`.
- **Herramienta `dotnet ef` ausente** → `dotnet tool install -g dotnet-ef` (EF 10.0.9).

## Migration Plan

1. Alinear el paquete de diseño de EF en `Api.csproj` (decisión 13).
2. Crear las 18 configuraciones y los 13 repositorios; compilar.
3. Generar la migración:
   `dotnet ef migrations add InitialCreate --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api`
   → aparece `Persistence/Migrations/`.
4. Revisar el SQL con `dotnet ef migrations script --no-build` y comprobar tablas, claves e índices.

**Reversión**: al no aplicarse a ninguna base de datos, revertir consiste en eliminar la carpeta
`Migrations/` y los archivos añadidos. No hay datos que restaurar.
