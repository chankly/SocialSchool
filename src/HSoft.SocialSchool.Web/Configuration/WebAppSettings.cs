namespace HSoft.SocialSchool.Web.Configuration;

/// <summary>
/// Configuración no sensible del cliente. Las credenciales de Keycloak nunca deben
/// viajar al navegador: aquí solo quedan los valores públicos del cliente OIDC.
/// </summary>
public sealed class WebAppSettings
{
    /// <summary>URL base de la API de SocialSchool.</summary>
    public string ApiBaseUrl { get; init; } = string.Empty;

    /// <summary>Authority pública del realm de Keycloak.</summary>
    public string OidcAuthority { get; init; } = string.Empty;

    /// <summary>
    /// Identificador del cliente público (PKCE) de la web. No es un secreto y no debe
    /// confundirse con la audiencia de la API.
    /// </summary>
    public string OidcClientId { get; init; } = string.Empty;

    /// <summary>
    /// Indica si hay configuración suficiente para hablar con la API y con Keycloak.
    /// Sin ella no se registra ningún servicio de datos: la aplicación solo muestra el aviso.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiBaseUrl)
        && Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out _)
        && !string.IsNullOrWhiteSpace(OidcAuthority)
        && !string.IsNullOrWhiteSpace(OidcClientId);

    /// <summary>
    /// Claves que faltan o no son válidas, separadas por comas. Se muestra al usuario
    /// en lugar de cualquier dato de ejemplo.
    /// </summary>
    public string MissingConfiguration
    {
        get
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(ApiBaseUrl))
            {
                missing.Add("ApiBaseUrl");
            }
            else if (!Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out _))
            {
                missing.Add("ApiBaseUrl (no es una URL absoluta)");
            }

            if (string.IsNullOrWhiteSpace(OidcAuthority))
            {
                missing.Add("Oidc:Authority");
            }

            if (string.IsNullOrWhiteSpace(OidcClientId))
            {
                missing.Add("Oidc:ClientId");
            }

            return string.Join(", ", missing);
        }
    }
}
