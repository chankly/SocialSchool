using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Friendship"/>.</summary>
    public interface IFriendshipRepository : IBaseRepository<Friendship>
    {
        /// <summary>
        /// Devuelve las amistades en las que participa la cuenta, sin rastrear.
        /// </summary>
        /// <param name="userId">Cuenta de la que se quieren las amistades.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<Friendship>> GetByUserIdReadOnlyAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve la amistad entre las dos cuentas, en cualquier orden, o <see langword="null"/> si no
        /// existe. La amistad es simétrica, así que el orden de los identificadores no importa.
        /// </summary>
        /// <param name="userId">Una de las dos cuentas.</param>
        /// <param name="otherUserId">La otra cuenta.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<Friendship?> GetByPairReadOnlyAsync(
            Guid userId,
            Guid otherUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda la amistad recién creada y, en el mismo <c>SaveChanges</c>, la solicitud aceptada que la
        /// origina: o quedan las dos cosas o no queda ninguna.
        /// </summary>
        /// <param name="friendship">Amistad nueva.</param>
        /// <param name="friendRequest">Solicitud que se acaba de aceptar.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task AddWithRequestResolutionAsync(
            Friendship friendship,
            FriendRequest friendRequest,
            CancellationToken cancellationToken = default);
    }
}
