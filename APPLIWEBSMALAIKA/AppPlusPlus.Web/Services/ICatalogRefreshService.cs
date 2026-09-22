namespace AppPlusPlus.Web.Services;

/// <summary>Notifie le catalogue public qu'un article a été modifié.</summary>
public interface ICatalogRefreshService
{
    event Action? Changed;

    void NotifyChanged();
}
