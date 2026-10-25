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
    }
}
