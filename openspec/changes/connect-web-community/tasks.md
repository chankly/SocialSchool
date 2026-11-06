# Tasks

## 1. Quitar los datos de ejemplo

- [ ] 1.1 Eliminar `Services/Demo/*`, `MockSessionService`, `IDemoSessionService`, `MockFriendsService` y `MockSchoolsService`, y las referencias que queden. Verificación: no queda ninguna clase `Mock*` ni `Demo*` en el proyecto.
- [ ] 1.2 Simplificar `ServiceCollectionExtensions` y `WebAppSettings`: un único camino con servicios HTTP; sin `ApiBaseUrl` u `Oidc` no se registran y la aplicación muestra un aviso de configuración incompleta en vez de datos falsos.
- [ ] 1.3 `ISessionService` con una sola implementación (OIDC): identidad y tipo de cuenta a partir del usuario autenticado, y estado de perfil a partir de la consulta. Verificación: la cabecera ya no tiene selector de demo.

## 2. Amistades y escuelas reales

- [ ] 2.1 `HttpFriendsService` contra `GET /api/friends` y las cinco acciones del contrato, traduciendo los errores del servidor a mensajes mostrables y refrescando la lista tras cada acción.
- [ ] 2.2 `HttpSchoolsService` contra `GET /api/schools` y `GET /api/schools/{id}`, ajustando los DTOs a la respuesta real (estado de la escuela, servicios y asistentes).
- [ ] 2.3 Ajustar `Pages/Friends/FriendsPage.razor`, `Pages/Schools/SchoolsPage.razor`, `Pages/Schools/SchoolDetailPage.razor` y `Pages/Home.razor` a los servicios reales, con estados de carga, vacío y error en cada uno.

## 3. Verificación

- [ ] 3.1 `dotnet build` del proyecto web y de la solución, sin errores.
- [ ] 3.2 Revisar que no se ha tocado nada fuera de `src/HSoft.SocialSchool.Web`, y actualizar `README-frontend.md` (configuración obligatoria, sin modo demo, endpoints que consume).
