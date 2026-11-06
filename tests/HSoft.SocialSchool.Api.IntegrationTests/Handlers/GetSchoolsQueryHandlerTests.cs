using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Queries.Schools;
using HSoft.SocialSchool.Domain.Enumerations;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// El directorio de escuelas: lista ordenada por nombre, con las pendientes de validación incluidas y
    /// filtros opcionales que no distinguen mayúsculas.
    /// </summary>
    public class GetSchoolsQueryHandlerTests
    {
        private static readonly Guid SurfSportId = Guid.NewGuid();
        private static readonly Guid SkateSportId = Guid.NewGuid();

        [Fact]
        public async Task Directorio_sin_filtros_devuelve_las_escuelas_ordenadas_por_nombre()
        {
            var repository = CreateRepository();

            var result = await Handle(repository, new GetSchoolsQuery(null, null, null));

            Assert.True(result.IsSuccess);
            Assert.Collection(
                result.Value,
                first => Assert.Equal("Alfa", first.Name),
                second => Assert.Equal("Zeta", second.Name));
            Assert.Null(repository.LastSportId);
            Assert.Null(repository.LastCity);
            Assert.Null(repository.LastSearch);
        }

        [Fact]
        public async Task Las_escuelas_pendientes_aparecen_con_su_estado()
        {
            var repository = CreateRepository();

            var result = await Handle(repository, new GetSchoolsQuery(null, null, null));

            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value, school => school.AccountStatus == AccountStatus.PendingValidation);
        }

        [Fact]
        public async Task El_filtro_por_deporte_se_propaga_al_repositorio()
        {
            var repository = CreateRepository();

            var result = await Handle(repository, new GetSchoolsQuery(SkateSportId, null, null));

            Assert.True(result.IsSuccess);
            Assert.Equal(SkateSportId, repository.LastSportId);
            Assert.Single(result.Value);
            Assert.Equal("Zeta", result.Value[0].Name);
        }

        [Fact]
        public async Task El_filtro_por_ciudad_se_propaga_al_repositorio()
        {
            var repository = CreateRepository();

            var result = await Handle(repository, new GetSchoolsQuery(null, "gijón", null));

            Assert.True(result.IsSuccess);
            Assert.Equal("gijón", repository.LastCity);
            Assert.Single(result.Value);
            Assert.Equal("Alfa", result.Value[0].Name);
        }

        [Fact]
        public async Task La_busqueda_por_texto_no_distingue_mayusculas()
        {
            var repository = CreateRepository();

            var result = await Handle(repository, new GetSchoolsQuery(null, null, "ALFA"));

            Assert.True(result.IsSuccess);
            Assert.Equal("ALFA", repository.LastSearch);
            Assert.Single(result.Value);
            Assert.Equal("Alfa", result.Value[0].Name);
        }

        private static ValueTask<CSharpFunctionalExtensions.Result<IReadOnlyList<SchoolSummaryResponse>>> Handle(
            StubSchoolRepository repository,
            GetSchoolsQuery query) =>
            new GetSchoolsQueryHandler(repository).Handle(query, CancellationToken.None);

        private static StubSchoolRepository CreateRepository() =>
            new()
            {
                Directory =
                [
                    new SchoolDirectoryItem(Guid.NewGuid(), "Zeta", null, SkateSportId, "Skate", "Oviedo", AccountStatus.Active),
                    new SchoolDirectoryItem(Guid.NewGuid(), "Alfa", "https://img/alfa.png", SurfSportId, "Surf", "Gijón", AccountStatus.PendingValidation)
                ]
            };
    }
}
