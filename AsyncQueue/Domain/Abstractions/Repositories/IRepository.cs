using System.Linq.Expressions;

namespace Domain.Abstractions.Repositories;

public interface IRepository<TEntity, in TId>
{
    public Task<TEntity> AddAsync(TEntity entity);
    public Task<TEntity> UpdateAsync(TEntity entity);
    public Task<TEntity> PatchAsync(TId id, Action<TEntity> patch);
    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
    public Task<IEnumerable<TEntity?>> GetAllByFilterAsync(Expression<Func<TEntity, bool>> filter);
    public Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter);
}