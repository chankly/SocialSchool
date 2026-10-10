using HSoft.SocialSchool.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
    {
        public void Configure(EntityTypeBuilder<Skill> builder)
        {
            builder.ToTable("Skills");

            builder.HasKey(skill => skill.Id);
            builder.Property(skill => skill.Id).ValueGeneratedNever();

            builder.Property(skill => skill.Name).IsRequired().HasMaxLength(200);

            // Un deporte se desactiva, no se borra: la referencia se protege.
            builder.HasOne<Sport>()
                .WithMany()
                .HasForeignKey(skill => skill.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(skill => new { skill.SportId, skill.Name })
                .IsUnique()
                .HasDatabaseName("IX_Skills_SportId_Name");
        }
    }
}
