using System.Globalization;
using System.Text;
using AppPlusPlus.Application.Services.Inventaire;
using AppPlusPlus.Application.Services.Parametres;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Web.Print;

public class InventaireReceptionPrintHtmlRenderer
{
    private readonly IInventaireService _inventaire;
    private readonly IShopProfileService _shopProfile;

    public InventaireReceptionPrintHtmlRenderer(IInventaireService inventaire, IShopProfileService shopProfile)
    {
        _inventaire = inventaire;
        _shopProfile = shopProfile;
    }

    public async Task<string?> RenderAsync(int receptionId, bool autoPrint)
    {
        var recTask = _inventaire.GetReceptionAsync(receptionId);
        var shopTask = _shopProfile.GetAsync();
        await Task.WhenAll(recTask, shopTask);

        var rec = recTask.Result;
        if (rec is null) return null;

        var shop = shopTask.Result;
        var shopName = shop.AppNameSetting?.Value ?? "App++";
        var magasin = string.IsNullOrWhiteSpace(rec.NomMagasin) ? shopName : rec.NomMagasin;
        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var refNo = rec.NumeroReception ?? $"REC-{rec.IdReception}";
        var dateLabel = rec.DateJour.ToString("dddd d MMMM yyyy", culture);
        var totalLabel = CurrencyFormat.Cdf(rec.MontantTotal, "N0");

        var sb = new StringBuilder(8192 + rec.Lines.Count * 220);
        InventairePrintHtmlStyles.AppendDocumentStart(sb, $"Relevé {refNo}");
        InventairePrintHtmlStyles.AppendButtonBar(sb);
        InventairePrintHtmlStyles.AppendReceiptShellStart(sb);

        InventairePrintHtmlStyles.AppendReceiptHeader(sb, "Relevé", shop);

        InventairePrintHtmlStyles.AppendDocMeta(sb,
        [
            ("N° document", refNo),
            ("N° réception", rec.IdReception.ToString("D5")),
            ("Date", dateLabel),
            ("Statut", rec.StatutLabel),
            ("Saisi par", InventairePrintHtmlStyles.UserLabel(rec.CreePar)),
            ("Clôturé par", string.IsNullOrWhiteSpace(rec.CloturePar) ? "—" : rec.CloturePar!)
        ]);

        InventairePrintHtmlStyles.AppendParties(sb,
            shopName,
            [shopName, shop.Adresse1, shop.Adresse2],
            "Magasin",
            [magasin, !string.IsNullOrWhiteSpace(rec.SessionNom) ? $"Session : {rec.SessionNom}" : null]);

        InventairePrintHtmlStyles.AppendFormulaLine(sb,
            $"<strong>{InventairePrintHtmlStyles.H(totalLabel)}</strong> reçus le {InventairePrintHtmlStyles.H(dateLabel)}");

        var rows = rec.Lines.OrderBy(l => l.DateLigne).Select(line =>
        {
            var sub = $"{line.DateLigne:HH:mm} · {line.IdArticle}";
            return new[]
            {
                InventairePrintHtmlStyles.DescCell(line.ArticleName, sub),
                line.Quantite.ToString("N2", culture),
                line.DateExpiration?.ToString("dd/MM/yyyy", culture) ?? "—",
                CurrencyFormat.Cdf(line.PrixUnitaire, "N0"),
                CurrencyFormat.Cdf(line.MontantLigne, "N0")
            };
        });

        InventairePrintHtmlStyles.AppendDataTable(sb,
            ["Description", "Qté", "Exp.", "P.U.", "Montant"],
            [false, true, true, true, true],
            rows);

        InventairePrintHtmlStyles.AppendTotals(sb,
        [
            ("Sous-total", totalLabel, false),
            ("Total réception", totalLabel, false),
            ("Montant reçu", totalLabel, true)
        ]);

        InventairePrintHtmlStyles.AppendNote(sb, rec.Observation);

        InventairePrintHtmlStyles.AppendFooter(sb, shopName);
        InventairePrintHtmlStyles.AppendReceiptShellEnd(sb);

        if (autoPrint) InventairePrintHtmlStyles.AppendAutoPrintScript(sb);
        InventairePrintHtmlStyles.AppendDocumentEnd(sb);
        return sb.ToString();
    }
}
