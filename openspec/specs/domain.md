# Modelo de dominio — SocialSchool

Modelo de negocio del backend de SocialSchool, derivado de `project.md` (que es la fuente de verdad
del producto). Describe las **entidades** del negocio con sus responsabilidades, propiedades y reglas,
las **enumeraciones**, las **relaciones** entre entidades y los **invariantes globales**.

> **Idioma:** la documentación (responsabilidades, descripciones y reglas) se escribe en **español**;
> los **nombres de clases, propiedades, enumeraciones y miembros de enumeración** se escriben en
> **inglés**, porque son identificadores de código.
>
> Nota de modelado: toda cuenta es un **`User`** con un **perfil especializado** según su tipo
> (**`Athlete`** o **`School`**). Se modela así porque las relaciones (amistad, bloqueo,
> notificaciones) se establecen entre cuentas con independencia de su tipo. El dominio de ejemplo de
> la plantilla (partidos y árbitros) **no** forma parte de este modelo.

---

## User

Representa una cuenta registrada en el sistema, sea cual sea su tipo.

### Responsabilidades
- Representar la cuenta y su identidad externa.
- Determinar el tipo de cuenta y su estado.
- Servir de extremo de todas las relaciones (amistad, bloqueo, asistencia).

### Propiedades
- `Id`
- `Email`
- `Nickname`
- `AccountType`
- `AccountStatus`
- `IdentityProvider` (Google o Facebook)
- `ExternalIdentityId`
- `PreferredLanguage`
- `CreatedAt`
- `DeletedAt`

### Reglas de negocio
- El `Id` de la cuenta es el identificador del usuario en Keycloak: la cuenta y su perfil especializado
  comparten ese identificador.
- El `Email` proviene del proveedor de identidad; no hay credenciales propias.
- El `Nickname` es **único**, **inmutable** y **solo admite letras y números**. La cuenta nace **sin
  nick** —la crea el alta desde Keycloak— y es el usuario quien lo elige al completar su perfil: hasta
  entonces el perfil está incompleto.
- El `AccountType` se elige al crear la cuenta y es **inmutable**.
- Solo pueden registrarse **mayores de edad**.
- El `AccountStatus` puede ser activo, pendiente de validación (solo escuelas) o eliminado.
- La **eliminación es suave**: oculta todo el contenido a los demás y conserva **todo** internamente.
  La restauración la realiza el técnico.
- Un usuario existe siempre con uno de sus dos perfiles especializados, nunca con ambos.

---

## Athlete

Perfil especializado del usuario de tipo deportista.

### Responsabilidades
- Declarar los deportes que practica y las maniobras que domina.
- Recibir puntuaciones sobre sus maniobras.
- Declarar y mantener sus asistencias a escuelas.
- Publicar contenido y relacionarse con otros usuarios.

### Propiedades
- `Id` (el mismo que el de su `User`)
- `FirstName`
- `LastName`
- `DateOfBirth`
- `Country`
- `City`
- `ProfilePictureUrl`
- `Biography`
- `PracticedSports` (colección de `Sport`)
- `DeclaredSkills` (colección de `AthleteSkill`)
- `SchoolAttendances` (colección de `SchoolAttendance`)

### Reglas de negocio
- La `DateOfBirth` debe acreditar la **mayoría de edad**; la edad se muestra **calculada**.
- Un deportista puede practicar **varios deportes**, seleccionados del catálogo.
- **Solo puede declarar maniobras de los deportes que practica.**
- Su perfil es **privado**: solo lo ven sus **amigos** y las **escuelas con vínculo de asistencia
  aceptado**. Cualquier otro usuario ve únicamente **foto, nombre y la opción de enviar solicitud**.
- En su perfil se muestran sus últimas publicaciones mezclando sus deportes, con filtro por deporte, y
  sus maniobras organizadas por deporte.
- Muestra su lista de escuelas asistidas; las no aceptadas solo las ve él, como **"pendiente de
  validación"**.

---

## School

Perfil especializado del usuario de tipo escuela.

### Responsabilidades
- Ofrecer su información pública, su deporte y sus servicios.
- Validar y puntuar a los deportistas que han asistido a ella.
- Mantener la lista de deportistas que han asistido.

### Propiedades
- `Id` (el mismo que el de su `User`)
- `Name`
- `LogoUrl`
- `SportId` (exactamente uno)
- `Description`
- `Country`
- `City`
- `ContactEmail`
- `Phone`
- `Website`
- `Services` (colección de `SchoolService`)
- `SchoolAttendances` (colección de `SchoolAttendance`)

