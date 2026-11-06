# Perfil

## ADDED Requirements

### Requirement: Completar el perfil propio

El usuario autenticado cuya cuenta no tiene perfil SHALL poder crearlo con `POST /api/user/profile/me`,
enviando su nick y los datos del tipo de su cuenta. La API SHALL devolver `200 OK` con el perfil creado,
con la misma forma que la consulta. SHALL rechazar con `400 Bad Request` un cuerpo que no traiga
exactamente un bloque de perfil, un bloque que no corresponda al tipo de la cuenta, un nick que ya esté en
uso, una fecha de nacimiento que no acredite la mayoría de edad o un deporte que no exista. La API SHALL
fijar el nick en ese momento y SHALL usar el identificador de la cuenta para el perfil.

#### Scenario: Deportista completa su perfil
- **WHEN** un deportista sin perfil envía su nick, sus datos y los deportes que practica
- **THEN** la API crea el perfil, devuelve `200` con el bloque `athlete` y a partir de ahí la consulta del perfil contesta `200`

#### Scenario: Escuela completa su perfil
- **WHEN** una escuela sin perfil envía su nick, su nombre, su deporte y sus datos de contacto
- **THEN** la API crea el perfil, conserva el estado pendiente de validación y devuelve `200` con el bloque `school`

#### Scenario: Bloque que no corresponde al tipo de cuenta
- **WHEN** una cuenta de escuela envía el bloque de deportista, o al contrario
- **THEN** la API responde `400` y no crea nada

#### Scenario: Nick ocupado
- **WHEN** el nick enviado ya pertenece a otra cuenta
- **THEN** la API responde `400` con un detalle que identifica el nick, y no crea nada

#### Scenario: Deportista menor de edad
- **WHEN** la fecha de nacimiento enviada no acredita los 18 años
- **THEN** la API responde `400` y no crea nada

### Requirement: Catálogo de deportes para el formulario

La API SHALL exponer `GET /api/sports` para que el formulario pueda ofrecer los deportes del catálogo. La
respuesta SHALL incluir los deportes activos con su identificador y su nombre, y SHALL NOT incluir los
inactivos. La consulta SHALL requerir usuario autenticado.

#### Scenario: Catálogo disponible
- **WHEN** un usuario autenticado consulta el catálogo
- **THEN** recibe la lista de deportes activos con su identificador y su nombre
