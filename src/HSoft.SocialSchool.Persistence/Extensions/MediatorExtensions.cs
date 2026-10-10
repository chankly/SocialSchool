using HSoft.SocialSchool.Domain.Primitives;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Extensions
{
    public static class MediatorExtensions
    {
        /// <summary>
        /// Dispatches the transactional (pre-commit) domain events within the current UoW
        /// and returns the accumulated <see cref="IPostCommitNotification"/> events so the
        /// caller can publish them once the transaction has committed.
        /// </summary>
        public static async Task<IReadOnlyList<INotification>> DispatchDomainEventsAsync(
            this IMediator mediator, DbContext ctx, CancellationToken ct = default)
        {
            var postCommitEvents = new List<INotification>();

            while (true)
            {
                var domainEntities = ctx.ChangeTracker.Entries<Entity>()
                                        .Where(x => x.Entity.DomainEvents?.Count > 0)
                                        .ToList();

                // Single exit point: evaluated at the START of every iteration.
                // Events added by handlers during Publish() are detected on the next loop.
                if (domainEntities.Count == 0) break;

                var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();
                domainEntities.ForEach(e => e.Entity.ClearDomainEvents());

                foreach (var domainEvent in domainEvents)
                {
                    // Post-commit events are deferred: published outside the transaction.
                    // Clearing them here (via ClearDomainEvents) keeps the loop terminating.
                    if (domainEvent is IPostCommitNotification)
                        postCommitEvents.Add(domainEvent);
                    else
                        await mediator.Publish(domainEvent, ct);
                }
            }

            return postCommitEvents;
        }
    }
}
