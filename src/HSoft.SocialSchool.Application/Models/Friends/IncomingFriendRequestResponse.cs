namespace HSoft.SocialSchool.Application.Models.Friends
{
    /// <summary>
    /// Solicitud de amistad recibida por el usuario autenticado, a la espera de que la acepte o la rechace.
    /// </summary>
    /// <param name="Id">Identificador de la solicitud.</param>
    /// <param name="Requester">Cuenta que envió la solicitud.</param>
    /// <param name="RequestedAt">Momento en el que se envió la solicitud.</param>
    public sealed record IncomingFriendRequestResponse(
        Guid Id,
        FriendAccountResponse Requester,
        DateTime RequestedAt);
}
