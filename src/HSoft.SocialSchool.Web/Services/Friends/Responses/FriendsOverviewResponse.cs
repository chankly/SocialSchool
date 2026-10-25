namespace HSoft.SocialSchool.Web.Services.Friends.Responses;

/// <summary>Resumen de la pantalla de amistades.</summary>
public sealed record FriendsOverviewResponse(
    List<FriendResponse> Friends,
    List<FriendRequestResponse> IncomingRequests,
    List<FriendRequestResponse> OutgoingRequests,
    List<DiscoverablePersonResponse> Suggestions);
