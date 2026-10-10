using HSoft.SocialSchool.Domain.Entities.Posts;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class LikeConfiguration : IEntityTypeConfiguration<Like>
    {
        public void Configure(EntityTypeBuilder<Like> builder)
        {
            builder.ToTable("Likes");

            builder.HasKey(like => like.Id);
            builder.Property(like => like.Id).ValueGeneratedNever();

            // La relación con Post se configura en PostConfiguration, desde el lado principal.
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(like => new { like.PostId, like.UserId })
                .IsUnique()
                .HasDatabaseName("IX_Likes_PostId_UserId");
        }
    }
}
