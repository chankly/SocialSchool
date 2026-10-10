using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Ratings;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class RatingRepository(MyCompanyDbContext dbContext) : BaseRepository<Rating>(dbContext), IRatingRepository
    {
    }
}
