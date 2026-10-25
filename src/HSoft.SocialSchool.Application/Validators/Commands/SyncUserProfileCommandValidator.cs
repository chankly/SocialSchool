using FluentValidation;
using HSoft.SocialSchool.Application.Commands.Registration;

namespace HSoft.SocialSchool.Application.Validators.Commands
{
    /// <summary>
    /// Valida el mensaje de sincronización: sin un identificador de usuario no hay cuenta que actualizar. Que
    /// el evento no traiga nada que sincronizar no es un error, así que eso lo decide el handler.
    /// </summary>
    public sealed class SyncUserProfileCommandValidator : AbstractValidator<SyncUserProfileCommand>
    {
        public SyncUserProfileCommandValidator()
        {
            RuleFor(command => command.KeycloakUserId)
                .NotEmpty().WithMessage("The event does not identify a Keycloak user.")
                .Must(BeAnIdentifier).WithMessage("The Keycloak user identifier is not a GUID.");
        }

        private static bool BeAnIdentifier(string? keycloakUserId) => Guid.TryParse(keycloakUserId, out _);
    }
}
