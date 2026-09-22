namespace AppPlusPlus.Application.DTOs.GRH.Recruitment;

public class CreateJobOpeningDto
{
    public string OpeningCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public int JobPositionId { get; set; }
    public string? Description { get; set; }
    public int NumberOfPositions { get; set; }
    public DateTime OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public string? RequiredEducation { get; set; }
    public int? RequiredExperienceYears { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
}

public class CreateCandidateDto
{
    public string CandidateCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? CurrentPosition { get; set; }
    public string? CurrentCompany { get; set; }
    public string? Address { get; set; }
    public string? ResumePath { get; set; }
    public string? CoverLetterPath { get; set; }
}

public class ScheduleInterviewDto
{
    public int CandidateId { get; set; }
    public string InterviewType { get; set; } = string.Empty;
    public DateTime InterviewDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? InterviewerName { get; set; }
    public string? Location { get; set; }
}

public class JobOpeningReadDto
{
    public int Id { get; set; }
    public string OpeningCode { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int NumberOfPositions { get; set; }
    public DateTime OpeningDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
}

public class CandidateReadDto
{
    public int Id { get; set; }
    public string CandidateCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
    public DateTime DateOfBirth { get; set; }
    public string? CurrentPosition { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Rating { get; set; }
}
