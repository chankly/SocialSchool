using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Schools
{
    /// <summary>
    /// Perfil de un usuario de tipo escuela. Comparte identificador con su cuenta y está asociado a un
    /// único deporte.
    /// </summary>
    public sealed class School : Entity, IAggregateRoot
    {
        private readonly List<SchoolService> _services = [];

        public string Name { get; private set; } = string.Empty;
        public string? LogoUrl { get; private set; }
        public Guid SportId { get; private set; }
        public string? Description { get; private set; }
        public string Country { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string? ContactEmail { get; private set; }
        public string? Phone { get; private set; }
        public string? Website { get; private set; }

        public IReadOnlyCollection<SchoolService> Services => _services.AsReadOnly();

        private School() { }

        private School(Guid id) : base(id) { }

        /// <summary>
        /// Crea el perfil de la escuela. El deporte es único y se fija al crearla.
        /// </summary>
        public static School Create(Guid id, string name, Guid sportId, string country, string city)
        {
            ValidateName(name);

            if (sportId == Guid.Empty)
                throw new ArgumentException("Sport is required.", nameof(sportId));

            return new School(id)
            {
                Name = name,
                SportId = sportId,
                Country = country,
                City = city
            };
        }

        public void UpdateProfile(
            string name,
            string country,
            string city,
            string? description,
            string? contactEmail,
            string? phone,
            string? website,
            string? logoUrl)
        {
            ValidateName(name);

            Name = name;
            Country = country;
            City = city;
            Description = description;
            ContactEmail = contactEmail;
            Phone = phone;
            Website = website;
            LogoUrl = logoUrl;
        }

        /// <summary>Publica un servicio en la sección comercial de la escuela.</summary>
        public SchoolService AddService(string name, string description, decimal price, string currency)
        {
            var service = SchoolService.Create(this, name, description, price, currency);
            _services.Add(service);
            return service;
        }

        /// <summary>Edita un servicio. La escuela no conserva histórico de servicios.</summary>
        public void UpdateService(Guid serviceId, string name, string description, decimal price, string currency) =>
            FindService(serviceId).Update(name, description, price, currency);

        public void RemoveService(Guid serviceId) => _services.Remove(FindService(serviceId));

        private SchoolService FindService(Guid serviceId)
        {
            var service = _services.SingleOrDefault(x => x.Id == serviceId);

            if (service is null)
                throw new InvalidOperationException("The school does not offer this service.");

            return service;
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
        }
    }
}
