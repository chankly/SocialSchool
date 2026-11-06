using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Attendances;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// Repositorios simulados para los casos de uso de escuelas y amistades. Los métodos que el caso de uso
    /// no debe llamar lanzan <see cref="NotSupportedException"/>; los que sí usa se configuran desde el test.
    /// </summary>
    internal sealed class StubCurrentUser(string? externalId) : ICurrentUser
    {
        public string? ExternalId => externalId;

        public bool IsAuthenticated => externalId is not null;
    }

    internal abstract class StubRepository<T> : IBaseRepository<T> where T : class, IAggregateRoot
    {
        public List<T> Added { get; } = [];

        public List<T> Updated { get; } = [];

        public List<T> Deleted { get; } = [];

        public virtual Task<T?> GetById(Guid id) => throw new NotSupportedException();

        public virtual Task<T?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Add(T entity)
        {
            Added.Add(entity);
            return Task.CompletedTask;
        }

        public Task AddRange(IReadOnlyList<T> entities) => throw new NotSupportedException();

        public Task AddRange<TEntity>(
            IReadOnlyList<TEntity> entities,
            CancellationToken cancellationToken = default) where TEntity : class =>
            throw new NotSupportedException();

        public Task Update(T entity)
        {
            Updated.Add(entity);
            return Task.CompletedTask;
        }

        public Task Delete(T entity)
        {
            Deleted.Add(entity);
            return Task.CompletedTask;
        }

        public Task Save(T entity) => throw new NotSupportedException();

        public Task Save(IReadOnlyList<T> entities) => throw new NotSupportedException();
    }

    internal sealed class StubUserRepository : StubRepository<User>, IUserRepository
    {
        private readonly Dictionary<Guid, User> _usersById = [];
        private readonly Dictionary<string, User> _usersByExternalId = [];

        public List<User> Discoverable { get; set; } = [];

        public IReadOnlyList<Guid>? LastExcludedUserIds { get; private set; }

        public int? LastDiscoverableLimit { get; private set; }

        public StubUserRepository WithUsers(params User[] users)
        {
            foreach (var user in users)
            {
                _usersById[user.Id] = user;

                if (!string.IsNullOrWhiteSpace(user.ExternalIdentityId))
                    _usersByExternalId[user.ExternalIdentityId] = user;
            }

            return this;
        }

        public Task<User?> GetByExternalIdReadOnlyAsync(
            string externalIdentityId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_usersByExternalId.GetValueOrDefault(externalIdentityId));

        public Task<bool> ExistsByNicknameAsync(
            string nickname,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public override Task<User?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usersById.GetValueOrDefault(id));

        public Task<IReadOnlyList<User>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> userIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<User>>(
                userIds.Where(_usersById.ContainsKey).Select(id => _usersById[id]).ToList());

        public Task<IReadOnlyList<User>> GetDiscoverableReadOnlyAsync(
            Guid userId,
            IReadOnlyList<Guid> excludedUserIds,
            int limit,
            CancellationToken cancellationToken = default)
        {
            LastExcludedUserIds = excludedUserIds;
            LastDiscoverableLimit = limit;

            IReadOnlyList<User> discoverable = Discoverable
                .Where(user => user.Id != userId)
                .Where(user => !excludedUserIds.Contains(user.Id))
                .Take(limit)
                .ToList();

            return Task.FromResult(discoverable);
        }
    }

    internal sealed class StubFriendshipRepository : StubRepository<Friendship>, IFriendshipRepository
    {
        public IReadOnlyList<Friendship> Friendships { get; set; } = [];

        public Friendship? Pair { get; set; }

        public List<(Friendship Friendship, FriendRequest Request)> Resolutions { get; } = [];

        public Task<IReadOnlyList<Friendship>> GetByUserIdReadOnlyAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Friendship>>(
                Friendships.Where(friendship => friendship.Involves(userId)).ToList());

        public Task<Friendship?> GetByPairReadOnlyAsync(
            Guid userId,
            Guid otherUserId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Pair);

        public Task AddWithRequestResolutionAsync(
            Friendship friendship,
            FriendRequest friendRequest,
            CancellationToken cancellationToken = default)
        {
            Resolutions.Add((friendship, friendRequest));
            return Task.CompletedTask;
        }
    }

    internal sealed class StubFriendRequestRepository : StubRepository<FriendRequest>, IFriendRequestRepository
    {
        public IReadOnlyList<FriendRequest> PendingRequests { get; set; } = [];

        public FriendRequest? Existing { get; set; }

        public bool PendingBetween { get; set; }

        public override Task<FriendRequest?> GetById(Guid id) => Task.FromResult(Existing);

        public Task<IReadOnlyList<FriendRequest>> GetPendingByUserIdReadOnlyAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(PendingRequests);

        public Task<bool> ExistsPendingBetweenAsync(
            Guid userId,
            Guid otherUserId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(PendingBetween);
    }

    internal sealed class StubSchoolRepository : StubRepository<School>, ISchoolRepository
    {
        public List<SchoolDirectoryItem> Directory { get; set; } = [];

        public School? ExistingSchool { get; set; }

        public IReadOnlyList<School> SchoolsByIds { get; set; } = [];

        public Guid? LastSportId { get; private set; }

        public string? LastCity { get; private set; }

        public string? LastSearch { get; private set; }

        public Task<IReadOnlyList<SchoolDirectoryItem>> GetDirectoryReadOnlyAsync(
            Guid? sportId,
            string? city,
            string? search,
            CancellationToken cancellationToken = default)
        {
            LastSportId = sportId;
            LastCity = city;
            LastSearch = search;

            IReadOnlyList<SchoolDirectoryItem> directory = Directory
                .Where(item => !sportId.HasValue || item.SportId == sportId.Value)
                .Where(item => string.IsNullOrWhiteSpace(city)
                    || string.Equals(item.City, city, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(search)
                    || item.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Name)
                .Take(50)
                .ToList();

            return Task.FromResult(directory);
        }

        public override Task<School?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingSchool?.Id == id ? ExistingSchool : null);

        public Task<School?> GetByIdWithServicesReadOnlyAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingSchool?.Id == id ? ExistingSchool : null);

        public Task<IReadOnlyList<School>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> schoolIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<School>>(
                SchoolsByIds.Where(school => schoolIds.Contains(school.Id)).ToList());
    }

    internal sealed class StubSchoolAttendanceRepository : StubRepository<SchoolAttendance>, ISchoolAttendanceRepository
    {
        public IReadOnlyList<SchoolAttendeeItem> Attendees { get; set; } = [];

        public Task<IReadOnlyList<SchoolAttendeeItem>> GetAcceptedAttendeesReadOnlyAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Attendees);
    }

    internal sealed class StubAthleteRepository : StubRepository<Athlete>, IAthleteRepository
    {
        public IReadOnlyList<Athlete> Athletes { get; set; } = [];

        public Task<Athlete?> GetByIdWithSportsReadOnlyAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Athlete>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> athleteIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Athlete>>(
                Athletes.Where(athlete => athleteIds.Contains(athlete.Id)).ToList());
    }

    internal sealed class StubSportRepository : StubRepository<Sport>, ISportRepository
    {
        public IReadOnlyList<Sport> Sports { get; set; } = [];

        public Task<IReadOnlyList<Sport>> GetActiveReadOnlyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Sports);

        public Task<IReadOnlyList<Sport>> GetByIdsAsync(
            IReadOnlyList<Guid> sportIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Sport>>(
                Sports.Where(sport => sportIds.Contains(sport.Id)).ToList());

        public override Task<Sport?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Sports.FirstOrDefault(sport => sport.Id == id));
    }
}
