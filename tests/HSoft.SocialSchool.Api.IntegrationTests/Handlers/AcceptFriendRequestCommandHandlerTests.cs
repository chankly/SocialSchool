using HSoft.SocialSchool.Application.Commands.Friends;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// La resolución de una solicitud: solo el destinatario acepta, solo mientras siga pendiente y la
    /// amistad se crea junto con la solicitud aceptada.
    /// </summary>
    public class AcceptFriendRequestCommandHandlerTests
    {
        [Fact]
        public async Task Aceptar_crea_la_amistad_y_resuelve_la_solicitud()
        {
            var requester = CreateUser("solicitante", AccountType.Athlete);
            var receiver = CreateUser("destino", AccountType.School);
            var friendRequest = FriendRequest.Create(Guid.NewGuid(), requester.Id, receiver.Id);

            var requestRepository = new StubFriendRequestRepository { Existing = friendRequest };
            var friendshipRepository = new StubFriendshipRepository();

            var handler = CreateHandler(receiver, requester, requestRepository, friendshipRepository);

            var result = await handler.Handle(new AcceptFriendRequestCommand(friendRequest.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(RequestStatus.Accepted, friendRequest.Status);

            var resolution = Assert.Single(friendshipRepository.Resolutions);
            Assert.True(resolution.Friendship.Involves(requester.Id));
            Assert.True(resolution.Friendship.Involves(receiver.Id));
            Assert.Same(friendRequest, resolution.Request);
        }

        [Fact]
        public async Task Un_tercero_no_puede_aceptar_una_solicitud_ajena()
        {
            var requester = CreateUser("solicitante", AccountType.Athlete);
            var receiver = CreateUser("destino", AccountType.Athlete);
            var intruder = CreateUser("intruso", AccountType.Athlete);

            var friendRequest = FriendRequest.Create(Guid.NewGuid(), requester.Id, receiver.Id);

            var requestRepository = new StubFriendRequestRepository { Existing = friendRequest };
            var friendshipRepository = new StubFriendshipRepository();

            var handler = CreateHandler(intruder, requester, requestRepository, friendshipRepository);

            var result = await handler.Handle(new AcceptFriendRequestCommand(friendRequest.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(RequestStatus.Pending, friendRequest.Status);
            Assert.Empty(friendshipRepository.Resolutions);
        }

        [Fact]
        public async Task Una_solicitud_ya_resuelta_no_se_puede_aceptar()
        {
            var requester = CreateUser("solicitante", AccountType.Athlete);
            var receiver = CreateUser("destino", AccountType.Athlete);

            var friendRequest = FriendRequest.Create(Guid.NewGuid(), requester.Id, receiver.Id);
            friendRequest.Reject();

            var requestRepository = new StubFriendRequestRepository { Existing = friendRequest };
            var friendshipRepository = new StubFriendshipRepository();

            var handler = CreateHandler(receiver, requester, requestRepository, friendshipRepository);

            var result = await handler.Handle(new AcceptFriendRequestCommand(friendRequest.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(RequestStatus.Rejected, friendRequest.Status);
            Assert.Empty(friendshipRepository.Resolutions);
        }

        [Fact]
        public async Task Una_solicitud_inexistente_devuelve_fallo()
        {
            var receiver = CreateUser("destino", AccountType.Athlete);
            var friendshipRepository = new StubFriendshipRepository();

            var handler = CreateHandler(
                receiver,
                receiver,
                new StubFriendRequestRepository(),
                friendshipRepository);

            var result = await handler.Handle(new AcceptFriendRequestCommand(Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(friendshipRepository.Resolutions);
        }

        private static AcceptFriendRequestCommandHandler CreateHandler(
            User authenticated,
            User requester,
            StubFriendRequestRepository requestRepository,
            StubFriendshipRepository friendshipRepository) =>
            new(
                new StubCurrentUser(authenticated.ExternalIdentityId),
                new StubUserRepository().WithUsers(authenticated, requester),
                requestRepository,
                friendshipRepository);

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
