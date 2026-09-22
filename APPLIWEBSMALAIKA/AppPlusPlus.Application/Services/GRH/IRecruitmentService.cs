using AppPlusPlus.Domain.Entities.GRH.Recruitment;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer le recrutement
/// </summary>
public interface IRecruitmentService
{
    Task<JobOpening> CreateJobOpeningAsync(int jobPositionId, string title, string description, int numberOfPositions);
    Task<Candidate> AddCandidateAsync(int jobOpeningId, string firstName, string lastName, string email);
    Task<Interview> ScheduleInterviewAsync(int candidateId, DateTime interviewDate, string interviewType);
    Task<IEnumerable<JobOpening>> GetOpenJobsAsync();
    Task<IEnumerable<Candidate>> GetCandidatesByOpeningAsync(int openingId);
    Task<IEnumerable<Interview>> GetCandidateInterviewsAsync(int candidateId);
    Task UpdateCandidateStatusAsync(int candidateId, string status);
    Task CompleteInterviewAsync(int interviewId, int rating, string feedback);
    Task<int> GetApplicationCountAsync(int openingId);
}
