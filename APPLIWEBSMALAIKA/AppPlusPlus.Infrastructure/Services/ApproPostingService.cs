using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Approvisionnement;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Entities.Stock;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class ApproPostingService : IApproPostingService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IInternalTransferService _internalTransfer;

    public ApproPostingService(
        IDbContextFactory<AppDbContext> dbFactory,
        IInternalTransferService internalTransfer)
    {
        _dbFactory = dbFactory;
        _internalTransfer = internalTransfer;
    }

    public async Task<int> SaveDirectApproAsync(DirectApproSaveRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLines(request.Lines);

        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.Now;
        var user = request.UserLogin;
        var machine = Environment.MachineName;
        var taux = MonetaryStandard.ResolveTaux(request.Taux);

        Appro appro;
        if (request.ApproId.HasValue)
        {
            appro = await ctx.Appros
                .Include(a => a.Details)
                .FirstOrDefaultAsync(a => a.Id == request.ApproId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Approvisionnement introuvable.");

            if (ApproFilters.IsCancelled(appro))
                throw new InvalidOperationException("Impossible de modifier un approvisionnement annulé.");

            if (ApproFilters.IsInternalTransfer(appro))
                throw new InvalidOperationException("Les transferts internes ne sont pas modifiables depuis cet écran.");

            await ReverseApproImpactAsync(ctx, appro, user, now, cancellationToken);

            var oldDetails = await ctx.ApproDetails.Where(d => d.IdAppro == appro.Id).ToListAsync(cancellationToken);
            ctx.ApproDetails.RemoveRange(oldDetails);
        }
        else
        {
            appro = new Appro();
            ctx.Appros.Add(appro);
        }

        ApplyHeader(appro, request, user, machine, taux, request.Lines);
        appro.StatusId = ApproStatus.Direct;
        appro.IdCmdDetail = 0;
        appro.IdCmd = null;

        await ctx.SaveChangesAsync(cancellationToken);

        var moneyById = await ctx.Moneys.ToDictionaryAsync(m => m.IdMonais, m => m.DescriptionMonais, cancellationToken);

        foreach (var line in request.Lines)
        {
            await ApplyLineAsync(ctx, appro, line, request.LocalisationId, user, machine, now, taux, moneyById,
                $"Approvisionnement - {appro.Reference ?? "Direct"}", cancellationToken);
        }

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return appro.Id;
    }

    public async Task<int> ReceiveFromCommandAsync(CommandReceptionRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.Lines.Any())
            throw new InvalidOperationException("Aucune ligne à réceptionner.");

        ValidateLines(request.Lines.Select(l => new ApproLineInput
        {
            IdArticle = l.IdArticle,
            Qte = l.Qte,
            PA = l.PA,
            PV = l.PV
        }));

        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var user = request.UserLogin;
        var machine = Environment.MachineName;
        var taux = MonetaryStandard.ResolveTaux(request.Taux);
        var moneyById = await ctx.Moneys.ToDictionaryAsync(m => m.IdMonais, m => m.DescriptionMonais, cancellationToken);
        var createdIds = new List<int>();

        foreach (var line in request.Lines)
        {
            var lineMontant = ((line.PV ?? 0) - (line.PA ?? 0)) * line.Qte;
            var appro = new Appro
            {
                IdArticle = line.IdArticle,
                IdCmd = request.CmdId,
                IdCmdDetail = line.CmdDetailId,
                Qte = line.Qte,
                PA = line.PA,
                PV = line.PV,
                Ben = (line.PV ?? 0) - (line.PA ?? 0),
                BenTotal = lineMontant,
                Date = today,
                DateSys = now,
                User = user,
                Cumputer = machine,
                Commentaire = $"Réception Cmd N°{request.CmdId}",
                SupplierId = request.SupplierId,
                LocalisationId = request.LocalisationId,
                StatusId = ApproStatus.Direct,
                Reference = $"Cmd-{request.CmdId}"
            };
            MonetaryStandard.ApplyApproHeader(appro, lineMontant, taux);
            ctx.Appros.Add(appro);
            await ctx.SaveChangesAsync(cancellationToken);
            createdIds.Add(appro.Id);

            var cmdDetail = await ctx.CmdDetails.FindAsync([line.CmdDetailId], cancellationToken);
            if (cmdDetail != null)
                cmdDetail.QteReceive = (cmdDetail.QteReceive ?? 0) + (double)line.Qte;

            await ApplyLineAsync(ctx, appro, new ApproLineInput
            {
                IdArticle = line.IdArticle,
                Qte = line.Qte,
                PA = line.PA,
                PV = line.PV,
                DateExpiration = line.DateExpiration
            }, request.LocalisationId, user, machine, now, taux, moneyById,
                $"Réception depuis commande N°{request.CmdId}", cancellationToken,
                reference: $"Cmd N°{request.CmdId}");
        }

        await UpdateCommandStatusAsync(ctx, request.CmdId, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return createdIds.FirstOrDefault();
    }

    public async Task CancelApproAsync(int approId, string userLogin, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var appro = await ctx.Appros
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == approId, cancellationToken)
            ?? throw new InvalidOperationException("Approvisionnement introuvable.");

        if (ApproFilters.IsCancelled(appro))
            throw new InvalidOperationException("Cet approvisionnement est déjà annulé.");

        if (ApproFilters.IsInternalTransfer(appro))
        {
            await _internalTransfer.CancelAsync(approId, userLogin, cancellationToken);
            return;
        }

        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        appro = await ctx.Appros
            .Include(a => a.Details)
            .FirstAsync(a => a.Id == approId, cancellationToken);

        var now = DateTime.Now;
        await ReverseApproImpactAsync(ctx, appro, userLogin, now, cancellationToken);
        appro.StatusId = ApproStatus.Cancelled;
        appro.Commentaire = TextFieldLimits.AppendWithLimit(
            appro.Commentaire,
            $"[ANNULÉ {now:dd/MM/yy HH:mm}]",
            TextFieldLimits.ApproCommentaire);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task DeleteApproAsync(int approId, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var appro = await ctx.Appros
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == approId, cancellationToken)
            ?? throw new InvalidOperationException("Approvisionnement introuvable.");

        if (ApproFilters.IsInternalTransfer(appro))
        {
            await _internalTransfer.DeleteAsync(approId, cancellationToken);
            return;
        }

        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var trackedAppro = await ctx.Appros
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.Id == approId, cancellationToken)
            ?? throw new InvalidOperationException("Approvisionnement introuvable.");

        if (!ApproFilters.IsCancelled(trackedAppro))
        {
            var hasMovements = await ctx.MouvementsStock.AnyAsync(m =>
                m.TypeDocument == TypesDocument.APPRO &&
                m.IdDocument == approId &&
                !m.Annule, cancellationToken);

            if (hasMovements || trackedAppro.Details.Any())
                throw new InvalidOperationException(
                    "Annulez d'abord l'approvisionnement pour conserver la traçabilité avant suppression.");
        }

        var details = await ctx.ApproDetails.Where(d => d.IdAppro == approId).ToListAsync(cancellationToken);
        ctx.ApproDetails.RemoveRange(details);
        ctx.Appros.Remove(trackedAppro);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    static void ValidateLines(IEnumerable<ApproLineInput> lines)
    {
        var list = lines.ToList();
        if (!list.Any())
            throw new InvalidOperationException("Ajoutez au moins un article.");

        if (list.Any(l => string.IsNullOrWhiteSpace(l.IdArticle)))
            throw new InvalidOperationException("Tous les articles doivent être sélectionnés.");

        if (list.Any(l => l.Qte <= 0))
            throw new InvalidOperationException("Les quantités doivent être > 0.");

        foreach (var line in list)
            line.Qte = QuantityFormat.Normalize(line.Qte);

        if (list.Any(l => !l.PA.HasValue))
            throw new InvalidOperationException("Le prix d'achat (PA) est obligatoire pour tous les articles.");

        if (list.Any(l => l.PA.HasValue && l.PV.HasValue && l.PA.Value > l.PV.Value))
            throw new InvalidOperationException("Le prix d'achat ne peut pas être supérieur au prix de vente.");
    }

    static void ApplyHeader(Appro appro, DirectApproSaveRequest request, string user, string machine, decimal taux,
        List<ApproLineInput> lines)
    {
        var totalQte = lines.Sum(l => l.Qte);
        var totalDepense = lines.Sum(l => (l.PA ?? 0) * l.Qte);
        var weightedPa = totalQte > 0 ? totalDepense / totalQte : 0;
        var weightedPv = totalQte > 0 ? lines.Sum(l => (l.PV ?? 0) * l.Qte) / totalQte : 0;
        var benTotal = lines.Sum(l => ((l.PV ?? 0) - (l.PA ?? 0)) * l.Qte);

        appro.Reference = TextFieldLimits.Truncate(request.Reference, TextFieldLimits.ApproReference);
        appro.SupplierId = request.SupplierId;
        appro.Date = request.Date;
        appro.DateSys = DateTime.Now;
        appro.User = TextFieldLimits.Truncate(user, TextFieldLimits.ApproUser);
        appro.Cumputer = TextFieldLimits.Truncate(machine, TextFieldLimits.ApproUser);
        appro.LocalisationId = request.LocalisationId;
        appro.Commentaire = TextFieldLimits.Truncate(
            string.IsNullOrWhiteSpace(request.Commentaire) ? "Approvisionnement" : request.Commentaire,
            TextFieldLimits.ApproCommentaire);
        appro.IdArticle = lines.First().IdArticle;
        appro.Qte = totalQte;
        appro.PA = weightedPa;
        appro.PV = weightedPv;
        appro.Ben = weightedPv - weightedPa;
        appro.BenTotal = benTotal;
        appro.Taux = taux;
        MonetaryStandard.ApplyApproHeader(appro, benTotal, taux);
    }

    async Task ApplyLineAsync(
        AppDbContext ctx,
        Appro appro,
        ApproLineInput line,
        int localisationId,
        string user,
        string machine,
        DateTime now,
        decimal taux,
        Dictionary<int, string?> moneyById,
        string observation,
        CancellationToken cancellationToken,
        string? reference = null)
    {
        var art = await ctx.Articles.FirstOrDefaultAsync(a => a.IdArticle == line.IdArticle, cancellationToken);
        var lineQte = QuantityFormat.Normalize(line.Qte);
        var lineMontant = ((line.PV ?? 0) - (line.PA ?? 0)) * lineQte;
        var today = DateOnly.FromDateTime(now);

        var detail = new ApproDetail
        {
            IdAppro = appro.Id,
            IdArticle = line.IdArticle,
            IdLocalisation = localisationId,
            Qte = lineQte,
            PA = line.PA,
            PV = line.PV,
            Ben = (line.PV ?? 0) - (line.PA ?? 0),
            BenTotal = lineMontant,
            DateExpiration = line.DateExpiration,
            DateSys = now,
            User = user,
            Cumputer = machine
        };
        MonetaryStandard.ApplyApproDetail(detail, lineMontant, taux);
        ctx.ApproDetails.Add(detail);
        await ctx.SaveChangesAsync(cancellationToken);

        var stock = await ctx.Stocks.FirstOrDefaultAsync(s =>
            s.IdArticle == line.IdArticle && s.IdLocalisation == localisationId, cancellationToken);

        var qteAvant = stock?.Qte ?? 0;
        var qteApres = QuantityFormat.Normalize(qteAvant + lineQte);

        if (stock != null)
        {
            stock.Qte = qteApres;
            stock.DateSys = today;
            stock.UserLogin = user;
        }
        else
        {
            ctx.Stocks.Add(new Stock
            {
                IdArticle = line.IdArticle,
                IdLocalisation = localisationId,
                Qte = lineQte,
                Seuil = 0,
                QteMax = StockDefaults.DefaultQteMax,
                DateSys = today,
                UserLogin = user
            });
        }

        ctx.MouvementsStock.Add(new MouvementStock
        {
            IdArticle = line.IdArticle,
            IdLocalisation = localisationId,
            TypeMouvement = TypesMouvement.ENTREE,
            Quantite = lineQte,
            QteAvant = qteAvant,
            QteApres = qteApres,
            DateMouvement = now,
            TypeDocument = TypesDocument.APPRO,
            IdDocument = appro.Id,
            IdDocumentDetail = detail.Id,
            Reference = reference ?? appro.Reference,
            PrixUnitaire = line.PA,
            Observation = observation,
            DateExpiration = line.DateExpiration,
            CreePar = user,
            DateCreation = now
        });

        if (art != null)
        {
            await StockQuantitySync.SyncArticleGlobalAsync(ctx, line.IdArticle, cancellationToken);
            if (line.PV.HasValue && line.PV.Value > 0)
            {
                art.Price = (double)CurrencyFormat.RoundFcUp(line.PV.Value);
                art.IdMonais = CurrencyDefaults.MoneyIdCdf;
            }
        }
    }

    async Task ReverseApproImpactAsync(AppDbContext ctx, Appro appro, string user, DateTime now,
        CancellationToken cancellationToken)
    {
        var lines = appro.Details.Any()
            ? appro.Details.Select(d => (
                IdArticle: d.IdArticle,
                IdLocalisation: ResolveApproLineLocalisation(d.IdLocalisation, appro.LocalisationId),
                Qte: d.Qte,
                PA: d.PA)).ToList()
            : new List<(string IdArticle, int IdLocalisation, decimal Qte, decimal? PA)>
            {
                (appro.IdArticle, appro.LocalisationId ?? 0, appro.Qte, appro.PA)
            };

        var today = DateOnly.FromDateTime(now);

        foreach (var line in lines.Where(l => l.IdLocalisation > 0))
        {
            var stock = await ctx.Stocks.FirstOrDefaultAsync(s =>
                s.IdArticle == line.IdArticle && s.IdLocalisation == line.IdLocalisation, cancellationToken);

            var qteAvant = stock?.Qte ?? 0;
            var reverseQte = QuantityFormat.Normalize(line.Qte);
            var qteApres = QuantityFormat.Normalize(Math.Max(0, qteAvant - reverseQte));

            if (stock != null)
            {
                stock.Qte = qteApres;
                stock.DateSys = today;
                stock.UserLogin = user;
            }

            ctx.MouvementsStock.Add(new MouvementStock
            {
                IdArticle = line.IdArticle,
                IdLocalisation = line.IdLocalisation,
                TypeMouvement = TypesMouvement.SORTIE,
                Quantite = reverseQte,
                QteAvant = qteAvant,
                QteApres = qteApres,
                DateMouvement = now,
                TypeDocument = TypesDocument.APPRO,
                IdDocument = appro.Id,
                Reference = appro.Reference,
                PrixUnitaire = line.PA,
                Observation = $"Annulation / correction appro N°{appro.Id}",
                CreePar = user,
                DateCreation = now
            });
        }

        await StockQuantitySync.SyncArticlesGlobalAsync(
            ctx,
            lines.Select(l => l.IdArticle),
            cancellationToken);

        var mouvements = await ctx.MouvementsStock
            .Where(m => m.TypeDocument == TypesDocument.APPRO &&
                        m.IdDocument == appro.Id &&
                        !m.Annule &&
                        m.TypeMouvement == TypesMouvement.ENTREE)
            .ToListAsync(cancellationToken);

        foreach (var m in mouvements)
            m.Annule = true;

        if (appro.IdCmdDetail > 0)
        {
            var cmdDetail = await ctx.CmdDetails.FindAsync([appro.IdCmdDetail], cancellationToken);
            if (cmdDetail != null)
                cmdDetail.QteReceive = Math.Max(0, (cmdDetail.QteReceive ?? 0) - (double)appro.Qte);
        }

        if (appro.IdCmd.HasValue && appro.IdCmd.Value > 0)
            await UpdateCommandStatusAsync(ctx, appro.IdCmd.Value, cancellationToken);
    }

    static async Task UpdateCommandStatusAsync(AppDbContext ctx, int cmdId, CancellationToken cancellationToken)
    {
        var cmd = await ctx.Cmds.Include(c => c.Details).FirstOrDefaultAsync(c => c.Id == cmdId, cancellationToken);
        if (cmd == null) return;

        var articleIds = cmd.Details.Select(d => d.IdArticle).Where(a => a != null).ToList();
        var totalReceived = await ctx.Appros
            .Where(a => a.IdCmd == cmdId &&
                        articleIds.Contains(a.IdArticle) &&
                        a.StatusId != ApproStatus.Cancelled)
            .SumAsync(a => a.Qte, cancellationToken);

        var totalOrder = cmd.Details.Sum(d => (decimal)(d.QteOrder ?? 0));
        cmd.Status = totalReceived >= totalOrder ? 2 : totalReceived > 0 ? 1 : 0;
    }

    static int ResolveApproLineLocalisation(int detailLocalisationId, int? headerLocalisationId) =>
        detailLocalisationId > 0
            ? detailLocalisationId
            : headerLocalisationId ?? 0;
}
