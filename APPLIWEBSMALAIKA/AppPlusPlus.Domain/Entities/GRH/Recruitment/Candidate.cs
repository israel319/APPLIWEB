namespace AppPlusPlus.Domain.Entities.GRH.Recruitment;

/// <summary>
/// Entité représentant un candidat
/// </summary>
public class Candidate
{
    public int Id { get; set; }
    public string CandidateCode { get; set; } = null!;
    public int JobOpeningId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentCompany { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Resume { get; set; }
    public string? CoverLetter { get; set; }
    public string? Status { get; set; } // New/Shortlisted/Rejected/Hired/OnHold
    public int? Rating { get; set; } // 1-5
    public DateTime ApplicationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public JobOpening JobOpening { get; set; } = null!;
    public ICollection<CandidateApplication> Applications { get; set; } = new List<CandidateApplication>();
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}
