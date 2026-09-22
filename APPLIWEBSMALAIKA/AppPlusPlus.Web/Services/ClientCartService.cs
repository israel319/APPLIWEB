using System.Text.Json;
using AppPlusPlus.Application.Services.Catalogue;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Common;
using Microsoft.JSInterop;

namespace AppPlusPlus.Web.Services;

public class ClientCartService : IClientCartService
{
    readonly IJSRuntime _js;
    readonly List<ClientCartLine> _items = new();
    bool _storageLoaded;
    CancellationTokenSource? _persistCts;

    public event Action? Changed;

    public IReadOnlyList<ClientCartLine> Items => _items;

    public int TotalQuantity => _items.Sum(i => i.Qty);

    public decimal TotalAmount => _items.Sum(i => i.Qty * i.Pu);

    public decimal GetTotalAmountFc(decimal taux) =>
        CommandePricing.OrderTotalFc(
            _items.Select(i => (i.Pu, i.IdMonais, (decimal)i.Qty)),
            taux);

    public ClientCartService(IJSRuntime js) => _js = js;

    public async Task InitializeAsync()
    {
        if (_storageLoaded)
            return;

        _storageLoaded = true;

        try
        {
            if (_items.Count == 0)
            {
                var json = await _js.InvokeAsync<string?>("clientCart.readJson");
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonSerializer.Deserialize<List<ClientCartLine>>(json);
                    if (loaded != null)
                    {
                        _items.AddRange(loaded.Where(l => !string.IsNullOrWhiteSpace(l.ArticleId) && l.Qty > 0));
                    }
                }
            }
        }
        catch
        {
            // Stockage indisponible : panier mémoire uniquement.
        }

        Changed?.Invoke();
    }

    public async Task RefreshPricesFromCatalogAsync(ICatalogueService catalogue)
    {
        if (_items.Count == 0)
            return;

        var changed = false;
        foreach (var line in _items)
        {
            var article = await catalogue.GetArticleByIdAsync(line.ArticleId);
            if (article == null)
                continue;

            var nativePu = (decimal)article.Price;
            if (line.Pu != nativePu || line.IdMonais != article.IdMonais)
            {
                line.Pu = nativePu;
                line.IdMonais = article.IdMonais;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(line.ImageArticle) && !string.IsNullOrWhiteSpace(article.ImageArticle))
            {
                line.ImageArticle = article.ImageArticle;
                changed = true;
            }
        }

        if (changed)
            NotifyChanged();
    }

    public Task AddArticleAsync(Article article)
    {
        if (string.IsNullOrWhiteSpace(article.IdArticle))
            return Task.CompletedTask;

        var line = _items.FirstOrDefault(i => i.ArticleId == article.IdArticle);
        if (line != null)
        {
            line.Qty++;
            line.Pu = (decimal)article.Price;
            line.IdMonais = article.IdMonais;
            if (string.IsNullOrWhiteSpace(line.ImageArticle) && !string.IsNullOrWhiteSpace(article.ImageArticle))
                line.ImageArticle = article.ImageArticle;
        }
        else
        {
            _items.Add(new ClientCartLine
            {
                ArticleId = article.IdArticle,
                Description = article.Description ?? article.IdArticle,
                ImageArticle = article.ImageArticle,
                IdMonais = article.IdMonais,
                Pu = (decimal)article.Price,
                Qty = 1
            });
        }

        NotifyChanged();
        return Task.CompletedTask;
    }

    public Task ChangeQuantityAsync(string articleId, int delta)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return Task.CompletedTask;

        var line = _items.FirstOrDefault(i => i.ArticleId == articleId);
        if (line == null)
            return Task.CompletedTask;

        line.Qty += delta;
        if (line.Qty <= 0)
            _items.Remove(line);

        NotifyChanged();
        return Task.CompletedTask;
    }

    public Task RemoveArticleAsync(string articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return Task.CompletedTask;

        var line = _items.FirstOrDefault(i => i.ArticleId == articleId);
        if (line == null)
            return Task.CompletedTask;

        _items.Remove(line);
        NotifyChanged();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        if (_items.Count == 0)
            return Task.CompletedTask;

        _items.Clear();
        NotifyChanged();
        return Task.CompletedTask;
    }

    void NotifyChanged()
    {
        Changed?.Invoke();
        SchedulePersist();
    }

    void SchedulePersist()
    {
        _persistCts?.Cancel();
        _persistCts = new CancellationTokenSource();
        var token = _persistCts.Token;
        _ = PersistDebouncedAsync(token);
    }

    async Task PersistDebouncedAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(250, token);
            await PersistAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            // Ignore storage errors.
        }
    }

    async Task PersistAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_items);
            await _js.InvokeVoidAsync("clientCart.writeJson", json);
        }
        catch
        {
            // Ignore storage errors.
        }
    }
}
