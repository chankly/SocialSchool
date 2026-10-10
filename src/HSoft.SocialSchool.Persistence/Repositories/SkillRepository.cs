using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Sports;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class SkillRepository(MyCompanyDbContext dbContext) : BaseRepository<Skill>(dbContext), ISkillRepository
    {
    }
}
