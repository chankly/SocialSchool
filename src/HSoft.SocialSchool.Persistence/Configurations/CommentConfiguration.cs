using HSoft.SocialSchool.Domain.Entities.Posts;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(comment => comment.Id);
            builder.Property(comment => comment.Id).ValueGeneratedNever();

            builder.Property(comment => comment.Text).IsRequired().HasMaxLength(1000);

            // La relación con Post se configura en PostConfiguration, desde el lado principal.
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(comment => comment.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