### Reglas de negocio
- Está asociada a **un único deporte**.
- Al registrarse queda **pendiente de validación**; el técnico la valida.
- Su perfil es **público** para los usuarios autenticados.
- Solo puede **puntuar maniobras de deportistas con vínculo de asistencia aceptado**.
- La lista de asistentes se compone de los vínculos **aceptados**, y se muestra a los usuarios
  autenticados con **nombre y foto**.
- Sus publicaciones se asocian **siempre a su propio deporte**.
- **No** declara maniobras ni recibe puntuaciones.

---

## Sport

Elemento de catálogo que agrupa maniobras.

### Responsabilidades
- Identificar un deporte dentro del catálogo del sistema.

### Propiedades
- `Id`
- `Name`
- `IsActive`

### Reglas de negocio
- El catálogo lo **mantiene el técnico** y es **ampliable**.
- Los deportistas seleccionan de aquí los deportes que practican; una escuela se asocia a **uno solo**.

---

## Skill

Elemento de catálogo que representa una habilidad o maniobra concreta de un deporte.

### Responsabilidades
- Identificar una maniobra puntuable dentro de un deporte.

### Propiedades
- `Id`
- `SportId`
- `Name`
- `IsActive`

### Reglas de negocio
- Toda maniobra pertenece a **un deporte**.
- El catálogo lo **mantiene el técnico** y es **ampliable**.
- Solo pueden declararse maniobras de deportes que el deportista practica.

---

## AthleteSkill

Declaración de una maniobra por parte de un deportista.

### Responsabilidades
- Representar que un deportista domina una maniobra concreta.
- Agrupar las puntuaciones recibidas sobre esa maniobra.

### Propiedades
- `Id`
- `AthleteId`
- `SkillId`
- `IsEnabled`
- `DeclaredAt`

### Reglas de negocio
- Solo puede declararse una maniobra del catálogo correspondiente a un deporte que el deportista
  practique.
- Un deportista no puede declarar dos veces la misma maniobra.
- El deportista puede **deshabilitarla** (`IsEnabled = false`, quitarla de su lista) y **volver a
  habilitarla**.
- **Deshabilitar no borra las puntuaciones**: al rehabilitarla vuelve a mostrarse con el histórico.
- El valor mostrado de una maniobra es la **media** de sus puntuaciones; si no tiene ninguna, se
  muestra con las **5 estrellas vacías**.

---

## Rating

Valoración que un usuario autorizado emite sobre una maniobra declarada.

### Responsabilidades
- Registrar la valoración de una maniobra declarada por parte de un usuario.

### Propiedades
- `Id`
- `AthleteSkillId`
- `AuthorId`
- `Value` (entero de 1 a 5)
- `CreatedAt`
- `UpdatedAt`

### Reglas de negocio
- Solo se puntúan **maniobras declaradas**; **no existe puntuación de un nivel global**.
- El `Value` es un entero entre **1 y 5** (estrellas).
- Un autor solo puede tener **una puntuación por maniobra declarada**; puede **editarla** o
  **borrarla**.
- Está permitida la **autopuntuación**.
- Puede puntuar:
  - un **deportista amigo** del deportista puntuado,
  - una **escuela con vínculo de asistencia aceptado** con el deportista puntuado.
- El autor de la puntuación debe mantener la relación que la habilita; al perderse la relación
  (deshacer la amistad, retirar la asistencia o bloquear), **la puntuación se borra**.
- El usuario puntuado **no puede ocultar ni borrar** las puntuaciones ajenas.
- La puntuación de una **escuela vale lo mismo** que la de un deportista.

---

## FriendRequest

Petición para establecer una amistad.

### Responsabilidades
- Registrar la voluntad de un usuario de ser amigo de otro y su resolución.

### Propiedades
- `Id`
- `RequesterId`
- `ReceiverId`
- `Status`
- `RequestedAt`
- `ResolvedAt`

### Reglas de negocio
- Solo puede darse entre **deportista↔deportista** o **deportista↔escuela**; **nunca entre dos
  escuelas**.
- No puede emitirse si existe un **bloqueo** entre los usuarios.
- No puede existir más de una solicitud **pendiente** para el mismo par.
- Al **aceptarse**, se crea la `Friendship` correspondiente.
- El **rechazo es silencioso**: el solicitante no recibe aviso y puede **volver a solicitarla**.
- El solicitante puede cancelar su propia solicitud.

