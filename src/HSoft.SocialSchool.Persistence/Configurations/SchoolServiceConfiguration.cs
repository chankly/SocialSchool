using HSoft.SocialSchool.Domain.Entities.Schools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class SchoolServiceConfiguration : IEntityTypeConfiguration<SchoolService>
    {
        public void Configure(EntityTypeBuilder<SchoolService> builder)
        {
            builder.ToTable("SchoolServices");

            builder.HasKey(service => service.Id);
            builder.Property(service => service.Id).ValueGeneratedNever();

            builder.Property(service => service.Name).IsRequired().HasMaxLength(200);
            builder.Property(service => service.Description).HasMaxLength(1000);
            builder.Property(service => service.Price).HasPrecision(18, 2);
            builder.Property(service => service.Currency).IsRequired().HasMaxLength(3);

            // La relación con School se configura en SchoolConfiguration, desde el lado principal.
        }
    }
}
