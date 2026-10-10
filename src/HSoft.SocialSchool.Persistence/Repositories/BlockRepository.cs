using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Blocks;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class BlockRepository(MyCompanyDbContext dbContext) : BaseRepository<Block>(dbContext), IBlockRepository
    {
    }
}
