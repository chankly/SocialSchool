using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            builder.ToTable("Schools");

            builder.HasKey(school => school.Id);
            builder.Property(school => school.Id).ValueGeneratedNever();

            builder.Property(school => school.Name).IsRequired().HasMaxLength(200);
            builder.Property(school => school.LogoUrl).HasMaxLength(2048);
            builder.Property(school => school.Description).HasMaxLength(1000);
            builder.Property(school => school.Country).HasMaxLength(100);
            builder.Property(school => school.City).HasMaxLength(100);
            builder.Property(school => school.ContactEmail).HasMaxLength(320);
            builder.Property(school => school.Phone).HasMaxLength(30);
            builder.Property(school => school.Website).HasMaxLength(2048);

            // El perfil comparte la clave primaria con su cuenta, de modo que no puede existir sin ella.
            builder.HasOne<User>()
                .WithOne()
                .HasForeignKey<School>(school => school.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Sport>()
                .WithMany()
                .HasForeignKey(school => school.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Se declara desde el lado principal para que EF use el backing field de la colección.
            builder.HasMany(school => school.Services)
                .WithOne()
                .HasForeignKey(service => service.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
