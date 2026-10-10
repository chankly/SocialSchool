using HSoft.SocialSchool.Application.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HSoft.SocialSchool.Persistence.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly ILogger _logger;
        private readonly MyCompanyDbContext _myCompanyDbContext;

        public MigrationService(MyCompanyDbContext myCompanyDbContext, ILogger logger)
        {
            _myCompanyDbContext = myCompanyDbContext;
            _logger = logger;
        }

        public async Task ApplyMigrationsAsync()
        {
            try
            {
                var pendingMigrations = await _myCompanyDbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.Information("Applying pending migrations.");
                    await _myCompanyDbContext.Database.MigrateAsync();
                }
                else
                {
                    _logger.Information("No pending migrations to apply.");
                }
            }
            catch (Exception)
            {
                _logger.Error("An error occurred while applying migrations.");
                throw;
            }
        }
    }
}
