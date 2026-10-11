using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace HSoft.SocialSchool.Api.Core
{
    public static class HostBuilderExtensions
    {
        public static WebApplicationBuilder ConfigureHost(this WebApplicationBuilder builder, Action<ContainerBuilder> configureAutofacContainer)
        {
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer(configureAutofacContainer);

            // WebApplicationBuilder does not support the IHostBuilder configuration APIs. It already
            // sets the content root to the working directory and registers appsettings.json,
            // appsettings.{Environment}.json, user secrets and environment variables. The sources are
            // declared here to keep appsettings.json mandatory and to add the optional secrets file,
            // which the default builder does not know about. Order matters: the last source wins.
            var configuration = builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                //.AddJsonFile("Config/security.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<Program>();

            if (File.Exists("secrets/akv_secrets.json"))
            {
                configuration.AddJsonFile("secrets/akv_secrets.json", optional: true);
            }

            configuration.AddEnvironmentVariables();

            return builder;
        }
    }
}
