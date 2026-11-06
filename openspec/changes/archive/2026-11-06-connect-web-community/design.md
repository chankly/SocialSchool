# Design: La web contra datos reales, sin mocks

## Context

- `Extensions/ServiceCollectionExtensions.cs` decide hoy entre servicios simulados y HTTP según
  `WebAppSettings.IsApiConfigured`; con los mocks se registran `MockDataStore`, `MockSessionService`,
  `IDemoSessionService`, `MockFriendsService` y `MockSchoolsService`.
- La cabecera (`Components/Layout/MainLayout.razor`) tiene un desplegable "Sesión de demo" y el inicio
  (`Pages/Home.razor`) enseña el catálogo de escenarios.
- El perfil y el catálogo de deportes ya van por HTTP cuando hay configuración.
- Los DTOs de amistades y escuelas ya existen en `Services/Friends/Responses` y `Services/Schools/Responses`
  y se parecen a lo que devolverá el API; se ajustan donde haga falta.

## Decisions

**1. Se borra el modo demo, no se esconde.** Mantener los mocks detrás de una bandera sería seguir teniendo
datos de ejemplo en el producto; el usuario pidió que la web sea funcional. Sin configuración, la aplicación
muestra un aviso y no navega.

**2. La sesión pasa a tener una sola implementación (OIDC).** `ISessionService` se queda con lo que la UI
necesita —identidad, tipo de cuenta y estado— y se alimenta del usuario de la autenticación y de la consulta
del perfil. Desaparecen `IDemoSessionService` y `MockSessionService`.

**3. Un servicio por ámbito funcional, como hasta ahora**: `HttpFriendsService` e `HttpSchoolsService`
sustituyen a los mocks y usan el mismo `HttpClient` con bearer que el perfil. Las acciones de amistad
refrescan la lista tras cada operación, para que la UI no mienta.

**4. Los errores del servidor se muestran tal cual vienen en `detail`**, como ya hace el formulario del
perfil. Un `400` de "ya sois amigos" o "no puedes invitarte a ti mismo" es un mensaje para el usuario.

**5. La ficha de escuela usa los asistentes y servicios tal y como los devuelve el API**, sin inventar los
que falten: si una escuela no tiene servicios, la sección dice que no tiene.

## Risks / Trade-offs

- **Sin Keycloak configurado no se puede ver nada**: es el precio de no tener datos falsos. Para probar hay
  que configurar el realm y el cliente público.
- **La UI queda a expensas del contrato**: cualquier cambio de forma en el API obliga a tocar los DTOs.
- **Sin paginación**: el directorio trae hasta 50 escuelas y la lista de invitables hasta 20.

## Migration Plan

1. Desplegar el API con los endpoints del cambio `add-community-endpoints`.
2. Configurar `ApiBaseUrl` y `Oidc` en `wwwroot/appsettings.json`.
3. Entrar con un usuario real y comprobar que amigos y escuelas vienen del servidor.
4. **Reversión**: no aplica; el modo demo desaparece.

## Open Questions

- Si conviene un modo "solo lectura" para enseñar la UI sin Keycloak en una demo comercial.
