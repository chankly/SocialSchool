using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Repositories;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Registration
{
    /// <summary>
    /// Lleva a la cuenta el correo y el idioma que el usuario ha cambiado en Keycloak. No crea cuentas —eso es
    /// del alta— ni toca el tipo de cuenta, y solo escribe si el dato recibido es distinto del guardado.
    /// </summary>
    public sealed class SyncUserProfileCommandHandler : IRequestHandler<SyncUserProfileCommand, Result>
    {
        private readonly IUserRepository _userRepository;

        public SyncUserProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async ValueTask<Result> Handle(SyncUserProfileCommand request, CancellationToken cancellationToken)
        {
            // Un cambio que no nos afecta (por ejemplo el nombre, que es dato de nuestro perfil).
            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PreferredLanguage))
                return Result.Success();

            var user = await _userRepository.GetByExternalIdReadOnlyAsync(
                request.KeycloakUserId,
                cancellationToken);

            // Crear la cuenta es del evento de alta: aquí un usuario desconocido no se materializa.
            if (user is null)
                return Result.Success();

            var changed = false;

            if (!string.IsNullOrWhiteSpace(request.Email)
                && !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.ChangeEmail(request.Email);
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(request.PreferredLanguage)
                && !string.Equals(user.PreferredLanguage, request.PreferredLanguage, StringComparison.OrdinalIgnoreCase))
            {
                user.ChangePreferredLanguage(request.PreferredLanguage);
                changed = true;
            }

            if (changed)
                await _userRepository.Update(user);

            return Result.Success();
        }
    }
}
