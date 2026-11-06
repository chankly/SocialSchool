using FluentValidation;
using HSoft.SocialSchool.Web.Authentication;
using HSoft.SocialSchool.Web.Configuration;
using HSoft.SocialSchool.Web.Services.Demo;
using HSoft.SocialSchool.Web.Services.Friends;
using HSoft.SocialSchool.Web.Services.Profile;
using HSoft.SocialSchool.Web.Services.Profile.Requests;
using HSoft.SocialSchool.Web.Services.Schools;
using HSoft.SocialSchool.Web.Services.Session;
using HSoft.SocialSchool.Web.Services.Sports;
using HSoft.SocialSchool.Web.State;
using HSoft.SocialSchool.Web.Validations.Profile;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HSoft.SocialSchool.Web.Extensions;

/// <summary>
/// Registro de dependencias del cliente. El modo demo y el modo API real se deciden aquí,
/// en un único punto, según estén completos <c>ApiBaseUrl</c> y <c>Oidc</c>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra la configuración, la autenticación y los servicios que consume la UI.
    /// Sin <c>ApiBaseUrl</c> u <c>Oidc</c> completos se mantienen los mocks y la sesión de demo.
    /// </summary>
    public static IServiceCollection AddWebAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = new WebAppSettings
        {
            ApiBaseUrl = configuration["ApiBaseUrl"]?.Trim() ?? string.Empty,
            OidcAuthority = configuration["Oidc:Authority"]?.Trim() ?? string.Empty,
            OidcClientId = configuration["Oidc:ClientId"]?.Trim() ?? string.Empty
        };

        services.AddSingleton(settings);

        if (settings.UseMocks)
        {
            AddDemoServices(services);
        }
        else
        {
            AddApiServices(services, settings);
        }

        services.AddScoped<ProfileState>();

        services.AddSingleton<IValidator<CreateAthleteProfileRequest>, CreateAthleteProfileRequestValidator>();
        services.AddSingleton<IValidator<CreateSchoolProfileRequest>, CreateSchoolProfileRequestValidator>();

        return services;
    }

    /// <summary>Servicios simulados y sesión de demostración, sin Keycloak ni API.</summary>
    private static void AddDemoServices(IServiceCollection services)
    {
        services.AddSingleton<MockDataStore>();

        services.AddSingleton<MockSessionService>();
        services.AddSingleton<IDemoSessionService>(provider => provider.GetRequiredService<MockSessionService>());
        services.AddSingleton<ISessionService>(provider => provider.GetRequiredService<MockSessionService>());

        services.AddSingleton<ISportsService, MockSportsService>();
        services.AddSingleton<IProfileService, MockProfileService>();
        services.AddSingleton<IFriendsService, MockFriendsService>();
        services.AddSingleton<ISchoolsService, MockSchoolsService>();
    }

    /// <summary>Servicios HTTP contra el API y autenticación OIDC con un cliente público (PKCE).</summary>
    private static void AddApiServices(IServiceCollection services, WebAppSettings settings)
    {
        services.AddOidcAuthentication(options =>
        {
            options.ProviderOptions.Authority = settings.OidcAuthority;
            options.ProviderOptions.ClientId = settings.OidcClientId;
            options.ProviderOptions.ResponseType = "code";
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("profile");
            options.ProviderOptions.DefaultScopes.Add("email");
        });

        services.AddScoped<ISessionService, OidcSessionService>();
        services.AddScoped<ApiAuthorizationMessageHandler>();

        services.AddHttpClient<IProfileService, HttpProfileService>(client =>
            {
                client.BaseAddress = ToBaseAddress(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

        services.AddHttpClient<ISportsService, HttpSportsService>(client =>
            {
                client.BaseAddress = ToBaseAddress(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

        // Amistades y escuelas todavía no tienen backend: siguen con los mocks. Son Scoped
        // porque consumen la sesión OIDC, que también es Scoped.
        services.AddSingleton<MockDataStore>();
        services.AddScoped<IFriendsService, MockFriendsService>();
        services.AddScoped<ISchoolsService, MockSchoolsService>();
    }

    private static Uri ToBaseAddress(string apiBaseUrl) =>
        new($"{apiBaseUrl.TrimEnd('/')}/", UriKind.Absolute);
}

