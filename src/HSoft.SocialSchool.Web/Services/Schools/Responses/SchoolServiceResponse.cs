namespace HSoft.SocialSchool.Web.Services.Schools.Responses;

/// <summary>Servicio que ofrece una escuela.</summary>
public sealed record SchoolServiceResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Schedule);
