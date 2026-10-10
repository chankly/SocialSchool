using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace HSoft.SocialSchool.Api.Core
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureHost(this IHostBuilder host, Action<ContainerBuilder> configureAutofacContainer)
        {
            host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer(configureAutofacContainer)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Obtener el environment desde el contexto del host
                    var environment = context.HostingEnvironment.EnvironmentName;

                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                          .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
                    //config.AddJsonFile("Config/security.json", optional: false, reloadOnChange: false);
                    config.AddUserSecrets<Program>();

                    if (File.Exists("secrets/akv_secrets.json"))
                    {
                        config.AddJsonFile("secrets/akv_secrets.json", optional: true);
                    }
                    config.AddEnvironmentVariables();
                })
                .UseContentRoot(Directory.GetCurrentDirectory());

            return host;
        }
    }
}
