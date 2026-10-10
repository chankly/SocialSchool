using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Notifications
{
    /// <summary>
    /// Aviso generado por un evento del sistema para un usuario. Respeta las preferencias del usuario por
    /// canal; en el alcance actual solo se implementa el canal dentro de la aplicación.
    /// </summary>
    public sealed class Notification : Entity, IAggregateRoot
    {
        public Guid RecipientId { get; private set; }
        public NotificationType Type { get; private set; }
        public string? ContentReference { get; private set; }
        public NotificationChannel Channel { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Notification() { }

        private Notification(Guid id) : base(id) { }

        public static Notification Create(
            Guid id,
            Guid recipientId,
            NotificationType type,
            NotificationChannel channel,
            string? contentReference)
        {
            if (recipientId == Guid.Empty)
                throw new ArgumentException("Recipient is required.", nameof(recipientId));

            return new Notification(id)
            {
                RecipientId = recipientId,
                Type = type,
                Channel = channel,
                ContentReference = contentReference,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsRead()
        {
            if (IsRead)
                return;

            IsRead = true;
        }
    }
}
