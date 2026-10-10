using HSoft.SocialSchool.Domain.Entities.Reports;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");

            builder.HasKey(report => report.Id);
            builder.Property(report => report.Id).ValueGeneratedNever();

            builder.Property(report => report.FreeText).HasMaxLength(200);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(report => report.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // El contenido denunciado se referencia por tipo e Id, sin clave ajena: puede ser un
            // comentario o una publicación.
            builder.HasIndex(report => report.Status).HasDatabaseName("IX_Reports_Status");
        }
    }
}
