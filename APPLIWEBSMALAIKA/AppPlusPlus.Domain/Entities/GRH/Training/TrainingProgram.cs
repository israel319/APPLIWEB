namespace AppPlusPlus.Domain.Entities.GRH.Training;

/// <summary>
/// Entité représentant un programme de formation
/// </summary>
public class TrainingProgram
{
    public int Id { get; set; }
    public string ProgramCode { get; set; } = null!;
    public string ProgramName { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; } // Technical/Soft Skills/Leadership
    public string? Provider { get; set; }
    public int? DurationHours { get; set; }
    public decimal? Cost { get; set; }
    public bool IsExternal { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<TrainingSession> Sessions { get; set; } = new List<TrainingSession>();
}
