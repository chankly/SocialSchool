using HSoft.SocialSchool.Domain.Entities.Ratings;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Rating"/>.</summary>
    public interface IRatingRepository : IBaseRepository<Rating>
    {
    }
}
