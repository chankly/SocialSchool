using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Schools
{
    /// <summary>
    /// Servicio ofrecido por una escuela. Entidad hija del agregado <see cref="School"/>. Es informativo:
    /// no se puede contratar, reservar ni pagar desde la aplicación.
    /// </summary>
    public sealed class SchoolService : Entity
    {
        public Guid SchoolId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public string Currency { get; private set; } = string.Empty;

        private SchoolService() { }

        private SchoolService(Guid id) : base(id) { }

        internal static SchoolService Create(
            School school,
            string name,
            string description,
            decimal price,
            string currency)
        {
            Validate(name, price, currency);

            return new SchoolService(Guid.NewGuid())
            {
                SchoolId = school.Id,
                Name = name,
                Description = description,
                Price = price,
                Currency = currency
            };
        }

        internal void Update(string name, string description, decimal price, string currency)
        {
            Validate(name, price, currency);

            Name = name;
            Description = description;
            Price = price;
            Currency = currency;
        }

        private static void Validate(string name, decimal price, string currency)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty.", nameof(currency));
        }
    }
}
