using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Sports
{
    /// <summary>
    /// Deporte del catálogo del sistema. Lo mantiene el técnico y es ampliable con el tiempo.
    /// </summary>
    public sealed class Sport : Entity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        private Sport() { }

        private Sport(Guid id) : base(id) { }

        public static Sport Create(Guid id, string name)
        {
            ValidateName(name);

            return new Sport(id)
            {
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
