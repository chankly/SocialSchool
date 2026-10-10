using HSoft.SocialSchool.Persistence.Extensions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence
{
    public class MyCompanyDbContext : DbContext
    {
        private readonly IMediator _mediator;

        public MyCompanyDbContext(DbContextOptions<MyCompanyDbContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyCompanyDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = 0;

            IReadOnlyList<INotification> postCommitEvents = [];

            var strategy = Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                postCommitEvents = await _mediator.DispatchDomainEventsAsync(this, cancellationToken);
                result = await base.SaveChangesAsync(cancellationToken);
            });

            // Outside the execution strategy (not retryable) and after the commit:
            // post-commit handlers can safely read already-persisted data.
            foreach (var domainEvent in postCommitEvents)
                await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
