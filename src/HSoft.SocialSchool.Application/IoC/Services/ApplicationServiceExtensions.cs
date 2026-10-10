using FluentValidation;
using HSoft.SocialSchool.Application.Behaviors;
using HSoft.SocialSchool.Domain.Primitives;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace HSoft.SocialSchool.Application.IoC.Services
{
    public static class ApplicationServiceExtensions
    {
        public static void RegisterMediator(this IServiceCollection services)
        {
            services.AddMediator(options =>
            {
                options.Namespace = "HSoft.SocialSchool.Application";
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.GenerateTypesAsInternal = true;
                options.NotificationPublisherType = typeof(ForeachAwaitPublisher);
                // Domain assembly must be included so the source generator can match
                // INotificationHandler<T> implementations to the notification types they handle.
                options.Assemblies = [typeof(ApplicationServiceExtensions).Assembly, typeof(Entity).Assembly];
                options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
                options.StreamPipelineBehaviors = [];
            });
        }

        public static void AddValidators(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);
        }
    }
}
