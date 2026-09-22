using System.Globalization;
using System.Text;
using AppPlusPlus.Application.DTOs.Inventaire;
using AppPlusPlus.Application.Services.Inventaire;
using AppPlusPlus.Application.Services.Parametres;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Web.Print;

public class InventaireSessionPrintHtmlRenderer
{
    private readonly IInventaireService _inventaire;
    private readonly IShopProfileService _shopProfile;

    public InventaireSessionPrintHtmlRenderer(IInventaireService inventaire, IShopProfileService shopProfile)
    {
        _inventaire = inventaire;
        _shopProfile = shopProfile;
    }

    public async Task<string?> RenderAsync(int sessionId, bool autoPrint)
    {
        var sessionTask = _inventaire.GetSessionAsync(sessionId);
        var shopTask = _shopProfile.GetAsync();
        await Task.WhenAll(sessionTask, shopTask);

        var session = sessionTask.Result;
        if (session is null) return null;

        var shop = shopTask.Result;
        var shopName = shop.AppNameSetting?.Value ?? "App++";
        var magasin = string.IsNullOrWhiteSpace(session.NomMagasin) ? shopName : session.NomMagasin;
        var culture = CultureInfo.GetCultureInfo("fr-FR");

        var receptions = await _inventaire.GetReceptionsAsync(sessionId);
        var ventes = await _inventaire.GetVentesAsync(sessionId);

        var fullReceptions = new List<(InventaireReceptionDto Summary, InventaireReceptionDto? Detail)>();
        foreach (var r in receptions.OrderByDescending(x => x.DateJour))
        {
            var detail = await _inventaire.GetReceptionAsync(r.IdReception);
            fullReceptions.Add((r, detail));
        }

        var periode = session.DateDebut.ToString("dd/MM/yyyy", culture)
                        + (session.DateFin.HasValue ? $" – {session.DateFin:dd/MM/yyyy}" : "");
        var resteLabel = CurrencyFormat.Cdf(session.MontantTheorique, "N0");
        var docNo = $"INV-{session.IdInventaire:D5}";

        var sb = new StringBuilder(16384);
        InventairePrintHtmlStyles.AppendDocumentStart(sb, $"Situation {session.Nom}");
        InventairePrintHtmlStyles.AppendButtonBar(sb);
        InventairePrintHtmlStyles.AppendReceiptShellStart(sb);

        InventairePrintHtmlStyles.AppendReceiptHeader(sb, "Relevé", shop);

        InventairePrintHtmlStyles.AppendDocMeta(sb,
        [
            ("N° document", docNo),
            ("Session", session.Nom),
            ("Période", periode),
            ("Statut", session.StatutLabel),
            ("Ouvert par", InventairePrintHtmlStyles.UserLabel(session.CreePar)),
            ("Clôturé par", string.IsNullOrWhiteSpace(session.CloturePar) ? "—" : session.CloturePar!)
        ]);

        InventairePrintHtmlStyles.AppendParties(sb,
            shopName,
            [shopName, shop.Adresse1, shop.Adresse2],
            "Magasin",
            [magasin, $"Période : {periode}"]);

        InventairePrintHtmlStyles.AppendCompactSummary(sb,
        [
            ("Départ", CurrencyFormat.Cdf(session.MontantInitial, "N0")),
            ("Réceptions", "+ " + CurrencyFormat.Cdf(session.MontantReceptions, "N0")),
            ("Ventes / dépenses", "− " + CurrencyFormat.Cdf(session.MontantVentes, "N0")),
            ("Reste théorique", resteLabel)
        ]);

        InventairePrintHtmlStyles.AppendFormulaLine(sb,
            $"{CurrencyFormat.Cdf(session.MontantInitial, "N0")} + {CurrencyFormat.Cdf(session.MontantReceptions, "N0")} − {CurrencyFormat.Cdf(session.MontantVentes, "N0")} = {resteLabel}");

        if (fullReceptions.Count > 0)
        {
            InventairePrintHtmlStyles.AppendSectionTitle(sb, "Réceptions");

            var recRows = fullReceptions.SelectMany(pair =>
            {
                var (summary, detail) = pair;
                var main = new[]
                {
                    InventairePrintHtmlStyles.DescCell(
                        summary.DateJour.ToString("dddd dd/MM/yyyy", culture),
                        $"{summary.StatutLabel} · {InventairePrintHtmlStyles.UserLabel(summary.CreePar, summary.CloturePar)}"),
                    summary.NbLignes.ToString(),
                    "—",
                    "+ " + CurrencyFormat.Cdf(summary.MontantTotal, "N0")
                };
                var list = new List<string[]> { main };

                if (detail?.Lines.Count > 0)
                {
                    foreach (var line in detail.Lines.OrderBy(l => l.DateLigne))
                    {
                        list.Add(
                        [
                            InventairePrintHtmlStyles.DescCell(line.ArticleName, $"{line.DateLigne:HH:mm} · {line.IdArticle}"),
                            line.Quantite.ToString("N2", culture),
                            CurrencyFormat.Cdf(line.PrixUnitaire, "N0"),
                            CurrencyFormat.Cdf(line.MontantLigne, "N0")
                        ]);
                    }
                }
                return list;
            });

            InventairePrintHtmlStyles.AppendDataTable(sb,
                ["Description", "Qté", "P.U.", "Montant"],
                [false, true, true, true],
                recRows);

            InventairePrintHtmlStyles.AppendTotals(sb,
            [
                ("Total réceptions", "+ " + CurrencyFormat.Cdf(session.MontantReceptions, "N0"), false)
            ]);
        }

        if (ventes.Count > 0)
        {
            InventairePrintHtmlStyles.AppendSectionTitle(sb, "Historique ventes et dépenses");

            var venteRows = ventes.OrderByDescending(v => v.DateVente).Select(v => new[]
            {
                InventairePrintHtmlStyles.DescCell(
                    v.DateVente.ToString("dddd dd/MM/yyyy", culture),
                    $"Par {InventairePrintHtmlStyles.UserLabel(v.CreePar)} · Ventes {CurrencyFormat.Cdf(v.MontantVentes, "N0")} · Dépenses {CurrencyFormat.Cdf(v.MontantDepenses, "N0")}" +
                    (string.IsNullOrWhiteSpace(v.Observation) ? "" : $" · {v.Observation}")),
                "1",
                CurrencyFormat.Cdf(v.MontantVentes + v.MontantDepenses, "N0"),
                "− " + CurrencyFormat.Cdf(v.Montant, "N0")
            });

            InventairePrintHtmlStyles.AppendDataTable(sb,
                ["Description", "Qté", "Montant journalier", "Total sorties"],
                [false, true, true, true],
                venteRows);

            var totalVentes = ventes.Sum(v => v.MontantVentes);
            var totalDepenses = ventes.Sum(v => v.MontantDepenses);
            var totalSorties = totalVentes + totalDepenses;

            InventairePrintHtmlStyles.AppendTotals(sb,
            [
                ("Ventes", CurrencyFormat.Cdf(totalVentes, "N0"), false),
                ("Dépenses", CurrencyFormat.Cdf(totalDepenses, "N0"), false),
                ("Total sorties", "− " + CurrencyFormat.Cdf(totalSorties, "N0"), true)
            ]);
        }

        InventairePrintHtmlStyles.AppendFooter(sb,
            $"{shopName} · Généré le {DateTime.Now:dd/MM/yyyy HH:mm}");

        InventairePrintHtmlStyles.AppendReceiptShellEnd(sb);

        if (autoPrint) InventairePrintHtmlStyles.AppendAutoPrintScript(sb);
        InventairePrintHtmlStyles.AppendDocumentEnd(sb);
        return sb.ToString();
    }
}
