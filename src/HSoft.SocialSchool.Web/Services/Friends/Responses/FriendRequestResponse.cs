using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Models.Friends;

namespace HSoft.SocialSchool.Web.Services.Friends.Responses;

/// <summary>Solicitud de amistad recibida o enviada.</summary>
public sealed record FriendRequestResponse(
    Guid Id,
    Guid PersonId,
    string DisplayName,
    string Nickname,
    string? PictureUrl,
    AccountType AccountType,
    string Subtitle,
    DateTimeOffset RequestedAt,
    FriendRequestDirection Direction);
