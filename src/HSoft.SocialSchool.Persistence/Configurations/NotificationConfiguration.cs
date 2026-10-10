using HSoft.SocialSchool.Domain.Entities.Notifications;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(notification => notification.Id);
            builder.Property(notification => notification.Id).ValueGeneratedNever();

            builder.Property(notification => notification.ContentReference).HasMaxLength(200);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(notification => notification.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(notification => new { notification.RecipientId, notification.IsRead })
                .HasDatabaseName("IX_Notifications_RecipientId_IsRead");
        }
    }
}
