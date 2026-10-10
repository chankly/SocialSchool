using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Reports
{
    /// <summary>
    /// Denuncia de un contenido inapropiado, dirigida al técnico que realiza la moderación. Mientras está
    /// pendiente, el contenido denunciado deja de mostrarse.
    /// </summary>
    public sealed class Report : Entity, IAggregateRoot
    {
        public ReportableContentType ContentType { get; private set; }
        public Guid ContentId { get; private set; }
        public Guid ReporterId { get; private set; }
        public ReportCategory Category { get; private set; }
        public string? FreeText { get; private set; }
        public ReportStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Report() { }

        private Report(Guid id) : base(id) { }

        public static Report Create(
            Guid id,
            ReportableContentType contentType,
            Guid contentId,
            Guid reporterId,
            ReportCategory category,
            string? freeText)
        {
            if (contentId == Guid.Empty)
                throw new ArgumentException("The reported content is required.", nameof(contentId));

            if (reporterId == Guid.Empty)
                throw new ArgumentException("Reporter is required.", nameof(reporterId));

            return new Report(id)
            {
                ContentType = contentType,
                ContentId = contentId,
                ReporterId = reporterId,
                Category = category,
                FreeText = freeText,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>Marca la denuncia como revisada. La resuelve el técnico, que recibe una notificación.</summary>
        public void Resolve()
        {
            if (Status == ReportStatus.Resolved)
                return;

            Status = ReportStatus.Resolved;
        }
    }
}
