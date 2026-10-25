# Registro

## Purpose

Registra en el dominio las cuentas que nacen en Keycloak a partir del evento de registro: la identidad y el
tipo de cuenta se materializan como un `User` sin perfil, que el usuario completará después en la
aplicación.

## Requirements

### Requirement: Alta de la cuenta desde el evento de registro

Al recibir un evento de Keycloak de tipo `access.REGISTER`, la API SHALL materializar la cuenta de negocio
de ese usuario. Para ello SHALL consultar su perfil en la Admin API de Keycloak usando `authDetails.userId`
como identificador y SHALL crear el `User` con el correo, el tipo de cuenta (`attributes.userType`) y el
proveedor de identidad (`federatedIdentities`) de ese perfil. El identificador de Keycloak SHALL ser tanto
el `Id` de la cuenta como su identidad externa, de modo que la cuenta y el perfil que se cree después
compartan identificador. SHALL NOT crear el perfil (`Athlete` o `School`): eso lo completa el usuario.

#### Scenario: Registro de un deportista
- **WHEN** llega un evento `access.REGISTER` y el perfil de Keycloak trae `userType` con `Athlete`
- **THEN** se crea una cuenta de tipo `Athlete`, activa, con la identidad externa del evento y sin perfil

#### Scenario: Registro de una escuela
- **WHEN** el `userType` del perfil es `School`
- **THEN** la cuenta se crea pendiente de validación, como cualquier escuela

#### Scenario: Falta el tipo de cuenta
- **WHEN** el perfil no trae el atributo `userType` o trae un valor desconocido
- **THEN** no se crea la cuenta, la respuesta no es satisfactoria y queda constancia en el log

### Requirement: El evento se procesa una sola vez

Un mismo registro SHALL NOT crear dos cuentas: si ya existe una cuenta con la misma identidad externa, la
API SHALL NOT crear otra y SHALL responder de forma satisfactoria. Los eventos que no son de registro SHALL
acusarse sin tocar la base de datos.

#### Scenario: Evento repetido
- **WHEN** llega por segunda vez el mismo `access.REGISTER`
- **THEN** no se crea una segunda cuenta y la respuesta es satisfactoria

#### Scenario: Otro tipo de evento
- **WHEN** llega un evento de otro tipo
- **THEN** se acusa recibo y no se modifica nada

### Requirement: El nick no se inventa

La cuenta creada desde el evento SHALL NOT llevar nick. El nick es del usuario: lo fija él al completar el
perfil, a partir de ahí es único e inmutable, y solo admite letras y dígitos. Un segundo intento de fijarlo
SHALL rechazarse.

#### Scenario: Cuenta recién creada
- **WHEN** se materializa una cuenta desde el evento
- **THEN** su nick está vacío y su perfil todavía no existe

#### Scenario: Fijar el nick una sola vez
- **WHEN** el usuario fija su nick al completar el perfil
- **THEN** se guarda, y un segundo intento de cambiarlo se rechaza
