using HSoft.SocialSchool.Application.Commands.Friends;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// Deshacer una amistad: cualquiera de las dos partes puede hacerlo y la amistad desaparece para las dos
    /// porque es una sola fila simétrica.
    /// </summary>
    public class RemoveFriendCommandHandlerTests
    {
        [Fact]
        public async Task Cualquiera_de_las_dos_partes_puede_deshacer_la_amistad()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var other = CreateUser("otro", AccountType.Athlete);

            var friendship = Friendship.Create(Guid.NewGuid(), me.Id, other.Id);

            var friendshipRepository = new StubFriendshipRepository { Pair = friendship };

            var handler = CreateHandler(me, other, friendshipRepository);

            var result = await handler.Handle(new RemoveFriendCommand(other.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Same(friendship, Assert.Single(friendshipRepository.Deleted));
        }

        [Fact]
        public async Task Si_no_son_amigos_no_se_borra_nada()
        {
            var me = CreateUser("yo", AccountType.Athlete);
            var other = CreateUser("otro", AccountType.Athlete);

            var friendshipRepository = new StubFriendshipRepository();

            var handler = CreateHandler(me, other, friendshipRepository);

            var result = await handler.Handle(new RemoveFriendCommand(other.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(friendshipRepository.Deleted);
        }

        private static RemoveFriendCommandHandler CreateHandler(
            User authenticated,
            User other,
            StubFriendshipRepository friendshipRepository) =>
            new(
                new StubCurrentUser(authenticated.ExternalIdentityId),
                new StubUserRepository().WithUsers(authenticated, other),
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
