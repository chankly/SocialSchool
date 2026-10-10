using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Sports
{
    /// <summary>
    /// Maniobra concreta de un deporte. Elemento del catálogo que mantiene el técnico: los deportistas
    /// seleccionan las que dominan y es sobre ellas sobre las que se puntúa.
    /// </summary>
    public sealed class Skill : Entity, IAggregateRoot
    {
        public Guid SportId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        private Skill() { }

        private Skill(Guid id) : base(id) { }

        public static Skill Create(Guid id, Guid sportId, string name)
        {
            if (sportId == Guid.Empty)
                throw new ArgumentException("Sport is required.", nameof(sportId));

            ValidateName(name);

            return new Skill(id)
            {
                SportId = sportId,
                Name = name,
                IsActive = true
            };
        }

        public void Rename(string name)
        {
            ValidateName(name);

            Name = name;
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
        }
    }
}
