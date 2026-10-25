using HSoft.SocialSchool.Web.Services.Sports.Responses;

namespace HSoft.SocialSchool.Web.Services.Sports;

/// <summary>Acceso al catálogo de deportes.</summary>
public interface ISportsService
{
    /// <summary>Obtiene todos los deportes del catálogo.</summary>
    Task<IReadOnlyList<SportResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un deporte por su identificador.</summary>
    Task<SportResponse?> GetByIdAsync(Guid sportId, CancellationToken cancellationToken = default);
}
