# Design

## Context

Motivación en `proposal.md`; comportamiento exigido en `specs/web-access/spec.md`.

Estado actual del cliente (`src/HSoft.SocialSchool.Web`), verificado leyendo el código:

- `App.razor` solo decide entre el aviso de configuración incompleta y el `Router` con
  `RouteView`. No hay `CascadingAuthenticationState`, ni `AuthorizeRouteView`, ni `[Authorize]` en
  ningún sitio.
- La protección es manual y dispersa: `FriendsPage`, `SchoolsPage` y `SchoolDetailPage` pintan su
  propia tarjeta `@if (!Session.IsAuthenticated)` con un botón que llama a
  `NavigateToLogin("authentication/login")`; `MyProfile` tiene su tarjeta de `401`; `Home` y
  `MainLayout` se muestran siempre, y el layout incluye un botón de «Iniciar sesión».
- `Authentication.razor` sirve `RemoteAuthenticatorView` en `/authentication/{action}` y navega a
  `/profile` en `OnLogInSucceeded`, descartando la ruta de la que venía el usuario.
- `OidcSessionService.EnsureLoadedAsync` resuelve la identidad con `GetAuthenticationStateAsync()` y
  solo consulta el perfil al API cuando ya está autenticado.
- Los cuatro clientes tipados (`IProfileService`, `ISportsService`, `IFriendsService`,
  `ISchoolsService`) pasan por `ApiAuthorizationMessageHandler`, que adjunta el bearer pero no
  interpreta un `401`.
- El paquete `Microsoft.AspNetCore.Components.WebAssembly.Authentication` 10.0.0 documenta
  `NavigateToLogin(NavigationManager, string, InteractiveRequestOptions)` con `ReturnUrl` y
  `RemoteAuthenticationState.ReturnUrl`.
- No hay proyecto de tests para la web.

## Goals / Non-Goals

**Goals:**

- Que la regla «todo exige sesión» sea estructural: una pantalla nueva queda protegida sin que nadie
  se acuerde de marcarla.
- Un único punto que decide qué ve quien no tiene sesión, en lugar de una tarjeta por pantalla.
- Que el `401` en mitad de uso tenga el mismo efecto que abrir la aplicación sin sesión.

**Non-Goals:**

- No se toca el API, su `FallbackPolicy` ni el webhook anónimo.
- No se añade renovación silenciosa de token: un `401` se trata como sesión rechazada.
- No se protege la interfaz de Swagger ni se revisan los endpoints anónimos del API.
- No se introduce un proyecto de tests para el cliente.

## Decisions

### 1. Denegación por defecto con `[Authorize]` global y `AuthorizeRouteView`

`_Imports.razor` recibe `@attribute [Authorize]`, que se aplica a todas las páginas del proyecto;
`Authentication.razor` se marca `@attribute [AllowAnonymous]`. `App.razor` envuelve el `Router` en
`CascadingAuthenticationState` y sustituye `RouteView` por `AuthorizeRouteView`, con
`<Authorizing>` → `LoadingPanel` y `<NotAuthorized>` → el componente de redirección.

Es la misma filosofía que ya tiene el API —denegar por defecto y abrir excepciones explícitas— y
convierte «proteger una pantalla nueva» en no hacer nada.

Alternativas descartadas:

- **Componente envoltorio que mira `Session.IsAuthenticated` antes de pintar el `Router`.** Funciona,
  pero tiene que exceptuar a mano las rutas `/authentication/*` comparando la URL, y deja la puerta
  abierta a que una ruta nueva se cuele si el prefijo cambia. Además no habilita `AuthorizeView` para
  el futuro.
- **`[Authorize]` página por página.** Explícito, pero rompe la denegación por defecto: la sexta
  pantalla que alguien añada se le olvida.

### 2. La llegada sin sesión arranca el acceso, sin portada

`<NotAuthorized>` no pinta un botón: monta un componente que, en cuanto se le renderiza, llama a
`NavigateToLogin("authentication/login", <ruta actual>)` y no devuelve nada. `AuthorizeRouteView`
solo llega a `NotAuthorized` cuando el estado de autenticación ya está resuelto, así que no hay
parpadeo de contenido ni carrera con la resolución de la sesión.

La ruta se pasa con `NavigationManager.ToBaseRelativePath(NavigationManager.Uri)`.

