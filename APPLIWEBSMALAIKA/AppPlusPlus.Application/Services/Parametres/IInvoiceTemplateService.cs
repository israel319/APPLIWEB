using AppPlusPlus.Domain.Entities.Parametres;

namespace AppPlusPlus.Application.Services.Parametres;

public interface IInvoiceTemplateService
{
    Task<List<InvoiceTemplate>> GetAllAsync();
    Task<InvoiceTemplate?> GetByIdAsync(int id);
    Task<InvoiceTemplate> GetActiveAsync();
    Task<InvoiceTemplate> SaveAsync(InvoiceTemplate template);
    Task SetActiveAsync(int id);
    Task DeleteAsync(int id);
}
