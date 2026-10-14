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
        }
    }
}
