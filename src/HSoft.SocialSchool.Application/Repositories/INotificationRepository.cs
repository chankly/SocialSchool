using HSoft.SocialSchool.Domain.Entities.Notifications;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Notification"/>.</summary>
    public interface INotificationRepository : IBaseRepository<Notification>
    {
    }
}
