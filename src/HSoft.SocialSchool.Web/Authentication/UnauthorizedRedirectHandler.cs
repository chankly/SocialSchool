using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace HSoft.SocialSchool.Web.Authentication;

/// <summary>
/// Devuelve al login cuando el servidor rechaza las credenciales de la sesión: el token ha caducado
/// o se ha revocado y la petición ya no puede funcionar con él. La respuesta se devuelve intacta,
/// de modo que cada servicio la sigue traduciendo como hasta ahora.
/// </summary>
public sealed class UnauthorizedRedirectHandler(NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && IsAppRoute())
        {
            navigation.NavigateToLogin(
                "authentication/login",
                new InteractiveRequestOptions
                {
                    Interaction = InteractionType.SignIn,
                    ReturnUrl = CurrentReturnUrl()
                });
        }

        return response;
    }

    /// <summary>Las rutas de acceso no llaman al API; se excluyen para no encadenar redirecciones.</summary>
    private bool IsAppRoute() =>
        !CurrentRoute().StartsWith("authentication", StringComparison.OrdinalIgnoreCase);

    private string CurrentRoute() => navigation.ToBaseRelativePath(navigation.Uri);

    private string CurrentReturnUrl() => CurrentRoute() is { Length: > 0 } route ? route : "profile";
}
