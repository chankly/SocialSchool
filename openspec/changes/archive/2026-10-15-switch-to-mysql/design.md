# Design: Cambiar la API a MySQL

## Context

Ver `proposal.md` — Why. Estado que condiciona el enfoque, verificado en el código:

- `PersistenceServiceExtensions.AddDbContextPool` usa
  `options.UseSqlServer(cadena, opt => { MigrationsAssembly; EnableRetryOnFailure(3, 5 s, null); })`.
- Los paquetes de Persistence son EF Core **10.0.9** en `Design`, `Tools`, `Proxies` y `SqlServer`, todos
  alineados, así que el cambio de proveedor no obliga a mover ninguna versión.
- La única migración es `20261010171726_InitialCreate` (+ su `.Designer.cs` y el snapshot), **nunca
  aplicada** a ninguna base de datos.
- `SchoolAttendanceConfiguration` declara un índice único con `HasFilter("[Status] IN (0, 1)")`, que es
  SQL Server específico.
- `SchoolAttendance` tiene tres transiciones **terminales**: `Accept`, `Reject` y `Withdraw` cierran el
  estado con `EnsurePending()` y **no hay vuelta atrás**, así que volver a solicitar implica insertar una
  fila nueva. Eso es lo que obligaba al índice filtrado.
- La infraestructura ya despliega MySQL con la colación `utf8mb4_0900_ai_ci` (insensible a mayúsculas) y
  compone la cadena de conexión en formato MySQL, con la base `api`.

## Goals / Non-Goals

**Goals:**

- Que la API pueda hablar con la base de datos que la infraestructura despliega.
- Que el esquema deje de depender del motor: sin índices filtrados, sin construcciones de SQL Server, sin
  SQL escrito a mano en la migración.
- Dejar el vínculo de asistencia expresado con dos campos.

**Non-Goals:**

- Escribir casos de uso o endpoints de asistencia: el cambio de modelo es de esquema y de entidad.
- Tocar Keycloak ni su base de datos.
- Instalar o provisionar un MySQL local: no hay ninguno disponible.
- Alinear el resto de desajustes de paquetes del repositorio (Serilog, JwtBearer, Swashbuckle).

## Decisions

**1. El proveedor es `MySql.EntityFrameworkCore` 10.0.9 (Oracle).** Es el único alineado con EF Core 10: la
solución entera está en 10.0.9 y Pomelo se queda en EF Core 9, así que usarlo obligaría a **bajar de
versión los cuatro proyectos**. *Trade-off aceptado por el usuario*: la licencia del conector es **GPL-2.0
con la excepción FOSS universal** y arrastra `MySql.Data`.

**2. Una fila por par en el vínculo de asistencia.** El índice único pasa a ser de **dos campos**
(`AthleteId`, `SchoolId`), sin filtro y sin columna extra. Era necesario elegir entre tres caminos, y este
es el que responde a la pregunta del usuario ("¿por qué no un índice único de dos campos?"):
*Alternativa descartada, mantener histórico*: una columna generada (`CASE WHEN status IN (0, 1) THEN
CONCAT(...) END`) con índice único encima, que conservaría los intentos rechazados y retirados.
*Alternativa descartada, borrar la fila al rechazar o retirar*: también deja el índice en dos campos, pero
haría desaparecer `Rejected` y `Withdrawn` como estados persistidos y tocaría el ciclo de vida documentado.
**Consecuencias de la elegida, dichas en voz alta**: la garantía es **más fuerte** (una fila por par,
siempre, no solo entre las activas), los estados documentados siguen siendo los mismos, y se **pierde el
rastro de intentos anteriores** — algo que hoy nadie consume, porque el rechazo es silencioso por diseño y
no hay ningún endpoint que muestre ese histórico. Efecto secundario: si el vínculo se retira, la escuela no
conserva memoria de que ese deportista pasó por ella; la lista de asistidos se compone de los vínculos
**aceptados**, así que el comportamiento visible no cambia.

**3. `RequestAgain()` como transición explícita.** Volver a solicitar devuelve **la misma fila** a
`Pending`, actualizando quién la inicia y las fechas. Se permite solo desde `Rejected` o `Withdrawn`; desde
`Pending` o `Accepted` sigue siendo un error, porque ya está viva y `EnsurePending()` ya lo cubre para el
resto de transiciones.

**4. Se reemplaza `InitialCreate` en vez de añadir una migración encima.** La existente es SQL Server
específica y **nunca se ha aplicado**, así que conservarla solo arrastraría SQL de un motor que ya no vamos
a usar y dejaría dos migraciones que no describen el esquema real.

**5. La colación se queda como está** (`utf8mb4_0900_ai_ci`, insensible a mayúsculas), que es lo que el
usuario decidió: `Peter` y `peter` colisionan en el nick. El cambio no toca la base: la consecuencia ya
estaba asumida en la infraestructura.

**6. La cadena de conexión no cambia de sitio**, solo de formato: sigue leyéndose de
`ConnectionStrings:MyCompany`, así que ni la composición ni la configuración de Terraform (que ya compone
una cadena MySQL) necesitan cambios.

**7. Si el proveedor no soporta `EnableRetryOnFailure`, se quita.** Es una opción heredada de SQL Server;
si no existe en el proveedor de Oracle, se elimina y se deja un comentario, en vez de inventar un
equivalente.

## Risks / Trade-offs

- **Que el proveedor de Oracle no soporte `dotnet ef migrations`** → es la primera comprobación del cambio;
  si falla, hay que replantear con el usuario (Pomelo bajando EF Core a 9, o quedarse en SQL Server).
- **Sin MySQL a la que aplicar** → el SQL se revisa leyendo `migrations script`; `database update` es paso
  del usuario, igual que en el cambio de infraestructura.
- **Mapeo de tipos que el proveedor decide** (por ejemplo `Guid` como `char(36)` o `binary(16)`, o la
  precisión de `DateTime`) → se revisan en el script SQL generado y se documentan si sorprenden.
- **La colación insensible afecta a más que el nick** (nombres del catálogo de deportes y maniobras, por
  ejemplo) → se acepta como consecuencia de la decisión, y se deja escrito en `QWEN.md`.
- **Sin MySQL local no se puede probar el arranque real de la API** → el techo es compilar, generar la
  migración y pasar los tests de integración, que no tocan la base de datos.

## Migration Plan

1. Comprobar que el proveedor reconoce el modelo (`dotnet ef dbcontext info`) antes de tocar nada más.
2. Cambiar paquete, `UseMySQL` y el índice; añadir `RequestAgain()`.
3. Borrar y regenerar `InitialCreate`; revisar el SQL.
4. Ajustar las cadenas de ejemplo y la documentación.
5. **Reversión**: el cambio es de código; volver atrás es revertir el commit. Como no se ha aplicado a
   ninguna base de datos, no hay datos que restaurar.

## Open Questions

- Dónde se prueba el arranque real de la API contra MySQL mientras no haya una instancia: el entorno local
  del repositorio (`devops/keycloak-webhooks/compose.yaml`) no tiene base de datos para la API y el usuario
  lo considera solo para pruebas.
- Si conviene añadir el motor de base de datos a las integraciones de `project.md` §11, donde hoy no
  aparece.
