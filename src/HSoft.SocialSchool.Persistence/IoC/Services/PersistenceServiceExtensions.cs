using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HSoft.SocialSchool.Persistence.IoC.Services
{
    public static class PersistenceServiceExtensions
    {
        public static void AddDbContextPool(this IServiceCollection services, IConfiguration configuration)
        {
            var licenserConnectionString = configuration.GetConnectionString("MyCompany");
            services.AddDbContextPool<MyCompanyDbContext>(options =>
            options.UseSqlServer(licenserConnectionString, opt =>
            {
                opt.MigrationsAssembly(typeof(MyCompanyDbContext).Assembly.GetName().Name);
                opt.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null
                );
            }));
        }
    }
}
