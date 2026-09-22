namespace AppPlusPlus.Domain.Entities.GRH.Recruitment;

/// <summary>
/// Entité représentant un entretien d'embauche
/// </summary>
public class Interview
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string InterviewCode { get; set; } = null!;
    public string InterviewType { get; set; } = null!; // Phone/Video/Face-to-Face
    public DateTime InterviewDate { get; set; }
    public int? DurationMinutes { get; set; }
    public string? InterviewerName { get; set; }
    public string? Feedback { get; set; }
    public int? Rating { get; set; } // 1-5
    public string? Status { get; set; } // Scheduled/Completed/Cancelled
    public bool RecommendedForHiring { get; set; }
    public string? NextStep { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Candidate Candidate { get; set; } = null!;
}
