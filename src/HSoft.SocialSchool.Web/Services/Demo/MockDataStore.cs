using HSoft.SocialSchool.Web.Services.Friends.Responses;
using HSoft.SocialSchool.Web.Services.Profile.Responses;

namespace HSoft.SocialSchool.Web.Services.Demo;

/// <summary>
/// Estado en memoria compartido por todos los servicios simulados. Al recargar la
/// aplicación se vuelve a sembrar, de modo que la maqueta siempre parte del mismo punto.
/// </summary>
public sealed class MockDataStore
{
    private readonly HashSet<string> _reservedNicknames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Crea el almacén y carga los datos de demostración.</summary>
    public MockDataStore() => Reset();

    /// <summary>Se dispara cuando cambia el estado mutable de la maqueta.</summary>
    public event Action? Changed;

    /// <summary>Perfiles ya completados, indexados por cuenta.</summary>
    public Dictionary<Guid, ProfileResponse> Profiles { get; } = new();

    /// <summary>Amistades aceptadas.</summary>
    public List<FriendResponse> Friends { get; } = [];

    /// <summary>Solicitudes de amistad recibidas y enviadas.</summary>
    public List<FriendRequestResponse> Requests { get; } = [];

    /// <summary>Cuentas que se pueden descubrir para enviar una solicitud.</summary>
    public List<DiscoverablePersonResponse> People { get; } = [];

    /// <summary>Intenta recuperar el perfil de una cuenta.</summary>
    public bool TryGetProfile(Guid userId, out ProfileResponse? profile) =>
        Profiles.TryGetValue(userId, out profile);

    /// <summary>Comprueba si un nick ya está reservado por otra cuenta.</summary>
    public bool IsNicknameTaken(string nickname) => _reservedNicknames.Contains(nickname.Trim());

    /// <summary>Guarda un perfil nuevo y reserva su nick.</summary>
    public void SaveProfile(ProfileResponse profile)
    {
        Profiles[profile.Id] = profile;
        _reservedNicknames.Add(profile.Nickname);
        Changed?.Invoke();
    }

    /// <summary>Vuelve a sembrar los datos iniciales de la maqueta.</summary>
    public void Reset()
    {
        Profiles.Clear();
        Friends.Clear();
        Requests.Clear();
        People.Clear();
        _reservedNicknames.Clear();

        foreach (var nickname in new[] { "admin", "socialschool", "soporte", "ada", "surf" })
        {
            _reservedNicknames.Add(nickname);
        }

        DemoDataSeeder.Seed(this);
        Changed?.Invoke();
    }

    internal void ReserveNickname(string nickname) => _reservedNicknames.Add(nickname.Trim());
}
