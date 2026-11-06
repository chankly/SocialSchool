using HSoft.SocialSchool.Application.Commands.Friends;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// El envío de una solicitud de amistad: nace pendiente y se rechaza sin escribir nada si es a uno mismo,
    /// entre dos escuelas, si ya son amigos o si ya hay una solicitud pendiente en cualquier dirección.
    /// </summary>
    public class SendFriendRequestCommandHandlerTests
    {
        [Fact]
        public async Task Una_solicitud_correcta_nace_pendiente()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var other = CreateUser("otro", AccountType.Athlete);
            var requestRepository = new StubFriendRequestRepository();

            var handler = CreateHandler(me, other, new StubFriendshipRepository(), requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(other.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);

            var created = Assert.Single(requestRepository.Added);
            Assert.Equal(RequestStatus.Pending, created.Status);
            Assert.Equal(me.Id, created.RequesterId);
            Assert.Equal(other.Id, created.ReceiverId);
            Assert.Equal(created.Id, result.Value);
        }

        [Fact]
        public async Task Un_deportista_puede_invitar_a_una_escuela()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var school = CreateUser("escuela", AccountType.School);
            var requestRepository = new StubFriendRequestRepository();

            var handler = CreateHandler(me, school, new StubFriendshipRepository(), requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(school.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Single(requestRepository.Added);
        }

        [Fact]
        public async Task No_se_puede_enviar_una_solicitud_a_uno_mismo()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var requestRepository = new StubFriendRequestRepository();

            var handler = CreateHandler(me, null, new StubFriendshipRepository(), requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(me.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(requestRepository.Added);
        }

        [Fact]
        public async Task Dos_escuelas_no_pueden_ser_amigas()
        {
            var me = CreateUser("escuelauno", AccountType.School);
            var other = CreateUser("escuelados", AccountType.School);
            var requestRepository = new StubFriendRequestRepository();

            var handler = CreateHandler(me, other, new StubFriendshipRepository(), requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(other.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(requestRepository.Added);
        }

        [Fact]
        public async Task Si_ya_son_amigos_no_se_crea_la_solicitud()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var other = CreateUser("otro", AccountType.Athlete);
            var requestRepository = new StubFriendRequestRepository();

            var friendshipRepository = new StubFriendshipRepository
            {
                Pair = Friendship.Create(Guid.NewGuid(), me.Id, other.Id)
            };

            var handler = CreateHandler(me, other, friendshipRepository, requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(other.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(requestRepository.Added);
        }

        [Fact]
        public async Task Si_ya_hay_una_solicitud_pendiente_no_se_crea_otra()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var other = CreateUser("otro", AccountType.Athlete);

            var requestRepository = new StubFriendRequestRepository { PendingBetween = true };

            var handler = CreateHandler(me, other, new StubFriendshipRepository(), requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(other.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(requestRepository.Added);
        }

        [Fact]
        public async Task Un_destinatario_inexistente_devuelve_fallo()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var requestRepository = new StubFriendRequestRepository();

            var handler = CreateHandler(me, null, new StubFriendshipRepository(), requestRepository);

            var result = await handler.Handle(new SendFriendRequestCommand(Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(requestRepository.Added);
        }

        [Fact]
        public async Task Un_token_sin_cuenta_de_negocio_devuelve_fallo()
        {
            var handler = new SendFriendRequestCommandHandler(
                new StubCurrentUser("keycloak-desconocido"),
                new StubUserRepository(),
                new StubFriendshipRepository(),
                new StubFriendRequestRepository());

            var result = await handler.Handle(new SendFriendRequestCommand(Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        private static SendFriendRequestCommandHandler CreateHandler(
            User authenticated,
            User? receiver,
            StubFriendshipRepository friendshipRepository,
            StubFriendRequestRepository requestRepository)
        {
            var users = new StubUserRepository().WithUsers(authenticated);

            if (receiver is not null)
                users.WithUsers(receiver);

            return new SendFriendRequestCommandHandler(
                new StubCurrentUser(authenticated.ExternalIdentityId),
                users,
                friendshipRepository,
                requestRepository);
        }

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
