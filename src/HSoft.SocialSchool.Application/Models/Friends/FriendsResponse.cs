namespace HSoft.SocialSchool.Application.Models.Friends
{
    /// <summary>
    /// Resumen de las amistades del usuario autenticado: sus amigos, las solicitudes recibidas, las
    /// enviadas y las cuentas a las que todavía puede invitar.
    /// </summary>
    /// <param name="Friends">Amigos, ordenados por nick.</param>
    /// <param name="IncomingRequests">Solicitudes recibidas y pendientes.</param>
    /// <param name="OutgoingRequests">Solicitudes enviadas y pendientes.</param>
    /// <param name="Discoverable">Cuentas invitables, como máximo veinte.</param>
    public sealed record FriendsResponse(
        IReadOnlyList<FriendAccountResponse> Friends,
        IReadOnlyList<IncomingFriendRequestResponse> IncomingRequests,
        IReadOnlyList<OutgoingFriendRequestResponse> OutgoingRequests,
        IReadOnlyList<DiscoverableAccountResponse> Discoverable);
}
