using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Users;

namespace HSoft.SocialSchool.Application.Services
{
    /// <summary>
    /// Resuelve la cuenta de negocio del usuario autenticado. El <c>sub</c> del token identifica la
    /// identidad del proveedor, no la cuenta de nuestra base de datos, así que hay que buscarla.
    /// </summary>
    public static class CurrentUserExtensions
    {
        /// <summary>
        /// Devuelve la cuenta del usuario autenticado, o <see langword="null"/> si el token no trae
        /// identidad o todavía no se ha materializado la cuenta.
        /// </summary>
        /// <param name="userRepository">Repositorio de cuentas.</param>
        /// <param name="currentUser">Identidad de la petición actual.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        public static async Task<User?> GetAuthenticatedAccountAsync(
            this IUserRepository userRepository,
            ICurrentUser currentUser,
            CancellationToken cancellationToken = default)
        {
            var externalId = currentUser.ExternalId;

            if (string.IsNullOrWhiteSpace(externalId))
                return null;

            return await userRepository.GetByExternalIdReadOnlyAsync(externalId, cancellationToken);
        }
    }
}
