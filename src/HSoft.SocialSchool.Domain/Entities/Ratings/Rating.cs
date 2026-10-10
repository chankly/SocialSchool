using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Ratings
{
    /// <summary>
    /// Puntuación de 1 a 5 estrellas que un usuario autorizado emite sobre una maniobra declarada.
    /// Solo se puntúan maniobras: no existe puntuación de un nivel global. La valoración de una escuela
    /// vale lo mismo que la de un deportista, y está permitida la autopuntuación.
    /// </summary>
    public sealed class Rating : Entity, IAggregateRoot
    {
        private const int MinimumValue = 1;
        private const int MaximumValue = 5;

        public Guid AthleteSkillId { get; private set; }
        public Guid AuthorId { get; private set; }
        public int Value { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Rating() { }

        private Rating(Guid id) : base(id) { }

        public static Rating Create(Guid id, Guid athleteSkillId, Guid authorId, int value)
        {
            if (athleteSkillId == Guid.Empty)
                throw new ArgumentException("Declared skill is required.", nameof(athleteSkillId));

            if (authorId == Guid.Empty)
                throw new ArgumentException("Author is required.", nameof(authorId));

            ValidateValue(value);

            return new Rating(id)
            {
                AthleteSkillId = athleteSkillId,
                AuthorId = authorId,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>El autor de la puntuación puede editarla o borrarla.</summary>
        public void UpdateValue(int value)
        {
            ValidateValue(value);

            if (Value == value)
                return;

            Value = value;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void ValidateValue(int value)
        {
            if (value is < MinimumValue or > MaximumValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Rating value must be between {MinimumValue} and {MaximumValue}.");
        }
    }
}
