using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using AppPlusPlus.Domain.Enums.GRH;
using Microsoft.EntityFrameworkCore;
using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public EmployeeRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<GrhEmployee> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"Employee with ID {id} not found");
    }

    public async Task<GrhEmployee?> GetByCodeAsync(string employeeCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
    }

    public async Task<IEnumerable<GrhEmployee>> GetAllActiveAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees
            .Where(e => e.Status == EmployeeStatusEnum.Active)
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<GrhEmployee>> GetByDepartmentAsync(int departmentId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.Where(e => e.DepartmentId == departmentId).ToListAsync();
    }

    public async Task<IEnumerable<GrhEmployee>> GetByServiceAsync(int serviceId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.Where(e => e.ServiceId == serviceId).ToListAsync();
    }

    public async Task<IEnumerable<GrhEmployee>> GetByPositionAsync(int jobPositionId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.Where(e => e.JobPositionId == jobPositionId).ToListAsync();
    }

    public async Task AddAsync(GrhEmployee employee)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Employees.Add(employee);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(GrhEmployee employee)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Employees.Update(employee);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var emp = await ctx.Employees.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"Employee with ID {id} not found");
        ctx.Employees.Remove(emp);
        await ctx.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.AnyAsync(e => e.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.CountAsync();
    }

    public async Task<IEnumerable<GrhEmployee>> GetPagedAsync(int pageNumber, int pageSize)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Employees.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}
