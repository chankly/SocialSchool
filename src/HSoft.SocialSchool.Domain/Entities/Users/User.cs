using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Users
{
    /// <summary>
    /// Cuenta registrada en el sistema. Toda cuenta es un <see cref="User"/> con un perfil especializado
    /// (<c>Athlete</c> o <c>School</c>) que comparte su identificador.
    /// </summary>
    public sealed class User : Entity, IAggregateRoot
    {
        private readonly List<NotificationPreference> _notificationPreferences = [];

        public string Email { get; private set; } = string.Empty;

        /// <summary>
        /// Nick del usuario. Nace vacío: lo elige él al completar el perfil y a partir de ahí no cambia.
        /// </summary>
        public string? Nickname { get; private set; }
        public AccountType AccountType { get; private set; }
        public AccountStatus AccountStatus { get; private set; }
        public string IdentityProvider { get; private set; } = string.Empty;
        public string ExternalIdentityId { get; private set; } = string.Empty;
        public string PreferredLanguage { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public IReadOnlyCollection<NotificationPreference> NotificationPreferences => _notificationPreferences.AsReadOnly();

        private User() { }

        private User(Guid id) : base(id) { }

        /// <summary>
        /// Crea una cuenta. El tipo es inmutable y las cuentas de escuela nacen pendientes de validación.
        /// La cuenta nace <b>sin nick</b>: el nick lo elige el usuario al completar su perfil.
        /// </summary>
        public static User Create(
            Guid id,
            string email,
            AccountType accountType,
            string identityProvider,
            string externalIdentityId,
            string preferredLanguage)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            if (string.IsNullOrWhiteSpace(identityProvider))
                throw new ArgumentException("Identity provider cannot be empty.", nameof(identityProvider));

            if (string.IsNullOrWhiteSpace(externalIdentityId))
                throw new ArgumentException("External identity id cannot be empty.", nameof(externalIdentityId));

            return new User(id)
            {
                Email = email,
                AccountType = accountType,
                AccountStatus = accountType == AccountType.School
                    ? AccountStatus.PendingValidation
                    : AccountStatus.Active,
                IdentityProvider = identityProvider,
                ExternalIdentityId = externalIdentityId,
                PreferredLanguage = preferredLanguage,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Fija el nick del usuario. Solo se puede llamar una vez: el nick es único e inmutable, y lo elige
        /// el usuario al completar su perfil, no el alta desde Keycloak.
        /// </summary>
        public void SetNickname(string nickname)
        {
            if (!string.IsNullOrWhiteSpace(Nickname))
                throw new InvalidOperationException("The nickname can only be set once.");

            ValidateNickname(nickname);

            Nickname = nickname;
        }

        /// <summary>
        /// Valida una cuenta de escuela pendiente. Lo provoca el técnico, que opera fuera de la aplicación.
        /// </summary>
        public void Validate()
        {
            if (AccountType != AccountType.School)
                throw new InvalidOperationException("Only school accounts require validation.");

            if (AccountStatus == AccountStatus.Deleted)
                throw new InvalidOperationException("A deleted account cannot be validated.");

            AccountStatus = AccountStatus.Active;
        }

        /// <summary>
        /// Eliminación suave: la cuenta deja de ser visible para los demás y conserva todo su contenido.
        /// </summary>
        public void Delete()
        {
            if (AccountStatus == AccountStatus.Deleted)
                return;

            AccountStatus = AccountStatus.Deleted;
            DeletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restaura una cuenta eliminada. El dominio no conserva el estado previo a la eliminación, así que
        /// la cuenta vuelve a estar activa; una escuela restaurada no vuelve a quedar pendiente de validación.
        /// </summary>
        public void Restore()
        {
            if (AccountStatus != AccountStatus.Deleted)
                throw new InvalidOperationException("The account is not deleted.");

            AccountStatus = AccountStatus.Active;
            DeletedAt = null;
        }

        /// <summary>
        /// Actualiza el correo cuando cambia en el proveedor de identidad. El correo no lo elige el usuario
        /// en nuestra aplicación: llega de Keycloak.
        /// </summary>
        public void ChangeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            Email = email;
        }

        public void ChangePreferredLanguage(string preferredLanguage)
        {
            if (string.IsNullOrWhiteSpace(preferredLanguage))
                throw new ArgumentException("Preferred language cannot be empty.", nameof(preferredLanguage));

            PreferredLanguage = preferredLanguage;
        }

        /// <summary>
        /// Crea o actualiza la preferencia del usuario para un canal de notificación. El usuario puede
        /// desactivar de forma independiente el canal de aplicación, el de email y el de push.
        /// </summary>
        public void SetNotificationPreference(NotificationChannel channel, bool isEnabled)
        {
            var preference = _notificationPreferences.SingleOrDefault(x => x.Channel == channel);

            if (preference is null)
                _notificationPreferences.Add(NotificationPreference.Create(this, channel, isEnabled));
            else
                preference.SetEnabled(isEnabled);
        }

        private static void ValidateNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("Nickname cannot be empty.", nameof(nickname));

            if (!nickname.All(char.IsLetterOrDigit))
                throw new ArgumentException("Nickname can only contain letters and digits.", nameof(nickname));
        }
    }
}
