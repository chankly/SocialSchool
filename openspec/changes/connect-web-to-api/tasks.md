# Tasks

> Amistades y escuelas siguen en mock: no bloquean nada de aquí.

## 1. Configuración y autenticación

- [ ] 1.1 Ampliar `Configuration/WebAppSettings.cs` y `wwwroot/appsettings.json` con `ApiBaseUrl`, `Oidc:Authority` y `Oidc:ClientId` (vacíos por defecto, sin secretos). Verificación: sin valores, la aplicación arranca en modo demo.
- [ ] 1.2 Registrar en `Program.cs` la autenticación OIDC (`AddOidcAuthentication` con `AuthorizationMessageHandler`) y el `HttpClient` del API, solo cuando la configuración esté completa. Verificación: la compilación no exige Keycloak y el modo demo sigue funcionando.

## 2. Servicios reales

- [ ] 2.1 Implementar `HttpProfileService` contra `GET` y `POST /api/user/profile/me`, traduciendo `204` a "sin perfil", `401` a "sin sesión" y los errores del servidor a mensajes mostrables. Verificación: la página no cambia de forma, solo de origen de datos.
- [ ] 2.2 Implementar `HttpSportsService` contra `GET /api/sports`. Verificación: el selector de deportes del formulario se llena con la respuesta.
- [ ] 2.3 Elegir implementación por configuración en `Extensions/ServiceCollectionExtensions.cs`: servicios HTTP si hay `ApiBaseUrl` + `Oidc`, mocks si no. Verificación: un único punto de decisión.

## 3. Página del perfil

- [ ] 3.1 Ajustar `Pages/Profile/MyProfile.razor` y el estado para que el alta llame al servicio real y, tras el `200`, refresque con el perfil devuelto sin una segunda consulta. Verificación: `204` → formulario → `200`.
- [ ] 3.2 Mostrar el error del servidor (por ejemplo nick ocupado) junto al campo cuando la respuesta no sea satisfactoria. Verificación: no se pierde el formulario ni lo escrito.

## 4. Verificación

- [ ] 4.1 `dotnet build` del proyecto web y de la solución, sin errores nuevos.
- [ ] 4.2 Revisar que no se ha tocado nada fuera de `src/HSoft.SocialSchool.Web` y actualizar `README-frontend.md` con la configuración necesaria y el modo demo.
