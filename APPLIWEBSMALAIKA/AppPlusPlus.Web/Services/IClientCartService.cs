using AppPlusPlus.Application.Services.Catalogue;
using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Web.Services;

public interface IClientCartService
{
    event Action? Changed;

    IReadOnlyList<ClientCartLine> Items { get; }

    int TotalQuantity { get; }

    decimal TotalAmount { get; }

    decimal GetTotalAmountFc(decimal taux);

    Task InitializeAsync();

    Task RefreshPricesFromCatalogAsync(ICatalogueService catalogue);

    Task AddArticleAsync(Article article);

    Task ChangeQuantityAsync(string articleId, int delta);

    Task RemoveArticleAsync(string articleId);

    Task ClearAsync();
}
