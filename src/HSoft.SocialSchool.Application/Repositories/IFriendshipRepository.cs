using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Friendship"/>.</summary>
    public interface IFriendshipRepository : IBaseRepository<Friendship>
    {
    }
}
