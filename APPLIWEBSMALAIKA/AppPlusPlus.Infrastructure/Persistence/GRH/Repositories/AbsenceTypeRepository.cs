using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Absence;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class AbsenceTypeRepository : IAbsenceTypeRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public AbsenceTypeRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<AbsenceType> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.AbsenceTypes.FirstOrDefaultAsync(at => at.Id == id)
            ?? throw new InvalidOperationException($"Type d'absence ID {id} introuvable.");
    }

    public async Task<IEnumerable<AbsenceType>> GetAllActiveAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.AbsenceTypes.Where(at => at.IsActive).OrderBy(at => at.AbsenceTypeName).ToListAsync();
    }

    public async Task AddAsync(AbsenceType absenceType)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.AbsenceTypes.Add(absenceType);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(AbsenceType absenceType)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.AbsenceTypes.Update(absenceType);
        await ctx.SaveChangesAsync();
    }
}
