namespace HSoft.SocialSchool.Web.Services.Schools.Responses;

/// <summary>Perfil público completo de una escuela.</summary>
public sealed record SchoolDetailResponse(
    SchoolListItemResponse School,
    string? ContactEmail,
    string? Phone,
    string? Website,
    List<SchoolServiceResponse> Services,
    List<SchoolAttendeeResponse> Attendees);
