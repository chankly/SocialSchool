using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="User"/>.</summary>
    public interface IUserRepository : IBaseRepository<User>
    {
    }
}
