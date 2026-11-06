using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="FriendRequest"/>.</summary>
    public interface IFriendRequestRepository : IBaseRepository<FriendRequest>
    {
        /// <summary>
        /// Devuelve las solicitudes pendientes en las que participa la cuenta, en cualquier dirección, sin
        /// rastrear. Las resueltas —aceptadas, rechazadas o canceladas— no se devuelven.
        /// </summary>
        /// <param name="userId">Cuenta de la que se quieren las solicitudes.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<FriendRequest>> GetPendingByUserIdReadOnlyAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si las dos cuentas ya tienen una solicitud pendiente en cualquier dirección. Es la
        /// comprobación que impide duplicar la solicitud.
        /// </summary>
        /// <param name="userId">Una de las dos cuentas.</param>
        /// <param name="otherUserId">La otra cuenta.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<bool> ExistsPendingBetweenAsync(
            Guid userId,
            Guid otherUserId,
            CancellationToken cancellationToken = default);
    }
}
