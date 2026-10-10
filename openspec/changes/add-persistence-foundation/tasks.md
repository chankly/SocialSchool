# Tasks

> No hay proyecto de tests en la solución, así que la verificación de cada grupo es la compilación
> (`dotnet build HSoft.SocialSchool.slnx`, 0 errores) y, para el modelo, la generación de la
> migración y del SQL (grupo 7), que es cuando EF construye y valida el modelo completo.

## 1. Preparación

- [x] 1.1 Alinear `Microsoft.EntityFrameworkCore.Design` en `src/HSoft.SocialSchool.Api/HSoft.SocialSchool.Api.csproj` de `8.0.22` a `10.0.9`; verificar con `dotnet build HSoft.SocialSchool.slnx` (0 errores) y comprobando que el paquete listado para Api es 10.0.9.
- [x] 1.2 Comprobar que la herramienta `dotnet ef` está disponible (y con versión 10.x) e instalarla con `dotnet tool install -g dotnet-ef` si falta; verificar ejecutando `dotnet ef --version`.

## 2. Configuraciones: cuentas y perfiles

- [x] 2.1 Crear `Persistence/Configurations/UserConfiguration.cs` (`internal sealed`, tabla `Users`, PK `Guid` con `ValueGeneratedNever()`, índice único `IX_Users_Nickname`, longitudes según design.md, `Email` sin índice único) y verificar que `dotnet build` compila.
- [x] 2.2 Crear `Persistence/Configurations/NotificationPreferenceConfiguration.cs` (tabla `NotificationPreferences`, FK `UserId`→`Users` en cascada, índice único `IX_NotificationPreferences_UserId_Channel`) y verificar que `dotnet build` compila.
- [x] 2.3 Crear `Persistence/Configurations/AthleteConfiguration.cs` (tabla `Athletes`, PK `Id` además FK a `Users.Id` en cascada mediante `HasOne<User>().WithOne().HasForeignKey<Athlete>(a => a.Id)`, relación N:M con `Sport` sobre la tabla de unión `AthleteSports` con PK compuesta `(AthleteId, SportId)`, y 1:N con `AthleteSkill` en cascada) y verificar que `dotnet build` compila y que la configuración referencia la colección `DeclaredSkills`.
- [x] 2.4 Crear `Persistence/Configurations/SchoolConfiguration.cs` (tabla `Schools`, PK `Id` además FK a `Users.Id` en cascada, `SportId`→`Sports` con `DeleteBehavior.Restrict`, 1:N con `SchoolService` en cascada) y verificar que `dotnet build` compila.
- [x] 2.5 Crear `Persistence/Configurations/AthleteSkillConfiguration.cs` (tabla `AthleteSkills`, `AthleteId`→`Athletes` en cascada, `SkillId`→`Skills` con `Restrict`, índice único `IX_AthleteSkills_AthleteId_SkillId`) y verificar que `dotnet build` compila.
- [x] 2.6 Crear `Persistence/Configurations/SchoolServiceConfiguration.cs` (tabla `SchoolServices`, `SchoolId`→`Schools` en cascada, `Price` `decimal(18,2)`, `Currency` `nvarchar(3)`, longitudes de design.md) y verificar que `dotnet build` compila.

## 3. Configuraciones: catálogo y puntuaciones

- [x] 3.1 Crear `Persistence/Configurations/SportConfiguration.cs` (tabla `Sports`, índice único `IX_Sports_Name`) y verificar que `dotnet build` compila.
- [x] 3.2 Crear `Persistence/Configurations/SkillConfiguration.cs` (tabla `Skills`, `SportId`→`Sports` con `Restrict`, índice único `IX_Skills_SportId_Name`) y verificar que `dotnet build` compila.
- [x] 3.3 Crear `Persistence/Configurations/RatingConfiguration.cs` (tabla `Ratings`, `AthleteSkillId`→`AthleteSkills` en cascada, `AuthorId`→`Users` con `Restrict` para no crear un segundo camino de cascada, índice único `IX_Ratings_AthleteSkillId_AuthorId`) y verificar que `dotnet build` compila.
- [x] 3.4 Crear `Persistence/Configurations/SchoolAttendanceConfiguration.cs` (tabla `SchoolAttendances`, `AthleteId` y `SchoolId` con `Restrict`, índices únicos y de consulta según design.md) y verificar que `dotnet build` compila.

## 4. Configuraciones: contenido

