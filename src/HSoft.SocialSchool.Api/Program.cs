using Autofac;
using HSoft.SocialSchool.Api.Core;
using HSoft.SocialSchool.Api.Extensions;
using HSoft.SocialSchool.Application.IoC.Services;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Persistence.IoC.Autofac;
using HSoft.SocialSchool.Persistence.IoC.Services;
using HSoft.SocialSchool.Persistence.Services;

var builder = WebApplication
    .CreateBuilder(args)
    .ConfigureWebApplicationBuilder(ConfigureAutofacContainer, ConfigureServices);

var app = builder.Build();

await app.Configure();
//app.UseEventBus();
app.LogRuntimeInformation();
await app.RunAsync();

static void ConfigureAutofacContainer(ContainerBuilder builder)
{
    //builder.RegisterModule<MainModule>();
    //builder.RegisterModule<ApplicationModule>();
    builder.RegisterModule<PersistenceModule>();
    //builder.RegisterModule<AzureCommunicationModule>();
    //builder.RegisterModule<AzureDataLakeModule>();
    //builder.RegisterModule<CachingStackExchangeModule>();
    //builder.RegisterType<SmtpClientFake>().InstancePerLifetimeScope().As<ISmtpClient>();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddMemoryCache().AddDistributedMemoryCache();
    //services.AddEventBusOptions(configuration);
    services.RegisterMediator();
    services.AddValidators();
    services.AddDbContextPool(configuration);
    // La interfaz la declara Application y la implementa Persistence; el registro va aquí porque
    // AddHttpClient vive en el framework de ASP.NET y no en un proyecto de clase.
    services.AddHttpClient<IKeycloakUsersApi, KeycloakUsersApi>();
    //services.RegisterExternalAgents();
    //services.AddStackExchangeOptions(configuration);
}

// Top-level statements generate an internal Program class; the integration tests need it reachable.
/// <summary>Punto de entrada de la API. Se declara parcial para que los tests de integración puedan referenciarlo.</summary>
public partial class Program { }