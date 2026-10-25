using HSoft.SocialSchool.Application.Commands.Registration;
using HSoft.SocialSchool.Application.Validators.Commands;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests.Validators
{
    /// <summary>
    /// El validador del mensaje: lo que se comprueba antes de que el handler consulte Keycloak.
    /// </summary>
    public class RegisterUserCommandValidatorTests
    {
        private readonly RegisterUserCommandValidator _validator = new();

        [Fact]
        public void Un_identificador_de_keycloak_valido_pasa()
        {
            var result = _validator.Validate(new RegisterUserCommand("ba31e1e2-0d4a-4b44-a3a9-641cc4309e7c"));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Un_evento_sin_usuario_no_pasa()
        {
            var result = _validator.Validate(new RegisterUserCommand(string.Empty));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterUserCommand.KeycloakUserId));
        }

        [Fact]
        public void Un_identificador_que_no_es_un_guid_no_pasa()
        {
            var result = _validator.Validate(new RegisterUserCommand("no-es-un-guid"));

            Assert.False(result.IsValid);
        }
    }
}
