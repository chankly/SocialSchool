using HSoft.SocialSchool.Web.Models.Accounts;

namespace HSoft.SocialSchool.Web.Services.Friends.Responses;

/// <summary>Amistad aceptada tal y como se muestra en el listado.</summary>
public sealed record FriendResponse(
    Guid Id,
    string DisplayName,
    string Nickname,
    string? PictureUrl,
    AccountType AccountType,
    string Subtitle);
