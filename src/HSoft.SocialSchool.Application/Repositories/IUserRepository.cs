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
    }
}
