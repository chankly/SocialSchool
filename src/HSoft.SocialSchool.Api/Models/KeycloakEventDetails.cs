namespace HSoft.SocialSchool.Api.Models
{
    /// <summary>Datos de autenticación de un evento de Keycloak.</summary>
    /// <param name="UserId">Identificador del usuario en Keycloak, que es el que se usa para pedir su perfil.</param>
    public sealed record KeycloakEventDetails(string? UserId);
}
