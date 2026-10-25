using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Registration;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Registration
{
    /// <summary>
    /// Crea la cuenta de negocio a partir del perfil de Keycloak del usuario registrado. Es idempotente:
    /// si la cuenta ya existe no hace nada, e <b>no</b> crea el perfil (eso lo completa el usuario).
    /// El mensaje lo valida antes <c>RegisterUserCommandValidator</c>, en el pipeline de Mediator.
    /// </summary>
    public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakUsersApi _keycloakUsersApi;

        public RegisterUserCommandHandler(IUserRepository userRepository, IKeycloakUsersApi keycloakUsersApi)
        {
            _userRepository = userRepository;
            _keycloakUsersApi = keycloakUsersApi;
        }

        public async ValueTask<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Idempotencia: Keycloak puede entregar el mismo evento más de una vez.
            if (await _userRepository.GetByExternalIdReadOnlyAsync(
                    request.KeycloakUserId,
                    cancellationToken) is not null)
            {
                return Result.Success();
            }

            var profileResult = await _keycloakUsersApi.GetUserAsync(request.KeycloakUserId, cancellationToken);

            if (profileResult.IsFailure)
                return Result.Failure(profileResult.Error);

            var userResult = BuildUser(profileResult.Value);

            if (userResult.IsFailure)
                return Result.Failure(userResult.Error);

            await _userRepository.Add(userResult.Value);

            return Result.Success();
        }

        /// <summary>
        /// Traduce el perfil de Keycloak a la cuenta, o explica por qué no se puede crear. Todo lo que depende
        /// de la respuesta de Keycloak se comprueba aquí, porque el validador del mensaje no puede verlo.
        /// </summary>
        private static Result<User> BuildUser(KeycloakUserProfile? profile)
        {
            if (profile is null)
                return Result.Failure<User>("The Keycloak user does not exist.");

            // El identificador de Keycloak es el del usuario también en nuestro dominio: la cuenta y el
            // perfil que se cree después comparten Id con la identidad del token.
            if (!Guid.TryParse(profile.Id, out var userId))
                return Result.Failure<User>("The Keycloak user has no valid identifier.");

            if (string.IsNullOrWhiteSpace(profile.Email))
                return Result.Failure<User>($"Keycloak user '{profile.Id}' has no email.");

            if (!TryGetAccountType(profile.UserType, out var accountType))
                return Result.Failure<User>($"Keycloak user '{profile.Id}' has no valid userType.");

            return User.Create(
                userId,
                profile.Email,
                accountType,
                // Un alta que no venga de un proveedor federado es una cuenta local de Keycloak.
                profile.IdentityProvider ?? "keycloak",
                profile.Id,
                profile.Locale ?? "en");
        }

        /// <summary>
        /// Keycloak entrega el tipo de cuenta en el idioma con el que se rellenó el registro, así que hay dos
        /// vocabularios: el inglés (<c>Athlete</c>, <c>School</c>) y el español (<c>Deportista</c>,
        /// <c>Escuela</c>). Se comparan sin distinguir mayúsculas y solo se aceptan esos cuatro valores: un
        /// número también lo interpretaría <see cref="Enum.TryParse{TEnum}(string?, out TEnum)"/> y crearía una
        /// cuenta del tipo equivocado.
        /// </summary>
        private static readonly Dictionary<string, AccountType> AccountTypeNames = new(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(AccountType.Athlete)] = AccountType.Athlete,
            ["Deportista"] = AccountType.Athlete,
            [nameof(AccountType.School)] = AccountType.School,
            ["Escuela"] = AccountType.School
        };

        private static bool TryGetAccountType(string? userType, out AccountType accountType)
        {
            accountType = default;

            return !string.IsNullOrWhiteSpace(userType)
                && AccountTypeNames.TryGetValue(userType.Trim(), out accountType);
        }
    }
}