Alternativa descartada: portada de bienvenida con botón (decisión de producto: el usuario eligió
acceso automático).

### 3. El destino se conserva, con `/profile` como respaldo

`OnLogInSucceeded` navega a `state.ReturnUrl` cuando viene informado y a `/profile` cuando no. Se
mantiene `/profile` como respaldo para no cambiar el aterrizaje actual en los accesos que no tienen
ruta pedida.

Detalle a confirmar al implementar: si `RemoteAuthenticatorView` ya navega por su cuenta al
`ReturnUrl` tras completar el acceso, la llamada manual sobra y hay que quitarla en vez de duplicar
la navegación. Se comprueba con la matriz de verificación, no por inspección del binario.

### 4. El `401` se atiende en la cadena de handlers, no en cada servicio

Nuevo `Authentication/UnauthorizedRedirectHandler : DelegatingHandler`, registrado en los cuatro
clientes tipados después de `ApiAuthorizationMessageHandler`. Al ver un `401` navega al login con la
ruta actual y devuelve la respuesta tal cual: cada servicio sigue traduciendo el fallo como hoy.

Es un solo sitio para una regla que hoy habría que repetir en tres servicios con tres estilos
distintos —`IProfileService` devuelve un resultado `Unauthorized`, las amistades y las escuelas
lanzan— y no obliga a cambiar esos contratos.

Alternativas descartadas:

- **Atrapar `AccessTokenNotAvailableException` en cada pantalla** (el patrón de la plantilla). Cubre
  «no puedo conseguir token», no «el servidor ha rechazado el que llevaba», que es el caso del
  token caducado o revocado.
- **Comprobar el `401` dentro de cada `Http*Service`.** Cuatro copias de la misma navegación y un
  contrato nuevo para avisar a la UI.

### 5. El aviso de configuración queda fuera del árbol de autenticación

Sin `ApiBaseUrl`/`Oidc` no se registra `AddOidcAuthentication`, así que no hay proveedor de
autenticación que `CascadingAuthenticationState` pueda consumir. La comprobación
`Settings.IsConfigured` se mantiene como rama previa del `App.razor`, y el aviso conserva su texto
actual. Es lo que evita que un cliente mal configurado entre en un bucle de login.

### 6. Se elimina lo que queda inalcanzable

Las tarjetas de «sin sesión» y sus botones en `FriendsPage`, `SchoolsPage`, `SchoolDetailPage` y
`MyProfile`, y la rama de «Iniciar sesión» de `MainLayout`, dejan de poder renderizarse. Se retiran
para no dejar dos fuentes de verdad sobre el acceso. `ProfileLoadStatus.Unauthorized` y los
resultados `Unauthorized` de los servicios se conservan: siguen siendo la traducción correcta de un
`401` y el handler nuevo no cambia esos contratos.

## Risks / Trade-offs

- **Doble navegación al completar el acceso** si `RemoteAuthenticatorView` ya navega al `ReturnUrl` →
  se comprueba en la matriz; si ocurre, se retira la navegación manual y se deja la del componente.
- **Bucle de login** si una ruta de acceso deja de ser anónima (por ejemplo, alguien quita el
  `[AllowAnonymous]`) → el escenario «Inicio de acceso sin sesión» de la spec es la comprobación
  explícita de que no ocurre.
- **Caché del DevServer** al cambiar `App.razor`/`_Imports.razor`: una recarga forzada o reiniciar la
  depuración, no basta con un refresco normal.
- **El login puede fallar por configuración externa**: el cliente de Keycloak tiene que admitir
  `https://localhost:7200/authentication/login-callback` y el *web origin*, y la `Authority` debe
  apuntar al realm real (`/realms/SocialSchool` frente a `socialschool` de Terraform). No es código y
  no se toca aquí, pero si falla el login hay que descartarlo antes de culpar al cambio.
- **Cerrar sesión devuelve al login de Keycloak** de inmediato, porque la aplicación ya no tiene un
  estado «sin sesión pero dentro». Es la consecuencia coherente de la regla; si más adelante se
  quiere una despedida, será otro cambio.

## Migration Plan

Cambio solo de cliente: se publica el nuevo `wwwroot` y no hay migración de datos ni de esquema. Para
revertir, se revierte el commit del cliente; el API y Keycloak no se enteran.
