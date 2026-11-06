using HSoft.SocialSchool.Web.Models.Accounts;

namespace HSoft.SocialSchool.Web.Services.Schools.Responses;

/// <summary>Perfil público completo de una escuela.</summary>
public sealed record SchoolDetailResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    Guid SportId,
    string SportName,
    string? Description,
    string Country,
    string City,
    string? ContactEmail,
    string? Phone,
    string? Website,
    AccountStatus? AccountStatus,
    List<SchoolServiceResponse> Services,
    List<SchoolAttendeeResponse> Attendees);
