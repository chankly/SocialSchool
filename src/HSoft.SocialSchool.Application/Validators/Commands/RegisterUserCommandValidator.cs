using FluentValidation;
using HSoft.SocialSchool.Application.Commands.Registration;

namespace HSoft.SocialSchool.Application.Validators.Commands
{
    /// <summary>
    /// Valida el mensaje antes de que el handler lo toque: sin un identificador de usuario no hay nada que
    /// consultar en Keycloak. Lo que depende del perfil que devuelve Keycloak no se puede comprobar aquí.
    /// </summary>
    public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(command => command.KeycloakUserId)
                .NotEmpty().WithMessage("The event does not identify a Keycloak user.")
                .Must(BeAnIdentifier).WithMessage("The Keycloak user identifier is not a GUID.");
        }

        private static bool BeAnIdentifier(string? keycloakUserId) => Guid.TryParse(keycloakUserId, out _);
    }
}
