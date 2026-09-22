using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class RecruitmentRepository : IRecruitmentRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public RecruitmentRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<JobOpening> GetJobOpeningByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.JobOpenings.FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new InvalidOperationException($"JobOpening with ID {id} not found");
    }

    public async Task<Candidate> GetCandidateByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Candidates.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"Candidate with ID {id} not found");
    }

    public async Task<Interview> GetInterviewByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Interviews.FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new InvalidOperationException($"Interview with ID {id} not found");
    }

    public async Task<IEnumerable<JobOpening>> GetOpenJobsAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.JobOpenings.Where(j => j.Status == "Open").ToListAsync();
    }

    public async Task<IEnumerable<Candidate>> GetCandidatesByOpeningAsync(int openingId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Candidates.Where(c => c.JobOpeningId == openingId).ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetInterviewsByCandidateAsync(int candidateId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Interviews.Where(i => i.CandidateId == candidateId).ToListAsync();
    }

    public async Task AddJobOpeningAsync(JobOpening jobOpening)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.JobOpenings.Add(jobOpening);
        await ctx.SaveChangesAsync();
    }

    public async Task AddCandidateAsync(Candidate candidate)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Candidates.Add(candidate);
        await ctx.SaveChangesAsync();
    }

    public async Task AddInterviewAsync(Interview interview)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Interviews.Add(interview);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateCandidateAsync(Candidate candidate)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Candidates.Update(candidate);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteCandidateAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var c = await ctx.Candidates.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"Candidate with ID {id} not found");
        ctx.Candidates.Remove(c);
        await ctx.SaveChangesAsync();
    }
}
