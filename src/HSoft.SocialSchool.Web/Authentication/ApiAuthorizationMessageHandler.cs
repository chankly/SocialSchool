using HSoft.SocialSchool.Web.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace HSoft.SocialSchool.Web.Authentication;

/// <summary>
/// Adjunta el access token de Keycloak a las llamadas que van al API. El token lo gestiona
/// la librería de autenticación: nunca se lee ni se guarda a mano en el navegador.
/// </summary>
public sealed class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    /// <summary>Configura las URLs que pueden recibir el token.</summary>
    public ApiAuthorizationMessageHandler(
        IAccessTokenProvider accessTokenProvider,
        NavigationManager navigationManager,
        WebAppSettings settings)
        : base(accessTokenProvider, navigationManager)
    {
        ConfigureHandler(authorizedUrls: [settings.ApiBaseUrl]);
    }
}
