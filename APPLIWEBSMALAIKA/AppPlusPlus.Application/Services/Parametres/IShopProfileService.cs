using AppPlusPlus.Domain.Entities.Parametres;

namespace AppPlusPlus.Application.Services.Parametres;

public interface IShopProfileService
{
    Task<ShopProfile> GetAsync();
    Task SaveAsync(string? photoShop, string? adresse1, string? adresse2);
}
