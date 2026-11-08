using FluentValidation;
using HSoft.SocialSchool.Web.Authentication;
using HSoft.SocialSchool.Web.Configuration;
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
/// Registro de dependencias del cliente. Solo hay un camino: servicios HTTP contra el API y
/// sesión OIDC. Sin <c>ApiBaseUrl</c> y <c>Oidc</c> completos no se registra ninguno y la
/// aplicación muestra un aviso de configuración incompleta en lugar de datos de ejemplo.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registra la configuración y, si está completa, los servicios que consume la UI.</summary>
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

        services.AddSingleton<IValidator<CreateAthleteProfileRequest>, CreateAthleteProfileRequestValidator>();
        services.AddSingleton<IValidator<CreateSchoolProfileRequest>, CreateSchoolProfileRequestValidator>();

        // Sin configuración completa no hay nada que consultar: App.razor pinta el aviso.
        if (!settings.IsConfigured)
        {
            return services;
        }

        AddApiServices(services, settings);

        services.AddScoped<ProfileState>();

        return services;
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
        services.AddScoped<UnauthorizedRedirectHandler>();

        services.AddHttpClient<IProfileService, HttpProfileService>(client =>
            {
                client.BaseAddress = ToBaseAddress(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
            .AddHttpMessageHandler<UnauthorizedRedirectHandler>();

        services.AddHttpClient<ISportsService, HttpSportsService>(client =>
            {
                client.BaseAddress = ToBaseAddress(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
            .AddHttpMessageHandler<UnauthorizedRedirectHandler>();

        services.AddHttpClient<IFriendsService, HttpFriendsService>(client =>
            {
                client.BaseAddress = ToBaseAddress(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
            .AddHttpMessageHandler<UnauthorizedRedirectHandler>();

        services.AddHttpClient<ISchoolsService, HttpSchoolsService>(client =>
            {
                client.BaseAddress = ToBaseAddress(settings.ApiBaseUrl);
            })
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>()
            .AddHttpMessageHandler<UnauthorizedRedirectHandler>();
    }

    private static Uri ToBaseAddress(string apiBaseUrl) =>
        new($"{apiBaseUrl.TrimEnd('/')}/", UriKind.Absolute);
}

