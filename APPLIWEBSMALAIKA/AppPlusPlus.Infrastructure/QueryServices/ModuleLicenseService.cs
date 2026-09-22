using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Interfaces;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices;

public class ModuleLicenseService : IModuleLicenseService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ModuleLicenseService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> IsModuleActiveAsync(string moduleCode)
    {
        try
        {
            await using var ctx = _factory.CreateDbContext();

            // Instance vierge / backup sans données : table vide → tout actif pour la configuration initiale.
            if (!await ctx.ModuleLicenses.AsNoTracking().AnyAsync())
                return true;

            var module = await ctx.ModuleLicenses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleCode == moduleCode);

            if (module is null)
                return true;

            return module.IsActive || module.IsCore;
        }
        catch
        {
            // Table absente ou indisponible : ne pas bloquer l'application au démarrage.
            return true;
        }
    }

    public async Task<IList<ModuleLicense>> GetAllModulesAsync()
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.ModuleLicenses
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    public async Task<bool> ToggleModuleAsync(int moduleId, bool isActive)
    {
        await using var ctx = _factory.CreateDbContext();
        var module = await ctx.ModuleLicenses.FindAsync(moduleId);
        if (module is null || module.IsCore) return false;

        module.IsActive = isActive;
        module.UpdatedAt = DateTime.UtcNow;
        if (isActive) module.ActivatedAt = DateTime.UtcNow;

        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLicenseAsync(ModuleLicense updated)
    {
        await using var ctx = _factory.CreateDbContext();
        var module = await ctx.ModuleLicenses.FindAsync(updated.Id);
        if (module is null) return false;

        if (!module.IsCore)
        {
            module.IsActive = updated.IsActive;
            if (updated.IsActive && module.ActivatedAt is null)
                module.ActivatedAt = DateTime.UtcNow;
        }

        module.Price = updated.Price;
        module.ExpiresAt = updated.ExpiresAt;
        module.LicenseNotes = updated.LicenseNotes;
        module.UpdatedAt = DateTime.UtcNow;

        await ctx.SaveChangesAsync();
        return true;
    }
}
