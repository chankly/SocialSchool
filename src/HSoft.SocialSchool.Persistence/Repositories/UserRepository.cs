using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class UserRepository(MyCompanyDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
    {
        public Task<User?> GetByExternalIdReadOnlyAsync(
            string externalIdentityId,
            CancellationToken cancellationToken = default) =>
            _dbContext.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.ExternalIdentityId == externalIdentityId, cancellationToken);
    }
}
