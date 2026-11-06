# Amistades

## Purpose

Permite que dos cuentas se hagan amigas con consentimiento: enviar una solicitud, aceptarla o rechazarla,
cancelarla y deshacer la amistad, con las listas que el cliente necesita para mostrarlo.

## ADDED Requirements

### Requirement: Listas de amistades

La API SHALL exponer `GET /api/friends` y SHALL devolver los amigos, las solicitudes recibidas, las
enviadas y las cuentas a las que se puede invitar. SHALL excluir de las cuentas invitables a uno mismo, a
los amigos y a quien ya tiene una solicitud pendiente en cualquier dirección. SHALL requerir usuario
autenticado.

#### Scenario: Resumen de amistades
- **WHEN** un usuario autenticado consulta sus amistades
- **THEN** recibe amigos, solicitudes recibidas y enviadas, y un máximo de veinte cuentas invitables

#### Scenario: Nadie se propone a sí mismo ni repite
- **WHEN** se calculan las cuentas invitables
- **THEN** no aparece el propio usuario, ni sus amigos, ni quien tiene una solicitud pendiente con él

### Requirement: Enviar una solicitud de amistad

La API SHALL permitir enviar una solicitud a otra cuenta y SHALL rechazarla cuando el destinatario sea uno
mismo, cuando ambas cuentas sean escuelas, cuando ya exista amistad o cuando ya haya una solicitud pendiente
entre ellas. La solicitud SHALL nacer pendiente y SHALL NOT generar amistad hasta que se acepte.

#### Scenario: Solicitud correcta
- **WHEN** un deportista envía una solicitud a otro deportista o a una escuela, sin relación previa
- **THEN** la solicitud queda pendiente y la API devuelve su identificador

#### Scenario: Dos escuelas no pueden ser amigas
- **WHEN** una escuela envía una solicitud a otra escuela
- **THEN** la API responde `400` y no crea nada

#### Scenario: Solicitud duplicada
- **WHEN** ya hay una solicitud pendiente entre las dos cuentas, en cualquier dirección
- **THEN** la API responde `400` y no crea otra

### Requirement: Resolver una solicitud

Aceptar una solicitud SHALL corresponder solo al destinatario, rechazarla solo al destinatario y cancelarla
solo al emisor, y solo mientras esté pendiente. Aceptar SHALL crear la amistad. El rechazo SHALL ser
silencioso y SHALL NOT generar aviso ni impedir una solicitud futura.

#### Scenario: Aceptar crea la amistad
- **WHEN** el destinatario acepta una solicitud pendiente
- **THEN** la amistad queda creada y la solicitud resuelta

#### Scenario: Rechazo silencioso
- **WHEN** el destinatario rechaza la solicitud
- **THEN** la solicitud queda rechazada, no se crea amistad y no se genera ningún aviso

#### Scenario: Tercero ajeno
- **WHEN** una cuenta distinta de las dos implicadas intenta aceptar, rechazar o cancelar
- **THEN** la API responde `400` y no cambia nada

### Requirement: Deshacer una amistad

Cualquiera de las dos partes SHALL poder deshacer la amistad, y la API SHALL dejar de mostrarla en las
listas de ambas.

#### Scenario: Deshacer
- **WHEN** una de las dos partes deshace la amistad
- **THEN** la amistad desaparece para las dos
