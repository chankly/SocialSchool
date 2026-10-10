using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="School"/>.</summary>
    public interface ISchoolRepository : IBaseRepository<School>
    {
    }
}
