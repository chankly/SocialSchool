using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Ratings;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.ToTable("Ratings");

            builder.HasKey(rating => rating.Id);
            builder.Property(rating => rating.Id).ValueGeneratedNever();

            builder.HasOne<AthleteSkill>()
                .WithMany()
                .HasForeignKey(rating => rating.AthleteSkillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: una cascada desde Users crearía un segundo camino hacia Ratings.
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(rating => rating.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(rating => new { rating.AthleteSkillId, rating.AuthorId })
                .IsUnique()
                .HasDatabaseName("IX_Ratings_AthleteSkillId_AuthorId");
        }
    }
}
