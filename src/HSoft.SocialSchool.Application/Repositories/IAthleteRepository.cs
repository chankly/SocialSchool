using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Athlete"/>.</summary>
    public interface IAthleteRepository : IBaseRepository<Athlete>
    {
    }
}
