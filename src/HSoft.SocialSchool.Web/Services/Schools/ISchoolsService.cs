using HSoft.SocialSchool.Web.Services.Schools.Responses;

namespace HSoft.SocialSchool.Web.Services.Schools;

/// <summary>Catálogo y perfil público de las escuelas.</summary>
public interface ISchoolsService
{
    /// <summary>Obtiene todas las escuelas visibles para el usuario autenticado.</summary>
    Task<IReadOnlyList<SchoolListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene el detalle de una escuela.</summary>
    Task<SchoolDetailResponse?> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken = default);
}
