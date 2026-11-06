namespace HSoft.SocialSchool.Web.Services.Schools.Responses;

/// <summary>Deportista que asiste a una escuela. Solo se publica el nick y la foto.</summary>
public sealed record SchoolAttendeeResponse(
    Guid Id,
    string Nickname,
    string? ProfilePictureUrl);
