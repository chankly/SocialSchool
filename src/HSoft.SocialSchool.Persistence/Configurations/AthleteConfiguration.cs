using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class AthleteConfiguration : IEntityTypeConfiguration<Athlete>
    {
        public void Configure(EntityTypeBuilder<Athlete> builder)
        {
            builder.ToTable("Athletes");

            builder.HasKey(athlete => athlete.Id);
            builder.Property(athlete => athlete.Id).ValueGeneratedNever();

            builder.Property(athlete => athlete.FirstName).IsRequired().HasMaxLength(200);
            builder.Property(athlete => athlete.LastName).IsRequired().HasMaxLength(200);
            builder.Property(athlete => athlete.Country).HasMaxLength(100);
            builder.Property(athlete => athlete.City).HasMaxLength(100);
            builder.Property(athlete => athlete.ProfilePictureUrl).HasMaxLength(2048);
            builder.Property(athlete => athlete.Biography).HasMaxLength(1000);

            // El perfil comparte la clave primaria con su cuenta, de modo que no puede existir sin ella.
            builder.HasOne<User>()
                .WithOne()
                .HasForeignKey<Athlete>(athlete => athlete.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(athlete => athlete.PracticedSports)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "AthleteSports",
                    join => join.HasOne<Sport>().WithMany().HasForeignKey("SportId").OnDelete(DeleteBehavior.Restrict),
                    join => join.HasOne<Athlete>().WithMany().HasForeignKey("AthleteId").OnDelete(DeleteBehavior.Cascade));

            // Se declara desde el lado principal para que EF use el backing field de la colección.
            builder.HasMany(athlete => athlete.DeclaredSkills)
                .WithOne()
                .HasForeignKey(athleteSkill => athleteSkill.AthleteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
