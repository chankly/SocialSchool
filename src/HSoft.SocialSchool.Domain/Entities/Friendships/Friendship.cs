using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Friendships
{
    /// <summary>
    /// Amistad entre dos cuentas. Es simétrica y cualquiera de las dos partes puede deshacerla en
    /// cualquier momento. Al deshacerse se borran las puntuaciones emitidas entre ambos.
    /// </summary>
    public sealed class Friendship : Entity, IAggregateRoot
    {
        public Guid UserAId { get; private set; }
        public Guid UserBId { get; private set; }
        public DateTime StartedAt { get; private set; }

        private Friendship() { }

        private Friendship(Guid id) : base(id) { }

        public static Friendship Create(Guid id, Guid userId, Guid otherUserId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User is required.", nameof(userId));

            if (otherUserId == Guid.Empty)
                throw new ArgumentException("The other user is required.", nameof(otherUserId));

            if (userId == otherUserId)
                throw new ArgumentException("A user cannot befriend themselves.", nameof(otherUserId));

            // The pair is stored in a canonical order so that a symmetric relationship can only exist once,
            // regardless of who sent the request.
            var (userAId, userBId) = userId.CompareTo(otherUserId) < 0
                ? (userId, otherUserId)
                : (otherUserId, userId);

            return new Friendship(id)
            {
                UserAId = userAId,
                UserBId = userBId,
                StartedAt = DateTime.UtcNow
            };
        }

        /// <summary>Indica si el usuario forma parte de esta amistad.</summary>
        public bool Involves(Guid userId) => UserAId == userId || UserBId == userId;
    }
}