---

## Friendship

Relación simétrica y voluntaria entre dos cuentas.

### Responsabilidades
- Representar el vínculo de amistad y habilitar el acceso mutuo a los perfiles.

### Propiedades
- `Id`
- `UserAId`
- `UserBId`
- `StartedAt`

### Reglas de negocio
- Es **simétrica**: el par de usuarios es único, sin orden ni jerarquía.
- Solo existe entre deportistas, o entre deportista y escuela.
- Cualquiera de las partes puede **deshacerla** en cualquier momento, sin consentimiento de la otra.
- Da acceso al **perfil completo** del deportista y habilita **puntuar sus maniobras**.
- Al deshacerse, **se borran las puntuaciones** emitidas entre ambos.
- Se destruye si cualquiera de las partes **bloquea** a la otra.

---

## Block

Relación por la que dos usuarios dejan de verse.

### Responsabilidades
- Impedir toda interacción y visibilidad entre dos usuarios.

### Propiedades
- `Id`
- `BlockerId`
- `BlockedId`
- `BlockedAt`

### Reglas de negocio
- Es **mutuo en efecto**: ninguno de los dos ve al otro.
- Es **permanente e irreversible**: no existe desbloqueo.
- **Rompe la amistad** si existía y **borra las puntuaciones** entre ambos.
- **Retira el vínculo de asistencia** si existía.
- Excluye a la otra parte de las **búsquedas** y de las **listas de asistentes**.
- El usuario bloqueado **no puede enviar solicitudes** de amistad.

---

## SchoolAttendance

Relación entre un deportista y una escuela que acredita que el deportista ha asistido a ella.

### Responsabilidades
- Acreditar la asistencia de un deportista a una escuela.
- Habilitar a la escuela la visibilidad del perfil del deportista y la puntuación de sus maniobras.
- Alimentar la lista de asistentes de la escuela.

### Propiedades
- `Id`
- `AthleteId`
- `SchoolId`
- `Status`
- `InitiatedById`
- `RequestedAt`
- `ResolvedAt`

### Reglas de negocio
- Puede **iniciarlo cualquiera de las dos partes**: la escuela añade al deportista, o el deportista
  declara que ha asistido a la escuela.
- La otra parte **acepta o rechaza**; el **rechazo es silencioso** y la propuesta puede **reenviarse**:
  volver a solicitar devuelve **la misma fila** a pendiente, no inserta otra, así que **no se conserva
  histórico** de los intentos anteriores.
- El deportista puede declarar asistencia a una escuela **aunque no practique el deporte de esa
  escuela**.
- Mientras está **aceptado**, la escuela ve el perfil del deportista **como si fueran amigos** y puede
  **puntuarle**.
- Mientras está **aceptado**, la escuela ve, aunque no sean amigos, **a qué otras escuelas ha asistido**
  el deportista.
- Cualquiera de las dos partes puede **retirar** el vínculo; al retirarlo, la asistencia desaparece, el
  deportista sale de la lista de asistentes y **se borran las puntuaciones** que esa escuela había dado.
- **Una sola fila por par** deportista-escuela: la unicidad no distingue estados, de modo que volver a
  solicitar reutiliza la fila en lugar de crear otra.
- El deportista ve sus vínculos no aceptados como **"pendiente de validación"**; los demás usuarios
  solo ven los **aceptados**.
- La **amistad** y la **asistencia** son listas independientes, aunque puedan coincidir.

---

## Post

Contenido publicado por un usuario.

### Responsabilidades
- Registrar una aportación formada por un elemento multimedia, un comentario y un deporte.
- Recibir comentarios y "me gusta".

### Propiedades
- `Id`
- `AuthorId`
- `SportId`
- `MediaType` (foto o vídeo)
- `MediaUrl`
- `Caption`
- `PublishedAt`

### Reglas de negocio
- Una publicación lleva **un único elemento multimedia**: **una foto o un vídeo**, nunca ambos ni
  varios.
- Está asociada a **un deporte**; nunca a una maniobra.
- Si el autor es una **escuela**, el deporte es **siempre el suyo**.
- El autor puede **borrar** la publicación; **no puede editarla**.
- El acceso sigue las reglas de visibilidad del perfil del autor: las de un deportista se ven entre
  amigos y escuelas con asistencia; las de una escuela son públicas para usuarios autenticados.

