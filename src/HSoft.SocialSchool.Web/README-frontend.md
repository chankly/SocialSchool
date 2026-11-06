# HSoft.SocialSchool.Web · boceto del cliente web

> **Estado:** cliente navegable con dos modos. Por defecto usa datos simulados en memoria; con
> `ApiBaseUrl` y `Oidc` configurados habla con el API real y autentica contra Keycloak.
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

El resto de pantallas (inicio, escuela, directorio y amistades) completa la navegación y el lenguaje
visual. El perfil y el catálogo de deportes pueden hablar con el API; amistades y escuelas siguen
con servicios simulados.

## Cómo ejecutarlo

Requisitos: **.NET 10 SDK**. En modo demo no hace falta base de datos, Keycloak ni la API en
marcha; en modo real sí se necesitan el API, el realm de Keycloak y un cliente público de la web.

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

## Modo demo y modo API real

La decisión se toma en un único punto, `Extensions/ServiceCollectionExtensions.cs`, a partir de
`wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "",
  "Oidc": {
    "Authority": "",
    "ClientId": ""
  }
}
```

- **Modo demo (valores vacíos, por defecto):** se registran los mocks, la sesión de demostración y el
  selector de escenarios de la cabecera. No interviene Keycloak.
- **Modo real (los tres valores completos):** se registra `AddOidcAuthentication` con un cliente
  **público (PKCE)** y dos `HttpClient` tipados que pasan por `ApiAuthorizationMessageHandler`, de modo
  que el bearer lo pone la librería OIDC y el navegador no guarda tokens a mano.

Ejemplo de configuración real (valores públicos, sin secretos):

```json
{
  "ApiBaseUrl": "https://localhost:7146",
  "Oidc": {
    "Authority": "https://keycloak.example.com/realms/socialschool",
    "ClientId": "SocialSchool-Web"
  }
}
```

`Oidc:ClientId` es el identificador del cliente **público** de la web; **no** es el GUID de
audiencia del API. Basta con vaciar cualquiera de los tres valores para volver al modo demo: la
reversión no requiere tocar código.

