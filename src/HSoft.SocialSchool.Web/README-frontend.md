# HSoft.SocialSchool.Web · boceto del cliente web

> **Estado:** maqueta inicial y navegable. Consume datos simulados en memoria; no llama al backend.
> **Alcance:** cliente Blazor WebAssembly independiente del backend. No modifica ni referencia
> `Api`, `Application`, `Domain` ni `Persistence`.

## Qué es

Una primera aproximación a la web de **SocialSchool**, una red social para deportistas y escuelas
de deporte. El objetivo prioritario es representar el flujo real de **«mi perfil»**:

1. El usuario se autentica en Keycloak y vuelve a la web.
2. La web consulta `GET /api/user/profile/me`.
3. La UI reacciona a **200** (perfil existente), **204** (cuenta sin perfil → formulario de alta) y
   **401** (sin sesión).
4. El **tipo de cuenta** llega en el token como `Athlete`/`School` o `Deportista`/`Escuela`, y se
   normaliza en el cliente.
5. El **nick** no se pide al crear la cuenta: se fija una sola vez al completar el perfil.

El resto de pantallas (inicio, escuela, directorio y amistades) completa la maqueta para evaluar la
navegación y el lenguaje visual, siempre con servicios simulados.

## Cómo ejecutarlo

Requisitos: **.NET 10 SDK**. No hace falta base de datos, Keycloak ni la API en marcha.

Desde la raíz del repositorio:

```bash
dotnet restore src/HSoft.SocialSchool.Web/HSoft.SocialSchool.Web.csproj
dotnet run --project src/HSoft.SocialSchool.Web
```

El perfil de lanzamiento `HSoft.SocialSchool.Web` abre `https://localhost:7200` y `http://localhost:5200`.
Si el certificado de desarrollo no está instalado, entra por HTTP con el perfil alternativo:

```bash
dotnet run --project src/HSoft.SocialSchool.Web --launch-profile http
```

Compilación aislada:

```bash
dotnet build src/HSoft.SocialSchool.Web/HSoft.SocialSchool.Web.csproj
```

El proyecto está incluido en `HSoft.SocialSchool.slnx`, así que también compila con:

```bash
dotnet build HSoft.SocialSchool.slnx
```

## Cómo recorrer los estados del perfil

La **sesión de demo** se cambia en el desplegable de la cabecera. No hay login real ni tokens.

| Sesión de demo | Token (`userType`) | Respuesta simulada | Qué se ve |
|---|---|---|---|
| Deportista · con perfil | `Deportista` | **200 OK** | Perfil de Ana Otxoa con su bloque `athlete` y sus deportes |
| Deportista · sin perfil | `Athlete` | **204 No Content** | Formulario de alta de deportista |
| Escuela · con perfil | `School` | **200 OK** | Perfil de Olas del Norte con su bloque `school` |
| Escuela · sin perfil | `Escuela` | **204 No Content** | Formulario de alta de escuela + aviso de validación pendiente |
| Sin sesión (401) | — | **401 Unauthorized** | Tarjeta de «no hay sesión» con botones para simular el regreso del login |

### Del 204 al 200

En cualquiera de las dos sesiones sin perfil, rellena el formulario y envíalo. El mock guarda el
perfil por cuenta, la etiqueta de la API pasa a **200 OK** y la misma pantalla muestra la vista del
perfil. El nick queda fijado.

Nicks ya ocupados en los datos de ejemplo (para ver el conflicto de unicidad):

`anaotxoa`, `olasdelnorte`, `admin`, `socialschool`, `soporte`, `ada`, `surf`.

El botón **Reiniciar datos** del menú lateral recarga la aplicación y devuelve todos los mocks a su
estado inicial.

## Páginas creadas

