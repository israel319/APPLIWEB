using AppPlusPlus.Domain.Entities.Parametres;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IParametresRepository
{
    // AppSetting
    Task<AppSetting?> GetSettingAsync(string key);
    Task<List<AppSetting>> GetAllSettingsAsync();
    Task SetSettingAsync(string key, string? value);

    // ShopProfile
    Task<ShopProfile?> GetShopProfileAsync();
    Task AddShopProfileAsync(ShopProfile profile);
    Task SaveShopProfileAsync(ShopProfile profile);
    Task UpdateShopProfileAsync(ShopProfile profile);

    // InvoiceTemplate
    Task<List<InvoiceTemplate>> GetAllInvoiceTemplatesAsync();
    Task<List<InvoiceTemplate>> GetInvoiceTemplateSummariesAsync();
    Task<InvoiceTemplate?> GetInvoiceTemplateByIdAsync(int id);
    Task<InvoiceTemplate?> GetActiveInvoiceTemplateAsync();
    Task AddInvoiceTemplateAsync(InvoiceTemplate template);
    Task UpdateInvoiceTemplateAsync(InvoiceTemplate template);
    Task DeleteInvoiceTemplateAsync(int id);
    Task SetActiveInvoiceTemplateAsync(int id);
}
