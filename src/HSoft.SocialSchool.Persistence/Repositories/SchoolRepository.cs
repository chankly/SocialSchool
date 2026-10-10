using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Schools;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class SchoolRepository(MyCompanyDbContext dbContext) : BaseRepository<School>(dbContext), ISchoolRepository
    {
    }
}
