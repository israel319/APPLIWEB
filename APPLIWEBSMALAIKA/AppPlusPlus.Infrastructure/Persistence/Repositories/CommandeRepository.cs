using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class CommandeRepository : RepositoryBase<Commande>, ICommandeRepository
{
    public CommandeRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<Commande?> GetWithDetailsAsync(int commandeId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Include(c => c.Details)
            .FirstOrDefaultAsync(c => c.CommandeId == commandeId);
    }

    public async Task<Commande?> GetWithDetailsAndLivraisonsAsync(int commandeId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Include(c => c.Details)
            .Include(c => c.Livraisons).ThenInclude(l => l.Details)
            .FirstOrDefaultAsync(c => c.CommandeId == commandeId);
    }

    public async Task<List<Commande>> GetByCustomerAsync(int customerId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Where(c => c.CustomerId == customerId)
            .Include(c => c.Details)
            .ToListAsync();
    }

    public async Task<List<Commande>> GetByCustomerWithFullDetailsAsync(int customerId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Where(c => c.CustomerId == customerId)
            .Include(c => c.Details).ThenInclude(d => d.Article)
            .Include(c => c.Livraisons)
            .Include(c => c.Factures)
            .OrderByDescending(c => c.CreationDate)
            .ToListAsync();
    }

    public async Task<List<Commande>> GetForLoyaltyCalculationAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Where(c => c.CustomerId != null)
            .OrderByDescending(c => c.CreationDate)
            .ToListAsync();
    }

    public async Task<List<Commande>> GetByStatusAsync(int status)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Where(c => c.Status == status)
            .Include(c => c.Details)
            .ToListAsync();
    }

    public async Task<List<Commande>> GetByUserAsync(string userLogin)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Where(c => c.UserLogin == userLogin)
            .Include(c => c.Details)
            .ToListAsync();
    }

    public async Task<List<Commande>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Where(c => c.CreationDate >= from && c.CreationDate <= to)
            .Include(c => c.Details)
            .OrderByDescending(c => c.CreationDate)
            .ToListAsync();
    }

    public async Task<List<Livraison>> GetLivraisonsByCommandeAsync(int commandeId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Livraisons
            .Where(l => l.CommandeId == commandeId)
            .Include(l => l.Details)
            .ToListAsync();
    }

    public async Task<Livraison?> GetLivraisonWithDetailsAsync(int livraisonId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Livraisons
            .Include(l => l.Details)
            .FirstOrDefaultAsync(l => l.LivraisonId == livraisonId);
    }

    public async Task AddLivraisonAsync(Livraison livraison)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Livraisons.Add(livraison);
        await ctx.SaveChangesAsync();
    }

    public async Task<List<Commande>> GetAllWithFullDetailsAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Include(c => c.Customer)
            .Include(c => c.Details).ThenInclude(d => d.Article)
            .Include(c => c.Livraisons)
            .Include(c => c.Factures)
            .OrderByDescending(c => c.CreationDate)
            .ToListAsync();
    }

    public async Task<Dictionary<int, decimal>> GetDeliveredQtyByDetailIdsAsync(List<int> commandeDetailIds)
    {
        if (!commandeDetailIds.Any())
            return new Dictionary<int, decimal>();

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.LivraisonDetails
            .Where(ld => ld.CommandDetailId != null && commandeDetailIds.Contains(ld.CommandDetailId.Value))
            .GroupBy(ld => ld.CommandDetailId!.Value)
            .Select(g => new { Id = g.Key, Total = g.Sum(x => x.Qte ?? 0) })
            .ToDictionaryAsync(x => x.Id, x => x.Total);
    }

    public async Task<Dictionary<int, decimal>> GetPaidAmountByCommandeIdsAsync(List<int> commandeIds)
    {
        if (!commandeIds.Any())
            return new Dictionary<int, decimal>();

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Where(f => f.CommandeId != null && commandeIds.Contains(f.CommandeId.Value))
            .Include(f => f.Payments)
            .GroupBy(f => f.CommandeId!.Value)
            .Select(g => new { CmdId = g.Key, TotalPaye = g.SelectMany(f => f.Payments).Sum(p => p.Montant) })
            .ToDictionaryAsync(x => x.CmdId, x => x.TotalPaye);
    }

    public async Task UpdateCommandeStatusBatchAsync(List<Commande> commandes)
    {
        if (!commandes.Any()) return;

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        foreach (var cmd in commandes)
        {
            ctx.Attach(cmd);
            ctx.Entry(cmd).Property(c => c.Status).IsModified = true;
            ctx.Entry(cmd).Property(c => c.MontantPaye).IsModified = true;
            ctx.Entry(cmd).Property(c => c.MontantRest).IsModified = true;
        }
        await ctx.SaveChangesAsync();
    }

    public async Task<Commande?> GetWithAllNavigationsAsync(int commandeId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Commandes
            .Include(c => c.Customer)
            .Include(c => c.Details).ThenInclude(d => d.Article)
            .Include(c => c.Livraisons).ThenInclude(l => l.Details)
            .Include(c => c.Livraisons).ThenInclude(l => l.Fact).ThenInclude(f => f!.Payments)
            .FirstOrDefaultAsync(c => c.CommandeId == commandeId);
    }

    public async Task SaveCommandeWithDetailsAsync(Commande commande, List<CommandeDetail> details)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var articleIds = details
            .Select(d => d.ArticleId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (articleIds.Count > 0)
        {
            var catalogById = await ctx.Articles
                .AsNoTracking()
                .Where(a => articleIds.Contains(a.IdArticle))
                .Select(a => new { a.IdArticle, a.Price, a.IdMonais })
                .ToDictionaryAsync(a => a.IdArticle);

            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.ArticleId))
                    continue;
                if (!catalogById.TryGetValue(d.ArticleId, out var art))
                    continue;

                d.Pu = (decimal)art.Price;
                d.MoneyId = art.IdMonais;
            }
        }

        var taux = await ctx.TauxChanges
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync();
        MonetaryStandard.ApplyCommande(commande, details, taux);

        if (commande.CommandeId == 0)
        {
            // New commande
            foreach (var d in details)
                commande.Details.Add(d);

            ctx.Commandes.Add(commande);
        }
        else
        {
            // Update existing commande
            ctx.Commandes.Update(commande);

            // Replace details: remove old, add new
            var existingDetails = await ctx.CommandeDetails
                .Where(d => d.CommandeId == commande.CommandeId)
                .ToListAsync();

            ctx.CommandeDetails.RemoveRange(existingDetails);

            foreach (var d in details)
            {
                d.CommandeId = commande.CommandeId;
                ctx.CommandeDetails.Add(d);
            }
        }

        await ctx.SaveChangesAsync();
    }
}
