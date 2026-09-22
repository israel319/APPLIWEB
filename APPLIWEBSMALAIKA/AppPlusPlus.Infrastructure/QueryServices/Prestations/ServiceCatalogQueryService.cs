using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Prestations;
using AppPlusPlus.Application.Services.Prestations;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Prestations;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices.Prestations;

public class ServiceCatalogQueryService : IServiceCatalogService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ServiceCatalogQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ServiceCatalogRowDto>> GetCatalogAsync(string? search = null, bool activeOnly = false)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.ServiceCatalogItems
            .AsNoTracking()
            .Include(s => s.Money)
            .AsQueryable();

        if (activeOnly)
            q = q.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(x =>
                x.Code.Contains(s)
                || x.Description.Contains(s)
                || (x.Category != null && x.Category.Contains(s)));
        }

        return await q
            .OrderBy(x => x.Description)
            .Select(x => new ServiceCatalogRowDto
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                Price = x.Price,
                MoneyId = x.MoneyId,
                MoneyLabel = x.Money != null ? x.Money.DescriptionMonais : null,
                IsUsd = x.Money != null && x.Money.DescriptionMonais == "USD",
                UnitType = x.UnitType,
                Category = x.Category,
                IsActive = x.IsActive,
                DateSys = x.DateSys
            })
            .ToListAsync();
    }

    public async Task<ServiceCatalogDetailDto?> GetByCodeAsync(string code)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var item = await ctx.ServiceCatalogItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code);
        return item == null ? null : MapDetail(item);
    }

    public async Task<ServiceCatalogDetailDto?> GetByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var item = await ctx.ServiceCatalogItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item == null ? null : MapDetail(item);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.ServiceCatalogItems.Where(x => x.Code == code.Trim());
        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<bool> DescriptionExistsAsync(string description, int? excludeId = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var desc = description.Trim();
        var q = ctx.ServiceCatalogItems.Where(x => x.Description == desc);
        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<ServiceResult<int>> SaveAsync(ServiceCatalogDetailDto dto, string login)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return ServiceResult.Fail<int>("Le code est obligatoire.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            return ServiceResult.Fail<int>("La description est obligatoire.");
        if (dto.Price < 0)
            return ServiceResult.Fail<int>("Le prix ne peut pas être négatif.");

        await using var ctx = await _dbFactory.CreateDbContextAsync();

        ServiceCatalogItem entity;
        if (dto.Id > 0)
        {
            entity = await ctx.ServiceCatalogItems.FirstOrDefaultAsync(x => x.Id == dto.Id)
                     ?? new ServiceCatalogItem();
            if (entity.Id == 0)
                return ServiceResult.Fail<int>("Prestation introuvable.");
        }
        else
        {
            if (await CodeExistsAsync(dto.Code))
                return ServiceResult.Fail<int>($"Le code « {dto.Code.Trim()} » existe déjà.");
            entity = new ServiceCatalogItem
            {
                User = login,
                Cumputer = Environment.MachineName,
                DateSys = DateTime.Now
            };
            ctx.ServiceCatalogItems.Add(entity);
        }

        if (await DescriptionExistsAsync(dto.Description, entity.Id > 0 ? entity.Id : null))
            return ServiceResult.Fail<int>($"Une prestation « {dto.Description.Trim()} » existe déjà.");

        if (entity.Id == 0)
            entity.Code = dto.Code.Trim().ToUpperInvariant();
        entity.Description = dto.Description.Trim();
        entity.Detail = dto.Detail?.Trim();
        entity.Price = dto.Price;
        entity.MoneyId = dto.MoneyId > 0 ? dto.MoneyId : CurrencyDefaults.MoneyIdCdf;
        entity.UnitType = dto.UnitType;
        entity.Category = dto.Category?.Trim();
        entity.IsActive = dto.IsActive;
        entity.DateEditing = DateTime.Now;

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok(entity.Id, "Prestation enregistrée.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var item = await ctx.ServiceCatalogItems.FindAsync(id);
        if (item == null)
            return ServiceResult.Fail("Prestation introuvable.");

        ctx.ServiceCatalogItems.Remove(item);
        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Prestation supprimée.");
    }

    static ServiceCatalogDetailDto MapDetail(ServiceCatalogItem x) => new()
    {
        Id = x.Id,
        Code = x.Code,
        Description = x.Description,
        Detail = x.Detail,
        Price = x.Price,
        MoneyId = x.MoneyId,
        UnitType = x.UnitType,
        Category = x.Category,
        IsActive = x.IsActive
    };
}
