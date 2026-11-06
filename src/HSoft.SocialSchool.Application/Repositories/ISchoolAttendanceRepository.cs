using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Domain.Entities.Attendances;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="SchoolAttendance"/>.</summary>
    public interface ISchoolAttendanceRepository : IBaseRepository<SchoolAttendance>
    {
        /// <summary>
        /// Devuelve los deportistas con asistencia aceptada en la escuela, con su nick y su foto, ordenados
        /// por nick. Solo cuentan las asistencias aceptadas: las pendientes, rechazadas o retiradas no
        /// publican a nadie.
        /// </summary>
        /// <param name="schoolId">Escuela de la que se quieren los asistentes.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<SchoolAttendeeItem>> GetAcceptedAttendeesReadOnlyAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default);
    }
}
