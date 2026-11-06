using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="User"/>.</summary>
    public interface IUserRepository : IBaseRepository<User>
    {
        /// <summary>
        /// Devuelve la cuenta cuya identidad externa coincide con la indicada —el <c>sub</c> del token—,
        /// o <see langword="null"/> si todavía no se ha materializado.
        /// </summary>
        Task<User?> GetByExternalIdReadOnlyAsync(string externalIdentityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si alguna cuenta lleva ya el nick indicado. La comparación la resuelve la base de datos, cuya
        /// colación no distingue mayúsculas; el índice único de <c>Users.Nickname</c> sigue siendo la garantía
        /// si dos peticiones coinciden.
        /// </summary>
        Task<bool> ExistsByNicknameAsync(string nickname, CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve las cuentas cuyo identificador está en la lista, sin rastrear. Las desconocidas
        /// simplemente no aparecen en el resultado.
        /// </summary>
        /// <param name="userIds">Identificadores que se quieren resolver.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<User>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> userIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve las cuentas a las que el usuario puede enviar una solicitud: activas, con nick, distintas
        /// de él y fuera de la lista de excluidas (amigos y solicitudes pendientes en cualquier dirección).
        /// Ordenadas por nick y con un tope fijo.
        /// </summary>
        /// <param name="userId">Cuenta del usuario autenticado.</param>
        /// <param name="excludedUserIds">Cuentas que no deben aparecer.</param>
        /// <param name="limit">Número máximo de cuentas a devolver.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<User>> GetDiscoverableReadOnlyAsync(
            Guid userId,
            IReadOnlyList<Guid> excludedUserIds,
            int limit,
            CancellationToken cancellationToken = default);
    }
}
