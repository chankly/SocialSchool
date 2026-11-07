# HSoft.SocialSchool.Web · cliente web

> **Estado:** cliente conectado al API. No hay modo demo ni datos de ejemplo: si faltan
> `ApiBaseUrl` u `Oidc` la aplicación muestra un aviso de configuración incompleta.
> **Alcance:** cliente Blazor WebAssembly independiente del backend. No modifica ni referencia
> `Api`, `Application`, `Domain` ni `Persistence`.
>
> **Endpoints que consume:** `GET`/`POST /api/user/profile/me`, `GET /api/sports`,
> `GET /api/schools`, `GET /api/schools/{id}`, `GET /api/friends`,
> `POST /api/friends/requests`, `POST /api/friends/requests/{id}/accept|reject`,
> `DELETE /api/friends/requests/{id}` y `DELETE /api/friends/{userId}`.

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
visual, y todas consultan el API: la aplicación no contiene datos de ejemplo.

## Cómo ejecutarlo

Requisitos: **.NET 10 SDK**, el API en marcha, el realm de Keycloak y un cliente público de la web.
Sin esa configuración la aplicación solo muestra el aviso de las claves que faltan.

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

## Configuración

`WebAssemblyHostBuilder.CreateDefault` descarga la configuración de `wwwroot`; nunca lee el disco del
equipo. En el navegador **no existen los user secrets** ni las variables de entorno, así que
`dotnet user-secrets` no puede funcionar aquí. Los ficheros que se leen son dos:

| Fichero | Cuándo | Contenido |
|---|---|---|
| `wwwroot/appsettings.json` | Siempre | Plantilla versionada, con los tres valores vacíos |
| `wwwroot/appsettings.Development.json` | Si el entorno es `Development` | Los valores locales de depuración |

El entorno lo pone el DevServer desde `Properties/launchSettings.json`, así que al depurar con Visual
Studio o con `dotnet run` se carga el segundo fichero, y sus valores ganan sobre los del primero. En
la publicación solo se pide el primero, salvo que el host anuncie `Development`.

Las claves son las mismas en los dos:

```json
{
  "ApiBaseUrl": "https://localhost:7146",
  "Oidc": {
    "Authority": "https://<tu-keycloak>/realms/socialschool",
    "ClientId": "SocialSchool-Web"
  }
}
```

- **Los tres valores completos:** se registra `AddOidcAuthentication` con un cliente
  **público (PKCE)** y dos `HttpClient` tipados que pasan por `ApiAuthorizationMessageHandler`, de modo
  que el bearer lo pone la librería OIDC y el navegador no guarda tokens a mano.

- **Alguno vacío:** no se registra ningún servicio de datos y la aplicación indica qué claves faltan.

`Oidc:ClientId` es el identificador del cliente **público** de la web; **no** es el GUID de
audiencia del API. Los tres valores son públicos por naturaleza: **nada de lo que se configure aquí
es un secreto**, y por eso no hay un `secrets.json` que el navegador pueda leer. Si algún día hace
falta un valor que no deba viajar en el bundle, tiene que servirlo la API y el cliente cargarlo
antes de `Build()` con `AddJsonStream`.

Ojo al publicar: `appsettings.Development.json` viaja en el bundle como cualquier fichero de
`wwwroot` y es descargable, aunque la aplicación no lo lea fuera de `Development`.

