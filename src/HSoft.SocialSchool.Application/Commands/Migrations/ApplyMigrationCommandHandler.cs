using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Services;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Migrations
{
    public class ApplyMigrationCommandHandler : IRequestHandler<ApplyMigrationCommand, Result>
    {
        private readonly IMigrationService _migrationService;

        public ApplyMigrationCommandHandler(IMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        public async ValueTask<Result> Handle(ApplyMigrationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _migrationService.ApplyMigrationsAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }
    }
}
