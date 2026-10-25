namespace HSoft.SocialSchool.Web.Configuration;

/// <summary>
/// Configuración no sensible del cliente. Las credenciales de Keycloak nunca deben
/// viajar al navegador: aquí solo quedan los valores públicos del cliente OIDC.
/// </summary>
public sealed class WebAppSettings
{
    /// <summary>URL base de la API de SocialSchool.</summary>
    public string ApiBaseUrl { get; init; } = "https://localhost:7146";

    /// <summary>Indica si la aplicación usa los servicios simulados.</summary>
    public bool UseMocks { get; init; } = true;

    /// <summary>Authority pública del realm de Keycloak.</summary>
    public string KeycloakAuthority { get; init; } = "https://keycloak.example.com/realms/socialschool";

    /// <summary>Identificador público del cliente web.</summary>
    public string KeycloakClientId { get; init; } = "SocialSchool-Web";

    /// <summary>
    /// Audiencia que espera la API en el access token. Es el clientId público del cliente
    /// de la API en Keycloak (no es un secreto) y debe ajustarse por entorno.
    /// </summary>
    public string ApiAudience { get; init; } = "0B28A13A-DF06-4840-958A-CC33605B3EB7";
}
