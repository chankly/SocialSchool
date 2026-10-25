using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Registration;

namespace HSoft.SocialSchool.Application.Services
{
    /// <summary>
    /// Lectura de usuarios de Keycloak a través de su Admin API. El contrato no menciona HTTP ni tokens:
    /// el token de <c>client_credentials</c> es cosa de la implementación.
    /// </summary>
    public interface IKeycloakUsersApi
    {
        /// <summary>
        /// Devuelve el perfil del usuario en Keycloak, o <see langword="null"/> si el usuario no existe.
        /// Un fallo del resultado significa que no se ha podido consultar, no que no exista.
        /// </summary>
        Task<Result<KeycloakUserProfile?>> GetUserAsync(string keycloakUserId, CancellationToken cancellationToken = default);
    }
}
