namespace HSoft.SocialSchool.Domain.Primitives
{
    public interface IBaseRepository<T> where T : class, IAggregateRoot
    {
        Task<T?> GetById(Guid id);
        Task<T?> GetByIdReadOnly(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task Add(T entity);
        Task AddRange(IReadOnlyList<T> entities);
        Task AddRange<TEntity>(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;
        Task Update(T entity);
        Task Delete(T entity);
        Task Save(T entity);
        Task Save(IReadOnlyList<T> entities);
    }
}
