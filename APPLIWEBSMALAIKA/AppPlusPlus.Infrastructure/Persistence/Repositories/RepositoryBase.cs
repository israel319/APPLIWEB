using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly IDbContextFactory<AppDbContext> _dbFactory;

    protected RepositoryBase(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Set<T>().Add(entity);
        await ctx.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Set<T>().Update(entity);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Set<T>().Remove(entity);
        await ctx.SaveChangesAsync();
    }
}
