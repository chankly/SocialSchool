using HSoft.SocialSchool.Application.Queries.Friends;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// El resumen de amistades: amigos, solicitudes en cada dirección y cuentas invitables. Nadie se propone
    /// a sí mismo, ni repite amistad, ni vuelve a aparecer quien ya tiene una solicitud pendiente.
    /// </summary>
    public class GetFriendsQueryHandlerTests
    {
        private const string ExternalId = "keycloak-yo";

        [Fact]
        public async Task El_resumen_separa_amigos_solicitudes_recibidas_y_enviadas()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var friend = CreateUser("amigo", AccountType.Athlete);
            var requester = CreateUser("solicitante", AccountType.Athlete);
            var receiver = CreateUser("destino", AccountType.School);

            var friendshipRepository = new StubFriendshipRepository
            {
                Friendships = [Friendship.Create(Guid.NewGuid(), me.Id, friend.Id)]
            };

            var requestRepository = new StubFriendRequestRepository
            {
                PendingRequests =
                [
                    FriendRequest.Create(Guid.NewGuid(), requester.Id, me.Id),
                    FriendRequest.Create(Guid.NewGuid(), me.Id, receiver.Id)
                ]
            };

            var handler = CreateHandler(me, friend, requester, receiver, friendshipRepository, requestRepository);

            var result = await handler.Handle(new GetFriendsQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);

            var friendResponse = Assert.Single(result.Value.Friends);
            Assert.Equal(friend.Id, friendResponse.Id);

            var incoming = Assert.Single(result.Value.IncomingRequests);
            Assert.Equal(requester.Id, incoming.Requester.Id);

            var outgoing = Assert.Single(result.Value.OutgoingRequests);
            Assert.Equal(receiver.Id, outgoing.Receiver.Id);

            Assert.Empty(result.Value.Discoverable);
        }

        [Fact]
        public async Task Los_descubribles_excluyen_a_uno_mismo_los_amigos_y_los_pendientes()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var friend = CreateUser("amigo", AccountType.Athlete);
            var requester = CreateUser("solicitante", AccountType.Athlete);
            var receiver = CreateUser("destino", AccountType.School);
            var candidate = CreateUser("candidato", AccountType.Athlete);

            var friendshipRepository = new StubFriendshipRepository
            {
                Friendships = [Friendship.Create(Guid.NewGuid(), me.Id, friend.Id)]
            };

            var requestRepository = new StubFriendRequestRepository
            {
                PendingRequests =
                [
                    FriendRequest.Create(Guid.NewGuid(), requester.Id, me.Id),
                    FriendRequest.Create(Guid.NewGuid(), me.Id, receiver.Id)
                ]
            };

            var handler = CreateHandler(
                me,
                friend,
                requester,
                receiver,
                friendshipRepository,
                requestRepository,
                candidate);

            var result = await handler.Handle(new GetFriendsQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);

            var discoverable = Assert.Single(result.Value.Discoverable);
            Assert.Equal(candidate.Id, discoverable.Id);
            Assert.Equal("Bilbao", discoverable.City);
        }

        [Fact]
        public async Task Las_cuentas_excluidas_viajan_al_repositorio()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var friend = CreateUser("amigo", AccountType.Athlete);
            var requester = CreateUser("solicitante", AccountType.Athlete);
            var receiver = CreateUser("destino", AccountType.School);

            var friendshipRepository = new StubFriendshipRepository
            {
                Friendships = [Friendship.Create(Guid.NewGuid(), me.Id, friend.Id)]
            };

            var requestRepository = new StubFriendRequestRepository
            {
                PendingRequests =
                [
                    FriendRequest.Create(Guid.NewGuid(), requester.Id, me.Id),
                    FriendRequest.Create(Guid.NewGuid(), me.Id, receiver.Id)
                ]
            };

            var users = new StubUserRepository()
                .WithUsers(me, friend, requester, receiver);
            users.Discoverable = [me, friend, requester, receiver];

            var handler = new GetFriendsQueryHandler(
                new StubCurrentUser(ExternalId),
                users,
                friendshipRepository,
                requestRepository,
                new StubAthleteRepository(),
                new StubSchoolRepository());

            var result = await handler.Handle(new GetFriendsQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.Discoverable);

            var excluded = users.LastExcludedUserIds!;

            // Uno mismo no viaja en la lista: la consulta lo excluye con `user.Id != userId`, que es el
            // primer argumento del repositorio (UserRepository.GetDiscoverableReadOnlyAsync).
            Assert.DoesNotContain(me.Id, excluded);
            Assert.Contains(friend.Id, excluded);
            Assert.Contains(requester.Id, excluded);
            Assert.Contains(receiver.Id, excluded);
            Assert.Equal(20, users.LastDiscoverableLimit ?? 0);
        }

        [Fact]
        public async Task Un_token_sin_cuenta_de_negocio_devuelve_fallo()
        {
            var handler = new GetFriendsQueryHandler(
                new StubCurrentUser("keycloak-desconocido"),
                new StubUserRepository(),
                new StubFriendshipRepository(),
                new StubFriendRequestRepository(),
                new StubAthleteRepository(),
                new StubSchoolRepository());

            var result = await handler.Handle(new GetFriendsQuery(), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        private static GetFriendsQueryHandler CreateHandler(
            User me,
            User friend,
            User requester,
            User receiver,
            StubFriendshipRepository friendshipRepository,
            StubFriendRequestRepository requestRepository,
            params User[] discoverable)
        {
            var userRepository = new StubUserRepository()
                .WithUsers([me, friend, requester, receiver, .. discoverable]);

            userRepository.Discoverable = [me, friend, requester, receiver, .. discoverable];

            return new GetFriendsQueryHandler(
                new StubCurrentUser(me.ExternalIdentityId),
                userRepository,
                friendshipRepository,
                requestRepository,
                new StubAthleteRepository
                {
                    Athletes = discoverable
                        .Select(user => CreateAthlete(user, "Bilbao"))
                        .ToList()
                },
                new StubSchoolRepository());
        }

        private static Athlete CreateAthlete(User user, string city) =>
            Athlete.Create(user.Id, "Ada", "Lovelace", new DateOnly(1990, 1, 1), "España", city);

        private static User CreateUser(string nickname, AccountType accountType)
        {
            var user = User.Create(
                Guid.NewGuid(),
                $"{nickname}@example.com",
                accountType,
                "Google",
                $"keycloak-{nickname}",
                "es");

            user.SetNickname(nickname);

            return user;
        }
    }
}
