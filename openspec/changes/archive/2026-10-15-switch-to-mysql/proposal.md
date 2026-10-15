# Proposal: Cambiar la API a MySQL

## Why

La capa de Persistencia está cableada a **SQL Server** (`UseSqlServer`,
`Microsoft.EntityFrameworkCore.SqlServer` 10.0.9) mientras la infraestructura que acabamos de construir
despliega **MySQL** (`socialschool-mysql-<entorno>`, con las bases `api` y `keycloak`). Consecuencia
directa: la API **no puede usar su base de datos en absoluto**, porque la cadena de conexión que recibe
apunta a un motor que su código no sabe hablar. Y la única migración que existe (`InitialCreate`) es SQL
Server específica —incluido un índice **filtrado**, que MySQL no tiene—, así que tampoco describe el
esquema que queremos.

## What Changes

- **Proveedor**: fuera `Microsoft.EntityFrameworkCore.SqlServer`, dentro
  **`MySql.EntityFrameworkCore` 10.0.9** (el de Oracle, alineado con EF Core 10.0.9) y `UseSqlServer` →
  el `UseMySQL` de ese proveedor.
- **BREAKING (esquema)**: el **vínculo de asistencia pasa a ser una sola fila por par (deportista,
  escuela)** cuyo estado evoluciona. El índice único **filtrado** (`WHERE [Status] IN (0, 1)`) se
  sustituye por un **índice único de dos campos**, sin columna extra ni SQL escrito a mano, y se añade la
  transición que devuelve a *pendiente* un vínculo rechazado o retirado cuando se vuelve a solicitar.
- **Migración**: se **reemplaza `InitialCreate`** y se **regenera** contra MySQL. Se puede hacer sin
  conservar nada porque nunca se ha aplicado a ninguna base de datos.
- **Cadenas de conexión de ejemplo**: las de `appsettings.json` de la API y del proyecto de tests pasan a
  formato MySQL; hoy las dos son de SQL Server.
- **Documentación**: `QWEN.md` (motor, proveedor, migración e índice), el ciclo de vida del vínculo de
  asistencia en `project.md` §10 y `domain.md`, y la nota de `devops/iac/README.md` que dice que la API
  todavía no puede hablar con la base de datos, que deja de ser cierta.

## Capabilities

### New Capabilities

Ninguna. Este cambio **no modifica comportamiento observable**: no añade endpoints ni reglas que un
usuario del sistema pueda notar. Es persistencia más una precisión del modelo de datos, así que se declara
`skip_specs: true` en `.openspec.yaml`. La única capacidad existente (`authentication`) no cambia: sus
tests de integración no tocan la base de datos.

### Modified Capabilities

Ninguna.

## Impact

- **Persistence**: `HSoft.SocialSchool.Persistence.csproj`, `IoC/Services/PersistenceServiceExtensions.cs`,
  `Configurations/SchoolAttendanceConfiguration.cs` y los tres ficheros de `Migrations/`.
- **Domain**: un método nuevo en `Entities/Attendances/SchoolAttendance.cs`.
- **Configuración**: los `appsettings.json` de la API y del proyecto de tests.
- **Documentación**: `QWEN.md`, `openspec/specs/project.md`, `openspec/specs/domain.md` y
  `devops/iac/README.md`.
- **Sin cambios** en la API, en los tests de integración ni en los contratos de repositorio.
- **Riesgo que se comprueba antes de nada**: que el proveedor de Oracle soporte `dotnet ef migrations`.
  De eso depende todo el plan: si no lo soportara, habría que replantear el enfoque.
- **Techo de verificación**: no hay ninguna instancia de MySQL disponible, así que `database update` sigue
  siendo paso del usuario.
