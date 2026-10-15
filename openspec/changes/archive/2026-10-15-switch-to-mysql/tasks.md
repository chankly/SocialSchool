# Tasks

> Las dos decisiones abiertas del diseño (dónde probar contra MySQL y si el motor entra en
> `project.md` §11) no cambian lo que hay que construir, así que no bloquean estas tareas.

## 1. Comprobación previa

- [x] 1.1 Añadir `MySql.EntityFrameworkCore` 10.0.9 a `HSoft.SocialSchool.Persistence.csproj` y comprobar que las herramientas reconocen el modelo: `dotnet ef dbcontext info --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api`. Si el proveedor no soportara las herramientas de EF, **parar y avisar** antes de seguir, porque el plan depende de ello.

## 2. El motor

- [x] 2.1 Quitar `Microsoft.EntityFrameworkCore.SqlServer` del `.csproj`, dejando el resto de paquetes de EF Core en 10.0.9; verificar que `dotnet build HSoft.SocialSchool.slnx` compila.
- [x] 2.2 Cambiar en `IoC/Services/PersistenceServiceExtensions.cs` `UseSqlServer` por el `UseMySQL` que exponga el proveedor (con su namespace), conservando `MigrationsAssembly`; si el proveedor no ofrece `EnableRetryOnFailure`, quitarlo y dejar un comentario explicando por qué. Verificar que compila.

## 3. Una fila por par en el vínculo de asistencia

- [x] 3.1 Sustituir en `Configurations/SchoolAttendanceConfiguration.cs` el índice filtrado por un índice **único de dos campos** (`AthleteId`, `SchoolId`) sin `HasFilter`, ajustando el comentario para que explique la regla nueva; verificar que compila.
- [x] 3.2 Añadir `RequestAgain(...)` a `Entities/Attendances/SchoolAttendance.cs`: permitido solo desde `Rejected` o `Withdrawn`, devuelve la fila a `Pending` y actualiza quién la inicia y las fechas; verificar que compila y que `Accept`, `Reject` y `Withdraw` siguen exigiendo `Pending` donde ya lo exigían.
- [x] 3.3 Actualizar el ciclo de vida en `openspec/specs/project.md` §10 y las reglas de `SchoolAttendance` en `openspec/specs/domain.md`: volver a solicitar devuelve **la misma fila** a *pendiente*, la garantía es una fila por par y **no se conserva histórico de intentos**; verificar leyendo las dos secciones.

## 4. La migración

- [x] 4.1 Borrar los tres ficheros de `Persistence/Migrations/` y regenerar con `dotnet ef migrations add InitialCreate --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api`; verificar que aparecen la migración nueva, su `.Designer.cs` y el snapshot.
- [x] 4.2 Revisar el SQL con `dotnet ef migrations script --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api`: comprobar que el índice de asistencias es **único y sin filtro**, y que **no queda ni una construcción de SQL Server** (`nvarchar`, `datetime2`, `uniqueidentifier`, `WHERE [Status]`, `UseIdentityColumns`); verificar leyendo el script y anotar en el cambio cómo mapea el proveedor los `Guid` y los `DateTime`.

## 5. Configuración y documentación

- [x] 5.1 Poner cadenas de conexión de ejemplo en **formato MySQL** en `src/HSoft.SocialSchool.Api/appsettings.json` y en `tests/HSoft.SocialSchool.Api.IntegrationTests/appsettings.json`; verificar que ninguna de las dos contiene ya `Trusted_Connection` ni `Server=localhost;Database=...`.
- [x] 5.2 Actualizar `QWEN.md`: motor MySQL, proveedor de Oracle, migración regenerada, índice sin filtro y la consecuencia de la colación (nick único sin distinguir mayúsculas), donde hoy dice SQL Server; verificar leyendo las secciones de Persistencia y de restricciones.
- [x] 5.3 Corregir la nota de `devops/iac/README.md` que dice que la API todavía no puede hablar con la base de datos, porque deja de ser cierta con este cambio; verificar leyendo la sección.

## 6. Verificación

- [x] 6.1 Ejecutar `dotnet build HSoft.SocialSchool.slnx` (0 errores) y `dotnet test` (los tests de integración de autenticación en verde; no tocan la base de datos).
- [x] 6.2 Comprobar por búsqueda que en `src/` no queda ningún rastro de SQL Server (`SqlServer`, `nvarchar`, `datetime2`, `HasFilter`) y que no se han tocado la API ni los contratos de repositorio.
- [x] 6.3 Dejar escrito en el cambio que **no hay ninguna instancia de MySQL** disponible, así que `dotnet ef database update` es paso del usuario, y anotar los dos pasos que le corresponden: crear la base y aplicar la migración.
