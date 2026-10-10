using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Blocks
{
    /// <summary>
    /// Bloqueo entre dos usuarios. Es mutuo en efecto, permanente e irreversible: no existe desbloqueo.
    /// Rompe la amistad, borra las puntuaciones entre ambos, retira el vínculo de asistencia y excluye a
    /// la otra parte de las búsquedas y de las listas de asistentes.
    /// </summary>
    public sealed class Block : Entity, IAggregateRoot
    {
        public Guid BlockerId { get; private set; }
        public Guid BlockedId { get; private set; }
        public DateTime BlockedAt { get; private set; }

        private Block() { }

        private Block(Guid id) : base(id) { }

        public static Block Create(Guid id, Guid blockerId, Guid blockedId)
        {
            if (blockerId == Guid.Empty)
                throw new ArgumentException("Blocker is required.", nameof(blockerId));

            if (blockedId == Guid.Empty)
                throw new ArgumentException("Blocked user is required.", nameof(blockedId));

            if (blockerId == blockedId)
                throw new ArgumentException("A user cannot block themselves.", nameof(blockedId));

            return new Block(id)
            {
                BlockerId = blockerId,
                BlockedId = blockedId,
                BlockedAt = DateTime.UtcNow
            };
        }
    }
}
