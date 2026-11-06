using HSoft.SocialSchool.Application.Commands.Profiles;
using HSoft.SocialSchool.Application.Models.Profiles;
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
    /// El alta del perfil propio: el tipo de cuenta decide el bloque, el perfil comparte identificador con la
    /// cuenta, el nick se fija una sola vez y un rechazo no deja nada escrito.
    /// </summary>
    public class CompleteMyProfileCommandHandlerTests
    {
        private const string ExternalId = "keycloak-sub";
        private static readonly DateOnly AdultBirthDate = new(1990, 1, 1);

        [Fact]
        public async Task Deportista_completa_su_perfil_con_sus_deportes()
        {
            var user = CreateUser(AccountType.Athlete);
            var skate = Sport.Create(Guid.NewGuid(), "Skate");
            var surf = Sport.Create(Guid.NewGuid(), "Surf");
            var users = new FakeUserRepository(user);
            var athletes = new FakeAthleteRepository();

            var result = await CreateHandler(users, athletes, new FakeSchoolRepository(), new FakeSportRepository([skate, surf]))
                .Handle(AthleteCommand("ada", skate.Id, surf.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("ada", user.Nickname);
            Assert.Single(users.Updated);

            var athlete = Assert.Single(athletes.Added);
            Assert.Equal(user.Id, athlete.Id);
            Assert.Equal(2, athlete.PracticedSports.Count);

            var profile = Assert.IsType<MyProfileResponse>(result.Value);
            Assert.Equal(AccountType.Athlete, profile.AccountType);
            Assert.Null(profile.School);

            var block = Assert.IsType<AthleteProfileResponse>(profile.Athlete);
            Assert.Equal("Ada", block.FirstName);
            Assert.Equal(2, block.PracticedSports.Count);
        }

        [Fact]
        public async Task Escuela_completa_su_perfil_y_conserva_el_estado_pendiente()
        {
            var user = CreateUser(AccountType.School);
            var surf = Sport.Create(Guid.NewGuid(), "Surf");
            var schools = new FakeSchoolRepository();

            var result = await CreateHandler(
                    new FakeUserRepository(user),
                    new FakeAthleteRepository(),
                    schools,
                    new FakeSportRepository([surf]))
                .Handle(SchoolCommand("escuela", surf.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("escuela", user.Nickname);

            var school = Assert.Single(schools.Added);
            Assert.Equal(user.Id, school.Id);
            Assert.Equal(surf.Id, school.SportId);

            var profile = Assert.IsType<MyProfileResponse>(result.Value);
            Assert.Equal(AccountStatus.PendingValidation, profile.AccountStatus);
            Assert.Null(profile.Athlete);

            var block = Assert.IsType<SchoolProfileResponse>(profile.School);
            Assert.Equal("Escuela del Norte", block.Name);
        }

        [Fact]
        public async Task Bloque_que_no_corresponde_al_tipo_de_cuenta_devuelve_fallo_y_no_escribe_nada()
        {
            var user = CreateUser(AccountType.School);
            var users = new FakeUserRepository(user);
            var athletes = new FakeAthleteRepository();
            var schools = new FakeSchoolRepository();

            var result = await CreateHandler(users, athletes, schools, new FakeSportRepository([]))
                .Handle(AthleteCommand("ada"), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(athletes.Added);
            Assert.Empty(schools.Added);
            Assert.Empty(users.Updated);
            Assert.Null(user.Nickname);
        }

        [Fact]
        public async Task Nick_ocupado_devuelve_fallo_sin_crear_nada()
        {
            var user = CreateUser(AccountType.Athlete);
            var users = new FakeUserRepository(user, nicknameExists: true);
            var athletes = new FakeAthleteRepository();

            var result = await CreateHandler(users, athletes, new FakeSchoolRepository(), new FakeSportRepository([]))
                .Handle(AthleteCommand("ada"), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Contains("ada", result.Error);
            Assert.Empty(athletes.Added);
            Assert.Empty(users.Updated);
            Assert.Null(user.Nickname);
        }

        [Fact]
        public async Task Deporte_desconocido_devuelve_fallo_sin_crear_nada()
        {
            var user = CreateUser(AccountType.Athlete);
            var users = new FakeUserRepository(user);
            var athletes = new FakeAthleteRepository();

            var result = await CreateHandler(users, athletes, new FakeSchoolRepository(), new FakeSportRepository([]))
                .Handle(AthleteCommand("ada", Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(athletes.Added);
            Assert.Empty(users.Updated);
            Assert.Null(user.Nickname);
        }

        [Fact]
        public async Task Deporte_desconocido_de_la_escuela_devuelve_fallo_sin_crear_nada()
        {
            var user = CreateUser(AccountType.School);
            var users = new FakeUserRepository(user);
            var schools = new FakeSchoolRepository();

            var result = await CreateHandler(users, new FakeAthleteRepository(), schools, new FakeSportRepository([]))
                .Handle(SchoolCommand("escuela", Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(schools.Added);
            Assert.Empty(users.Updated);
            Assert.Null(user.Nickname);
        }

        [Fact]
        public async Task El_perfil_se_crea_con_el_identificador_de_la_cuenta()
        {
            var user = CreateUser(AccountType.Athlete);
            var athletes = new FakeAthleteRepository();

            await CreateHandler(new FakeUserRepository(user), athletes, new FakeSchoolRepository(), new FakeSportRepository([]))
                .Handle(AthleteCommand("ada"), CancellationToken.None);

            Assert.Equal(user.Id, Assert.Single(athletes.Added).Id);
        }

        [Fact]
        public async Task Reintentar_con_el_mismo_nick_no_lo_vuelve_a_fijar()
        {
            var user = CreateUser(AccountType.Athlete);
            user.SetNickname("ada");
            var users = new FakeUserRepository(user);
            var athletes = new FakeAthleteRepository();

            var result = await CreateHandler(users, athletes, new FakeSchoolRepository(), new FakeSportRepository([]))
                .Handle(AthleteCommand("ada"), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(users.Updated);
            Assert.Single(athletes.Added);
        }

        [Fact]
        public async Task Un_nick_ya_fijado_distinto_devuelve_fallo()
        {
            var user = CreateUser(AccountType.Athlete);
            user.SetNickname("ada");
            var users = new FakeUserRepository(user);
            var athletes = new FakeAthleteRepository();

            var result = await CreateHandler(users, athletes, new FakeSchoolRepository(), new FakeSportRepository([]))
                .Handle(AthleteCommand("lovelace"), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Empty(users.Updated);
            Assert.Empty(athletes.Added);
        }

        [Fact]
        public async Task Cuenta_desconocida_devuelve_fallo()
        {
            var result = await CreateHandler(
                    new FakeUserRepository(null),
                    new FakeAthleteRepository(),
                    new FakeSchoolRepository(),
                    new FakeSportRepository([]))
                .Handle(AthleteCommand("ada"), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Token_sin_identidad_externa_devuelve_fallo()
        {
            var result = await new CompleteMyProfileCommandHandler(
                    new FakeCurrentUser(null),
                    new FakeUserRepository(CreateUser(AccountType.Athlete)),
                    new FakeAthleteRepository(),
                    new FakeSchoolRepository(),
                    new FakeSportRepository([]))
                .Handle(AthleteCommand("ada"), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        private static CompleteMyProfileCommandHandler CreateHandler(
            FakeUserRepository userRepository,
            FakeAthleteRepository athleteRepository,
            FakeSchoolRepository schoolRepository,
            FakeSportRepository sportRepository) =>
            new(
                new FakeCurrentUser(ExternalId),
                userRepository,
                athleteRepository,
                schoolRepository,
                sportRepository);

        private static CompleteMyProfileCommand AthleteCommand(string nickname, params Guid[] sportIds) =>
            new(
                nickname,
                new AthleteProfileRequest(
                    "Ada",
                    "Lovelace",
                    AdultBirthDate,
                    "ES",
                    "Madrid",
                    null,
                    null,
                    sportIds),
                null);

        private static CompleteMyProfileCommand SchoolCommand(string nickname, Guid sportId) =>
            new(
                nickname,
                null,
                new SchoolProfileRequest(
                    "Escuela del Norte",
                    null,
                    sportId,
                    null,
                    "ES",
                    "Bilbao",
                    null,
                    null,
                    null));

        private static User CreateUser(AccountType accountType) =>
            User.Create(Guid.NewGuid(), "ada@example.com", accountType, "keycloak", ExternalId, "es");

        private sealed class FakeCurrentUser(string? externalId) : ICurrentUser
        {
            public string? ExternalId => externalId;

            public bool IsAuthenticated => externalId is not null;
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

            public virtual Task Update(T entity) => throw new NotSupportedException();

            public Task Delete(T entity) => throw new NotSupportedException();

            public Task Save(T entity) => throw new NotSupportedException();

            public Task Save(IReadOnlyList<T> entities) => throw new NotSupportedException();
        }

        private sealed class FakeUserRepository(User? user, bool nicknameExists = false)
            : FakeRepository<User>, IUserRepository
        {
            public List<User> Updated { get; } = [];

            public Task<User?> GetByExternalIdReadOnlyAsync(
                string externalIdentityId,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(user?.ExternalIdentityId == externalIdentityId ? user : null);

            public Task<bool> ExistsByNicknameAsync(
                string nickname,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(nicknameExists);

            public override Task Update(User entity)
            {
                Updated.Add(entity);
                return Task.CompletedTask;
            }
        }

        private sealed class FakeAthleteRepository : FakeRepository<Athlete>, IAthleteRepository
        {
            public List<Athlete> Added { get; } = [];

            public Task<Athlete?> GetByIdWithSportsReadOnlyAsync(
                Guid id,
                CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public override Task Add(Athlete entity)
            {
                Added.Add(entity);
                return Task.CompletedTask;
            }
        }

        private sealed class FakeSchoolRepository : FakeRepository<School>, ISchoolRepository
        {
            public List<School> Added { get; } = [];

            public override Task Add(School entity)
            {
                Added.Add(entity);
                return Task.CompletedTask;
            }
        }

        private sealed class FakeSportRepository(IReadOnlyList<Sport> sports)
            : FakeRepository<Sport>, ISportRepository
        {
            public Task<IReadOnlyList<Sport>> GetActiveReadOnlyAsync(
                CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<IReadOnlyList<Sport>> GetByIdsAsync(
                IReadOnlyList<Guid> sportIds,
                CancellationToken cancellationToken = default) =>
                Task.FromResult<IReadOnlyList<Sport>>(
                    sports.Where(sport => sportIds.Contains(sport.Id)).ToList());

            public override Task<Sport?> GetByIdReadOnly(
                Guid id,
                CancellationToken cancellationToken = default) =>
                Task.FromResult(sports.FirstOrDefault(sport => sport.Id == id));
        }
    }
}