- [x] 4.1 Crear `Persistence/Configurations/PostConfiguration.cs` (tabla `Posts`, `AuthorId`→`Users` y `SportId`→`Sports` con `Restrict`, longitudes, índices `IX_Posts_AuthorId_PublishedAt` e `IX_Posts_SportId`, y 1:N con `Comment` y `Like` en cascada) y verificar que `dotnet build` compila.
- [x] 4.2 Crear `Persistence/Configurations/CommentConfiguration.cs` (tabla `Comments`, `PostId`→`Posts` en cascada, `Text` `nvarchar(1000)`) y `Persistence/Configurations/LikeConfiguration.cs` (tabla `Likes`, `PostId`→`Posts` en cascada, índice único `IX_Likes_PostId_UserId`); verificar que `dotnet build` compila.

## 5. Configuraciones: relaciones sociales y notificaciones

- [x] 5.1 Crear `Persistence/Configurations/FriendRequestConfiguration.cs` (tabla `FriendRequests`, `RequesterId` y `ReceiverId` con `Restrict`, índices `IX_FriendRequests_ReceiverId_Status` e `IX_FriendRequests_RequesterId`) y verificar que `dotnet build` compila.
- [x] 5.2 Crear `Persistence/Configurations/FriendshipConfiguration.cs` (tabla `Friendships`, ambos extremos con `Restrict`, índice único `IX_Friendships_UserAId_UserBId` e índice `IX_Friendships_UserBId`) y verificar que `dotnet build` compila.
- [x] 5.3 Crear `Persistence/Configurations/BlockConfiguration.cs` (tabla `Blocks`, ambos extremos con `Restrict`, índice único `IX_Blocks_BlockerId_BlockedId`) y verificar que `dotnet build` compila.
- [x] 5.4 Crear `Persistence/Configurations/ReportConfiguration.cs` (tabla `Reports`, `ReporterId`→`Users` con `Restrict`, `FreeText` `nvarchar(200)`, índice `IX_Reports_Status`) y verificar que `dotnet build` compila.
- [x] 5.5 Crear `Persistence/Configurations/NotificationConfiguration.cs` (tabla `Notifications`, `RecipientId`→`Users` en cascada, `ContentReference` `nvarchar(200)`, índice `IX_Notifications_RecipientId_IsRead`) y verificar que `dotnet build` compila.

## 6. Repositorios

- [x] 6.1 Crear las 13 interfaces en `Application/Repositories/I<X>Repository.cs` (`User`, `Athlete`, `School`, `Sport`, `Skill`, `Rating`, `FriendRequest`, `Friendship`, `Block`, `SchoolAttendance`, `Post`, `Report`, `Notification`), cada una extendiendo `IBaseRepository<X>` sin métodos adicionales; verificar que `dotnet build` compila.
- [x] 6.2 Crear las 13 clases en `Persistence/Repositories/<X>Repository.cs` como `public sealed class <X>Repository(MyCompanyDbContext dbContext) : BaseRepository<X>(dbContext), I<X>Repository { }`; verificar que `dotnet build` compila y que cada nombre termina en `Repository` y ninguno empieza por `Caching` (condición que exige `PersistenceModule` para registrarlos por convención).

## 7. Migración inicial

- [x] 7.1 Generar la migración con `dotnet ef migrations add InitialCreate --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api`; verificar que aparecen `Persistence/Migrations/*_InitialCreate.cs`, su `.Designer.cs` y `MyCompanyDbContextModelSnapshot.cs`.
- [x] 7.2 Verificar el SQL del modelo con `dotnet ef migrations script --project HSoft.SocialSchool.Persistence --startup-project HSoft.SocialSchool.Api`; comprobar que incluye las 19 tablas (`Users`, `Athletes`, `Schools`, `Sports`, `Skills`, `AthleteSkills`, `AthleteSports`, `SchoolServices`, `Ratings`, `FriendRequests`, `Friendships`, `Blocks`, `SchoolAttendances`, `Posts`, `Comments`, `Likes`, `Reports`, `Notifications`, `NotificationPreferences`), las PK compartidas de `Athletes`/`Schools`, los índices únicos declarados y ninguna cascada entre agregados. Si el host de diseño no arranca, añadir `IDesignTimeDbContextFactory<MyCompanyDbContext>` en Persistence y repetir.
- [x] 7.3 Descartar el aviso de *multiple cascade paths* si apareciera al aplicar el script: revisar que la FK culpable está con `DeleteBehavior.Restrict` según design.md y regenerar la migración.

## 8. Cierre

- [x] 8.1 Actualizar `QWEN.md` (Persistencia: `Configurations/` ya no está vacío y existe `Migrations/`; Repositorios: 13 implementaciones; Estado actual: el dominio ya se persiste) y verificar leyendo las secciones modificadas.
- [x] 8.2 Ejecutar `openspec validate --change add-persistence-foundation` y `openspec status --change add-persistence-foundation`; verificar que la validación pasa y que los cuatro artefactos quedan `done` o `skipped` y ninguno `blocked`.