| Ruta | Página | Contenido |
|---|---|---|
| `/` | Inicio | Resumen de la sesión, estados del endpoint de perfil, pilares del producto, actividad simulada y configuración de la maqueta |
| `/profile` | **Mi perfil** | Núcleo de la maqueta: 200/204/401, formulario por tipo de cuenta, validación y vista del perfil |
| `/friends` | Amistades | Amigos, solicitudes recibidas/enviadas y cuentas por descubrir; aceptar, rechazar, cancelar, quitar y enviar |
| `/schools` | Escuelas | Directorio con búsqueda por nombre/ciudad/deporte y filtro por deporte |
| `/schools/{id}` | Detalle de escuela | Perfil público, contacto, servicios, asistentes y solicitud de amistad |
| cualquier otra | No encontrada | Estado vacío dentro del layout |

## Servicios simulados

| Contrato | Implementación | Responsabilidad |
|---|---|---|
| `ISessionService` | `MockSessionService` | Identidad y tipo de cuenta de la sesión de demo; notifica cambios |
| `IProfileService` | `MockProfileService` | `GET /api/user/profile/me` (200/204/401) y alta simulada de perfil |
| `ISportsService` | `MockSportsService` | Catálogo ficticio de diez deportes |
| `IFriendsService` | `MockFriendsService` | Amistades y solicitudes con operaciones CRUD en memoria |
| `ISchoolsService` | `MockSchoolsService` | Directorio y perfil público de seis escuelas |
| `MockDataStore` | (infraestructura de demo) | Estado compartido y sembrado inicial; reserva de nicks |
| `ProfileState` | (estado de UI) | Estado compartido del perfil, deduplicación de consultas y notificación |

Los servicios se registran en `Extensions/ServiceCollectionExtensions.cs`. Los mocks son
`Singleton` porque comparten el `MockDataStore`; `ProfileState` es `Scoped`.

### Operaciones simuladas

- Alta de perfil de deportista y de escuela, con latencia artificial y validación del tipo de cuenta.
- Conflicto de nick (equivalente a un 409) que se pinta sobre el campo.
- Aceptar/rechazar solicitudes, cancelar una solicitud enviada, quitar una amistad y enviar solicitudes.
- Envío de solicitud desde el detalle de una escuela, reutilizando `IFriendsService`.
- Regla de producto aplicada en el mock: **dos escuelas no pueden ser amigas**.
- Filtro de solicitudes y sugerencias cuando la sesión es de escuela (solo deportistas).

## Supuestos de diseño

1. **Ubicación y aislamiento:** el proyecto vive en `src/HSoft.SocialSchool.Web/` y no referencia
   ningún proyecto del backend. Se añade a la solución, pero se puede compilar de forma aislada.
2. **Sin autenticación real:** `ISessionService` sustituye a Keycloak. `wwwroot/appsettings.json`
   contiene solo valores públicos de ejemplo (authority, clientId y audiencia). **No hay secretos en
   el cliente.** La audiencia debe coincidir con el `clientId` del cliente de la API en Keycloak: en
   este despliegue es el GUID `0B28A13A-DF06-4840-958A-CC33605B3EB7`, que **no es un secreto** y
   tiene que ajustarse por entorno.
3. **Normalización del tipo de cuenta:** `AccountTypeParser` acepta `Athlete`/`Deportista` y
   `School`/`Escuela`, sin distinguir mayúsculas ni espacios. Un valor desconocido deja la página en
   un estado informativo.
4. **El 204 no es un error:** se representa como un estado propio que conduce al formulario de alta;
   nunca se usa un 404. El 401 se representa con una tarjeta de sesión, sin redirección real.
5. **El nick se pide solo al completar el perfil**, una única vez, y se valida como alfanumérico de
   3 a 20 caracteres. La vista lo muestra con un candado y el mock rechaza duplicados.
6. **Contrato de alta propuesto:** la API implementada solo expone la consulta del perfil. Los
   formularios envían `CreateAthleteProfileRequest` / `CreateSchoolProfileRequest` a un alta simulada
   (conceptualmente, `POST /api/user/profile/me`). Es una propuesta de contrato para la maqueta, no
   un endpoint existente.
7. **Fecha de nacimiento:** la respuesta usa `DateOnly`, pero el formulario trabaja con `DateTime?`
   porque es el tipo que mejor encaja con `InputDate`; la conversión se hace en el mock.
