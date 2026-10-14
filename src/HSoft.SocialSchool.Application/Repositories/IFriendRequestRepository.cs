using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="FriendRequest"/>.</summary>
    public interface IFriendRequestRepository : IBaseRepository<FriendRequest>
    {
    }
}
