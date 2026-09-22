using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public DepartmentRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<Department> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Departments.Include(d => d.Company).Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException($"Department with ID {id} not found");
    }

    public async Task<Department?> GetByCodeAsync(string departmentCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == departmentCode);
    }

    public async Task<IEnumerable<Department>> GetByCompanyAsync(int companyId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Departments.Include(d => d.Company).Include(d => d.Employees)
            .Where(d => d.CompanyId == companyId).OrderBy(d => d.DepartmentName).ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Departments.Include(d => d.Company).Include(d => d.Employees)
            .OrderBy(d => d.DepartmentName).ToListAsync();
    }

    public async Task AddAsync(Department department)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Departments.Add(department);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Departments.Update(department);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var dept = await ctx.Departments.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException($"Department with ID {id} not found");
        ctx.Departments.Remove(dept);
        await ctx.SaveChangesAsync();
    }
}
