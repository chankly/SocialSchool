# Tasks

> Las preguntas abiertas del diseño (almacén de imágenes, cambiar el deporte de una escuela) no bloquean
> nada de aquí.

## 1. Contrato y aplicación

- [ ] 1.1 Añadir `ExistsByNicknameAsync(string nickname, CancellationToken)` a `IUserRepository` y su implementación en `UserRepository` (`Set<User>()` + `AsNoTracking`). Verificación: no toca el índice único de `UserConfiguration`.
- [ ] 1.2 Crear `Commands/Profiles/CompleteMyProfileCommand` con el nick y los dos bloques opcionales (`AthleteProfileRequest`, `SchoolProfileRequest`) y su handler `public sealed`: busca la cuenta por el `sub`, exige que el bloque presente corresponda a su `AccountType`, fija el nick, crea el perfil con el mismo `Id` y devuelve `MyProfileResponse`. Verificación: un bloque que no corresponde es `Result.Failure`; el nick se fija con `SetNickname`; no se inventan deportes.
- [ ] 1.3 Validadores en `Validators/Commands`: nick obligatorio y alfanumérico, exactamente un bloque, fecha de nacimiento que acredite mayoría de edad y `sportId` obligatorio en la escuela. Verificación: se descubren por assembly, sin registro nuevo.
- [ ] 1.4 Los DTOs de entrada en `Models/Profiles/` (`AthleteProfileRequest`, `SchoolProfileRequest`) con los deportes como `IReadOnlyList<Guid>` de identificadores. Verificación: los de salida no cambian.

## 2. Api

- [ ] 2.1 Añadir `POST me` a `UserProfileController` que envíe el comando y devuelva `Ok(result.Value)` o `Problem(detail:)`. Verificación: la ruta sigue siendo `api/user/profile` y no lleva `[AllowAnonymous]`.
- [ ] 2.2 Crear `SportsController` con `GET api/sports` y un `GetSportsQuery` que devuelva solo los activos, reutilizando el repositorio de `Sport`. Verificación: no se exponen entidades de dominio.

## 3. Persistencia

- [ ] 3.1 Sembrar seis deportes con `HasData` en `SportConfiguration` (identificadores fijos, `IsActive`) y generar la migración. Verificación: la migración solo inserta filas de `Sports`; el snapshot queda coherente.

## 4. Verificación

- [ ] 4.1 Tests del handler con repositorios simulados: deportista correcto, escuela correcta, bloque que no corresponde al tipo, nick ocupado, deporte desconocido, y que el `Id` del perfil es el de la cuenta.
- [ ] 4.2 `dotnet build` y `dotnet test` en verde (salvo el fallo preexistente de audiencia).
