using HSoft.SocialSchool.Domain.Entities.Blocks;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Block"/>.</summary>
    public interface IBlockRepository : IBaseRepository<Block>
    {
    }
}
