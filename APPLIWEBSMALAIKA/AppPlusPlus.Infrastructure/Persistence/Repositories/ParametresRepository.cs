using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Parametres;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class ParametresRepository : IParametresRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ParametresRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<AppSetting?> GetSettingAsync(string key)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.AppSettings.FindAsync(key);
    }

    public async Task<List<AppSetting>> GetAllSettingsAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.AppSettings.ToListAsync();
    }

    public async Task SetSettingAsync(string key, string? value)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.AppSettings.FindAsync(key);
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            ctx.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        await ctx.SaveChangesAsync();
    }

    public async Task<ShopProfile?> GetShopProfileAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ShopProfiles
            .Include(p => p.AppNameSetting)
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task AddShopProfileAsync(ShopProfile profile)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ShopProfiles.Add(profile);
        await ctx.SaveChangesAsync();
    }

    public async Task SaveShopProfileAsync(ShopProfile profile)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.ShopProfiles.OrderBy(p => p.Id).FirstOrDefaultAsync();
        if (existing is null)
        {
            ctx.ShopProfiles.Add(profile);
        }
        else
        {
            existing.PhotoShop = profile.PhotoShop;
            existing.Adresse1 = profile.Adresse1;
            existing.Adresse2 = profile.Adresse2;
            existing.AppNameSettingKey = profile.AppNameSettingKey;
        }
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateShopProfileAsync(ShopProfile profile)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ShopProfiles.Update(profile);
        await ctx.SaveChangesAsync();
    }

    public async Task<List<InvoiceTemplate>> GetAllInvoiceTemplatesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InvoiceTemplates
            .AsNoTracking()
            .OrderByDescending(t => t.IsActive)
            .ThenBy(t => t.Nom)
            .ToListAsync();
    }

    public async Task<List<InvoiceTemplate>> GetInvoiceTemplateSummariesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InvoiceTemplates
            .AsNoTracking()
            .OrderByDescending(t => t.IsActive)
            .ThenBy(t => t.Nom)
            .Select(t => new InvoiceTemplate
            {
                Id = t.Id,
                Nom = t.Nom,
                IsActive = t.IsActive,
                PrintFormat = t.PrintFormat,
                ShowLetterheadBackground = t.ShowLetterheadBackground,
                FooterText = t.FooterText
            })
            .ToListAsync();
    }

    public async Task<InvoiceTemplate?> GetInvoiceTemplateByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InvoiceTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<InvoiceTemplate?> GetActiveInvoiceTemplateAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InvoiceTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.IsActive);
    }

    public async Task AddInvoiceTemplateAsync(InvoiceTemplate template)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.InvoiceTemplates.Add(template);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateInvoiceTemplateAsync(InvoiceTemplate template)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.InvoiceTemplates.Update(template);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteInvoiceTemplateAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var row = await ctx.InvoiceTemplates.FindAsync(id);
        if (row is null) return;
        ctx.InvoiceTemplates.Remove(row);
        await ctx.SaveChangesAsync();
    }

    public async Task SetActiveInvoiceTemplateAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var all = await ctx.InvoiceTemplates.ToListAsync();
        foreach (var t in all)
            t.IsActive = t.Id == id;
        await ctx.SaveChangesAsync();
    }
}
