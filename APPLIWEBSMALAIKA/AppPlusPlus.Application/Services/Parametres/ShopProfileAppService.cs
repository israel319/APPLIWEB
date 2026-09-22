using AppPlusPlus.Domain.Entities.Parametres;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Parametres;

public class ShopProfileAppService : IShopProfileService
{
    private readonly IParametresRepository _parametresRepo;

    public ShopProfileAppService(IParametresRepository parametresRepo)
    {
        _parametresRepo = parametresRepo;
    }

    public async Task<ShopProfile> GetAsync()
    {
        var profile = await _parametresRepo.GetShopProfileAsync();
        if (profile is not null) return profile;

        profile = new ShopProfile { AppNameSettingKey = "AppName" };
        await _parametresRepo.AddShopProfileAsync(profile);
        return profile;
    }

    public async Task SaveAsync(string? photoShop, string? adresse1, string? adresse2)
    {
        var profile = await _parametresRepo.GetShopProfileAsync();
        if (profile is null)
        {
            profile = new ShopProfile { AppNameSettingKey = "AppName" };
        }

        profile.PhotoShop = string.IsNullOrWhiteSpace(photoShop) ? null : photoShop.Trim();
        profile.Adresse1 = string.IsNullOrWhiteSpace(adresse1) ? null : adresse1.Trim();
        profile.Adresse2 = string.IsNullOrWhiteSpace(adresse2) ? null : adresse2.Trim();

        await _parametresRepo.SaveShopProfileAsync(profile);
    }
}
