using Mediator;

namespace HSoft.SocialSchool.Domain.Primitives
{
    public abstract class Entity : IEntity
    {
        protected Entity()
        {
        }

        protected Entity(Guid id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// The identifier of the entity
        /// </summary>
        public virtual Guid Id { get; private set; }

        private readonly List<INotification> _domainEvents = [];

        /// <summary>
        /// List that holds Domain Events for this entity instance
        /// </summary>
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents;

        /// <summary>
        /// Adds a domain event to the DomainEvents collection.
        /// Events marked with <see cref="IUniqueDomainEvent"/> are deduplicated by value
        /// (records): an equivalent instance already queued is not added again.
        /// </summary>
        /// <param name="eventItem">The event being added</param>
        public void AddDomainEvent(INotification eventItem)
        {
            if (eventItem is IUniqueDomainEvent && _domainEvents.Contains(eventItem))
                return;

            _domainEvents.Add(eventItem);
        }

        /// <summary>
        /// Remove a domain event from the DomainEvents collection
        /// </summary>
        /// <param name="eventItem">The event being removed from the collection</param>
        public void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents.Remove(eventItem);
        }

        /// <summary>
        /// Removes all the events from the DomainEvents collection
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public override bool Equals(object obj)
        {
            if (obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetRealType() != other.GetRealType())
                return false;

            if (Id == Guid.Empty || other.Id == Guid.Empty)
                return false;

            return Id == other.Id;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b) => !(a == b);

        public override int GetHashCode() => $"{GetRealType()}{Id}".GetHashCode();

        private Type GetRealType()
        {
            var type = GetType();
            if (type.ToString().Contains("Castle.Proxies."))
                return type.BaseType ?? type;
            return type;
        }
    }
}
