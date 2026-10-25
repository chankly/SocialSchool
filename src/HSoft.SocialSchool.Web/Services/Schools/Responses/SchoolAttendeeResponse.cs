namespace HSoft.SocialSchool.Web.Services.Schools.Responses;

/// <summary>Deportista que asiste a una escuela.</summary>
public sealed record SchoolAttendeeResponse(
    Guid Id,
    string DisplayName,
    string Subtitle);
