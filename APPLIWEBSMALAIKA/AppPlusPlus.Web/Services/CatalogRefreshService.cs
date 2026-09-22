namespace AppPlusPlus.Web.Services;

public sealed class CatalogRefreshService : ICatalogRefreshService
{
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}
