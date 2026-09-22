using AppPlusPlus.Domain.Entities.GRH.Organization;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class JobPositionService : IJobPositionService
{
    private readonly IJobPositionRepository _jobPositionRepository;

    public JobPositionService(IJobPositionRepository jobPositionRepository)
    {
        _jobPositionRepository = jobPositionRepository;
    }

    public async Task<JobPosition> CreateJobPositionAsync(string positionCode, string positionName, int departmentId, int companyId)
    {
        var jobPosition = new JobPosition
        {
            PositionCode = positionCode,
            PositionName = positionName,
            DepartmentId = departmentId,
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _jobPositionRepository.AddAsync(jobPosition);
        return jobPosition;
    }

    public async Task<JobPosition> GetJobPositionAsync(int id)
    {
        return await _jobPositionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<JobPosition>> GetDepartmentPositionsAsync(int departmentId)
    {
        return await _jobPositionRepository.GetByDepartmentAsync(departmentId);
    }

    public async Task<IEnumerable<JobPosition>> GetAllPositionsAsync()
    {
        return await _jobPositionRepository.GetAllAsync();
    }

    public async Task UpdateJobPositionAsync(JobPosition jobPosition)
    {
        jobPosition.UpdatedAt = DateTime.UtcNow;
        await _jobPositionRepository.UpdateAsync(jobPosition);
    }

    public async Task DeleteJobPositionAsync(int id)
    {
        await _jobPositionRepository.DeleteAsync(id);
    }
}
