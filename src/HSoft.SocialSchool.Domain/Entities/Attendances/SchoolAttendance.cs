using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Attendances
{
    /// <summary>
    /// Vínculo de asistencia entre un deportista y una escuela. Puede iniciarlo cualquiera de las dos
    /// partes y la otra debe aceptarlo. Mientras está aceptado, la escuela ve el perfil del deportista
    /// como si fueran amigos y puede puntuar sus maniobras.
    /// </summary>
    public sealed class SchoolAttendance : Entity, IAggregateRoot
    {
        public Guid AthleteId { get; private set; }
        public Guid SchoolId { get; private set; }
        public Guid InitiatedById { get; private set; }
        public AttendanceStatus Status { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? ResolvedAt { get; private set; }

        private SchoolAttendance() { }

        private SchoolAttendance(Guid id) : base(id) { }

        public static SchoolAttendance Create(Guid id, Guid athleteId, Guid schoolId, Guid initiatedById)
        {
            if (athleteId == Guid.Empty)
                throw new ArgumentException("Athlete is required.", nameof(athleteId));

            if (schoolId == Guid.Empty)
                throw new ArgumentException("School is required.", nameof(schoolId));

            if (initiatedById != athleteId && initiatedById != schoolId)
                throw new ArgumentException(
                    "The attendance link must be started by the athlete or the school.",
                    nameof(initiatedById));

            return new SchoolAttendance(id)
            {
                AthleteId = athleteId,
                SchoolId = schoolId,
                InitiatedById = initiatedById,
                Status = AttendanceStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };
        }

        /// <summary>Confirma la asistencia: habilita la visibilidad del perfil y el derecho a puntuar.</summary>
        public void Accept()
        {
            EnsurePending();

            Status = AttendanceStatus.Accepted;
            ResolvedAt = DateTime.UtcNow;
        }

        /// <summary>Rechaza la propuesta. El rechazo es silencioso y puede reenviarse más adelante.</summary>
        public void Reject()
        {
            EnsurePending();

            Status = AttendanceStatus.Rejected;
            ResolvedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Retira el vínculo. Cualquiera de las dos partes puede hacerlo: la asistencia desaparece, el
        /// deportista sale de la lista de asistentes y se borran las puntuaciones que la escuela había dado.
        /// </summary>
        public void Withdraw()
        {
            if (Status is not (AttendanceStatus.Pending or AttendanceStatus.Accepted))
                throw new InvalidOperationException("Only a pending or accepted attendance link can be withdrawn.");

            Status = AttendanceStatus.Withdrawn;
            ResolvedAt = DateTime.UtcNow;
        }

        private void EnsurePending()
        {
            if (Status != AttendanceStatus.Pending)
                throw new InvalidOperationException("Only a pending attendance link can be resolved.");
        }
    }
}