Se añaden dos paquetes: `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
(flujo OIDC y `AuthorizationMessageHandler`) y `Microsoft.Extensions.Http` (clientes tipados).

## Sesión y estados del perfil

La identidad la pone Keycloak: el diálogo de acceso se sirve en `/authentication/login` y la sesión
la mantiene `OidcSessionService`. La consulta `GET /api/user/profile/me` decide qué se pinta:

| Respuesta | Qué se ve |
|---|---|
| **200 OK** | El perfil del tipo de cuenta que lleve el token, con sus datos y su deporte |
| **204 No Content** | El formulario de alta del tipo de cuenta, con aviso de validación si es escuela |
| **401** | Tarjeta de «no hay sesión» con el botón de acceso |

En las dos sesiones sin perfil, rellena el formulario y envíalo: se crea contra
`POST /api/user/profile/me`, la pantalla pasa a la vista de **200** y el nick queda fijado.

## Páginas creadas

| Ruta | Página | Contenido |
|---|---|---|
| `/` | Inicio | Resumen de la sesión, estados del endpoint de perfil, pilares del producto y configuración activa |
| `/profile` | **Mi perfil** | Núcleo del cliente: 200/204/401, formulario por tipo de cuenta, validación y vista del perfil |
| `/friends` | Amistades | Amigos, solicitudes recibidas/enviadas y cuentas por descubrir; aceptar, rechazar, cancelar, quitar y enviar |
| `/schools` | Escuelas | Directorio con búsqueda por nombre/ciudad/deporte y filtro por deporte |
| `/schools/{id}` | Detalle de escuela | Perfil público, contacto, servicios, asistentes y solicitud de amistad |
| `/authentication/{action}` | Acceso | Callback de Keycloak; sin configuración muestra un aviso |
| cualquier otra | No encontrada | Estado vacío dentro del layout |

## Servicios

Todos son HTTP contra la API, con el bearer que pone `ApiAuthorizationMessageHandler`.

| Contrato | Implementación | Responsabilidad | Endpoints |
|---|---|---|---|
| `ISessionService` | `OidcSessionService` | Identidad y tipo de cuenta; notifica cambios | — (claims del token) |
| `IProfileService` | `HttpProfileService` | Consulta y alta del perfil propio | `GET`/`POST /api/user/profile/me` |
| `ISportsService` | `HttpSportsService` | Catálogo de deportes para los formularios | `GET /api/sports` |
| `IFriendsService` | `HttpFriendsService` | Amistades, solicitudes y cuentas invitables, con sus acciones | `GET /api/friends` y las cinco acciones |
| `ISchoolsService` | `HttpSchoolsService` | Directorio y ficha pública de escuelas | `GET /api/schools`, `GET /api/schools/{id}` |
| `ProfileState` | (estado de UI) | Estado compartido del perfil y deduplicación de consultas | — |

Los servicios se registran en `Extensions/ServiceCollectionExtensions.cs`, en un único camino. Si
`ApiBaseUrl` u `Oidc` están incompletos no se registra ninguno: quedan solo `WebAppSettings` y los
validadores, y `App.razor` pinta el aviso. `WebAppSettings` y los validadores son `Singleton`; los
clientes tipados, `ProfileState` y la sesión OIDC son `Scoped`.

### Qué puede hacer la interfaz

- Consultar el perfil propio y darse de alta como deportista o como escuela, con validación del tipo
  de cuenta y el conflicto de nick pintado sobre el campo.
- Ver amigos, solicitudes recibidas y enviadas, y cuentas por descubrir; aceptar, rechazar, cancelar,
  quitar una amistad y enviar solicitudes.
- Enviar una solicitud desde la ficha de una escuela, reutilizando `IFriendsService`.
- Recorrer el directorio de escuelas con búsqueda y filtro por deporte, y abrir su ficha con
  servicios y asistentes.

Los rechazos de negocio llegan como `400` con `detail` y se muestran tal cual; un `401` deja la
tarjeta de sesión. **Dos escuelas no pueden ser amigas**: lo rechaza el API y la interfaz solo
muestra el motivo.

## Supuestos de diseño

1. **Ubicación y aislamiento:** el proyecto vive en `src/HSoft.SocialSchool.Web/` y no referencia
   ningún proyecto del backend. Se añade a la solución, pero se puede compilar de forma aislada.
2. **Autenticación OIDC:** `AddOidcAuthentication` usa un cliente público (PKCE) y
   `ApiAuthorizationMessageHandler` adjunta el bearer; nunca se leen ni se guardan tokens a mano.
   `wwwroot/appsettings.json` solo contiene valores públicos: **no hay secretos en el cliente** y el
   GUID de audiencia del API no se usa como `clientId`.
3. **Normalización del tipo de cuenta:** `AccountTypeParser` acepta `Athlete`/`Deportista` y
   `School`/`Escuela`, sin distinguir mayúsculas ni espacios. Un valor desconocido deja la página en
   un estado informativo.
4. **El 204 no es un error:** se representa como un estado propio que conduce al formulario de alta;
   nunca se usa un 404. El 401 se representa con una tarjeta de sesión, sin redirección real.
5. **El nick se pide solo al completar el perfil**, una única vez, y se valida como alfanumérico de
   3 a 20 caracteres. La vista lo muestra con un candado y el API rechaza los duplicados; el mensaje
   se pinta sobre el campo.
6. **Contrato de alta:** los formularios envían `nickname` y exactamente un bloque (`athlete` o
   `school`) a `POST /api/user/profile/me`. El `200` devuelve el mismo objeto que la consulta y se
   aplica directamente al estado, sin una segunda petición. Un `400` con `detail` se muestra en el
   formulario; si el mensaje habla del nick, se pinta sobre ese campo y no se pierde lo escrito.
7. **Fecha de nacimiento:** la respuesta usa `DateOnly`, pero el formulario trabaja con `DateTime?`
   porque es el tipo que mejor encaja con `InputDate`; la conversión se hace en `HttpProfileService`.
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
    hay imagen, para no depender de recursos externos. Iconos SVG inline.
13. **Amistades y escuelas:** consumen `GET /api/friends` y `GET /api/schools`, y cubren listados,
    estados de carga/vacío/error y las cinco acciones de amistad. Las rutas de escuela exigen
    sesión, igual que las consultas del producto.

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
│   ├── Common/            ProblemDetailsReader
│   ├── Session/           ISessionService y OidcSessionService
│   ├── Profile/           contrato, servicio HTTP, Requests/ y Responses/
│   ├── Sports/            contrato, servicio HTTP y Responses/
│   ├── Friends/           contrato, servicio HTTP, Requests/ y Responses/
│   └── Schools/           contrato, servicio HTTP y Responses/
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
- La compilación se verifica con `dotnet build src/HSoft.SocialSchool.Web/HSoft.SocialSchool.Web.csproj`
  y `dotnet build HSoft.SocialSchool.slnx`.

## Fuera de alcance

- Edición de perfil, catálogo de maniobras y puntuaciones, muro/publicaciones, notificaciones,
  reportes y subida de imágenes.
- Bloqueos y paginación: el API todavía no expone los bloqueos, y los listados van con un límite fijo
  (50 escuelas y 20 cuentas invitables).
- Pruebas automatizadas del cliente: la solución no tiene un proyecto de tests para la web.
