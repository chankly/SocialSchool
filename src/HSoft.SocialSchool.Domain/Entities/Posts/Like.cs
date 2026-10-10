using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Domain.Entities.Posts
{
    /// <summary>
    /// "Me gusta" de un usuario sobre una publicación. Entidad hija del agregado <see cref="Post"/>.
    /// </summary>
    public sealed class Like : Entity
    {
        public Guid PostId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Like() { }

        private Like(Guid id) : base(id) { }

        internal static Like Create(Post post, Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User is required.", nameof(userId));

            return new Like(Guid.NewGuid())
            {
                PostId = post.Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
