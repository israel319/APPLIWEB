using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Parametres;
using Microsoft.Extensions.Caching.Memory;

namespace AppPlusPlus.Application.Services.Parametres;

public class InvoiceTemplateAppService : IInvoiceTemplateService
{
    private const string ActiveCacheKey = "invoice-template:active";
    private readonly IParametresRepository _repo;
    private readonly IMemoryCache _cache;

    public InvoiceTemplateAppService(IParametresRepository parametresRepo, IMemoryCache cache)
    {
        _repo = parametresRepo;
        _cache = cache;
    }

    public Task<List<InvoiceTemplate>> GetAllAsync() => _repo.GetInvoiceTemplateSummariesAsync();

    public Task<InvoiceTemplate?> GetByIdAsync(int id) => _repo.GetInvoiceTemplateByIdAsync(id);

    public async Task<InvoiceTemplate> GetActiveAsync()
    {
        if (_cache.TryGetValue(ActiveCacheKey, out InvoiceTemplate? cached) && cached is not null)
            return cached;

        var active = await _repo.GetActiveInvoiceTemplateAsync();
        if (active is not null)
        {
            _cache.Set(ActiveCacheKey, active, TimeSpan.FromMinutes(15));
            return active;
        }

        var all = await _repo.GetInvoiceTemplateSummariesAsync();
        if (all.Count > 0)
        {
            await _repo.SetActiveInvoiceTemplateAsync(all[0].Id);
            active = await _repo.GetInvoiceTemplateByIdAsync(all[0].Id) ?? all[0];
            active.IsActive = true;
            _cache.Set(ActiveCacheKey, active, TimeSpan.FromMinutes(15));
            return active;
        }

        var created = new InvoiceTemplate
        {
            Nom = "Ticket caisse (80 mm)",
            IsActive = true,
            PrintFormat = InvoicePrintFormat.Ticket80,
            FooterText = "Merci de votre visite"
        };
        await _repo.AddInvoiceTemplateAsync(created);
        _cache.Set(ActiveCacheKey, created, TimeSpan.FromMinutes(15));
        return created;
    }

    public async Task<InvoiceTemplate> SaveAsync(InvoiceTemplate template)
    {
        Normalize(template);

        if (template.Id <= 0)
        {
            var count = (await _repo.GetInvoiceTemplateSummariesAsync()).Count;
            template.IsActive = count == 0;
            await _repo.AddInvoiceTemplateAsync(template);
            InvalidateCache();
            return template;
        }

        var stored = await _repo.GetInvoiceTemplateByIdAsync(template.Id)
            ?? throw new InvalidOperationException("Modèle introuvable.");

        stored.Nom = template.Nom;
        stored.PrintFormat = template.PrintFormat;
        stored.ShowLetterheadBackground = template.ShowLetterheadBackground;
        if (!string.IsNullOrWhiteSpace(template.Logo))
            stored.Logo = template.Logo;
        if (!string.IsNullOrWhiteSpace(template.LetterheadBackground))
            stored.LetterheadBackground = template.LetterheadBackground;
        else if (!template.ShowLetterheadBackground)
            stored.LetterheadBackground = null;
        stored.LegalRccm = template.LegalRccm;
        stored.LegalIdNat = template.LegalIdNat;
        stored.LegalAdresse = template.LegalAdresse;
        stored.LegalContact = template.LegalContact;
        stored.LegalEmail = template.LegalEmail;
        stored.LegalWebsite = template.LegalWebsite;
        stored.FooterText = template.FooterText;
        stored.ContentMarginTopMm = template.ContentMarginTopMm;
        stored.ContentMarginBottomMm = template.ContentMarginBottomMm;

        await _repo.UpdateInvoiceTemplateAsync(stored);
        InvalidateCache();
        return stored;
    }

    public async Task SetActiveAsync(int id)
    {
        await _repo.SetActiveInvoiceTemplateAsync(id);
        InvalidateCache();
    }

    public async Task DeleteAsync(int id)
    {
        var row = await _repo.GetInvoiceTemplateByIdAsync(id)
            ?? throw new InvalidOperationException("Modèle introuvable.");
        if (row.IsActive)
            throw new InvalidOperationException("Impossible de supprimer le modèle actif. Activez d'abord un autre modèle.");

        await _repo.DeleteInvoiceTemplateAsync(id);
        InvalidateCache();
    }

    void InvalidateCache() => _cache.Remove(ActiveCacheKey);

    static void Normalize(InvoiceTemplate t)
    {
        t.Nom = string.IsNullOrWhiteSpace(t.Nom) ? "Modèle sans nom" : t.Nom.Trim();
        t.PrintFormat = InvoicePrintFormat.IsA4(t.PrintFormat)
            ? InvoicePrintFormat.A4Portrait
            : InvoicePrintFormat.Ticket80;
        t.Logo = TrimOrNull(t.Logo);
        t.LetterheadBackground = TrimOrNull(t.LetterheadBackground);
        t.LegalRccm = TrimOrNull(t.LegalRccm);
        t.LegalIdNat = TrimOrNull(t.LegalIdNat);
        t.LegalAdresse = TrimOrNull(t.LegalAdresse);
        t.LegalContact = TrimOrNull(t.LegalContact);
        t.LegalEmail = TrimOrNull(t.LegalEmail);
        t.LegalWebsite = TrimOrNull(t.LegalWebsite);
        t.FooterText = TrimOrNull(t.FooterText);
        t.ContentMarginTopMm = Math.Clamp(t.ContentMarginTopMm, 20, 80);
        t.ContentMarginBottomMm = Math.Clamp(t.ContentMarginBottomMm, 15, 60);
    }

    static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
