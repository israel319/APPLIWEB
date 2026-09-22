namespace AppPlusPlus.Domain.Entities.GRH.Employee;

/// <summary>
/// Entité représentant une compétence d'employé
/// </summary>
public class EmployeeSkill
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string SkillName { get; set; } = null!;
    public string? Category { get; set; } // Technical/Soft/Language
    public int? ProficiencyLevel { get; set; } // 1-5
    public string? Certification { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee Employee { get; set; } = null!;
}
