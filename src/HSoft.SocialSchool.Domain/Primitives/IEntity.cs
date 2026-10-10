using Mediator;

namespace HSoft.SocialSchool.Domain.Primitives
{
    public interface IEntity
    {
        /// <summary>
        /// The identifier of the entity
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// List that holds Domain Events for this entity instance
        /// </summary>
        IReadOnlyCollection<INotification> DomainEvents { get; }

        /// <summary>
        /// Adds a domain event to the DomainEvents collection.
        /// Events marked with <see cref="IUniqueDomainEvent"/> are deduplicated by value.
        /// </summary>
        /// <param name="eventItem">The event being added</param>
        void AddDomainEvent(INotification eventItem);

        /// <summary>
        /// Remove a domain event from the DomainEvents collection
        /// </summary>
        /// <param name="eventItem">The event being removed from the collection</param>
        void RemoveDomainEvent(INotification eventItem);

        /// <summary>
        /// Removes all the events from the DomainEvents collection
        /// </summary>
        void ClearDomainEvents();
    }
}
