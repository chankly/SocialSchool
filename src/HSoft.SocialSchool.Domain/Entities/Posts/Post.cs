using HSoft.SocialSchool.Domain.Enumerations;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Posts
{
    /// <summary>
    /// Publicación de un usuario: un único elemento multimedia (una foto o un vídeo), un comentario y un
    /// deporte. Nunca se vincula a una maniobra. El autor puede borrarla, pero no editarla.
    /// </summary>
    public sealed class Post : Entity, IAggregateRoot
    {
        private readonly List<Comment> _comments = [];
        private readonly List<Like> _likes = [];

        public Guid AuthorId { get; private set; }
        public Guid SportId { get; private set; }
        public PostMediaType MediaType { get; private set; }
        public string MediaUrl { get; private set; } = string.Empty;
        public string? Caption { get; private set; }
        public DateTime PublishedAt { get; private set; }

        public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
        public IReadOnlyCollection<Like> Likes => _likes.AsReadOnly();

        private Post() { }

        private Post(Guid id) : base(id) { }

        public static Post Create(
            Guid id,
            Guid authorId,
            Guid sportId,
            PostMediaType mediaType,
            string mediaUrl,
            string? caption)
        {
            if (authorId == Guid.Empty)
                throw new ArgumentException("Author is required.", nameof(authorId));

            if (sportId == Guid.Empty)
                throw new ArgumentException("Sport is required.", nameof(sportId));

            if (string.IsNullOrWhiteSpace(mediaUrl))
                throw new ArgumentException("The publication must carry a photo or a video.", nameof(mediaUrl));

            return new Post(id)
            {
                AuthorId = authorId,
                SportId = sportId,
                MediaType = mediaType,
                MediaUrl = mediaUrl,
                Caption = caption,
                PublishedAt = DateTime.UtcNow
            };
        }

        public Comment AddComment(Guid authorId, string text)
        {
            var comment = Comment.Create(this, authorId, text);
            _comments.Add(comment);
            return comment;
        }

        /// <summary>
        /// Borra un comentario. Solo su autor puede hacerlo: nadie borra comentarios ajenos, ni siquiera el
        /// autor de la publicación, que solo puede denunciarlos.
        /// </summary>
        public void DeleteComment(Guid commentId, Guid requesterId)
        {
            var comment = _comments.SingleOrDefault(x => x.Id == commentId)
                ?? throw new InvalidOperationException("The publication does not contain this comment.");

            if (!comment.IsAuthoredBy(requesterId))
                throw new InvalidOperationException("A user can only delete their own comments.");

            _comments.Remove(comment);
        }

        /// <summary>Da "me gusta" a la publicación. Un usuario solo puede reaccionar una vez.</summary>
        public Like AddLike(Guid userId)
        {
            if (_likes.Any(x => x.UserId == userId))
                throw new InvalidOperationException("The user has already liked this publication.");

            var like = Like.Create(this, userId);
            _likes.Add(like);
            return like;
        }

        public void RemoveLike(Guid userId)
        {
            var like = _likes.SingleOrDefault(x => x.UserId == userId)
                ?? throw new InvalidOperationException("The user has not liked this publication.");

            _likes.Remove(like);
        }
    }
}
