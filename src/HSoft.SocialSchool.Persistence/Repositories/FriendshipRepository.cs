using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Friendships;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class FriendshipRepository(MyCompanyDbContext dbContext) : BaseRepository<Friendship>(dbContext), IFriendshipRepository
    {
    }
}
