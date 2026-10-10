using Mediator;

namespace HSoft.SocialSchool.Domain.Primitives
{
    /// <summary>
    /// Domain event dispatched AFTER EF commits the transaction.
    /// For side effects that must read already-persisted data (summary materialization,
    /// integration events, etc.). It does not take part in the transaction that raised it:
    /// it is published exactly once, outside the execution strategy, after
    /// SaveChangesAsync has committed the changes.
    /// </summary>
    public interface IPostCommitNotification : INotification;
}
