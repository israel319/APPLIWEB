using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class RecruitmentService : IRecruitmentService
{
    private readonly IRecruitmentRepository _recruitmentRepository;

    public RecruitmentService(IRecruitmentRepository recruitmentRepository)
    {
        _recruitmentRepository = recruitmentRepository;
    }

    public async Task<JobOpening> CreateJobOpeningAsync(int jobPositionId, string title, string description, int numberOfPositions)
    {
        var jobOpening = new JobOpening
        {
            OpeningCode = $"JO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            JobPositionId = jobPositionId,
            Title = title,
            Description = description,
            NumberOfPositions = numberOfPositions,
            OpeningDate = DateTime.UtcNow,
            Status = "Open",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _recruitmentRepository.AddJobOpeningAsync(jobOpening);
        return jobOpening;
    }

    public async Task<Candidate> AddCandidateAsync(int jobOpeningId, string firstName, string lastName, string email)
    {
        var candidate = new Candidate
        {
            CandidateCode = $"CAN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            JobOpeningId = jobOpeningId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            ApplicationDate = DateTime.UtcNow,
            Status = "New",
            CreatedAt = DateTime.UtcNow
        };

        await _recruitmentRepository.AddCandidateAsync(candidate);
        return candidate;
    }

    public async Task<Interview> ScheduleInterviewAsync(int candidateId, DateTime interviewDate, string interviewType)
    {
        var interview = new Interview
        {
            CandidateId = candidateId,
            InterviewCode = $"INT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            InterviewType = interviewType,
            InterviewDate = interviewDate,
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };

        await _recruitmentRepository.AddInterviewAsync(interview);
        return interview;
    }

    public async Task<IEnumerable<JobOpening>> GetOpenJobsAsync()
    {
        return await _recruitmentRepository.GetOpenJobsAsync();
    }

    public async Task<IEnumerable<Candidate>> GetCandidatesByOpeningAsync(int openingId)
    {
        return await _recruitmentRepository.GetCandidatesByOpeningAsync(openingId);
    }

    public async Task<IEnumerable<Interview>> GetCandidateInterviewsAsync(int candidateId)
    {
        return await _recruitmentRepository.GetInterviewsByCandidateAsync(candidateId);
    }

    public async Task UpdateCandidateStatusAsync(int candidateId, string status)
    {
        var candidate = await _recruitmentRepository.GetCandidateByIdAsync(candidateId);
        candidate.Status = status;
        await _recruitmentRepository.UpdateCandidateAsync(candidate);
    }

    public async Task CompleteInterviewAsync(int interviewId, int rating, string feedback)
    {
        var interview = await _recruitmentRepository.GetInterviewByIdAsync(interviewId);
        interview.Status = "Completed";
        interview.Rating = rating;
        interview.Feedback = feedback;
    }

    public async Task<int> GetApplicationCountAsync(int openingId)
    {
        var candidates = await _recruitmentRepository.GetCandidatesByOpeningAsync(openingId);
        return candidates.Count();
    }
}
