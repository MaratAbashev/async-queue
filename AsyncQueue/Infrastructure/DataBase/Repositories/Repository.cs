using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public abstract class Repository<TEntity, TId>: 
    IRepository<TEntity,TId> 
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly BrokerDbContext context;

    protected Repository(BrokerDbContext context)
    {
        this.context = context;
        _dbSet = this.context.Set<TEntity>();
    }
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var existingEntity = await _dbSet
            .FirstOrDefaultAsync(e => e.Id.Equals(entity.Id));
        if (existingEntity != null)
        {
            throw new ArgumentException($"Entity with {existingEntity.Id} already exists");
        }
        await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var existingEntity = await _dbSet
            .FirstOrDefaultAsync(e => e.Id.Equals(entity.Id));
        if (existingEntity == null)
        {
            throw new ArgumentException("Entity not found");
        }
        _dbSet.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<TEntity> PatchAsync(TId id, Action<TEntity> patch)
    {
        var existingEntity = await _dbSet
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
        if (existingEntity == null)
        {
            throw new ArgumentException("Entity not found");
        }
        patch(existingEntity);
        await context.SaveChangesAsync();
        return existingEntity;
    }

    public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        await _dbSet
            .Where(predicate)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsDeleted,true));
    }

    public virtual async Task<IEnumerable<TEntity?>> GetAllByFilterAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(filter)
            .ToListAsync();
    }

    public virtual async Task<TEntity?> GetByFilterAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(filter);
    }
}