# Tasks

> Las decisiones abiertas del diseño (el endpoint de alta, las cuentas eliminadas, el nombre del deporte
> de la escuela) no cambian lo que hay que construir aquí, así que no bloquean estas tareas.

## 1. Application

- [x] 1.1 Añadir a `IUserRepository` un método de lectura por identidad externa (`GetByExternalIdReadOnlyAsync`) y a `IAthleteRepository` uno que devuelva el deportista **con sus deportes** (`GetByIdWithSportsReadOnlyAsync`), documentados con XML. Verificación: las interfaces no añaden nada más y la escuela no necesita método propio.
- [x] 1.2 Crear `Queries/Profiles/GetMyProfileQuery.cs` (`public sealed record`, `IRequest<Result<MyProfileResponse?>>`) y su handler `public sealed` con `ValueTask`: busca por `sub`, y si no hay cuenta o no hay fila del tipo devuelve éxito con `null`; si el token no trae `sub`, devuelve fallo. Verificación: no consulta el tipo de cuenta en el token y no usa excepciones para el caso "sin perfil".
- [x] 1.3 Crear los DTOs de salida en `Models/Profiles/`: uno común (`Id`, `Nickname`, `Email`, `AccountType`, `AccountStatus`) con los bloques `Athlete` y `School` opcionales, y los de cada tipo con los campos del formulario (el deportista, además, sus deportes con nombre). Verificación: no se exponen entidades de dominio ni colecciones hijas.

## 2. Persistence

- [x] 2.1 Implementar los dos métodos en `UserRepository` y `AthleteRepository` usando `_dbContext.Set<T>()` y `AsNoTracking`, con el `Include` de los deportes en el del deportista. Verificación: `dotnet build` y que el `Include` está en la consulta del deportista, no en la del usuario.
- [x] 2.2 Añadir el índice único sobre `Users.ExternalIdentityId` en `UserConfiguration` con nombre explícito (`IX_Users_ExternalIdentityId`) y generar la migración. Verificación: la migración nueva aparece con su `.Designer.cs` y el snapshot, y el índice es único.

## 3. Api

- [x] 3.1 Crear `UserProfileController` con `[Route("api/user/profile")]` y `[HttpGet("me")]`, constructor primario con `IMediator`, `Send(..., cancellationToken)`, `Problem(detail:)` si falla y `NoContent()`/`Ok(...)` según el valor; con XML docs y `[ProducesResponseType]` para 200 y 204. Verificación: la ruta compuesta es `GET /api/user/profile/me` y el endpoint no declara `[AllowAnonymous]`.

## 4. Spec y documentación

- [x] 4.1 Alinear `openspec/specs/project.md` §4: el alta de la cuenta la materializa el webhook con la identidad y el tipo, y los **datos del perfil** los completa el usuario en la aplicación, con `204` mientras no exista. Verificación: el apartado ya no dice que el backend no expone endpoints para rellenar el perfil.
- [x] 4.2 Anotar en `QWEN.md` que la capa Application ya tiene casos de uso (el de migraciones y esta consulta), porque el documento afirma que esas carpetas están vacías. Verificación: la afirmación deja de ser falsa.

## 5. Verificación

- [x] 5.1 `dotnet build` de la solución sin errores nuevos. *(Resultado real: compila; los avisos son los preexistentes de documentación XML y nulabilidad.)*
- [x] 5.2 Tests del handler con repositorios simulados: deportista con perfil → valor con el bloque `Athlete` y los deportes; escuela con perfil → bloque `School` y `sportId`; cuenta sin fila de perfil → éxito con `null`; sin cuenta → éxito con `null`; token sin `sub` → fallo. *(Resultado real: los cinco pasan. La suite queda en 16/17 porque `AuthenticationConfigurationTests` falla por un GUID de audiencia fijado en el test que ya no coincide con la configuración local; es anterior a este cambio y no se toca aquí.)*
- [x] 5.3 Revisar el diff: solo los ficheros de este cambio (Application, Persistence con su migración, Api, spec y `QWEN.md`), sin tocar el dominio ni la configuración de autenticación.
