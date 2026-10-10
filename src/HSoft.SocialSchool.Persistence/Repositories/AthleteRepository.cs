using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Athletes;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class AthleteRepository(MyCompanyDbContext dbContext) : BaseRepository<Athlete>(dbContext), IAthleteRepository
    {
    }
}
