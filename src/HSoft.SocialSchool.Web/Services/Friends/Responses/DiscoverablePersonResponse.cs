using HSoft.SocialSchool.Web.Models.Accounts;

namespace HSoft.SocialSchool.Web.Services.Friends.Responses;

/// <summary>Cuenta que el usuario puede descubrir para enviarle una solicitud.</summary>
public sealed record DiscoverablePersonResponse(
    Guid Id,
    string DisplayName,
    string Nickname,
    AccountType AccountType,
    string Subtitle);