---

## Comment

Aportación de texto de un usuario sobre una publicación.

### Responsabilidades
- Registrar la opinión de un usuario sobre una publicación.

### Propiedades
- `Id`
- `PostId`
- `AuthorId`
- `Text`
- `CreatedAt`

### Reglas de negocio
- Solo puede comentar quien puede ver la publicación: en el perfil de un deportista, los **deportistas
  amigos** y las **escuelas con vínculo de asistencia**; en el de una escuela, **cualquier usuario
  autenticado**.
- Un autor **solo puede borrar su propio comentario**; **nadie** puede borrar comentarios ajenos, ni el
  autor de la publicación.
- Un comentario que no gusta se **denuncia**; el técnico puede borrarlo.
- Mientras una denuncia está pendiente, el comentario **deja de mostrarse**.

---

## Like

Reacción de un usuario sobre una publicación.

### Responsabilidades
- Registrar el "me gusta" de un usuario sobre una publicación.

### Propiedades
- `Id`
- `PostId`
- `UserId`
- `CreatedAt`

### Reglas de negocio
- Un usuario solo puede dar **un "me gusta" por publicación** y puede retirarlo.
- Solo puede reaccionar quien puede ver la publicación, con las mismas reglas que el comentario.

---

## Report

Aviso de contenido inapropiado dirigido al técnico.

### Responsabilidades
- Registrar la denuncia de un contenido y su categoría.
- Ocultar el contenido denunciado mientras está pendiente.

### Propiedades
- `Id`
- `ContentType` (comentario o publicación)
- `ContentId`
- `ReporterId`
- `Category`
- `FreeText`
- `Status`
- `CreatedAt`

### Reglas de negocio
- Se pueden denunciar **comentarios y publicaciones**.
- La categoría es obligatoria y una de: **Obsceno, Racista, Discriminatorio, Otros**; admite **texto
  libre**.
- Mientras está **pendiente**, el contenido denunciado **deja de mostrarse**.
- La resuelve el **técnico**, que recibe una **notificación**.

---

## SchoolService

Servicio ofrecido por una escuela en su sección comercial.

### Responsabilidades
- Describir un servicio de la escuela y su precio.

### Propiedades
- `Id`
- `SchoolId`
- `Name`
- `Description`
- `Price`
- `Currency`

### Reglas de negocio
- Se compone de **nombre, descripción, precio y moneda**.
- La **moneda** la elige la escuela.
- Se **edita libremente** y **no hay histórico**.
- **No** se puede contratar, reservar ni pagar desde la aplicación: es informativo.

---

## Notification

Aviso generado por un evento del sistema para un usuario.

### Responsabilidades
- Informar al usuario de los eventos que le afectan.

### Propiedades
- `Id`
- `RecipientId`
- `Type`
- `ContentReference`
- `Channel`
- `IsRead`
- `CreatedAt`

### Reglas de negocio
- Se genera para los eventos: solicitud de amistad **recibida** y **aceptada**, asistencia
  **solicitada**, **aceptada** y **rechazada**, nuevo **comentario**, nuevo **"me gusta"**, nueva
  **puntuación** recibida y **denuncia** dirigida al técnico.
- El **rechazo de una amistad** y el **rechazo de una asistencia** son **silenciosos**: no generan
  notificación.
- Los canales son **aplicación, email y push**; en el alcance actual solo se implementa el de
  **aplicación**.
- Respeta las **preferencias del usuario** por canal.

---

## NotificationPreference

Configuración del usuario sobre los canales de notificación.

### Responsabilidades
- Registrar qué canales de notificación tiene activos o desactivados el usuario.

### Propiedades
- `Id`
- `UserId`
- `Channel`
- `IsEnabled`

### Reglas de negocio
- Existe una preferencia por cada **canal** (aplicación, email y push).
- El usuario puede **desactivar** de forma independiente el canal de aplicación, el de email o el de
  push.

---

## Enumeraciones

### AccountType
- `Athlete`
- `School`

### AccountStatus
- `Active`
- `PendingValidation` (solo escuelas)
- `Deleted`

### RequestStatus
- `Pending`
- `Accepted`
- `Rejected`
- `Cancelled`

### AttendanceStatus
- `Pending`
- `Accepted`
- `Rejected`
- `Withdrawn`

