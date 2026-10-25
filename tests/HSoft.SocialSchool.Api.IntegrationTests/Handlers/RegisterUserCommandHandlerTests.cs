using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Commands.Registration;
using HSoft.SocialSchool.Application.Models.Registration;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// El alta de la cuenta desde el evento de registro: mapea el perfil de Keycloak, no crea el perfil, es
    /// idempotente y no se inventa el tipo de cuenta.
    /// </summary>
    public class RegisterUserCommandHandlerTests
    {
        private const string KeycloakUserId = "ba31e1e2-0d4a-4b44-a3a9-641cc4309e7c";

        [Fact]
        public async Task Deportista_nuevo_crea_la_cuenta_sin_nick()
        {
            var repository = new FakeUserRepository(null);

            var result = await Handler(repository, Profile("Athlete"))
                .Handle(new RegisterUserCommand(KeycloakUserId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            var user = Assert.Single(repository.Added);
            Assert.Equal(Guid.Parse(KeycloakUserId), user.Id);
            Assert.Equal(AccountType.Athlete, user.AccountType);
            Assert.Equal(AccountStatus.Active, user.AccountStatus);
            Assert.Equal(KeycloakUserId, user.ExternalIdentityId);
            Assert.Equal("heviaincognito@gmail.com", user.Email);
            Assert.Equal("google", user.IdentityProvider);
            Assert.Equal("en", user.PreferredLanguage);
            Assert.Null(user.Nickname);
        }

        [Fact]
        public async Task Escuela_nueva_queda_pendiente_de_validacion()
        {
            var repository = new FakeUserRepository(null);

            var result = await Handler(repository, Profile("School"))
                .Handle(new RegisterUserCommand(KeycloakUserId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            var user = Assert.Single(repository.Added);
            Assert.Equal(AccountType.School, user.AccountType);
            Assert.Equal(AccountStatus.PendingValidation, user.AccountStatus);
        }

        [Fact]
        public async Task El_tipo_de_cuenta_en_espanol_tambien_se_acepta()
        {
            var repository = new FakeUserRepository(null);

            var result = await Handler(repository, Profile("Deportista"))
                .Handle(new RegisterUserCommand(KeycloakUserId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(AccountType.Athlete, Assert.Single(repository.Added).AccountType);
        }

        [Fact]
        public async Task La_escuela_en_espanol_queda_pendiente_de_validacion()
        {
            var repository = new FakeUserRepository(null);

            var result = await Handler(repository, Profile("Escuela"))
                .Handle(new RegisterUserCommand(KeycloakUserId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            var user = Assert.Single(repository.Added);
            Assert.Equal(AccountType.School, user.AccountType);
            Assert.Equal(AccountStatus.PendingValidation, user.AccountStatus);
        }

        [Fact]
        public async Task Cuenta_existente_no_crea_otra()
        {
            var existing = User.Create(Guid.NewGuid(), "heviaincognito@gmail.com", AccountType.Athlete, "google", KeycloakUserId, "en");
            var repository = new FakeUserRepository(existing);

            var result = await Handler(repository, Profile("Athlete"))
                .Handle(new RegisterUserCommand(KeycloakUserId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(repository.Added);
        }

        [Fact]
        public async Task Perfil_sin_tipo_de_cuenta_devuelve_fallo_y_no_crea_nada()
        {
            var repository = new FakeUserRepository(null);
            var profile = new KeycloakUserProfile(KeycloakUserId, "heviaincognito@gmail.com", null, "google", "en");

            var result = await Handler(repository, profile)
                .Handle(new RegisterUserCommand(KeycloakUserId), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(repository.Added);
        }

        [Fact]
        public async Task Perfil_inexistente_en_keycloak_devuelve_fallo()
        {
            var repository = new FakeUserRepository(null);
            var unknownUser = Guid.NewGuid().ToString();

            var result = await Handler(repository, Profile("Athlete"))
                .Handle(new RegisterUserCommand(unknownUser), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(repository.Added);
        }

        [Fact]
        public async Task Perfil_con_identificador_que_no_es_guid_devuelve_fallo()
        {
            var repository = new FakeUserRepository(null);
            var profile = new KeycloakUserProfile("no-es-un-guid", "heviaincognito@gmail.com", "Athlete", "google", "en");

            var result = await Handler(repository, profile)
                .Handle(new RegisterUserCommand("no-es-un-guid"), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(repository.Added);
        }

        private static KeycloakUserProfile Profile(string userType) =>
            new(KeycloakUserId, "heviaincognito@gmail.com", userType, "google", "en");

        private static RegisterUserCommandHandler Handler(FakeUserRepository repository, KeycloakUserProfile profile) =>
            new(repository, new FakeKeycloakUsersApi(profile));

        private sealed class FakeKeycloakUsersApi(KeycloakUserProfile profile) : IKeycloakUsersApi
        {
            public Task<Result<KeycloakUserProfile?>> GetUserAsync(
                string keycloakUserId,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(Result.Success<KeycloakUserProfile?>(
                    keycloakUserId == profile.Id ? profile : null));
        }

        /// <summary>Los métodos que el alta no usa fallan si se llaman, para que el test lo delate.</summary>
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

            public Task Update(T entity) => throw new NotSupportedException();

            public Task Delete(T entity) => throw new NotSupportedException();

            public Task Save(T entity) => throw new NotSupportedException();

            public Task Save(IReadOnlyList<T> entities) => throw new NotSupportedException();
        }

        private sealed class FakeUserRepository(User? existing) : FakeRepository<User>, IUserRepository
        {
            public List<User> Added { get; } = [];

            public Task<User?> GetByExternalIdReadOnlyAsync(
                string externalIdentityId,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(existing?.ExternalIdentityId == externalIdentityId ? existing : null);

            public override Task Add(User entity)
            {
                Added.Add(entity);
                return Task.CompletedTask;
            }
        }
    }
}
