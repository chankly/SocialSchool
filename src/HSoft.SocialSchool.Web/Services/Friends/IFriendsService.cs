using HSoft.SocialSchool.Web.Services.Friends.Responses;

namespace HSoft.SocialSchool.Web.Services.Friends;

/// <summary>
/// Amistades y solicitudes simuladas. Las operaciones modifican el estado en memoria para
/// que la interfaz responda como lo haría contra la API real.
/// </summary>
public interface IFriendsService
{
    /// <summary>Obtiene amigos, solicitudes y cuentas por descubrir.</summary>
    Task<FriendsOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>Acepta una solicitud recibida.</summary>
    Task<FriendActionResult> AcceptRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Rechaza una solicitud recibida. El rechazo es silencioso.</summary>
    Task<FriendActionResult> RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Retira una solicitud enviada.</summary>
    Task<FriendActionResult> CancelRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Deshace una amistad.</summary>
    Task<FriendActionResult> RemoveFriendAsync(Guid friendId, CancellationToken cancellationToken = default);

    /// <summary>Envía una solicitud a una cuenta descubierta.</summary>
    Task<FriendActionResult> SendRequestAsync(Guid personId, CancellationToken cancellationToken = default);
}
