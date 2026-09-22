using AppPlusPlus.Domain.Entities.GRH.Organization;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Service> CreateServiceAsync(string serviceCode, string serviceName, int departmentId, int companyId)
    {
        var service = new Service
        {
            ServiceCode = serviceCode,
            ServiceName = serviceName,
            DepartmentId = departmentId,
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _serviceRepository.AddAsync(service);
        return service;
    }

    public async Task<Service> GetServiceAsync(int id)
    {
        return await _serviceRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Service>> GetDepartmentServicesAsync(int departmentId)
    {
        return await _serviceRepository.GetByDepartmentAsync(departmentId);
    }

    public async Task<IEnumerable<Service>> GetAllServicesAsync()
    {
        return await _serviceRepository.GetAllAsync();
    }

    public async Task UpdateServiceAsync(Service service)
    {
        service.UpdatedAt = DateTime.UtcNow;
        await _serviceRepository.UpdateAsync(service);
    }

    public async Task DeleteServiceAsync(int id)
    {
        await _serviceRepository.DeleteAsync(id);
    }
}
