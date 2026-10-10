# Proposal: Cimientos de persistencia

## Why

El dominio está modelado (18 entidades, 10 enumeraciones, 8 raíces de agregado) pero **nada se
persiste**: no hay una sola configuración de EF Core, ni repositorios, ni migración. El dominio es
hoy código inerte y ningún cambio de comportamiento —alta de cuentas, publicaciones, puntuaciones—
puede empezar sin esta base. Además, las convenciones de mapeo (claves, cascadas, índices,
longitudes) deben fijarse una sola vez, sobre las 18 entidades, antes de que cada rebanada inventa
las suyas.

## What Changes

- **Mapeo del dominio a EF Core**: 18 configuraciones `IEntityTypeConfiguration<T>` en
  `Persistence/Configurations/`, una por entidad, con tabla, clave primaria, longitudes,
  `IsRequired`, índices con nombre explícito (`IX_...`) y relaciones.
- **Tabla de unión `AthleteSports`** para la relación N:M entre `Athlete` y `Sport`.
- **Clave primaria compartida**: `Athlete.Id` y `School.Id` son PK y a la vez FK a `Users.Id`.
- **Enumeraciones como `int`** en base de datos (los nombres siguen exponiéndose en JSON).
- **13 repositorios** (uno por raíz de agregado): interfaz en `Application/Repositories/` y clase
  mínima en `Persistence/Repositories/`, registrada por `PersistenceModule` mediante su convención
  de nombre.
- **Migración inicial** `InitialCreate` en `Persistence/Migrations/`.
- **Alineación de paquete**: `Microsoft.EntityFrameworkCore.Design` de `Api.csproj` pasa de `8.0.22`
  a `10.0.9`, para que las herramientas de EF no mezclen diseño 8 con runtime 10.
- **`QWEN.md`**: refleja que `Configurations/` ya no está vacío, que existen `Migrations/` y los
  repositorios.

## Capabilities

### New Capabilities

Ninguna. Este cambio **no modifica comportamiento observable del sistema**: no añade endpoints, ni
casos de uso, ni reglas de negocio. Es un cambio de infraestructura sobre un modelo ya especificado
en `openspec/specs/domain.md`, por lo que se declara `skip_specs: true` en `.openspec.yaml` y **no**
se crea delta de capacidad.

### Modified Capabilities

Ninguna.

## Impact

- **Persistence**: se crean `Configurations/` (18 archivos), `Repositories/` (13 archivos) y
  `Migrations/` (migración inicial + snapshot del modelo). `MyCompanyDbContext` no se modifica:
  `ApplyConfigurationsFromAssembly` ya descubre las configuraciones.
- **Application**: se crean 13 interfaces `I<X>Repository` en `Repositories/`, extendiendo
  `IBaseRepository<T>` sin métodos adicionales por ahora.
- **Api**: un único cambio de versión de paquete en `.csproj`. Sin cambios de código ni de
  configuración.
- **Base de datos**: la migración **no se aplica** en este cambio (`ConnectionStrings:MyCompany`
  sigue siendo la cadena de ejemplo inválida); solo se genera.
- **Sin cambios** en `Domain`, en los casos de uso, en los controladores ni en la API pública.
