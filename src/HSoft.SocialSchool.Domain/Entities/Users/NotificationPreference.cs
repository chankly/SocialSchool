using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Users
{
    /// <summary>
    /// Preferencia del usuario sobre un canal de notificación. Entidad hija del agregado <see cref="User"/>.
    /// </summary>
    public sealed class NotificationPreference : Entity
    {
        public Guid UserId { get; private set; }
        public NotificationChannel Channel { get; private set; }
        public bool IsEnabled { get; private set; }

        private NotificationPreference() { }

        private NotificationPreference(Guid id) : base(id) { }

        internal static NotificationPreference Create(User user, NotificationChannel channel, bool isEnabled) =>
            new(Guid.NewGuid())
            {
                UserId = user.Id,
                Channel = channel,
                IsEnabled = isEnabled
            };

        internal void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;
    }
}
