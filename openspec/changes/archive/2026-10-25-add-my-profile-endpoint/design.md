# Design: Consultar mi perfil

## Context

Ver `proposal.md` — Why. Lo que condiciona el enfoque, verificado en el código:

- `ICurrentUser` expone **solo** `ExternalId` (el `sub`) e `IsAuthenticated`. No hay ningún `Claim(...)`
  de tipo de cuenta en el código: los únicos usos de claims son los tests y el resto de plantilla
  (`RequireClaim("")` en `ServicesCollectionExtensions`).
- El dominio ya lo tiene todo: `User.AccountType` (inmutable), `Athlete` y `School` **comparten el `Id`**
  con su `User`, y las tres se crean con factory estático.
- `IBaseRepository<T>` solo ofrece `GetById`, `GetByIdReadOnly`, `ExistsAsync` y las escrituras: no hay
  forma de buscar por identidad externa, y `UserConfiguration` **no** tiene índice sobre
  `ExternalIdentityId` (solo el nick es único).
- `BaseRepository<T>` consulta con `_dbContext.Set<T>()`: el `MyCompanyDbContext` **no declara `DbSet`s**.
- **El lazy loading está apagado** (`UseLazyLoadingProxies` no se invoca), así que `Athlete.PracticedSports`
  solo llega cargado si la consulta la incluye.
- Ya hay un caso de uso (`ApplyMigrationCommand` + handler) que marca el patrón: mensaje, handler con
  constructor explícito, `ValueTask` y `Result`; y un controlador con constructor primario,
  `_mediator.Send(...)` y `Problem(detail: ...)`.
- El webhook que materializa la cuenta está **esbozado** (`WebHooksController` solo escribe el payload en
  consola), así que en la práctica hoy nadie crea la fila `User`.

## Goals / Non-Goals

**Goals:**

- Que la web sepa, con una sola llamada, si el usuario ya tiene perfil y cuál es su contenido.
- Que el caso "sin cuenta" sea un estado normal (`204`), no un error.
- Que el endpoint no dependa de nada que no exista todavía en Keycloak.

**Non-Goals:**

- Crear o editar el perfil (el formulario y su endpoint vienen después).
- Poner el tipo de cuenta en el token (mapper de Keycloak): lo necesita la web, no este endpoint.
- Devolver colecciones hijas (maniobras, asistencias, servicios): no forman parte del formulario.

## Decisions

**1. El tipo de cuenta se lee de la base, no del token.** El `200` implica que existen las filas, así que
`User.AccountType` responde sin ambigüedad; el `204` no lleva cuerpo y por tanto no necesita el tipo. Un
claim del token no aportaría nada aquí y ataría el endpoint a un mapper que hoy no existe.
*Alternativa descartada*: extender `ICurrentUser` para leer un claim de tipo de cuenta.

**2. La búsqueda es `sub` → `User` → `Athlete`/`School` por el mismo `Id`.** Es la única cadena posible:
el token no conoce nuestro `Guid`, y `Athlete`/`School` no guardan identidad externa. Requiere un método
nuevo en `IUserRepository` porque el repositorio base no busca por campos.

**3. El deportista se carga con sus deportes en una consulta con `Include`.** `IAthleteRepository` gana
un método que devuelve el deportista con `PracticedSports`; sin él la colección llegaría vacía y el JSON
mentiría. La escuela **no** necesita método nuevo: sus servicios no se devuelven y `GetByIdReadOnly`
basta. *Alternativa descartada*: cargar los deportes en una segunda consulta por `Sport`, que obligaría a
reconstruir la relación fuera del agregado.

**4. `200` con cuerpo, `204` sin cuerpo, y nada de `404`.** El spec de autenticación ya trata "sin cuenta"
como un estado normal, y `204` deja el flujo del cliente en un solo `if` sin inventar códigos de error.
El `Result` del handler admite `null` (`Result<MyProfileResponse?>`): "no hay perfil" es un éxito sin
valor, no un fallo, así que el controlador solo distingue `IsFailure` (problema) y `Value is null`.

**5. Una sola forma de respuesta, con dos bloques opcionales.** `accountType` dice cuál mirar y solo uno
de `athlete`/`school` viene relleno; el handler lo garantiza al construir. *Alternativa descartada*: una
jerarquía polimórfica, que en System.Text.Json exige atributos de polimorfismo y complica el contrato de
Swagger para dos formas.

**6. El bloque de la escuela lleva `sportId`, no el nombre del deporte.** La web necesita el catálogo de
todos modos para pintar el selector, así que resolver el nombre aquí sería una consulta más por una
comodidad que el cliente ya tiene. El deportista sí lleva los deportes con nombre porque forman parte de
su perfil como colección.

**7. Índice único sobre `Users.ExternalIdentityId`.** Es la clave de búsqueda de un endpoint que la web
llama al arrancar, y "una cuenta por identidad de Keycloak" es una invariante del negocio, no solo una
optimización. Es un cambio de esquema: necesita migración y `database update`.

**8. El handler es `public sealed`, como el que ya existe.** `QWEN.md` pide `internal sealed`, pero el
único handler del repositorio (`ApplyMigrationCommandHandler`) es `public class` y Mediator lo despacha sin
problema; seguir el código que hay evita dos estilos en la misma capa y permite instanciar el handler en
los tests sin añadir `InternalsVisibleTo`. El mensaje sí es `sealed record`, que es lo que importa para
Mediator.

**9. Se actualiza `project.md` §4.** El spec de producto describe un alta que ya no es la que se va a
construir; el de autenticación ya asume la nueva. Se alinean los dos documentos en lugar de dejar la
contradicción.

## Risks / Trade-offs

- **No se puede probar contra la base desde aquí**: no hay MySQL disponible y el webhook que crea la
  cuenta no está construido. La lógica se verifica con tests de handler y repositorios simulados; el
  `200` real es prueba del usuario.
- **Sin webhook, todo es `204`**: hasta que el webhook materialice la cuenta, cualquier llamada legítima
  devuelve "no hay perfil". Es coherente, pero conviene saberlo antes de interpretarlo como un fallo.
- **Carrera en el registro**: entre el alta en Keycloak y la llegada del evento, el endpoint responde
  `204` igual que si el perfil faltase. Para el cliente el comportamiento es el mismo, así que se acepta.
- **El índice único falla si ya hubiera duplicados**: hoy no puede haberlos (nadie crea cuentas), pero en
  un entorno con datos habría que comprobarlo antes de aplicar la migración.
- **Un perfil de una cuenta eliminada devuelve `200`**: la cuenta existe y el perfil también; el
  `accountStatus` del cuerpo lo dice. Si el producto quisiera otra cosa para una cuenta eliminada, es una
  regla nueva, no una consecuencia de este endpoint.

## Migration Plan

1. Aplicar la migración del índice único (`dotnet ef database update`, paso del usuario).
2. Sin datos que migrar: el endpoint es de solo lectura.
3. **Reversión**: quitar el endpoint y la migración; no deja estado.

## Open Questions

- Qué debe hacer la web si el webhook no llega nunca y por tanto el `204` no se resuelve con un
  formulario que pueda crear la cuenta: es el endpoint de alta, fuera de este cambio.
- Si una cuenta eliminada debería responder `204` en lugar de `200` con `accountStatus: Deleted`.
- Los nombres de los deportes en el perfil de la escuela, si la web acaba necesitándolos (hoy resuelve el
  `sportId` contra el catálogo).
