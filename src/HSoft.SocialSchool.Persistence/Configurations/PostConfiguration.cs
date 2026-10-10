using HSoft.SocialSchool.Domain.Entities.Posts;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");

            builder.HasKey(post => post.Id);
            builder.Property(post => post.Id).ValueGeneratedNever();

            builder.Property(post => post.MediaUrl).IsRequired().HasMaxLength(2048);
            builder.Property(post => post.Caption).HasMaxLength(1000);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(post => post.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Un deporte se desactiva, no se borra: la referencia se protege.
            builder.HasOne<Sport>()
                .WithMany()
                .HasForeignKey(post => post.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Se declaran desde el lado principal para que EF use el backing field de cada colección.
            builder.HasMany(post => post.Comments)
                .WithOne()
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(post => post.Likes)
                .WithOne()
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(post => new { post.AuthorId, post.PublishedAt })
                .HasDatabaseName("IX_Posts_AuthorId_PublishedAt");

            builder.HasIndex(post => post.SportId).HasDatabaseName("IX_Posts_SportId");
        }
    }
}
