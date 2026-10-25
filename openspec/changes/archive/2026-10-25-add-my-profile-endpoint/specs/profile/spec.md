# Perfil

## Purpose

Permite al usuario autenticado consultar el perfil de su propia cuenta —deportista o escuela— y saber si
todavía lo tiene pendiente de rellenar, que es lo que decide si la aplicación le muestra el perfil o el
formulario de alta.

## ADDED Requirements

### Requirement: Consulta del perfil propio

La API SHALL exponer `GET /api/user/profile/me` para que el usuario autenticado consulte su propio
perfil. El endpoint SHALL identificar al usuario por la identidad externa del token (`sub`) y SHALL
devolver `200 OK` con los datos del perfil cuando exista, `204 No Content` cuando no exista perfil
—tanto si la cuenta de negocio todavía no se ha materializado como si existe sin perfil rellenado— y
SHALL dejar que una petición sin token válido reciba `401 Unauthorized`. SHALL NOT usar `404` para
"todavía no hay perfil", porque ese estado no es un error.

#### Scenario: Perfil existente

- **WHEN** un usuario autenticado con perfil llama a `GET /api/user/profile/me`
- **THEN** la API responde `200 OK` con el perfil de su cuenta

#### Scenario: Cuenta sin perfil

- **WHEN** un usuario autenticado cuya cuenta de negocio existe pero no tiene perfil llama al endpoint
- **THEN** la API responde `204 No Content`, sin cuerpo

#### Scenario: Sin cuenta de negocio

- **WHEN** un usuario autenticado cuya cuenta de negocio todavía no existe llama al endpoint
- **THEN** la API responde `204 No Content` y no un error

#### Scenario: Petición sin token

- **WHEN** llega una petición al endpoint sin token válido
- **THEN** la API responde `401 Unauthorized` y el endpoint no se ejecuta

### Requirement: El tipo de cuenta determina el contenido

La respuesta SHALL indicar el tipo de cuenta y SHALL incluir únicamente el bloque correspondiente a ese
tipo: los datos del deportista con los deportes que practica, o los datos de la escuela. El tipo SHALL
obtenerse de la cuenta almacenada, no de un claim del token. La respuesta SHALL NOT incluir colecciones
que no forman parte del perfil: maniobras declaradas, asistencias ni servicios de la escuela.

#### Scenario: Cuenta de deportista

- **WHEN** el usuario autenticado es un deportista con perfil
- **THEN** la respuesta incluye su nombre, apellidos, fecha de nacimiento, país, ciudad, foto, biografía y los deportes que practica, y no incluye el bloque de escuela

#### Scenario: Cuenta de escuela

- **WHEN** el usuario autenticado es una escuela con perfil
- **THEN** la respuesta incluye su nombre, logo, deporte, descripción, país, ciudad y datos de contacto, y no incluye el bloque de deportista

### Requirement: Ningún perfil ajeno

El endpoint SHALL devolver exclusivamente el perfil del usuario identificado por el token. SHALL NOT
aceptar ningún parámetro que permita consultar el perfil de otra cuenta. La consulta de perfiles ajenos
SHALL resolverse en los endpoints de perfil público, sujetos a las reglas de visibilidad del producto.

#### Scenario: Intento de consultar otro perfil

- **WHEN** un usuario autenticado llama al endpoint añadiendo un identificador de otra cuenta como parámetro
- **THEN** el endpoint lo ignora y devuelve su propio perfil o `204`, nunca el de la otra cuenta
