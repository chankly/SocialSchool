using FluentValidation;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HSoft.SocialSchool.Web.Extensions;

/// <summary>Registro de dependencias de la maqueta.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra la configuración, los servicios simulados y los validadores. Los mocks
    /// comparten <see cref="MockDataStore"/>, por eso se registran como Singleton.
    /// </summary>
    public static IServiceCollection AddWebAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var apiSection = configuration.GetSection("Api");
        var authenticationSection = configuration.GetSection("Authentication");

        var settings = new WebAppSettings
        {
            ApiBaseUrl = apiSection["BaseUrl"] ?? "https://localhost:7146",
            UseMocks = !bool.TryParse(apiSection["UseMocks"], out var useMocks) || useMocks,
            KeycloakAuthority = authenticationSection["Authority"]
                ?? "https://keycloak.example.com/realms/socialschool",
            KeycloakClientId = authenticationSection["ClientId"] ?? "SocialSchool-Web",
            ApiAudience = authenticationSection["ApiAudience"]
                ?? "0B28A13A-DF06-4840-958A-CC33605B3EB7"
        };

        services.AddSingleton(settings);

        services.AddSingleton<MockDataStore>();

        services.AddSingleton<MockSessionService>();
        services.AddSingleton<ISessionService>(provider => provider.GetRequiredService<MockSessionService>());

        services.AddSingleton<ISportsService, MockSportsService>();
        services.AddSingleton<IProfileService, MockProfileService>();
        services.AddSingleton<IFriendsService, MockFriendsService>();
        services.AddSingleton<ISchoolsService, MockSchoolsService>();

        services.AddScoped<ProfileState>();

        services.AddSingleton<IValidator<CreateAthleteProfileRequest>, CreateAthleteProfileRequestValidator>();
        services.AddSingleton<IValidator<CreateSchoolProfileRequest>, CreateSchoolProfileRequestValidator>();

        // Preparado para cuando se sustituyan los mocks por servicios HTTP reales.
        services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri(settings.ApiBaseUrl) });

        return services;
    }
}
