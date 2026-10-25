# Proposal: Consultar mi perfil

## Why

Hoy no hay forma de que un cliente sepa si el usuario autenticado ya rellenó su perfil. El registro lo
hace Keycloak: el usuario se autentica y vuelve a la web, y a partir de ahí no existe ninguna consulta
que distinga "tiene perfil" de "todavía no lo ha rellenado". Sin esa consulta la web no puede decidir si
muestra el perfil o el formulario de alta, que es el primer paso del flujo que el producto necesita.

La consulta tiene que tolerar que **no exista ni la cuenta de negocio**: `openspec/specs/authentication/
spec.md` exige que la autenticación sea independiente de la cuenta, *"de modo que un usuario recién
llegado pueda comprobar y completar su registro"*. Ese caso no es un error, es el estado normal justo
después del registro.

## What Changes

- **Nuevo endpoint `GET /api/user/profile/me`**, bajo la política de denegación por defecto:
  - **200** con el perfil del usuario autenticado cuando existe.
  - **204** cuando no hay perfil: ni cuenta de negocio todavía, o cuenta sin `Athlete`/`School`.
  - **401** lo produce el middleware de autenticación cuando no hay token válido; el endpoint no llega a
    ejecutarse.
- **El tipo de cuenta no se lee del token**: se busca la cuenta por el `sub` y se lee su `AccountType`,
  así que el endpoint no depende de ningún mapper nuevo en Keycloak. El tipo viajará en el token para
  que la web elija el formulario, pero eso es del realm y queda fuera de este cambio.
- **Dos consultas nuevas de repositorio**: buscar el usuario por su identidad externa y cargar el
  deportista con sus deportes (el lazy loading está apagado, así que sin `Include` la colección llega
  vacía). La escuela reutiliza la lectura por identificador que ya existe.
- **Índice único sobre `Users.ExternalIdentityId`** con su migración: es la clave por la que se busca en
  cada llamada, y la unicidad es una invariante real (una cuenta por identidad de Keycloak).
- **Alineación del spec de producto**: `openspec/specs/project.md` §4 dice hoy que el alta —identidad y
  **datos del perfil**— se completa en Keycloak y que el backend **no** expone endpoints para rellenar el
  perfil. Eso deja de ser cierto: Keycloak autentica, el webhook materializa la cuenta y el perfil lo
  completa el usuario en la aplicación.

## Capabilities

### New Capabilities

- **`profile`**: consulta del perfil propio. Es la primera capacidad del área de perfil y en este cambio
  solo cubre la lectura; completar y editar llegarán después.

### Modified Capabilities

Ninguna. La capacidad `authentication` ya exige que estar autenticado no dependa de tener cuenta.

## Impact

- **Nuevo**: `Application/Queries/Profiles/` (consulta, handler y DTOs), `Api/Controllers/
  UserProfileController.cs` y, al archivar, `openspec/specs/profile/spec.md`.
- **Modificado**: `IUserRepository` e `IAthleteRepository` (un método cada uno), sus implementaciones en
  Persistence, `UserConfiguration` y `openspec/specs/project.md` §4, más **una migración nueva**.
- **Sin cambios en el dominio**: `User`, `Athlete` y `School` ya tienen lo necesario.
- **Migración**: el índice único necesita `dotnet ef database update`, que es paso del usuario.
- **Sin cambios de permisos**: el endpoint queda bajo la política por defecto.
- **Fuera de alcance**: el endpoint que crea la cuenta y el perfil (lo que la web enviará con el
  formulario), y el mapper de Keycloak que pone el tipo de cuenta en el token para la web.
