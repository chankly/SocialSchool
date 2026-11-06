# Escuelas

## Purpose

Publica el directorio de escuelas y su ficha para que cualquier usuario autenticado pueda descubrirlas, ver
sus servicios y quién ha asistido a ellas.

## ADDED Requirements

### Requirement: Directorio de escuelas

La API SHALL exponer `GET /api/schools` con filtros opcionales por deporte, ciudad y texto, y SHALL devolver
las escuelas ordenadas por nombre, con su identificador, nombre, logo, deporte y ciudad. SHALL incluir las
escuelas activas y las pendientes de validación, indicando el estado en cada una. La consulta SHALL requerir
usuario autenticado.

#### Scenario: Directorio completo
- **WHEN** un usuario autenticado consulta el directorio sin filtros
- **THEN** recibe las escuelas ordenadas por nombre con su deporte y su ciudad

#### Scenario: Filtro por deporte y por texto
- **WHEN** se consulta con un deporte o con un texto
- **THEN** solo aparecen las escuelas de ese deporte o cuyo nombre contiene ese texto, sin distinguir mayúsculas

#### Scenario: Escuela pendiente de validación
- **WHEN** una escuela todavía no ha sido validada por el técnico
- **THEN** aparece en el directorio con su estado de pendiente, para que el cliente pueda indicarlo

### Requirement: Ficha de una escuela

La API SHALL exponer `GET /api/schools/{id}` y SHALL devolver los datos públicos de la escuela, sus
servicios y los deportistas con asistencia aceptada, con su nick y su foto. SHALL responder `404` cuando la
escuela no exista.

#### Scenario: Ficha con servicios y asistentes
- **WHEN** un usuario autenticado pide una escuela existente
- **THEN** recibe sus datos, la lista de servicios y los deportistas que asistieron, con nick y foto

#### Scenario: Escuela inexistente
- **WHEN** se pide un identificador que no corresponde a ninguna escuela
- **THEN** la API responde `404`
