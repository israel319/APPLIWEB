namespace AppPlusPlus.Domain.Entities.GRH.Employee;

/// <summary>
/// Entité représentant la formation/éducation d'un employé
/// </summary>
public class EmployeeEducation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EducationLevel { get; set; } = null!; // High School/Bachelor/Master/PhD
    public string? DegreeType { get; set; }
    public string? Institution { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? FieldOfStudy { get; set; }
    public decimal? GPA { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee Employee { get; set; } = null!;
}
