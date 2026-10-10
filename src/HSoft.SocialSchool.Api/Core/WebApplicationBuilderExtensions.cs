using Autofac;
using FluentValidation;
using HSoft.SocialSchool.Api.Infrastructure.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;

namespace HSoft.SocialSchool.Api.Core
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder ConfigureWebApplicationBuilder(this WebApplicationBuilder builder,
    Action<ContainerBuilder> configureAutofacContainer,
    Action<IServiceCollection, IConfiguration> configureServices)
        {
            builder.Host.UseSerilog((context, config) =>
                config
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId(),
                writeToProviders: false);

            builder.Host.ConfigureHost(configureAutofacContainer);
            builder.WebHost.UseKestrel().UseContentRoot(Directory.GetCurrentDirectory());
            builder.Services.AddConfiguredOptions(builder.Configuration);
            builder.Services.AddConfiguredControllers(builder.Configuration);
            builder.Services.AddAuthentication(builder.Configuration);
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            /*
            builder.Services.AddProblemDetails(options =>
            {
                options.IncludeExceptionDetails = (ctx, ex) => false;

                // Surface FluentValidation failures thrown by the Mediator ValidationBehavior
                // (e.g. the command-level "match Id already exists" rule) as RFC 7807 validation
                // problems, mirroring the FluentValidationFilter that guards controller arguments.
                options.Map<ValidationException>((context, exception) =>
                {
                    var factory = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory>();
                    var modelState = new ModelStateDictionary();

                    foreach (var failure in exception.Errors)
                        modelState.AddModelError(failure.PropertyName ?? string.Empty, failure.ErrorMessage);

                    return factory.CreateValidationProblemDetails(
                        context,
                        modelState,
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Validation failed",
                        detail: "One or more validation errors occurred.");
                });
            });
            */
            builder.Services.AddSwaggerDocumentation();
            builder.Services.AddAuthorizationHandlers();
            configureServices(builder.Services, builder.Configuration);

            return builder;
        }
    }
}
