using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Users;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class UserRepository(MyCompanyDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
    {
    }
}
