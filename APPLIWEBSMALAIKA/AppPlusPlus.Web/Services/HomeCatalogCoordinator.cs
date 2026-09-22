namespace AppPlusPlus.Web.Services;

/// <summary>
/// Relie la barre de recherche / la navigation accueil au catalogue intégré sur la page d'accueil.
/// </summary>
public sealed class HomeCatalogCoordinator
{
    public string? PendingSearch { get; private set; }
    public int? PendingCategoryId { get; private set; }
    public bool PendingReset { get; private set; }

    public event Action? Changed;

    /// <summary>Affiche la grille des catégories (aucun filtre actif).</summary>
    public void RequestBrowseAll()
    {
        PendingReset = true;
        PendingSearch = null;
        PendingCategoryId = null;
        Changed?.Invoke();
    }

    public void RequestSearch(string? search)
    {
        PendingReset = false;
        PendingSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        PendingCategoryId = null;
        Changed?.Invoke();
    }

    public void RequestCategory(int categoryId)
    {
        if (categoryId <= 0) return;
        PendingReset = false;
        PendingCategoryId = categoryId;
        PendingSearch = null;
        Changed?.Invoke();
    }

    /// <summary>Filtre une catégorie (+ recherche optionnelle dans la catégorie).</summary>
    public void RequestCategoryFilter(int categoryId, string? searchWithinCategory = null)
    {
        if (categoryId <= 0) return;
        PendingReset = false;
        PendingCategoryId = categoryId;
        PendingSearch = string.IsNullOrWhiteSpace(searchWithinCategory) ? null : searchWithinCategory.Trim();
        Changed?.Invoke();
    }

    public (string? Search, int? CategoryId, bool Reset) TakePending()
    {
        var search = PendingSearch;
        var categoryId = PendingCategoryId;
        var reset = PendingReset;
        PendingSearch = null;
        PendingCategoryId = null;
        PendingReset = false;
        return (search, categoryId, reset);
    }
}
