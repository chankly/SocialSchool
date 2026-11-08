# Spec Delta

## Purpose

Define qué puede ver y hacer en el cliente web quien no ha iniciado sesión, cómo se arranca el acceso
y cómo se conserva el destino cuando la sesión falta, caduca o se rechaza.

## ADDED Requirements

### Requirement: Acceso restringido a usuarios autenticados

El cliente web SHALL NOT mostrar ninguna pantalla de la aplicación a quien no tenga una sesión válida.
Al abrir cualquier ruta que no sea de acceso, si no hay sesión SHALL iniciar el proceso de acceso
contra Keycloak sin intervención del usuario, y SHALL NOT mostrar contenido, navegación ni datos
mientras tanto. Con una sesión válida SHALL mostrar la pantalla pedida.

#### Scenario: Ruta protegida sin sesión

- **WHEN** alguien abre una ruta de la aplicación sin haber iniciado sesión
- **THEN** el cliente inicia el acceso contra Keycloak y no muestra la interfaz ni sus datos

#### Scenario: Ruta protegida con sesión

- **WHEN** alguien con una sesión válida abre una ruta de la aplicación
- **THEN** el cliente muestra esa pantalla con normalidad

#### Scenario: Aplicación recién abierta

- **WHEN** alguien abre la raíz del cliente sin sesión
- **THEN** se le pide autenticarse antes de ver nada, sin portada intermedia

### Requirement: Las pantallas de acceso permanecen anónimas

Las rutas de acceso SHALL ser alcanzables sin sesión, porque son las que completan el login y el
logout. Ninguna otra ruta SHALL serlo. Una ruta nueva que no declare lo contrario SHALL quedar
protegida.

#### Scenario: Inicio de acceso sin sesión

- **WHEN** sin sesión se abre la ruta de inicio de acceso
- **THEN** el cliente la atiende sin volver a redirigir a Keycloak

#### Scenario: Vuelta del acceso

- **WHEN** Keycloak devuelve al usuario a la ruta de retorno del acceso con la autenticación correcta
- **THEN** el cliente completa el acceso y deja la sesión disponible para el resto de la aplicación

#### Scenario: Ruta nueva sin declarar

- **WHEN** se añade una pantalla nueva sin marcarla como anónima
- **THEN** exige sesión como cualquier otra

### Requirement: El destino se conserva al autenticarse

Cuando el acceso se inicia desde una ruta concreta, el cliente SHALL devolver al usuario a esa ruta
una vez autenticado. Si no hay una ruta pedida, SHALL llevarlo al perfil propio.

#### Scenario: Acceso desde una ruta profunda

- **WHEN** sin sesión se abre la ficha de una escuela y el acceso termina bien
- **THEN** el usuario aterriza en esa misma ficha, no en el perfil

#### Scenario: Acceso sin ruta pedida

- **WHEN** el acceso concluye sin una ruta de destino
- **THEN** el cliente lleva al usuario al perfil propio

### Requirement: La sesión rechazada devuelve al acceso

Si el servidor rechaza una llamada por falta de credenciales válidas —sesión caducada, revocada o
rechazada— el cliente SHALL volver a iniciar el acceso conservando la ruta actual, y SHALL NOT
presentar el resultado fallido como si la operación hubiera funcionado.

#### Scenario: Token rechazado con la aplicación abierta

- **WHEN** una llamada al servidor responde por falta de credenciales válidas
- **THEN** el cliente inicia de nuevo el acceso con la ruta actual como destino

#### Scenario: Sin datos a medias

- **WHEN** falla una llamada por credenciales rechazadas
- **THEN** la pantalla no muestra datos parciales ni confirma la operación como completada

### Requirement: La falta de configuración no provoca un bucle de acceso

Si al cliente le faltan los datos para hablar con el servidor y con Keycloak, SHALL limitarse a
indicar qué claves faltan y SHALL NOT iniciar el acceso.

#### Scenario: Cliente sin configurar

- **WHEN** alguien abre el cliente sin los valores de configuración requeridos
- **THEN** ve el aviso de configuración incompleta y no se le redirige a Keycloak

#### Scenario: Configuración incompleta en las rutas de acceso

- **WHEN** se abre una ruta de acceso sin configuración
- **THEN** se informa de que el acceso no está configurado, sin intentar autenticar

### Requirement: Cerrar sesión deja la aplicación sin sesión

Al cerrar sesión el cliente SHALL descartar la sesión y SHALL volver a pedir autenticación, de forma
coherente con exigir sesión en toda la aplicación.

#### Scenario: Cierre de sesión

- **WHEN** un usuario autenticado cierra sesión
- **THEN** el cliente deja de considerarle autenticado y vuelve a pedirle acceso
