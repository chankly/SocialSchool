using Duende.AspNetCore.Authentication.OAuth2Introspection;
using HSoft.SocialSchool.Api.Extensions;
using HSoft.SocialSchool.Api.Filters;
using HSoft.SocialSchool.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

namespace HSoft.SocialSchool.Api.Core
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddConfiguredControllers(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddControllers(opt =>
            {
                opt.Filters.Add(typeof(PerformanceAuditFilter));
                opt.Filters.Add(typeof(FluentValidationFilter));
            })
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            services.AddEndpointsApiExplorer();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services, ConfigurationManager configuration)
        {
            services
                 .AddAuthorization(options =>
                 {
                     options.AddPolicy("assessment.list", p => p.RequireClaim(""));
                 })
                .AddAuthentication("token")
                // JWT tokens
                .AddJwtBearer("token", options =>
                {
                    options.Authority = configuration["securityServer:authority"];
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters.ValidTypes = ["at+jwt"];
                    options.TokenValidationParameters.ValidateAudience = false;


                    // if token does not contain a dot, it is a reference token
                    // Forward to the registered introspection scheme. Use the default name ("Introspection").
                    options.ForwardDefaultSelector = AuthenticationForwardSelector.ForwardReferenceToken();
                })
                .AddOAuth2Introspection("Introspection", options =>
                  {
                      options.Authority = configuration["SecurityServer:Authority"];
                      options.ClientId = configuration["SecurityServer:ApiName"];
                      options.ClientSecret = configuration["SecurityServer:ApiSecret"];
                  });
            return services;
        }

        public static IServiceCollection AddConfiguredOptions(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOptions();
            services.Configure<ApplicationOptions>(configuration);
            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var version = $"v{Domain.Constants.Application.Version}";

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                c.SwaggerDoc(version, new OpenApiInfo { Title = Domain.Constants.Application.ApiName, Version = version });
                c.CustomSchemaIds(x => x.FullName);
                c.DescribeAllParametersInCamelCase();
                IncludeAssemblyComments(c);
            });
            return services;
        }

        public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            return services;
        }

        private static void IncludeAssemblyComments(SwaggerGenOptions options)
        {
            try
            {
                var basePath = AppContext.BaseDirectory;
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlContent = Path.Combine(basePath, xmlFile);

                if (!string.IsNullOrWhiteSpace(xmlContent))
                {
                    options.IncludeXmlComments(xmlContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while loading the xml Documentation:{ex.Message}{ex.StackTrace}");
            }
        }
    }
}
