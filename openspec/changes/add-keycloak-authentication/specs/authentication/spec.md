# Spec Delta: Autenticación con Keycloak

## Purpose

La API valida los access tokens JWT que emite Keycloak, exige usuario autenticado por defecto y
expone la identidad externa del usuario a la capa de aplicación, de modo que ninguna funcionalidad
pueda construirse sobre peticiones no verificadas.

## ADDED Requirements

### Requirement: Validación de la procedencia, firma y vigencia del token

La API SHALL aceptar únicamente access tokens emitidos por el realm de Keycloak configurado, con firma
verificable y vigentes. SHALL NOT aceptar tokens sin firma, con firma inválida, emitidos por otro
emisor o expirados. Todas estas validaciones SHALL estar activas siempre; SHALL NOT desactivarse para
facilitar pruebas o desarrollo.

#### Scenario: Token válido

- **WHEN** llega una petición a un endpoint protegido con un access token vigente, firmado por el realm configurado
- **THEN** la petición se considera autenticada y continúa su procesamiento

#### Scenario: Token expirado

- **WHEN** llega un access token cuya vigencia ha terminado
- **THEN** la API responde `401 Unauthorized`

#### Scenario: Token de otro emisor

- **WHEN** llega un token correctamente firmado pero emitido por un realm distinto del configurado
- **THEN** la API responde `401 Unauthorized`

#### Scenario: Token sin firma

- **WHEN** llega un token con algoritmo de firma ausente o no firmado
- **THEN** la API responde `401 Unauthorized`

### Requirement: El token debe estar destinado a la API

La API SHALL exigir que el claim de audiencia del token incluya el cliente de la API
(`SocialSchool-Api`). Como consecuencia, la API SHALL rechazar los tokens de identidad (ID tokens)
emitidos para el cliente web, que van destinados a otra audiencia. SHALL NOT aceptar tokens
destinados exclusivamente a otro cliente.

#### Scenario: Access token destinado a la API

- **WHEN** llega un access token cuya audiencia incluye `SocialSchool-Api`
- **THEN** la petición se considera autenticada

#### Scenario: ID token del cliente web

- **WHEN** llega un ID token emitido para el cliente web, cuya audiencia no incluye `SocialSchool-Api`
- **THEN** la API responde `401 Unauthorized`

#### Scenario: Token de otro cliente del mismo realm

- **WHEN** llega un access token válido del mismo realm pero destinado a otro cliente
- **THEN** la API responde `401 Unauthorized`

### Requirement: Denegación por defecto y endpoints públicos explícitos

La API SHALL exigir un usuario autenticado en todo endpoint que no declare lo contrario. Un endpoint
público SHALL declararse de forma explícita como anónimo. Una petición sin credenciales válidas a un
endpoint no anónimo SHALL recibir `401 Unauthorized`.

#### Scenario: Petición sin token a un endpoint protegido

- **WHEN** llega una petición sin cabecera de autorización a un endpoint que no es anónimo
- **THEN** la API responde `401 Unauthorized`

#### Scenario: Petición a un endpoint declarado anónimo

- **WHEN** llega una petición sin token a un endpoint declarado explícitamente como anónimo
- **THEN** la API la atiende sin exigir autenticación

#### Scenario: Endpoint no declarado

- **WHEN** se añade un endpoint nuevo sin declarar si es público ni asignarle permisos
- **THEN** queda protegido por defecto y exige usuario autenticado

### Requirement: Permisos insuficientes

La API SHALL distinguir entre una petición no autenticada y una autenticada sin los permisos que exige
el endpoint. La primera SHALL recibir `401 Unauthorized`; la segunda SHALL recibir `403 Forbidden`. Las
políticas de autorización SHALL poder exigir requisitos adicionales al simple hecho de estar
autenticado.

#### Scenario: Usuario autenticado sin el requisito de la política

- **WHEN** llega una petición con un token válido de un usuario que no satisface el requisito adicional de la política del endpoint
- **THEN** la API responde `403 Forbidden`

#### Scenario: Usuario no autenticado

- **WHEN** llega una petición sin token a un endpoint que exige un requisito adicional
- **THEN** la API responde `401 Unauthorized` y no `403 Forbidden`

### Requirement: Identidad externa del usuario basada en `sub`

La identidad externa del usuario autenticado SHALL ser el claim `sub` del token, leído tal cual lo
emite Keycloak. La API SHALL NOT usar el correo electrónico como identificador de identidad, SHALL NOT
depender de claims opcionales (`email`, `name`, `preferred_username`) y SHALL NOT remapear los nombres
de los claims. La capa de aplicación SHALL poder obtener esa identidad sin conocer el contexto HTTP.

#### Scenario: Token con solo el identificador

- **WHEN** un token válido contiene el claim `sub` pero omite los claims opcionales
- **THEN** la API identifica al usuario por su `sub` y no falla por los claims ausentes

#### Scenario: Dos usuarios con el mismo correo

- **WHEN** dos cuentas distintas presentan el mismo valor en el claim de correo pero distinto `sub`
- **THEN** la API las considera usuarios diferentes

### Requirement: Usuario autenticado sin cuenta de negocio

La autenticación SHALL ser independiente de la existencia de una cuenta de negocio. La API SHALL NOT
exigir que exista un registro de usuario en la base de datos para considerar autenticada una petición,
de modo que un usuario recién llegado pueda comprobar y completar su registro.

#### Scenario: Primer acceso de un usuario

- **WHEN** llega una petición con un token válido de un usuario que todavía no tiene cuenta de negocio
- **THEN** la petición se considera autenticada y no se rechaza con `401` ni `403` por ese motivo

#### Scenario: Usuario con cuenta de negocio

- **WHEN** llega una petición con un token válido de un usuario que ya tiene cuenta de negocio
- **THEN** la petición se considera autenticada igual que en el caso anterior

### Requirement: Respuestas y registros sin información sensible

Las respuestas de error de autenticación y autorización SHALL NOT incluir detalles de la validación del
token ni secretos. La API SHALL NOT registrar access tokens completos ni credenciales en los logs, y
SHALL poder registrar la identidad del usuario sin volcar el token.

#### Scenario: Fallo de autenticación

- **WHEN** una petición se rechaza por un token inválido
- **THEN** la respuesta se limita a un error genérico de no autorizado, sin detalles de la validación

#### Scenario: Registro de auditoría

- **WHEN** se registra una petición autenticada en los logs de auditoría
- **THEN** el registro identifica al usuario y al cliente sin incluir el token completo