Para el modo real se añaden dos paquetes: `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
(flujo OIDC y `AuthorizationMessageHandler`) y `Microsoft.Extensions.Http` (clientes tipados).

## Cómo recorrer los estados del perfil

La **sesión de demo** se cambia en el desplegable de la cabecera. Este selector solo existe en modo
demo; con OIDC la identidad llega del token de Keycloak y el diálogo de acceso se sirve en
`/authentication/login`.

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
| `/authentication/{action}` | Acceso | Callback de Keycloak; en modo demo muestra un aviso de configuración |
| cualquier otra | No encontrada | Estado vacío dentro del layout |

## Servicios

| Contrato | Implementación en demo | Implementación real | Responsabilidad |
|---|---|---|---|
| `ISessionService` | `MockSessionService` | `OidcSessionService` | Identidad y tipo de cuenta; notifica cambios |
| `IProfileService` | `MockProfileService` | `HttpProfileService` | `GET`/`POST /api/user/profile/me` (200/204/401 y 400 con `detail`) |
| `ISportsService` | `MockSportsService` | `HttpSportsService` | Catálogo local o `GET /api/sports` |
| `IFriendsService` | `MockFriendsService` | — (sigue en mock) | Amistades y solicitudes con operaciones CRUD en memoria |
| `ISchoolsService` | `MockSchoolsService` | — (sigue en mock) | Directorio y perfil público de seis escuelas |
| `IDemoSessionService` | `MockSessionService` | — (no se registra) | Cambio de escenario de la sesión de demostración |
| `MockDataStore` | (infraestructura de demo) | (infraestructura de demo) | Estado compartido y sembrado inicial; reserva de nicks |
| `ProfileState` | (estado de UI) | (estado de UI) | Estado compartido del perfil, deduplicación de consultas y notificación |

Los servicios se registran en `Extensions/ServiceCollectionExtensions.cs`. El modo demo y el modo
real se eligen ahí, una sola vez. Los mocks son `Singleton` porque comparten el `MockDataStore`;
`ProfileState` y la sesión OIDC son `Scoped`.

### Operaciones simuladas

- Alta de perfil de deportista y de escuela, con latencia artificial y validación del tipo de cuenta.
- En modo real, alta y consulta contra el API; en demo se mantiene la latencia y la reserva de nicks.
- Conflicto de nick (equivalente a un 409) que se pinta sobre el campo.
- Aceptar/rechazar solicitudes, cancelar una solicitud enviada, quitar una amistad y enviar solicitudes.
- Envío de solicitud desde el detalle de una escuela, reutilizando `IFriendsService`.
- Regla de producto aplicada en el mock: **dos escuelas no pueden ser amigas**.
- Filtro de solicitudes y sugerencias cuando la sesión es de escuela (solo deportistas).

## Supuestos de diseño

1. **Ubicación y aislamiento:** el proyecto vive en `src/HSoft.SocialSchool.Web/` y no referencia
   ningún proyecto del backend. Se añade a la solución, pero se puede compilar de forma aislada.
2. **Autenticación OIDC opcional:** en modo real `AddOidcAuthentication` usa un cliente público
   (PKCE) y `ApiAuthorizationMessageHandler` adjunta el bearer; nunca se leen ni se guardan tokens a
   mano. En modo demo `ISessionService` lo sustituye una sesión ficticia. `wwwroot/appsettings.json`
   solo contiene valores públicos: **no hay secretos en el cliente** y el GUID de audiencia del API
   no se usa como `clientId`.
3. **Normalización del tipo de cuenta:** `AccountTypeParser` acepta `Athlete`/`Deportista` y
   `School`/`Escuela`, sin distinguir mayúsculas ni espacios. Un valor desconocido deja la página en
   un estado informativo.
4. **El 204 no es un error:** se representa como un estado propio que conduce al formulario de alta;
   nunca se usa un 404. El 401 se representa con una tarjeta de sesión, sin redirección real.
5. **El nick se pide solo al completar el perfil**, una única vez, y se valida como alfanumérico de
   3 a 20 caracteres. La vista lo muestra con un candado y el mock rechaza duplicados.
6. **Contrato de alta:** los formularios envían `nickname` y exactamente un bloque (`athlete` o
   `school`) a `POST /api/user/profile/me`. El `200` devuelve el mismo objeto que la consulta y se
   aplica directamente al estado, sin una segunda petición. Un `400` con `detail` se muestra en el
   formulario; si el mensaje habla del nick, se pinta sobre ese campo y no se pierde lo escrito.
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
│   ├── Schools/           SchoolsPage, SchoolDetailPage
│   └── Authentication/    Authentication.razor (callback OIDC)
├── Models/
│   ├── Accounts/          AccountType, AccountStatus, normalización y etiquetas
│   ├── Common/            Country, CountryCatalog
│   └── Friends/           FriendRequestDirection
├── Services/
│   ├── Session/           ISessionService, sesión de demo y OidcSessionService
│   ├── Profile/           contrato, mock, servicio HTTP, Requests/, Responses/
│   ├── Sports/            contrato, mock, servicio HTTP y catálogo
│   ├── Friends/           contrato, mock y Responses/
│   ├── Schools/           contrato, mock y Responses/
│   └── Demo/              MockDataStore, semilla y datos fijos
├── Validations/Profile/   validadores y reglas compartidas
├── State/                 ProfileState
├── Authentication/        ApiAuthorizationMessageHandler (bearer OIDC)
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

- Edición de perfil, catálogo de maniobras y puntuaciones, muro/publicaciones, notificaciones,
  reportes y subida de imágenes.
- Conexión real de amistades y escuelas: su backend todavía no existe y siguen con los mocks.
- Pruebas automatizadas: no existe proyecto de tests en la solución y esta maqueta no añade uno.
