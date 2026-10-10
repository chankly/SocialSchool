using Mediator;

namespace HSoft.SocialSchool.Domain.Primitives
{
    /// <summary>
    /// Idempotent domain event: it must not exist more than once within the same Unit of Work,
    /// regardless of who raises it. The deduplication policy is a property of the event, not of
    /// the call site. Since domain events are <c>record</c>s, two instances with the same values
    /// are considered equal and the second one is discarded when added.
    /// </summary>
    public interface IUniqueDomainEvent : INotification;
}
