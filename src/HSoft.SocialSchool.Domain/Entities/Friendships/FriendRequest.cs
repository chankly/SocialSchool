using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Friendships
{
    /// <summary>
    /// Solicitud de amistad. Solo puede darse entre deportistas, o entre deportista y escuela; nunca entre
    /// dos escuelas. La comprobación de los tipos de cuenta y la ausencia de bloqueo corresponden al caso
    /// de uso, que es quien dispone de esa información.
    /// </summary>
    public sealed class FriendRequest : Entity, IAggregateRoot
    {
        public Guid RequesterId { get; private set; }
        public Guid ReceiverId { get; private set; }
        public RequestStatus Status { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? ResolvedAt { get; private set; }

        private FriendRequest() { }

        private FriendRequest(Guid id) : base(id) { }

        public static FriendRequest Create(Guid id, Guid requesterId, Guid receiverId)
        {
            if (requesterId == Guid.Empty)
                throw new ArgumentException("Requester is required.", nameof(requesterId));

            if (receiverId == Guid.Empty)
                throw new ArgumentException("Receiver is required.", nameof(receiverId));

            if (requesterId == receiverId)
                throw new ArgumentException("A user cannot send a friend request to themselves.", nameof(receiverId));

            return new FriendRequest(id)
            {
                RequesterId = requesterId,
                ReceiverId = receiverId,
                Status = RequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };
        }

        /// <summary>Acepta la solicitud. El caso de uso crea la amistad correspondiente.</summary>
        public void Accept()
        {
            EnsurePending();

            Status = RequestStatus.Accepted;
            ResolvedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Rechaza la solicitud. El rechazo es silencioso: el solicitante no recibe aviso y puede volver a
        /// enviarla más adelante.
        /// </summary>
        public void Reject()
        {
            EnsurePending();

            Status = RequestStatus.Rejected;
            ResolvedAt = DateTime.UtcNow;
        }

        /// <summary>El propio solicitante retira su solicitud antes de que sea resuelta.</summary>
        public void Cancel()
        {
            EnsurePending();

            Status = RequestStatus.Cancelled;
            ResolvedAt = DateTime.UtcNow;
        }

        private void EnsurePending()
        {
            if (Status != RequestStatus.Pending)
                throw new InvalidOperationException("Only a pending friend request can be resolved.");
        }
    }
}
