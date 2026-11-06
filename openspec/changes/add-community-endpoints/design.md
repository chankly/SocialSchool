# Design: Escuelas y amistades reales

## Context

- El dominio ya tiene `Friendship`, `FriendRequest`, `School`, `SchoolService` y `SchoolAttendance`, con
  sus invariantes y factories. Hay un repositorio por agregado y `IBaseRepository<T>` solo ofrece lectura
  por identificador, así que las consultas de lista necesitan métodos nuevos.
- `AccountType` (Athlete/School) va en `User` y los perfiles comparten su `Id`. `ICurrentUser` da el `sub`.
- La política de denegación por defecto exige token en todo salvo `[AllowAnonymous]`.
- No hay endpoints de bloqueo ni de puntuación, así que esas reglas no se pueden ejercitar todavía.

## Decisions

**1. Las escuelas pendientes de validación se muestran en el directorio, con su estado.** Ocultarlas hasta
que un técnico las valide dejaría el directorio vacío en cualquier entorno, porque todavía no hay
herramienta de validación. El DTO lleva `accountStatus` y el cliente pinta "pendiente de validación".
*Alternativa descartada*: ocultarlas, que es más correcto a largo plazo pero deja la web sin escuelas.
Queda como pregunta abierta.

**2. La lista de asistentes muestra el nick y la foto**, no el nombre y los apellidos: el perfil del
deportista es privado para quien no es amigo ni escuela con asistencia, y el nick es el dato público.

**3. El bloqueo no filtra todavía.** No existe forma de crear un bloqueo, así que no se puede probar; las
listas no lo consultan. Se anota para cuando exista.

**4. "Descubribles" = usuarios activos que no son uno mismo, no son amigos y no tienen solicitud pendiente
en ninguna dirección**, como máximo 20 y con sus datos públicos. Incluye deportistas y escuelas; la regla
"nunca dos escuelas" se comprueba al enviar, no al listar.

**5. La amistad se crea en el handler al aceptar**, con `Friendship.Create(...)`, y se guarda junto con la
solicitud aceptada. La entidad `FriendRequest` solo cambia de estado.

**6. Errores de negocio con `Result` y `400` con detalle**; un identificador inexistente del recurso pedido
(la escuela) es `404`. El resto de reglas (uno mismo, ya amigos, pendiente duplicada, dos escuelas) devuelve
`400` con un mensaje que el cliente puede enseñar.

**7. Sin migración**: el esquema ya soporta todo. Las consultas nuevas van con `AsNoTracking` y `Include`
donde haga falta, porque la carga diferida está apagada.

## Risks / Trade-offs

- **Mostrar escuelas pendientes** es una decisión de producto tomada por necesidad del entorno; si el
  técnico valida, debería revisarse.
- **Sin filtrado por bloqueo** las listas pueden mostrar a alguien bloqueado en cuanto existan bloqueos.
- **Sin paginación**: los topes son fijos (50 escuelas, 20 descubribles). Con datos reales habrá que
  paginar.
- **Los tests no pueden tocar la base**: la verificación es de handler con repositorios simulados.

## Migration Plan

1. Desplegar la API con los endpoints nuevos.
2. Crear al menos una escuela real (registro por Keycloak + completar perfil) para ver el directorio.
3. **Reversión**: quitar los controladores; no hay estado nuevo.

## Open Questions

- Si las escuelas pendientes deben dejar de aparecer en el directorio al existir la validación del técnico.
- Paginación y orden por cercanía cuando haya volumen.
