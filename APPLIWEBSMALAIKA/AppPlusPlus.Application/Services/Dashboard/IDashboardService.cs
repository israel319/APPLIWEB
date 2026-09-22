using AppPlusPlus.Application.DTOs.Dashboard;

namespace AppPlusPlus.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardDataDto> GetDashboardDataAsync();
}
