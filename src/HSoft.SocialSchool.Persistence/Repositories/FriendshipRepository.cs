using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class FriendshipRepository(MyCompanyDbContext dbContext) : BaseRepository<Friendship>(dbContext), IFriendshipRepository
    {
        public async Task<IReadOnlyList<Friendship>> GetByUserIdReadOnlyAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<Friendship>()
                .AsNoTracking()
                .Where(friendship => friendship.UserAId == userId || friendship.UserBId == userId)
                .ToListAsync(cancellationToken);

        public Task<Friendship?> GetByPairReadOnlyAsync(
            Guid userId,
            Guid otherUserId,
            CancellationToken cancellationToken = default) =>
            _dbContext.Set<Friendship>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    friendship =>
                        (friendship.UserAId == userId && friendship.UserBId == otherUserId)
                        || (friendship.UserAId == otherUserId && friendship.UserBId == userId),
                    cancellationToken);

        public async Task AddWithRequestResolutionAsync(
            Friendship friendship,
            FriendRequest friendRequest,
            CancellationToken cancellationToken = default)
        {
            _dbContext.Add(friendship);

            // La solicitud se cargó rastreada y ya cambió de estado; se marca explícitamente para que el
            // SaveChanges escriba las dos cosas en la misma transacción.
            _dbContext.Entry(friendRequest).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
