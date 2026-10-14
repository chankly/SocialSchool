using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Skill"/>.</summary>
    public interface ISkillRepository : IBaseRepository<Skill>
    {
    }
}
