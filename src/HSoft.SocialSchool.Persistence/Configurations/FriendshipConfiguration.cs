using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            builder.ToTable("Friendships");

            builder.HasKey(friendship => friendship.Id);
            builder.Property(friendship => friendship.Id).ValueGeneratedNever();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(friendship => friendship.UserAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(friendship => friendship.UserBId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(friendship => new { friendship.UserAId, friendship.UserBId })
                .IsUnique()
                .HasDatabaseName("IX_Friendships_UserAId_UserBId");

            builder.HasIndex(friendship => friendship.UserBId)
                .HasDatabaseName("IX_Friendships_UserBId");
        }
    }
}
