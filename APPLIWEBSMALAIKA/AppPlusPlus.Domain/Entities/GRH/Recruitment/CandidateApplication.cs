namespace AppPlusPlus.Domain.Entities.GRH.Recruitment;

/// <summary>
/// Entité représentant une candidature
/// </summary>
public class CandidateApplication
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string ApplicationNumber { get; set; } = null!;
    public DateTime ApplicationDate { get; set; }
    public string? Status { get; set; } // Submitted/Review/Interview/Offer/Hired/Rejected
    public string? Notes { get; set; }
    public int? ReviewedById { get; set; }
    public DateTime? ReviewDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Candidate Candidate { get; set; } = null!;
}
