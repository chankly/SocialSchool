using HSoft.SocialSchool.Application.Commands.Profiles;
using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Validators.Commands;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Validators
{
    /// <summary>
    /// El validador del formulario: la forma del nick, exactamente un bloque, la mayoría de edad y el deporte
    /// obligatorio de la escuela. Lo que depende de la cuenta o del catálogo no se comprueba aquí.
    /// </summary>
    public class CompleteMyProfileCommandValidatorTests
    {
        private static readonly DateOnly AdultBirthDate = new(1990, 1, 1);
        private static readonly DateOnly MinorBirthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-17);

        private readonly CompleteMyProfileCommandValidator _validator = new();

        [Fact]
        public void Un_deportista_valido_pasa()
        {
            var result = _validator.Validate(AthleteCommand("ada", AdultBirthDate));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Una_escuela_valida_pasa()
        {
            var result = _validator.Validate(SchoolCommand("escuela", Guid.NewGuid()));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Un_nick_vacio_no_pasa()
        {
            var result = _validator.Validate(AthleteCommand(string.Empty, AdultBirthDate));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CompleteMyProfileCommand.Nickname));
        }

        [Fact]
        public void Un_nick_con_caracteres_no_alfanumericos_no_pasa()
        {
            var result = _validator.Validate(AthleteCommand("ada_lovelace", AdultBirthDate));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CompleteMyProfileCommand.Nickname));
        }

        [Fact]
        public void Sin_ningun_bloque_no_pasa()
        {
            var result = _validator.Validate(new CompleteMyProfileCommand("ada", null, null));

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Con_los_dos_bloques_no_pasa()
        {
            var athlete = new AthleteProfileRequest(
                "Ada",
                "Lovelace",
                AdultBirthDate,
                "ES",
                "Madrid",
                null,
                null,
                []);

            var school = new SchoolProfileRequest(
                "Escuela del Norte",
                null,
                Guid.NewGuid(),
                null,
                "ES",
                "Bilbao",
                null,
                null,
                null);

            var result = _validator.Validate(new CompleteMyProfileCommand("ada", athlete, school));

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Un_deportista_menor_de_edad_no_pasa()
        {
            var result = _validator.Validate(AthleteCommand("ada", MinorBirthDate));

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Errors,
                error => error.PropertyName == $"{nameof(CompleteMyProfileCommand.Athlete)}.DateOfBirth");
        }

        [Fact]
        public void Una_escuela_sin_deporte_no_pasa()
        {
            var result = _validator.Validate(SchoolCommand("escuela", Guid.Empty));

            Assert.False(result.IsValid);
            Assert.Contains(
                result.Errors,
                error => error.PropertyName == $"{nameof(CompleteMyProfileCommand.School)}.SportId");
        }

        private static CompleteMyProfileCommand AthleteCommand(string nickname, DateOnly dateOfBirth) =>
            new(
                nickname,
                new AthleteProfileRequest(
                    "Ada",
                    "Lovelace",
                    dateOfBirth,
                    "ES",
                    "Madrid",
                    null,
                    null,
                    []),
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
    }
}
