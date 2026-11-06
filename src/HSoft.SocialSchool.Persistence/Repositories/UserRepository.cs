using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
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

        public Task<bool> ExistsByNicknameAsync(
            string nickname,
            CancellationToken cancellationToken = default) =>
            _dbContext.Set<User>()
                .AsNoTracking()
                .AnyAsync(user => user.Nickname == nickname, cancellationToken);

        public async Task<IReadOnlyList<User>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> userIds,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<User>()
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<User>> GetDiscoverableReadOnlyAsync(
            Guid userId,
            IReadOnlyList<Guid> excludedUserIds,
            int limit,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<User>()
                .AsNoTracking()
                .Where(user =>
                    user.AccountStatus == AccountStatus.Active
                    && user.Id != userId
                    && user.Nickname != null
                    && !excludedUserIds.Contains(user.Id))
                .OrderBy(user => user.Nickname)
                .Take(limit)
                .ToListAsync(cancellationToken);
    }
}
