using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Posts
{
    /// <summary>
    /// Comentario de un usuario sobre una publicación. Entidad hija del agregado <see cref="Post"/>.
    /// Solo su autor puede borrarlo; los comentarios ajenos se denuncian.
    /// </summary>
    public sealed class Comment : Entity
    {
        public Guid PostId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Text { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private Comment() { }

        private Comment(Guid id) : base(id) { }

        internal static Comment Create(Post post, Guid authorId, string text)
        {
            if (authorId == Guid.Empty)
                throw new ArgumentException("Author is required.", nameof(authorId));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Comment text cannot be empty.", nameof(text));

            return new Comment(Guid.NewGuid())
            {
                PostId = post.Id,
                AuthorId = authorId,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };
        }

        internal bool IsAuthoredBy(Guid userId) => AuthorId == userId;
    }
}
