using HSoft.SocialSchool.Web.Services.Profile;
using HSoft.SocialSchool.Web.Services.Profile.Responses;
using HSoft.SocialSchool.Web.Services.Session;

namespace HSoft.SocialSchool.Web.State;

/// <summary>Estados posibles de la consulta del perfil propio.</summary>
public enum ProfileLoadStatus
{
    /// <summary>Todavía no se ha consultado.</summary>
    Unknown = 0,

    /// <summary>Consulta en curso.</summary>
    Loading = 1,

    /// <summary>Hay perfil (200).</summary>
    Loaded = 2,

    /// <summary>No hay perfil todavía (204).</summary>
    NoProfile = 3,

    /// <summary>No hay sesión (401).</summary>
    Unauthorized = 4,

    /// <summary>Error de la API simulada.</summary>
    Failed = 5
}

/// <summary>
/// Estado compartido del perfil propio. Evita repetir la misma consulta cuando varias
/// pantallas se montan a la vez y notifica a los componentes suscritos.
/// </summary>
public sealed class ProfileState : IDisposable
{
    private readonly IProfileService _profileService;
    private readonly ISessionService _sessionService;
    private Task? _inFlight;
    private int _version;

    /// <summary>Crea el estado y queda a la escucha de los cambios de sesión.</summary>
    public ProfileState(IProfileService profileService, ISessionService sessionService)
    {
        _profileService = profileService;
        _sessionService = sessionService;
        _sessionService.Changed += HandleSessionChanged;
    }

    /// <summary>Estado actual de la consulta.</summary>
    public ProfileLoadStatus Status { get; private set; } = ProfileLoadStatus.Unknown;

    /// <summary>Perfil cargado, si existe.</summary>
    public ProfileResponse? Profile { get; private set; }

    /// <summary>Mensaje del último fallo.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Se dispara cuando cambia el estado.</summary>
    public event Action? Changed;

    /// <summary>
    /// Carga el perfil. Las llamadas concurrentes comparten la misma tarea y, si el estado
    /// ya es definitivo, no se repite la consulta salvo que se fuerce.
    /// </summary>
    public Task LoadAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (!force && _inFlight is { IsCompleted: false })
        {
            return _inFlight;
        }

        if (!force && Status is ProfileLoadStatus.Loaded
            or ProfileLoadStatus.NoProfile
            or ProfileLoadStatus.Unauthorized)
        {
            return Task.CompletedTask;
        }

        _version++;
        var version = _version;
        var task = LoadCoreAsync(version, cancellationToken);
        _inFlight = task;
        return task;
    }

    /// <summary>Aplica un perfil recién creado sin volver a consultar la API.</summary>
    public void ApplyProfile(ProfileResponse profile)
    {
        Profile = profile;
        Status = ProfileLoadStatus.Loaded;
        ErrorMessage = null;
        Changed?.Invoke();
    }

    /// <summary>Descarta el perfil cargado porque la sesión ha cambiado.</summary>
    public void Invalidate()
    {
        Profile = null;
        Status = ProfileLoadStatus.Unknown;
        ErrorMessage = null;
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public void Dispose() => _sessionService.Changed -= HandleSessionChanged;

    private async Task LoadCoreAsync(int version, CancellationToken cancellationToken)
    {
        Status = ProfileLoadStatus.Loading;
        Profile = null;
        ErrorMessage = null;
        Changed?.Invoke();

        try
        {
            var result = await _profileService.GetMyProfileAsync(cancellationToken);

            if (version != _version)
            {
                return;
            }

            switch (result)
            {
                case ProfileQueryResult.Success success:
                    Profile = success.Profile;
                    Status = ProfileLoadStatus.Loaded;
                    break;

                case ProfileQueryResult.NoContent:
                    Status = ProfileLoadStatus.NoProfile;
                    break;

                case ProfileQueryResult.Unauthorized:
                    Status = ProfileLoadStatus.Unauthorized;
                    break;

                case ProfileQueryResult.Failure failure:
                    Status = ProfileLoadStatus.Failed;
                    ErrorMessage = failure.Message;
                    break;

                default:
                    Status = ProfileLoadStatus.Failed;
                    ErrorMessage = "La API simulada ha devuelto una respuesta no reconocida.";
                    break;
            }
        }
        catch (Exception)
        {
            if (version != _version)
            {
                return;
            }

            Status = ProfileLoadStatus.Failed;
            ErrorMessage = "No se ha podido consultar el perfil simulado.";
        }
        finally
        {
            if (version == _version)
            {
                _inFlight = null;
                Changed?.Invoke();
            }
        }
    }

    private void HandleSessionChanged()
    {
        Invalidate();
        _ = LoadAsync(force: true);
    }
}
