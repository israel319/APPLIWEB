using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les postes
/// </summary>
public interface IJobPositionService
{
    Task<JobPosition> CreateJobPositionAsync(string positionCode, string positionName, int departmentId, int companyId);
    Task<JobPosition> GetJobPositionAsync(int id);
    Task<IEnumerable<JobPosition>> GetDepartmentPositionsAsync(int departmentId);
    Task<IEnumerable<JobPosition>> GetAllPositionsAsync();
    Task UpdateJobPositionAsync(JobPosition jobPosition);
    Task DeleteJobPositionAsync(int id);
}
