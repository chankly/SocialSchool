using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Retira una solicitud de amistad enviada. Solo puede hacerlo su emisor y solo mientras siga
    /// pendiente; la solicitud queda cancelada en lugar de borrarse.
    /// </summary>
    /// <param name="RequestId">Identificador de la solicitud.</param>
    public sealed record CancelFriendRequestCommand(Guid RequestId) : IRequest<Result>;
}
