using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Acepta una solicitud de amistad recibida. Solo puede hacerlo el destinatario y solo mientras la
    /// solicitud esté pendiente; al aceptarla se crea la amistad.
    /// </summary>
    /// <param name="RequestId">Identificador de la solicitud.</param>
    public sealed record AcceptFriendRequestCommand(Guid RequestId) : IRequest<Result>;
}
