using Autofac;
using HSoft.SocialSchool.Api.Core;
using HSoft.SocialSchool.Api.Extensions;
using HSoft.SocialSchool.Application.IoC.Services;
using HSoft.SocialSchool.Persistence.IoC.Autofac;
using HSoft.SocialSchool.Persistence.IoC.Services;

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
    //services.RegisterExternalAgents();
    //services.AddStackExchangeOptions(configuration);
}