using HSoft.SocialSchool.Domain.Entities.Attendances;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HSoft.SocialSchool.Persistence.Configurations
{
    internal sealed class SchoolAttendanceConfiguration : IEntityTypeConfiguration<SchoolAttendance>
    {
        public void Configure(EntityTypeBuilder<SchoolAttendance> builder)
        {
            builder.ToTable("SchoolAttendances");

            builder.HasKey(attendance => attendance.Id);
            builder.Property(attendance => attendance.Id).ValueGeneratedNever();

            builder.HasOne<Athlete>()
                .WithMany()
                .HasForeignKey(attendance => attendance.AthleteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<School>()
                .WithMany()
                .HasForeignKey(attendance => attendance.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(attendance => attendance.InitiatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Solo puede existir un vínculo activo (Pending, Accepted) por par. El filtro deja convivir como
            // histórico los rechazos y las retiradas, porque el dominio admite volver a solicitarla.
            builder.HasIndex(attendance => new { attendance.AthleteId, attendance.SchoolId })
                .IsUnique()
                .HasFilter("[Status] IN (0, 1)")
                .HasDatabaseName("IX_SchoolAttendances_AthleteId_SchoolId");
        }
    }
}
