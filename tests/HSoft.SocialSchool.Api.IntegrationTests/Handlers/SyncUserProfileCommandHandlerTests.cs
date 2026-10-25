using HSoft.SocialSchool.Application.Commands.Registration;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// La sincronización de lo que el usuario cambia en Keycloak: solo correo e idioma, solo si cambian y sin
    /// tocar nunca el tipo de cuenta.
    /// </summary>
    public class SyncUserProfileCommandHandlerTests
    {
        private const string KeycloakUserId = "ba31e1e2-0d4a-4b44-a3a9-641cc4309e7c";

        [Fact]
        public async Task Cambia_el_idioma_preferido()
        {
            var user = CreateUser("es");
            var repository = new FakeUserRepository(user);

            var result = await Handle(repository, new SyncUserProfileCommand(KeycloakUserId, null, "en"));

            Assert.True(result.IsSuccess);
            Assert.Equal("en", user.PreferredLanguage);
            Assert.Single(repository.Updated);
        }

        [Fact]
        public async Task Cambia_el_correo()
        {
            var user = CreateUser("es");
            var repository = new FakeUserRepository(user);

            var result = await Handle(repository, new SyncUserProfileCommand(KeycloakUserId, "nuevo@example.com", null));

            Assert.True(result.IsSuccess);
            Assert.Equal("nuevo@example.com", user.Email);
            Assert.Single(repository.Updated);
        }

        [Fact]
        public async Task Sin_datos_que_sincronizar_no_escribe_nada()
        {
            var user = CreateUser("es");
            var repository = new FakeUserRepository(user);

            var result = await Handle(repository, new SyncUserProfileCommand(KeycloakUserId, null, null));

            Assert.True(result.IsSuccess);
            Assert.Empty(repository.Updated);
        }

        [Fact]
        public async Task Los_mismos_valores_no_escriben_nada()
        {
            var user = CreateUser("es");
            var repository = new FakeUserRepository(user);

            var result = await Handle(
                repository,
                new SyncUserProfileCommand(KeycloakUserId, "heviaincognito@gmail.com", "es"));

            Assert.True(result.IsSuccess);
            Assert.Empty(repository.Updated);
        }

        [Fact]
        public async Task Cuenta_desconocida_no_crea_nada_y_termina_bien()
        {
            var repository = new FakeUserRepository(null);

            var result = await Handle(repository, new SyncUserProfileCommand(KeycloakUserId, null, "en"));

            Assert.True(result.IsSuccess);
            Assert.Empty(repository.Updated);
            Assert.Empty(repository.Added);
        }

        [Fact]
        public async Task El_tipo_de_cuenta_no_cambia()
        {
            var user = CreateUser("es", AccountType.School);
            var repository = new FakeUserRepository(user);

            var result = await Handle(repository, new SyncUserProfileCommand(KeycloakUserId, "nuevo@example.com", "en"));

            Assert.True(result.IsSuccess);
            Assert.Equal(AccountType.School, user.AccountType);
            Assert.Equal(AccountStatus.PendingValidation, user.AccountStatus);
        }

        private static ValueTask<CSharpFunctionalExtensions.Result> Handle(
            FakeUserRepository repository,
            SyncUserProfileCommand command) =>
            new SyncUserProfileCommandHandler(repository).Handle(command, CancellationToken.None);

        private static User CreateUser(string preferredLanguage, AccountType accountType = AccountType.Athlete) =>
            User.Create(
                Guid.NewGuid(),
                "heviaincognito@gmail.com",
                accountType,
                "google",
                KeycloakUserId,
                preferredLanguage);

        /// <summary>Los métodos que la sincronización no usa fallan si se llaman, para que el test lo delate.</summary>
        private abstract class FakeRepository<T> : IBaseRepository<T> where T : class, IAggregateRoot
        {
            public Task<T?> GetById(Guid id) => throw new NotSupportedException();

            public virtual Task<T?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public virtual Task Add(T entity) => throw new NotSupportedException();

            public Task AddRange(IReadOnlyList<T> entities) => throw new NotSupportedException();

            public Task AddRange<TEntity>(
                IReadOnlyList<TEntity> entities,
                CancellationToken cancellationToken = default) where TEntity : class =>
                throw new NotSupportedException();

            public virtual Task Update(T entity) => throw new NotSupportedException();

            public Task Delete(T entity) => throw new NotSupportedException();

            public Task Save(T entity) => throw new NotSupportedException();

            public Task Save(IReadOnlyList<T> entities) => throw new NotSupportedException();
        }

        private sealed class FakeUserRepository(User? existing) : FakeRepository<User>, IUserRepository
        {
            public List<User> Added { get; } = [];

            public List<User> Updated { get; } = [];

            public Task<User?> GetByExternalIdReadOnlyAsync(
                string externalIdentityId,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(existing?.ExternalIdentityId == externalIdentityId ? existing : null);

            public override Task Add(User entity)
            {
                Added.Add(entity);
                return Task.CompletedTask;
            }

            public override Task Update(User entity)
            {
                Updated.Add(entity);
                return Task.CompletedTask;
            }
        }
    }
}
