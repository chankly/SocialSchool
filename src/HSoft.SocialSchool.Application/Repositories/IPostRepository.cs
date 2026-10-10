using HSoft.SocialSchool.Domain.Entities.Posts;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Post"/>.</summary>
    public interface IPostRepository : IBaseRepository<Post>
    {
    }
}
