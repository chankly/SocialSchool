using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Rechaza una solicitud de amistad recibida. Solo puede hacerlo el destinatario y solo mientras la
    /// solicitud esté pendiente. El rechazo es silencioso: el emisor no recibe aviso y puede volver a
    /// intentarlo.
    /// </summary>
    /// <param name="RequestId">Identificador de la solicitud.</param>
    public sealed record RejectFriendRequestCommand(Guid RequestId) : IRequest<Result>;
}
