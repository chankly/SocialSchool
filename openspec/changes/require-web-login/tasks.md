# Tasks

## 1. Puerta de acceso

- [x] 1.1 Marcar `@attribute [Authorize]` en `_Imports.razor` y `@attribute [AllowAnonymous]` en `Pages/Authentication/Authentication.razor`. Verificación: `dotnet build src/HSoft.SocialSchool.Web/HSoft.SocialSchool.Web.csproj` sin errores y la página de acceso sigue compilando sin atributos duplicados.
- [x] 1.2 Crear el componente que lanza el acceso solo (en `Components/Common/`), que navega con `NavigateToLogin("authentication/login", NavigationManager.ToBaseRelativePath(NavigationManager.Uri))` y no pinta nada. Verificación: al montarlo no deja contenido en pantalla y el destino que pasa es la ruta relativa actual.
- [x] 1.3 En `App.razor`, envolver el `Router` en `CascadingAuthenticationState` y cambiar `RouteView` por `AuthorizeRouteView`, con `<Authorizing>` → `LoadingPanel` y `<NotAuthorized>` → el componente de 1.2. La rama de configuración incompleta se queda fuera. Verificación: compila y, con sesión válida, las cinco rutas (`/`, `/profile`, `/friends`, `/schools`, `/schools/{id}`) siguen pintando.
- [ ] 1.4 Comprobar que las rutas de acceso siguen siendo anónimas y que no hay bucle. Verificación: sin sesión, `/authentication/login` muestra el flujo de Keycloak en vez de redirigirse a sí misma, y `wwwroot/index.html` no cambia.

## 2. Destino conservado

- [x] 2.1 En `Authentication.razor`, hacer que `OnLogInSucceeded` navegue a `state.ReturnUrl` cuando venga informado y a `/profile` cuando no. Verificación: abrir `/schools/{id}` sin sesión, autenticarse y aterrizar en esa ficha.
- [ ] 2.2 Confirmar si `RemoteAuthenticatorView` ya navega al `ReturnUrl` por su cuenta y, si es así, retirar la navegación manual en lugar de duplicarla. Verificación: en el flujo de 2.1 el navegador no pasa por una ruta intermedia (`/profile` y luego la ficha).

## 3. Sesión rechazada

- [x] 3.1 Crear `Authentication/UnauthorizedRedirectHandler.cs` (`DelegatingHandler`) que, ante un `401`, navegue al login con la ruta actual y devuelva la respuesta sin alterarla. Verificación: el handler no captura otras respuestas (`400`, `403`, `404`) y no entra en bucle sobre las rutas de acceso.
- [x] 3.2 Registrarlo en los cuatro clientes tipados de `Extensions/ServiceCollectionExtensions.cs`, después de `ApiAuthorizationMessageHandler`. Verificación: los cuatro servicios lo reciben (cuatro `AddHttpMessageHandler` más en el registro) y el proyecto compila.
- [ ] 3.3 Comprobar el caso real: con la aplicación abierta, invalidar la sesión en Keycloak y provocar una llamada al API. Verificación: el cliente vuelve al login conservando la ruta y la pantalla no muestra datos a medias ni confirma la operación.

## 4. Limpieza de las protecciones manuales

- [x] 4.1 Quitar de `FriendsPage`, `SchoolsPage` y `SchoolDetailPage` la tarjeta `@if (!Session.IsAuthenticated)` con su botón y su método de acceso, y de `MainLayout` la rama de «Iniciar sesión» (el botón de cerrar sesión se queda). Verificación: un `findstr` de `IsAuthenticated` en `Pages` no devuelve guardas de pantalla y el layout compila sin miembros huérfanos.
- [x] 4.2 Retirar en `MyProfile` la tarjeta de `401` y los ayudantes de acceso que queden sin uso. Verificación: el compilador no avisa de miembros privados sin usar y la pantalla sigue pintando 200 y 204.
- [x] 4.3 Describir la regla de acceso en `README-frontend.md` (toda la interfaz exige sesión, el login arranca solo, la ruta se conserva) y quitar cualquier frase que dé a entender que se puede navegar sin sesión. Verificación: el README no afirma que la portada sea pública.

## 5. Verificación de conjunto

- [ ] 5.1 `dotnet build HSoft.SocialSchool.slnx` sin errores nuevos.
- [ ] 5.2 Matriz manual completa con la API en marcha: aplicación recién abierta sin sesión → login; ruta profunda sin sesión → login → misma ruta; sesión válida → todas las pantallas; sesión revocada durante el uso → login; sin `Oidc` configurado → aviso y ningún intento de login; cerrar sesión → se vuelve a pedir acceso.
- [ ] 5.3 Si el login falla antes de mostrar la pantalla de Keycloak, descartar la configuración externa: `redirect_uri` y *web origin* del cliente público y que `Oidc:Authority` apunte al realm real.
