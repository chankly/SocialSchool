using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Notifications;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class NotificationRepository(MyCompanyDbContext dbContext) : BaseRepository<Notification>(dbContext), INotificationRepository
    {
    }
}
