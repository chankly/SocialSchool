using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
    {
        public void Configure(EntityTypeBuilder<FriendRequest> builder)
        {
            builder.ToTable("FriendRequests");

            builder.HasKey(friendRequest => friendRequest.Id);
            builder.Property(friendRequest => friendRequest.Id).ValueGeneratedNever();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(friendRequest => friendRequest.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(friendRequest => friendRequest.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(friendRequest => new { friendRequest.ReceiverId, friendRequest.Status })
                .HasDatabaseName("IX_FriendRequests_ReceiverId_Status");

            builder.HasIndex(friendRequest => friendRequest.RequesterId)
                .HasDatabaseName("IX_FriendRequests_RequesterId");
        }
    }
}
