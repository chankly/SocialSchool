using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Athletes
{
    /// <summary>
    /// Perfil de un usuario de tipo deportista. Comparte identificador con su cuenta.
    /// </summary>
    public sealed class Athlete : Entity, IAggregateRoot
    {
        private const int MinimumAge = 18;

        private readonly List<Sport> _practicedSports = [];
        private readonly List<AthleteSkill> _declaredSkills = [];

        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public DateOnly DateOfBirth { get; private set; }
        public string Country { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string? ProfilePictureUrl { get; private set; }
        public string? Biography { get; private set; }

        public IReadOnlyCollection<Sport> PracticedSports => _practicedSports.AsReadOnly();
        public IReadOnlyCollection<AthleteSkill> DeclaredSkills => _declaredSkills.AsReadOnly();

        private Athlete() { }

        private Athlete(Guid id) : base(id) { }

        /// <summary>
        /// Crea el perfil del deportista. La fecha de nacimiento debe acreditar la mayoría de edad.
        /// </summary>
        public static Athlete Create(
            Guid id,
            string firstName,
            string lastName,
            DateOnly dateOfBirth,
            string country,
            string city)
        {
            ValidateName(firstName, nameof(firstName));
            ValidateName(lastName, nameof(lastName));

            var oldestAllowedDateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-MinimumAge);
            if (dateOfBirth > oldestAllowedDateOfBirth)
                throw new ArgumentException($"The athlete must be at least {MinimumAge} years old.", nameof(dateOfBirth));

            return new Athlete(id)
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Country = country,
                City = city
            };
        }

        public void UpdateProfile(
            string firstName,
            string lastName,
            string country,
            string city,
            string? biography,
            string? profilePictureUrl)
        {
            ValidateName(firstName, nameof(firstName));
            ValidateName(lastName, nameof(lastName));

            FirstName = firstName;
            LastName = lastName;
            Country = country;
            City = city;
            Biography = biography;
            ProfilePictureUrl = profilePictureUrl;
        }

        /// <summary>Marca un deporte como practicado por el deportista.</summary>
        public void AddPracticedSport(Sport sport)
        {
            if (sport is null)
                throw new ArgumentNullException(nameof(sport));

            if (_practicedSports.Contains(sport))
                throw new InvalidOperationException("The athlete already practises this sport.");

            _practicedSports.Add(sport);
        }

        public void RemovePracticedSport(Sport sport)
        {
            if (sport is null)
                throw new ArgumentNullException(nameof(sport));

            _practicedSports.Remove(sport);
        }

        /// <summary>
        /// Declara que el deportista domina una maniobra del catálogo. Solo se admiten maniobras de los
        /// deportes que practica, y una misma maniobra no puede declararse dos veces.
        /// </summary>
        public void DeclareSkill(Skill skill)
        {
            if (skill is null)
                throw new ArgumentNullException(nameof(skill));

            if (!_practicedSports.Any(x => x.Id == skill.SportId))
                throw new InvalidOperationException("The athlete can only declare skills of the sports they practise.");

            if (_declaredSkills.Any(x => x.SkillId == skill.Id))
                throw new InvalidOperationException("The athlete has already declared this skill.");

            _declaredSkills.Add(AthleteSkill.Create(this, skill.Id));
        }

        /// <summary>Vuelve a habilitar una maniobra declarada conservando el histórico de puntuaciones.</summary>
        public void EnableSkill(Guid skillId) => FindDeclaredSkill(skillId).Enable();

        /// <summary>Deshabilita una maniobra declarada sin borrar sus puntuaciones.</summary>
        public void DisableSkill(Guid skillId) => FindDeclaredSkill(skillId).Disable();

        private AthleteSkill FindDeclaredSkill(Guid skillId)
        {
            var declaredSkill = _declaredSkills.SingleOrDefault(x => x.SkillId == skillId);

            if (declaredSkill is null)
                throw new InvalidOperationException("The athlete has not declared this skill.");

            return declaredSkill;
        }

        private static void ValidateName(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.", parameterName);
        }
    }
}
