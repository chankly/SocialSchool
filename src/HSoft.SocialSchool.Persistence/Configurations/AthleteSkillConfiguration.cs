using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class AthleteSkillConfiguration : IEntityTypeConfiguration<AthleteSkill>
    {
        public void Configure(EntityTypeBuilder<AthleteSkill> builder)
        {
            builder.ToTable("AthleteSkills");

            builder.HasKey(athleteSkill => athleteSkill.Id);
            builder.Property(athleteSkill => athleteSkill.Id).ValueGeneratedNever();

            // La relación con Athlete se configura en AthleteConfiguration, desde el lado principal.
            builder.HasOne<Skill>()
                .WithMany()
                .HasForeignKey(athleteSkill => athleteSkill.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(athleteSkill => new { athleteSkill.AthleteId, athleteSkill.SkillId })
                .IsUnique()
                .HasDatabaseName("IX_AthleteSkills_AthleteId_SkillId");
        }
    }
}
