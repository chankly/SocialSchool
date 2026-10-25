using HSoft.SocialSchool.Domain.Enumerations;

namespace HSoft.SocialSchool.Application.Models.Registration
{
    /// <summary>
    /// Datos del usuario en Keycloak que hacen falta para materializar la cuenta de negocio. Se devuelven
    /// tal y como los da la Admin API: el tipo de cuenta llega como texto y lo interpreta el caso de uso.
    /// </summary>
    /// <param name="Id">Identificador del usuario en Keycloak, que se guarda como identidad externa.</param>
    /// <param name="Email">Correo del usuario.</param>
    /// <param name="UserType">Valor del atributo <c>userType</c>: el tipo de cuenta que eligió al registrarse.</param>
    /// <param name="IdentityProvider">Proveedor de identidad federado (por ejemplo <c>google</c>).</param>
    /// <param name="Locale">Idioma preferido, si Keycloak lo tiene.</param>
    public sealed record KeycloakUserProfile(
        string Id,
        string Email,
        string? UserType,
        string? IdentityProvider,
        string? Locale);
}
