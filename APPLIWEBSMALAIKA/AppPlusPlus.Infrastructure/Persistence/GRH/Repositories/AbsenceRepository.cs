using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Absence;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class AbsenceRepository : IAbsenceRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public AbsenceRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<Absence> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Absences.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new InvalidOperationException($"Absence with ID {id} not found");
    }

    public async Task<Absence?> GetByNumberAsync(string absenceNumber)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Absences.FirstOrDefaultAsync(a => a.AbsenceNumber == absenceNumber);
    }

    public async Task<IEnumerable<Absence>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Absences.OrderByDescending(a => a.AbsenceDate).ToListAsync();
    }

    public async Task<IEnumerable<Absence>> GetByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Absences.Where(a => a.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<IEnumerable<Absence>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Absences.Where(a => a.AbsenceDate >= startDate && a.AbsenceDate <= endDate).ToListAsync();
    }

    public async Task<IEnumerable<Absence>> GetByStatusAsync(string status)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Absences.Where(a => a.Status == status).ToListAsync();
    }

    public async Task AddAsync(Absence absence)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Absences.Add(absence);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Absence absence)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Absences.Update(absence);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var a = await ctx.Absences.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"Absence with ID {id} not found");
        ctx.Absences.Remove(a);
        await ctx.SaveChangesAsync();
    }
}