### ReportCategory
- `Obscene`
- `Racist`
- `Discriminatory`
- `Other`

### ReportStatus
- `Pending`
- `Resolved`

### ReportableContentType
- `Comment`
- `Post`

### PostMediaType
- `Photo`
- `Video`

### NotificationType
- `FriendRequestReceived`
- `FriendRequestAccepted`
- `AttendanceRequested`
- `AttendanceAccepted`
- `AttendanceRejected`
- `NewComment`
- `NewLike`
- `NewRating`
- `ReportReceived`

### NotificationChannel
- `InApp`
- `Email`
- `Push`

---

## Relaciones entre entidades

| Relación | Cardinalidad | Notas |
|---|---|---|
| `User` — `Athlete` | 1 a 1 | Un usuario de tipo deportista tiene un perfil de deportista. |
| `User` — `School` | 1 a 1 | Un usuario de tipo escuela tiene un perfil de escuela. |
| `School` — `Sport` | 1 a 1 | Una escuela tiene exactamente un deporte. |
| `Skill` — `Sport` | N a 1 | Toda maniobra pertenece a un deporte. |
| `Athlete` — `Sport` | N a M | Un deportista practica varios deportes. |
| `Athlete` — `AthleteSkill` | 1 a N | Maniobras declaradas por el deportista. |
| `AthleteSkill` — `Skill` | N a 1 | Cada declaración referencia una maniobra del catálogo. |
| `AthleteSkill` — `Rating` | 1 a N | Puntuaciones recibidas por la maniobra declarada. |
| `User` — `Rating` | 1 a N | Puntuaciones emitidas como autor. |
| `User` — `FriendRequest` | 1 a N | Como solicitante y como receptor. |
| `User` — `Friendship` | 1 a N | Relación simétrica por par único. |
| `User` — `Block` | 1 a N | Como bloqueador y como bloqueado. |
| `Athlete`/`School` — `SchoolAttendance` | 1 a N | Vínculo único activo por par deportista-escuela. |
| `User` — `Post` | 1 a N | Autor de la publicación. |
| `Post` — `Comment` | 1 a N | Comentarios recibidos. |
| `Post` — `Like` | 1 a N | Reacciones recibidas. |
| `Post`/`Comment` — `Report` | 1 a N | El contenido denunciado. |
| `School` — `SchoolService` | 1 a N | Servicios ofrecidos. |
| `User` — `Notification` | 1 a N | Notificaciones recibidas. |
| `User` — `NotificationPreference` | 1 a N | Una por canal. |

---

## Invariantes globales

1. Toda cuenta es un `User` con **un solo tipo**, inmutable, y un **perfil especializado**.
2. El `Nickname` es **único, inmutable y solo alfanumérico**, y se fija **una sola vez** al completar el
   perfil: la cuenta nace sin él.
3. Solo pueden registrarse **mayores de edad**.
4. Una `School` tiene **exactamente un** deporte.
5. Un `Athlete` **solo declara maniobras** de los deportes que practica.
6. Un `AthleteSkill` puede estar habilitado o deshabilitado, y **deshabilitarlo no borra sus
   puntuaciones**.
7. Una `Rating` solo existe si el autor mantiene la relación habilitante (**amistad** o
   **asistencia aceptada**); al perderse, **se borra**.
8. Un autor tiene **una sola puntuación por maniobra declarada**, editable y borrable por él.
9. Una `Friendship` es **simétrica** y **no puede existir entre dos escuelas**.
10. Un `Block` es **mutuo, permanente y sin desbloqueo**, y arrastra amistad, puntuaciones y
    asistencia.
11. Un `SchoolAttendance` es **único y activo por par** deportista-escuela; su aceptación da a la
    escuela la visibilidad del perfil del deportista y el derecho a puntuarle.
12. Un `Post` lleva **un único** elemento multimedia (foto **o** vídeo) y **un** deporte; los de
    escuela, siempre el suyo.
13. Los `Post` **no se editan**; se **borran**.
14. Un `Comment` solo lo puede borrar **su autor**; los ajenos se **denuncian**.
15. Un `Report` **oculta** el contenido mientras está pendiente y lo resuelve el **técnico**.
16. Los **rechazos** de amistad y de asistencia son **silenciosos**.
17. Las `Notification` respetan las **preferencias por canal** del usuario.
18. Ninguna entidad de negocio es accesible sin un **usuario autenticado**.