8. **Lo que no devuelve el perfil propio:** maniobras, asistencias y servicios de la escuela no se
   incluyen en la vista de «mi perfil», tal y como fija el spec de perfil. Los servicios y asistentes
   sí aparecen en el detalle público de la escuela.
9. **Catálogo de países en cliente:** no existe endpoint de referencia, así que se mantiene una lista
   estática en `Models/Common/CountryCatalog.cs`.
10. **Validación con FluentValidation + `EditForm`:** no se usa `DataAnnotationsValidator`. Los
    errores se vuelcan a un `ValidationMessageStore` para que aparezcan junto al campo y en el
    resumen. Las reglas compartidas (nick, país, URL) viven en `ProfileValidationRules`.
11. **Estado compartido:** `ProfileState` evita consultas duplicadas y descarta respuestas antiguas
    cuando se cambia de sesión mientras hay una petición en vuelo.
12. **Diseño visual:** hoja de estilos propia, sin Bootstrap ni CDN. Avatares con iniciales cuando no
    hay imagen, para que la maqueta funcione sin conexión. Iconos SVG inline.
13. **Amistades y escuelas son bocetos adicionales:** no sustituyen a los contratos del backend;
    demuestran listados, estados de carga/vacío/error y operaciones en memoria. Las rutas de escuela
    exigen sesión, igual que las consultas del producto.

## Estructura

```text
HSoft.SocialSchool.Web/
├── Components/
│   ├── Layout/            MainLayout, NavMenu
│   ├── Profile/           ProfileView, AthleteProfileForm, SchoolProfileForm
│   └── Shared/            Icon, Avatar, Pill, Alert, PageHeader, FormField,
│                          EmptyState, LoadingPanel, UiTone
├── Pages/
│   ├── Home.razor
│   ├── Profile/MyProfile.razor
│   ├── Friends/FriendsPage.razor
│   └── Schools/           SchoolsPage, SchoolDetailPage
├── Models/
│   ├── Accounts/          AccountType, AccountStatus, normalización y etiquetas
│   ├── Common/            Country, CountryCatalog
│   └── Friends/           FriendRequestDirection
├── Services/
│   ├── Session/           ISessionService, MockSessionService, escenarios
│   ├── Profile/           contrato, mock, Requests/, Responses/
│   ├── Sports/            contrato, mock y catálogo
│   ├── Friends/           contrato, mock y Responses/
│   ├── Schools/           contrato, mock y Responses/
│   └── Demo/              MockDataStore, semilla y datos fijos
├── Validations/Profile/   validadores y reglas compartidas
├── State/                 ProfileState
├── Configuration/         WebAppSettings
├── Extensions/            registro de dependencias
├── wwwroot/               index.html, appsettings.json, css/app.css
├── App.razor
├── _Imports.razor
├── Program.cs
└── README-frontend.md
```

## Verificación

- El proyecto no referencia ensamblados del backend y no modifica `src/HSoft.SocialSchool.{Api,Application,Domain,Persistence}`,
  `tests/`, `devops/`, `openspec/` ni `QWEN.md`.
- Los estados de carga, vacío y error están representados en el perfil, las amistades y las escuelas.
- La compilación prevista es `dotnet build src/HSoft.SocialSchool.Web/HSoft.SocialSchool.Web.csproj`
  y `dotnet build HSoft.SocialSchool.slnx`. **En el entorno aislado en el que se generó la maqueta no
  había ejecución de comandos disponible**, por lo que la compilación debe confirmarse al abrir la
  solución; no se ha podido ejecutar `dotnet build` aquí.

## Fuera de alcance

- Autenticación real contra Keycloak (solo configuración con placeholders y sesión simulada).
- Llamadas HTTP reales a la API, subida de imágenes y almacenamiento de tokens.
- Edición de perfil, catálogo de maniobras y puntuaciones, muro/publicaciones, notificaciones,
  denuncias y eliminación de cuenta.
- Pruebas automatizadas: no existe proyecto de tests en la solución y esta maqueta no añade uno.
