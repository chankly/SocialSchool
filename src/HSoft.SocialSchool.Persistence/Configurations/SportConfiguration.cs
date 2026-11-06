using HSoft.SocialSchool.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class SportConfiguration : IEntityTypeConfiguration<Sport>
    {
        public void Configure(EntityTypeBuilder<Sport> builder)
        {
            builder.ToTable("Sports");

            builder.HasKey(sport => sport.Id);
            builder.Property(sport => sport.Id).ValueGeneratedNever();

            builder.Property(sport => sport.Name).IsRequired().HasMaxLength(200);

            builder.HasIndex(sport => sport.Name).IsUnique().HasDatabaseName("IX_Sports_Name");

            // Semilla de demostración: sin filas el selector del formulario no tiene nada que ofrecer. Son
            // identificadores fijos para que la migración sea reproducible y se puedan sustituir por el
            // catálogo real del técnico con otra migración.
            builder.HasData(
                new { Id = Guid.Parse("a1b2c3d4-0001-4000-8000-000000000001"), Name = "Skate", IsActive = true },
                new { Id = Guid.Parse("a1b2c3d4-0002-4000-8000-000000000002"), Name = "Surf", IsActive = true },
                new { Id = Guid.Parse("a1b2c3d4-0003-4000-8000-000000000003"), Name = "Snowboard", IsActive = true },
                new { Id = Guid.Parse("a1b2c3d4-0004-4000-8000-000000000004"), Name = "BMX", IsActive = true },
                new { Id = Guid.Parse("a1b2c3d4-0005-4000-8000-000000000005"), Name = "Parkour", IsActive = true },
                new { Id = Guid.Parse("a1b2c3d4-0006-4000-8000-000000000006"), Name = "Escalada", IsActive = true });
        }
    }
}
