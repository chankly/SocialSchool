using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.ToTable("NotificationPreferences");

            builder.HasKey(preference => preference.Id);
            builder.Property(preference => preference.Id).ValueGeneratedNever();

            // La relación con User se configura en UserConfiguration, desde el lado principal.
            builder.HasIndex(preference => new { preference.UserId, preference.Channel })
                .IsUnique()
                .HasDatabaseName("IX_NotificationPreferences_UserId_Channel");
        }
    }
}
