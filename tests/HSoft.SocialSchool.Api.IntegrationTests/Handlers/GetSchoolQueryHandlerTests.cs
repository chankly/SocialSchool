using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Queries.Schools;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Handlers
{
    /// <summary>
    /// La ficha de una escuela: datos públicos, servicios y deportistas con asistencia aceptada, con nick y
    /// foto. Un identificador que no es de una escuela no es un error: no hay ficha.
    /// </summary>
    public class GetSchoolQueryHandlerTests
    {
        private const string SchoolExternalId = "keycloak-school";

        [Fact]
        public async Task La_ficha_trae_los_servicios_y_los_asistentes_aceptados()
        {
            var school = CreateSchool();
            var attendeeId = Guid.NewGuid();

            var handler = CreateHandler(
                school,
                attendees: [new SchoolAttendeeItem(attendeeId, "ada", "https://img/ada.png")]);

            var result = await handler.Handle(new GetSchoolQuery(school.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);

            var detail = result.Value!;
            Assert.Equal(school.Id, detail.Id);
            Assert.Equal("Surf", detail.SportName);
            Assert.Equal("Gijón", detail.City);
            Assert.Equal(AccountStatus.PendingValidation, detail.AccountStatus);

            var service = Assert.Single(detail.Services);
            Assert.Equal("Clase de iniciación", service.Name);
            Assert.Equal(25m, service.Price);

            var attendee = Assert.Single(detail.Attendees);
            Assert.Equal(attendeeId, attendee.Id);
            Assert.Equal("ada", attendee.Nickname);
            Assert.Equal("https://img/ada.png", attendee.ProfilePictureUrl);
        }

        [Fact]
        public async Task Una_escuela_sin_servicios_ni_asistentes_devuelve_listas_vacias()
        {
            var school = CreateSchool(withService: false);

            var handler = CreateHandler(school);

            var result = await handler.Handle(new GetSchoolQuery(school.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);

            var detail = result.Value!;
            Assert.Empty(detail.Services);
            Assert.Empty(detail.Attendees);
        }

        [Fact]
        public async Task Un_identificador_que_no_es_de_una_escuela_no_devuelve_ficha()
        {
            var handler = CreateHandler(school: null);

            var result = await handler.Handle(new GetSchoolQuery(Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        private static GetSchoolQueryHandler CreateHandler(
            School? school,
            IReadOnlyList<SchoolAttendeeItem>? attendees = null)
        {
            var schoolRepository = new StubSchoolRepository { ExistingSchool = school };

            var sportRepository = new StubSportRepository();

            if (school is not null)
                sportRepository.Sports = [Sport.Create(school.SportId, "Surf")];

            var userRepository = new StubUserRepository();

            if (school is not null)
            {
                userRepository.WithUsers(
                    User.Create(school.Id, "escuela@example.com", AccountType.School, "Google", SchoolExternalId, "es"));
            }

            return new GetSchoolQueryHandler(
                schoolRepository,
                sportRepository,
                userRepository,
                new StubSchoolAttendanceRepository { Attendees = attendees ?? [] });
        }

        private static School CreateSchool(bool withService = true)
        {
            var school = School.Create(Guid.NewGuid(), "Escuela Norte", Guid.NewGuid(), "España", "Gijón");

            if (withService)
                school.AddService("Clase de iniciación", "Una hora de clase", 25m, "EUR");

            return school;
        }
    }
}
