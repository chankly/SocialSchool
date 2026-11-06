using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class FriendRequestRepository(MyCompanyDbContext dbContext) : BaseRepository<FriendRequest>(dbContext), IFriendRequestRepository
    {
        public async Task<IReadOnlyList<FriendRequest>> GetPendingByUserIdReadOnlyAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<FriendRequest>()
                .AsNoTracking()
                .Where(friendRequest =>
                    friendRequest.Status == RequestStatus.Pending
                    && (friendRequest.RequesterId == userId || friendRequest.ReceiverId == userId))
                .ToListAsync(cancellationToken);

        public Task<bool> ExistsPendingBetweenAsync(
            Guid userId,
            Guid otherUserId,
            CancellationToken cancellationToken = default) =>
            _dbContext.Set<FriendRequest>()
                .AsNoTracking()
                .AnyAsync(
                    friendRequest =>
                        friendRequest.Status == RequestStatus.Pending
                        && ((friendRequest.RequesterId == userId && friendRequest.ReceiverId == otherUserId)
                            || (friendRequest.RequesterId == otherUserId && friendRequest.ReceiverId == userId)),
                    cancellationToken);
    }
}
