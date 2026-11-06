using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Friends;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Friends
{
    /// <summary>
    /// Compone las listas de amistades del usuario autenticado. Las cuentas invitables excluyen al propio
    /// usuario, a sus amigos y a quien ya tiene una solicitud pendiente con él en cualquier dirección; el
    /// bloqueo no se consulta todavía porque no hay forma de crear bloqueos.
    /// </summary>
    public sealed class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, Result<FriendsResponse>>
    {
        private const int DiscoverableLimit = 20;

        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IAthleteRepository _athleteRepository;
        private readonly ISchoolRepository _schoolRepository;

        public GetFriendsQueryHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IFriendshipRepository friendshipRepository,
            IFriendRequestRepository friendRequestRepository,
            IAthleteRepository athleteRepository,
            ISchoolRepository schoolRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _friendRequestRepository = friendRequestRepository;
            _athleteRepository = athleteRepository;
            _schoolRepository = schoolRepository;
        }

        public async ValueTask<Result<FriendsResponse>> Handle(
            GetFriendsQuery request,
            CancellationToken cancellationToken)
        {
            var account = await _userRepository.GetAuthenticatedAccountAsync(_currentUser, cancellationToken);

            if (account is null)
                return Result.Failure<FriendsResponse>("The authenticated user has no business account.");

            var friendships = await _friendshipRepository.GetByUserIdReadOnlyAsync(account.Id, cancellationToken);
            var friendIds = friendships
                .Select(friendship => friendship.UserAId == account.Id ? friendship.UserBId : friendship.UserAId)
                .Distinct()
                .ToList();

            var pendingRequests = await _friendRequestRepository.GetPendingByUserIdReadOnlyAsync(
                account.Id,
                cancellationToken);

            var incomingRequests = pendingRequests
                .Where(friendRequest => friendRequest.ReceiverId == account.Id)
                .ToList();

            var outgoingRequests = pendingRequests
                .Where(friendRequest => friendRequest.RequesterId == account.Id)
                .ToList();

            var relatedUserIds = friendIds
                .Concat(incomingRequests.Select(friendRequest => friendRequest.RequesterId))
                .Concat(outgoingRequests.Select(friendRequest => friendRequest.ReceiverId))
                .Distinct()
                .ToList();

            var relatedUsers = await _userRepository.GetByIdsReadOnlyAsync(relatedUserIds, cancellationToken);
            var usersById = relatedUsers.ToDictionary(user => user.Id);

            var friends = friendIds
                .Where(usersById.ContainsKey)
                .Select(userId => usersById[userId])
                .OrderBy(user => user.Nickname, StringComparer.OrdinalIgnoreCase)
                .Select(ToAccountResponse)
                .ToList();

            var incoming = incomingRequests
                .Where(friendRequest => usersById.ContainsKey(friendRequest.RequesterId))
                .OrderBy(friendRequest => friendRequest.RequestedAt)
                .Select(friendRequest => new IncomingFriendRequestResponse(
                    friendRequest.Id,
                    ToAccountResponse(usersById[friendRequest.RequesterId]),
                    friendRequest.RequestedAt))
                .ToList();

            var outgoing = outgoingRequests
                .Where(friendRequest => usersById.ContainsKey(friendRequest.ReceiverId))
                .OrderBy(friendRequest => friendRequest.RequestedAt)
                .Select(friendRequest => new OutgoingFriendRequestResponse(
                    friendRequest.Id,
                    ToAccountResponse(usersById[friendRequest.ReceiverId]),
                    friendRequest.RequestedAt))
                .ToList();

            var excludedUserIds = friendIds
                .Concat(incomingRequests.Select(friendRequest => friendRequest.RequesterId))
                .Concat(outgoingRequests.Select(friendRequest => friendRequest.ReceiverId))
                .Distinct()
                .ToList();

            var discoverableUsers = await _userRepository.GetDiscoverableReadOnlyAsync(
                account.Id,
                excludedUserIds,
                DiscoverableLimit,
                cancellationToken);

            var discoverable = await BuildDiscoverableAsync(discoverableUsers, cancellationToken);

            return Result.Success(new FriendsResponse(friends, incoming, outgoing, discoverable));
        }

        private async Task<IReadOnlyList<DiscoverableAccountResponse>> BuildDiscoverableAsync(
            IReadOnlyList<User> discoverableUsers,
            CancellationToken cancellationToken)
        {
            if (discoverableUsers.Count == 0)
                return [];

            var discoverableIds = discoverableUsers.Select(user => user.Id).ToList();

            // La ciudad vive en el perfil, y cada tipo de cuenta tiene la suya: hay que resolver las dos.
            var athleteCities = (await _athleteRepository.GetByIdsReadOnlyAsync(discoverableIds, cancellationToken))
                .ToDictionary(athlete => athlete.Id, athlete => athlete.City);

            var schoolCities = (await _schoolRepository.GetByIdsReadOnlyAsync(discoverableIds, cancellationToken))
                .ToDictionary(school => school.Id, school => school.City);

            return discoverableUsers
                .Select(user => new DiscoverableAccountResponse(
                    user.Id,
                    user.Nickname,
                    user.AccountType,
                    ResolveCity(user, athleteCities, schoolCities)))
                .ToList();
        }

        private static string? ResolveCity(
            User user,
            IReadOnlyDictionary<Guid, string> athleteCities,
            IReadOnlyDictionary<Guid, string> schoolCities) =>
            user.AccountType switch
            {
                AccountType.Athlete => athleteCities.GetValueOrDefault(user.Id),
                AccountType.School => schoolCities.GetValueOrDefault(user.Id),
                _ => null
            };

        private static FriendAccountResponse ToAccountResponse(User user) =>
            new(user.Id, user.Nickname, user.AccountType);
    }
}
