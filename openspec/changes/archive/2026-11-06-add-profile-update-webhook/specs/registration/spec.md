# Registro

## ADDED Requirements

### Requirement: Sincronización de los cambios de perfil en Keycloak

Cuando el usuario modifica los datos de su cuenta en Keycloak, la API SHALL atender el evento
`access.UPDATE_PROFILE` y SHALL actualizar en la cuenta el correo y/o el idioma preferido que ese evento
indique como modificados. SHALL NOT modificar el tipo de cuenta, que solo cambia un técnico, ni los datos de
perfil que pertenecen a nuestra aplicación. Un evento que no afecte ni al correo ni al idioma SHALL acusarse
sin modificar la cuenta.

#### Scenario: Cambia el idioma preferido
- **WHEN** llega un `access.UPDATE_PROFILE` cuyo detalle indica un idioma nuevo
- **THEN** la cuenta de ese usuario guarda ese idioma como preferido

#### Scenario: Cambia el correo
- **WHEN** el detalle del evento indica un correo nuevo
- **THEN** el correo de la cuenta se actualiza

#### Scenario: Cambio que no nos afecta
- **WHEN** el evento solo indica cambios de nombre o de apellidos, que son datos de nuestro perfil
- **THEN** la cuenta no se modifica y la respuesta es satisfactoria

#### Scenario: El tipo de cuenta no cambia
- **WHEN** llega cualquier evento de actualización
- **THEN** el tipo de la cuenta sigue siendo el de su creación, aunque el evento mencione otro

#### Scenario: Cuenta desconocida
- **WHEN** el evento se refiere a un usuario que no tiene cuenta en nuestra base de datos
- **THEN** no se crea nada y la respuesta es satisfactoria: crear la cuenta es del evento de alta
