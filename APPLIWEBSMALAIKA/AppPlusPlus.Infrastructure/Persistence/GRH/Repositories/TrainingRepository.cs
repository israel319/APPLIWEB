using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Training;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class TrainingRepository : ITrainingRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public TrainingRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<TrainingProgram> GetProgramByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new InvalidOperationException($"TrainingProgram with ID {id} not found");
    }

    public async Task<TrainingSession> GetSessionByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.TrainingSessions.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new InvalidOperationException($"TrainingSession with ID {id} not found");
    }

    public async Task<EmployeeTraining> GetEmployeeTrainingByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeTrainings.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"EmployeeTraining with ID {id} not found");
    }

    public async Task<IEnumerable<TrainingProgram>> GetAllProgramsAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.TrainingPrograms.ToListAsync();
    }

    public async Task<IEnumerable<TrainingSession>> GetSessionsByProgramAsync(int programId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.TrainingSessions.Where(s => s.TrainingProgramId == programId).ToListAsync();
    }

    public async Task<IEnumerable<EmployeeTraining>> GetByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeTrainings.Where(e => e.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<IEnumerable<EmployeeTraining>> GetBySessionAsync(int sessionId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeTrainings.Where(e => e.TrainingSessionId == sessionId).ToListAsync();
    }

    public async Task AddProgramAsync(TrainingProgram program)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.TrainingPrograms.Add(program);
        await ctx.SaveChangesAsync();
    }

    public async Task AddSessionAsync(TrainingSession session)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.TrainingSessions.Add(session);
        await ctx.SaveChangesAsync();
    }

    public async Task AddEmployeeTrainingAsync(EmployeeTraining employeeTraining)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeTrainings.Add(employeeTraining);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateProgramAsync(TrainingProgram program)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.TrainingPrograms.Update(program);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(TrainingSession session)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.TrainingSessions.Update(session);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteProgramAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var p = await ctx.TrainingPrograms.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"TrainingProgram with ID {id} not found");
        ctx.TrainingPrograms.Remove(p);
        await ctx.SaveChangesAsync();
    }
}
