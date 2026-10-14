using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Posts;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class PostRepository(MyCompanyDbContext dbContext) : BaseRepository<Post>(dbContext), IPostRepository
    {
    }
}
