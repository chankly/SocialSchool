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

            // Una fila por par deportista-escuela: volver a solicitar no inserta otra fila, devuelve esa a
            // pendiente. Por eso la unicidad se expresa con dos campos y sin filtro del motor.
            builder.HasIndex(attendance => new { attendance.AthleteId, attendance.SchoolId })
                .IsUnique()
                .HasDatabaseName("IX_SchoolAttendances_AthleteId_SchoolId");
        }
    }
}
