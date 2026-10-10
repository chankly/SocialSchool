using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Athletes
{
    /// <summary>
    /// Maniobra declarada por un deportista. Entidad hija del agregado <see cref="Athlete"/>.
    /// Deshabilitarla no borra las puntuaciones recibidas.
    /// </summary>
    public sealed class AthleteSkill : Entity
    {
        public Guid AthleteId { get; private set; }
        public Guid SkillId { get; private set; }
        public bool IsEnabled { get; private set; }
        public DateTime DeclaredAt { get; private set; }

        private AthleteSkill() { }

        private AthleteSkill(Guid id) : base(id) { }

        internal static AthleteSkill Create(Athlete athlete, Guid skillId) =>
            new(Guid.NewGuid())
            {
                AthleteId = athlete.Id,
                SkillId = skillId,
                IsEnabled = true,
                DeclaredAt = DateTime.UtcNow
            };

        internal void Enable() => IsEnabled = true;

        internal void Disable() => IsEnabled = false;
    }
}
