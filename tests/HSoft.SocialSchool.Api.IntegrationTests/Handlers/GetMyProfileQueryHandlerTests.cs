using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Queries.Profiles;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// El handler de la consulta del perfil propio, con repositorios simulados: el caso "no hay perfil"
    /// tiene que salir como éxito sin valor, y el tipo de cuenta tiene que decidir qué bloque se construye.
    /// </summary>
    public class GetMyProfileQueryHandlerTests
    {
        private const string ExternalId = "keycloak-sub";

        [Fact]
        public async Task Deportista_con_perfil_devuelve_su_bloque_y_sus_deportes()
        {
            var user = CreateUser(AccountType.Athlete);
            var athlete = Athlete.Create(user.Id, "Ada", "Lovelace", new DateOnly(1990, 1, 1), "ES", "Madrid");
            athlete.AddPracticedSport(Sport.Create(Guid.NewGuid(), "Surf"));

            var result = await CreateHandler(user, athlete, null)
                .Handle(new GetMyProfileQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            var profile = Assert.IsType<MyProfileResponse>(result.Value);
            Assert.Equal(AccountType.Athlete, profile.AccountType);
            Assert.Null(profile.School);

            var block = Assert.IsType<AthleteProfileResponse>(profile.Athlete);
            Assert.Equal("Ada", block.FirstName);
            var sport = Assert.Single(block.PracticedSports);
            Assert.Equal("Surf", sport.Name);
        }

        [Fact]
        public async Task Escuela_con_perfil_devuelve_su_bloque_con_el_deporte()
        {
            var user = CreateUser(AccountType.School);
            var sportId = Guid.NewGuid();
            var school = School.Create(user.Id, "Escuela del Norte", sportId, "ES", "Bilbao");

            var result = await CreateHandler(user, null, school)
                .Handle(new GetMyProfileQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            var profile = Assert.IsType<MyProfileResponse>(result.Value);
            Assert.Equal(AccountType.School, profile.AccountType);
            Assert.Null(profile.Athlete);

            var block = Assert.IsType<SchoolProfileResponse>(profile.School);
            Assert.Equal("Escuela del Norte", block.Name);
            Assert.Equal(sportId, block.SportId);
        }

        [Fact]
        public async Task Cuenta_sin_perfil_devuelve_exito_sin_valor()
        {
            var user = CreateUser(AccountType.Athlete);

            var result = await CreateHandler(user, null, null)
                .Handle(new GetMyProfileQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Sin_cuenta_de_negocio_devuelve_exito_sin_valor()
        {
            var result = await CreateHandler(null, null, null)
                .Handle(new GetMyProfileQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Token_sin_identidad_externa_devuelve_fallo()
        {
            var result = await CreateHandler(CreateUser(AccountType.Athlete), null, null, externalId: null)
                .Handle(new GetMyProfileQuery(), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        private static GetMyProfileQueryHandler CreateHandler(
            User? user,
            Athlete? athlete,
            School? school,
            string? externalId = ExternalId) =>
            new(
                new FakeCurrentUser(externalId),
                new FakeUserRepository(user),
                new FakeAthleteRepository(athlete),
                new FakeSchoolRepository(school));

        private static User CreateUser(AccountType accountType) =>
            User.Create(Guid.NewGuid(), "ada@example.com", "ada", accountType, "Google", ExternalId, "es");

        private sealed class FakeCurrentUser(string? externalId) : ICurrentUser
        {
            public string? ExternalId => externalId;

            public bool IsAuthenticated => externalId is not null;
        }

        /// <summary>Los métodos que la consulta no usa fallan si se llaman, para que el test lo delate.</summary>
        private abstract class FakeRepository<T> : IBaseRepository<T> where T : class, IAggregateRoot
        {
            public Task<T?> GetById(Guid id) => throw new NotSupportedException();

            public virtual Task<T?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task Add(T entity) => throw new NotSupportedException();

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

        private sealed class FakeUserRepository(User? user) : FakeRepository<User>, IUserRepository
        {
            public Task<User?> GetByExternalIdReadOnlyAsync(
                string externalIdentityId,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(externalIdentityId == ExternalId ? user : null);
        }

        private sealed class FakeAthleteRepository(Athlete? athlete) : FakeRepository<Athlete>, IAthleteRepository
        {
            public Task<Athlete?> GetByIdWithSportsReadOnlyAsync(
                Guid id,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(athlete?.Id == id ? athlete : null);
        }

        private sealed class FakeSchoolRepository(School? school) : FakeRepository<School>, ISchoolRepository
        {
            public override Task<School?> GetByIdReadOnly(
                Guid id,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(school?.Id == id ? school : null);
        }
    }
}
