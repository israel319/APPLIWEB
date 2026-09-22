using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public ServiceRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<Service> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Services.Include(s => s.Department).Include(s => s.Employees)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new InvalidOperationException($"Service with ID {id} not found");
    }

    public async Task<Service?> GetByCodeAsync(string serviceCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Services.FirstOrDefaultAsync(s => s.ServiceCode == serviceCode);
    }

    public async Task<IEnumerable<Service>> GetByDepartmentAsync(int departmentId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Services.Include(s => s.Department).Include(s => s.Employees)
            .Where(s => s.DepartmentId == departmentId).OrderBy(s => s.ServiceName).ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Services.Include(s => s.Department).Include(s => s.Employees)
            .OrderBy(s => s.ServiceName).ToListAsync();
    }

    public async Task AddAsync(Service service)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Services.Add(service);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Services.Update(service);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var svc = await ctx.Services.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new InvalidOperationException($"Service with ID {id} not found");
        ctx.Services.Remove(svc);
        await ctx.SaveChangesAsync();
    }
}
