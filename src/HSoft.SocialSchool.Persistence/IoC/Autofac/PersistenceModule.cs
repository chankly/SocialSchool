using Autofac;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Persistence.Services;

namespace HSoft.SocialSchool.Persistence.IoC.Autofac
{
    public class PersistenceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterRepositories(builder);
            RegisterMigrationsService(builder);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .InstancePerLifetimeScope()
                .Where(type => type.Name.EndsWith("Repository") && !type.Name.StartsWith("Caching"))
                .AsImplementedInterfaces();

            //builder.RegisterDecorator<CachingLookupValueRepository, ILookupValueRepository>();
        }

        private static void RegisterMigrationsService(ContainerBuilder builder)
        {
            builder.RegisterType<MigrationService>()
                .InstancePerLifetimeScope()
                .As<IMigrationService>();
        }
    }
}
