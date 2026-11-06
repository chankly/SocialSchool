namespace HSoft.SocialSchool.Web.Configuration;

/// <summary>
/// Configuración no sensible del cliente. Las credenciales de Keycloak nunca deben
/// viajar al navegador: aquí solo quedan los valores públicos del cliente OIDC.
/// </summary>
public sealed class WebAppSettings
{
    /// <summary>URL base de la API de SocialSchool. Vacía mientras se usa el modo demo.</summary>
    public string ApiBaseUrl { get; init; } = string.Empty;

    /// <summary>Authority pública del realm de Keycloak. Vacía mientras se usa el modo demo.</summary>
    public string OidcAuthority { get; init; } = string.Empty;

    /// <summary>
    /// Identificador del cliente público (PKCE) de la web. No es un secreto y no debe
    /// confundirse con la audiencia de la API.
    /// </summary>
    public string OidcClientId { get; init; } = string.Empty;

    /// <summary>
    /// Indica si hay configuración suficiente para hablar con la API y con Keycloak.
    /// Basta con dejar vacío cualquiera de los tres valores para volver al modo demo.
    /// </summary>
    public bool IsApiConfigured =>
        !string.IsNullOrWhiteSpace(ApiBaseUrl)
        && Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out _)
        && !string.IsNullOrWhiteSpace(OidcAuthority)
        && !string.IsNullOrWhiteSpace(OidcClientId);

    /// <summary>Indica si la aplicación usa los servicios simulados y la sesión de demo.</summary>
    public bool UseMocks => !IsApiConfigured;
}
