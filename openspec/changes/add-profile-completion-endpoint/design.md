# Design: Completar el perfil propio

## Context

- `GET /api/user/profile/me` ya funciona: `200` con `MyProfileResponse`, `204` si no hay perfil.
- `User.SetNickname` existe y solo se puede llamar una vez; `User.AccountType` es inmutable.
- `Athlete.Create(id, firstName, lastName, dateOfBirth, country, city)` valida mayoría de edad y lanza
  `ArgumentException`; los campos opcionales se ponen con `Athlete.UpdateProfile(...)`, y los deportes con
  `AddPracticedSport(Sport)`.
- `School.Create(id, name, sportId, country, city)` exige deporte; el resto de campos van por
  `School.UpdateProfile(name, country, city, description, contactEmail, phone, website, logoUrl)`.
- Los perfiles comparten `Id` con la cuenta, así que crear el perfil es crear una fila con ese mismo `Id`.
- `UserConfiguration` tiene índice único en `Nickname`; `ICurrentUser` da el `sub`; `IUserRepository` busca
  por identidad externa.

## Goals / Non-Goals

**Goals:**

- Que un usuario con `204` pueda completar su perfil y pasar a `200` en una sola llamada.
- Que el formulario tenga el catálogo real de deportes.
- Que el servidor no acepte datos incoherentes con el tipo de cuenta.

**Non-Goals:**

- Editar el perfil después (`PUT`) y las maniobras declaradas.
- Subir imágenes: la foto y el logo son URL.
- Mantener el catálogo de deportes desde la API (lo hace el técnico).

## Decisions

**1. `POST` que devuelve `MyProfileResponse`, no `201` con `Location`.** El cliente ya tiene la pantalla del
perfil y lo que necesita es el objeto recién creado para pintarlo; devolverlo evita un segundo `GET`. La
edición será `PUT` en otro cambio.

**2. Un solo comando con los dos bloques opcionales.** El controlador no sabe el tipo de cuenta y no debe
averiguarlo; el handler busca la cuenta por el `sub`, comprueba que el bloque recibido corresponde a su
`AccountType` y delega en un constructor privado por tipo. Mismo patrón que `BuildUser` en el alta.

**3. Validación en `Validators/Commands`, no en el handler.** Nick obligatorio y alfanumérico (la regla del
dominio, adelantada para no provocar una excepción), exactamente un bloque, mayoría de edad en la fecha de
nacimiento y `sportId` obligatorio para una escuela. El handler queda como orquestación. Los errores se
comunican con `Result` y el controlador responde `Problem`, como el resto.

**4. Nick ocupado: comprobación previa y el índice único como red.** Se añade
`IUserRepository.ExistsByNicknameAsync`; si ya existe, `400` con un mensaje que el cliente enseña junto al
campo. El índice único sigue siendo la garantía si dos peticiones coinciden.

**5. Deporte del deportista: se cargan por identificador y se valida que existan.** Se añaden con
`AddPracticedSport`; un identificador desconocido es `400` (no se ignora en silencio). Los repetidos los
corta el dominio.

**6. Catálogo: `GET /api/sports` con los activos y una semilla en migración.** Sin filas el formulario no
tiene opciones; la semilla es dato de demostración (`HasData` con identificadores fijos) y se puede
sustituir por el catálogo real del técnico.

**7. El perfil se crea en una transacción lógica: cuenta + perfil se guardan con dos llamadas.** No hay
unidad de trabajo explícita en esta arquitectura (`Update`/`Add` persisten); se acepta que un fallo entre
las dos deje la cuenta sin perfil, que es justo el estado `204` y se puede reintentar.

## Risks / Trade-offs

- **Sin base de datos local** la ruta real no se puede probar aquí: se cubre con tests de handler y la
  comprobación de punta a punta es del propietario, con la base de `dv` y un token.
- **La semilla de deportes es dato de producto en una migración**: se documenta y se puede borrar con otra
  migración cuando exista el catálogo real.
- **Carrera en el nick**: dos peticiones a la vez pueden pasar la comprobación previa; la segunda choca
  contra el índice y devuelve `500`. Se acepta en esta primera versión.

## Migration Plan

1. Semilla de deportes con su migración; aplicarla en `dv`.
2. Desplegar la API (pipeline `BuildAndDeploy`).
3. Comprobar con un token: `204` → `POST` → `200`, y `GET /api/sports` con filas.
4. **Reversión**: quitar el `POST`; los perfiles creados se quedan.

## Open Questions

- Si la foto y el logo acabarán subiéndose a un almacén en vez de ser URL.
- Si el `PUT` de edición debe permitir cambiar el deporte de una escuela (hoy `School.UpdateProfile` no lo
  toca).
