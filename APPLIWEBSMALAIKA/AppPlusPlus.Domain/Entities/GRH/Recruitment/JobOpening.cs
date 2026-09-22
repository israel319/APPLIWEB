namespace AppPlusPlus.Domain.Entities.GRH.Recruitment;

/// <summary>
/// Entité représentant une ouverture de poste
/// </summary>
public class JobOpening
{
    public int Id { get; set; }
    public string OpeningCode { get; set; } = null!;
    public int JobPositionId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int NumberOfPositions { get; set; }
    public DateTime OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? Status { get; set; } // Open/Closed/OnHold
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? EducationRequired { get; set; }
    public int? YearsOfExperienceRequired { get; set; }
    public string? KeySkills { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Organization.JobPosition JobPosition { get; set; } = null!;
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}
