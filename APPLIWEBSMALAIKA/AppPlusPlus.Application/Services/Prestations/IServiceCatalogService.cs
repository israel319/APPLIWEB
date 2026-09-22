using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Prestations;

namespace AppPlusPlus.Application.Services.Prestations;

public interface IServiceCatalogService
{
    Task<List<ServiceCatalogRowDto>> GetCatalogAsync(string? search = null, bool activeOnly = false);
    Task<ServiceCatalogDetailDto?> GetByCodeAsync(string code);
    Task<ServiceCatalogDetailDto?> GetByIdAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task<bool> DescriptionExistsAsync(string description, int? excludeId = null);
    Task<ServiceResult<int>> SaveAsync(ServiceCatalogDetailDto dto, string login);
    Task<ServiceResult> DeleteAsync(int id);
}
