using System.Globalization;
using System.Net;
using System.Text;
using AppPlusPlus.Application.Interfaces;
using AppPlusPlus.Application.Services.Parametres;
using AppPlusPlus.Application.Services.Vente;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Parametres;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Web.Print;

public class FacturePrintHtmlRenderer
{
    private readonly IFacturationService _facturation;
    private readonly IShopProfileService _shopProfile;
    private readonly IInvoiceTemplateService _invoiceTemplate;
    private readonly INumberToWordsConverter _numberToWords;

    public FacturePrintHtmlRenderer(
        IFacturationService facturation,
        IShopProfileService shopProfile,
        IInvoiceTemplateService invoiceTemplate,
        INumberToWordsConverter numberToWords)
    {
        _facturation = facturation;
        _shopProfile = shopProfile;
        _invoiceTemplate = invoiceTemplate;
        _numberToWords = numberToWords;
    }

    public async Task<string?> RenderAsync(int factureId, bool autoPrint, string requestPath)
    {
        var shopTask = _shopProfile.GetAsync();
        var tplTask = _invoiceTemplate.GetActiveAsync();
        var factTask = _facturation.GetFactureWithDetailsAsync(factureId);
        await Task.WhenAll(shopTask, tplTask, factTask);

        var fact = factTask.Result;
        if (fact is null)
            return null;

        var shop = shopTask.Result;
        var template = tplTask.Result;
        var shopName = shop.AppNameSetting?.Value ?? "App++";
        var fermerHref = requestPath.Contains("/services/", StringComparison.OrdinalIgnoreCase)
            ? $"/services/facturation/{factureId}"
            : "/vente/facturation";

        var details = fact.Details.ToList();
        var sb = new StringBuilder(8192 + details.Count * 256);
        var isA4 = InvoicePrintFormat.IsA4(template.PrintFormat);
        var logo = ResolveLogo(template, shop);
        var footer = string.IsNullOrWhiteSpace(template.FooterText) ? "Merci de votre visite" : template.FooterText;

        sb.Append("<!DOCTYPE html><html lang=\"fr\"><head><meta charset=\"utf-8\"/>");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"/>");
        sb.Append("<title>Facture N°").Append(factureId).Append("</title>");
        sb.Append("<style>").Append(isA4 ? A4Css : TicketCss).Append("</style></head><body>");

        sb.Append("<div class=\"btn-bar no-print\">");
        sb.Append("<button type=\"button\" class=\"btn-print\" onclick=\"window.print()\">&#128424; Imprimer</button>");
        sb.Append("<a class=\"btn-fermer\" href=\"").Append(H(fermerHref)).Append("\">&#10005; Fermer</a>");
        sb.Append("</div>");

        if (isA4)
            RenderA4(sb, fact, details, shopName, logo, footer, template, shop);
        else
            RenderTicket(sb, fact, details, shopName, logo, shop.Adresse1, shop.Adresse2, footer);

        if (autoPrint)
            sb.Append("<script>window.addEventListener('load',function(){setTimeout(function(){window.print();},250);});</script>");

        sb.Append("</body></html>");
        return sb.ToString();
    }

    void RenderTicket(StringBuilder sb, Fact fact, List<FactDetail> details, string shopName, string? logo,
        string? adresse1, string? adresse2, string footer)
    {
        sb.Append("<div class=\"ticket-wrapper\"><div class=\"ticket tk\">");
        AppendTicketHeader(sb, shopName, logo, adresse1, adresse2);
        AppendInfo(sb, fact, details.Count, "tk");
        AppendLines(sb, fact, details, "tk");
        AppendTotals(sb, fact, footer, "tk");
        sb.Append("</div></div>");
    }

    void RenderA4(StringBuilder sb, Fact fact, List<FactDetail> details, string shopName, string? logo,
        string footer, InvoiceTemplate template, ShopProfile shop)
    {
        var fr = CultureInfo.GetCultureInfo("fr-FR");
        var netLabel = FormatFactCdf(fact, fact.TotalApresReduction ?? fact.Total ?? 0);
        var dateLabel = fact.DateSys.ToString("d MMMM yyyy", fr);
        var wm = template.ShowLetterheadBackground
            ? (!string.IsNullOrWhiteSpace(template.LetterheadBackground) ? template.LetterheadBackground : logo)
            : null;

        sb.Append("<div class=\"a4-sheet\">");
        if (!string.IsNullOrWhiteSpace(wm))
        {
            sb.Append("<div class=\"a4-watermark\" aria-hidden=\"true\">");
            sb.Append("<img class=\"a4-watermark__img\" src=\"").Append(Attr(wm)).Append("\" alt=\"\"/>");
            sb.Append("</div>");
        }

        sb.Append("<div class=\"a4-content\">");
        sb.Append("<div class=\"a4-top\">");
        sb.Append("<h1 class=\"a4-title\">Facture</h1>");
        if (!string.IsNullOrWhiteSpace(logo))
            sb.Append("<img class=\"a4-logo\" src=\"").Append(Attr(logo)).Append("\" alt=\"Logo\"/>");
        sb.Append("</div>");

        sb.Append("<div class=\"a4-meta\">");
        AppendMetaBlock(sb, "N° facture", fact.Id.ToString("D6"));
        AppendMetaBlock(sb, "Date", dateLabel);
        AppendMetaBlock(sb, "Caissier", fact.User ?? "—");
        sb.Append("</div>");

        sb.Append("<div class=\"a4-parties\">");
        sb.Append("<div class=\"a4-party\"><h2>Émetteur</h2><div class=\"a4-party-body\">");
        sb.Append("<p class=\"a4-party-name\">").Append(H(shopName)).Append("</p>");
        AppendPartyLine(sb, template.LegalAdresse ?? shop.Adresse1);
        AppendPartyLine(sb, shop.Adresse2);
        AppendPartyLine(sb, template.LegalContact);
        AppendPartyLine(sb, template.LegalEmail);
        AppendPartyLine(sb, template.LegalWebsite);
        sb.Append("</div></div>");
        sb.Append("<div class=\"a4-party\"><h2>Facturé à</h2><div class=\"a4-party-body\">");
        sb.Append("<p class=\"a4-party-name\">").Append(H(fact.DescriptionName ?? "Client Anonyme")).Append("</p>");
        sb.Append("</div></div></div>");

        sb.Append("<p class=\"a4-summary\"><strong>").Append(H(netLabel))
            .Append("</strong> · ").Append(H(dateLabel)).Append("</p>");

        sb.Append("<table class=\"a4-table\"><colgroup>");
        sb.Append("<col class=\"a4-col-desc\"/><col class=\"a4-col-qty\"/><col class=\"a4-col-pu\"/><col class=\"a4-col-amt\"/>");
        sb.Append("</colgroup><thead><tr>");
        sb.Append("<th>Description</th><th class=\"num\">Qté</th><th class=\"num\">P.U.</th><th class=\"num\">Montant</th>");
        sb.Append("</tr></thead><tbody>");
        foreach (var d in details)
        {
            sb.Append("<tr><td>").Append(H(d.Article?.Description ?? d.DescriptionLine ?? d.IdArticle ?? "—"));
            sb.Append("</td><td class=\"num\">").Append(H(QtyFormat.Display(d.Qte ?? 0)));
            sb.Append("</td><td class=\"num\">").Append(H(FormatLinePu(fact, d)));
            sb.Append("</td><td class=\"num\">").Append(H(FormatLineCdf(fact, d)));
            sb.Append("</td></tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append("<div class=\"a4-bottom\">");
        var netCdf = NetCdf(fact);
        sb.Append("<p class=\"a4-words\">").Append(H(_numberToWords.Convert(netCdf, "francs", "centimes")))
            .Append("</p>");

        sb.Append("<div class=\"a4-totals\">");
        if ((fact.Reduction ?? 0) > 0 || (fact.Total ?? 0) != (fact.TotalApresReduction ?? fact.Total ?? 0))
        {
            sb.Append("<div class=\"a4-total-row\"><span>Sous-total</span><span>")
                .Append(H(FormatFactCdf(fact, fact.Total ?? 0))).Append("</span></div>");
        }
        if ((fact.Reduction ?? 0) > 0)
        {
            var redMontant = (fact.Total ?? 0) * (fact.Reduction ?? 0) / 100m;
            sb.Append("<div class=\"a4-total-row muted\"><span>Réduction ")
                .Append((fact.Reduction ?? 0).ToString("0.##")).Append("%</span><span>− ")
                .Append(H(FormatFactCdf(fact, redMontant))).Append("</span></div>");
        }
        sb.Append("<div class=\"a4-total-row\"><span>Total</span><span>")
            .Append(H(netLabel)).Append("</span></div>");
        sb.Append("<div class=\"a4-total-row strong\"><span>Net à payer</span><span>")
            .Append(H(netLabel)).Append("</span></div></div></div>");

        if (!string.IsNullOrWhiteSpace(footer))
            sb.Append("<p class=\"a4-closing\">").Append(H(footer)).Append("</p>");

        sb.Append("<footer class=\"a4-legal\">");
        var legalParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(template.LegalRccm)) legalParts.Add("RCCM: " + template.LegalRccm);
        if (!string.IsNullOrWhiteSpace(template.LegalIdNat)) legalParts.Add("Id. Nat.: " + template.LegalIdNat);
        if (legalParts.Count > 0)
            sb.Append("<span>").Append(H(string.Join(" · ", legalParts))).Append("</span>");
        sb.Append("<span class=\"a4-page\">Page 1 / 1</span></footer>");

        sb.Append("</div></div>");
    }

    static void AppendMetaBlock(StringBuilder sb, string label, string value)
    {
        sb.Append("<div class=\"a4-meta-item\"><span class=\"a4-meta-label\">").Append(H(label))
            .Append("</span><span class=\"a4-meta-value\">").Append(H(value)).Append("</span></div>");
    }

    static void AppendPartyLine(StringBuilder sb, string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        sb.Append("<p>").Append(H(line)).Append("</p>");
    }

    static string FormatLinePu(Fact fact, FactDetail d)
    {
        var qte = (decimal)(d.Qte ?? 0);
        var pu = (decimal)(d.Pu ?? 0);
        if (pu <= 0 && qte > 0)
        {
            var snap = CurrencyFormat.ResolveMonetarySnapshot(
                d.Montant, d.MoneyId, d.Taux, d.MontantApresConversion,
                pu, qte, fact.MoneyId ?? CurrencyDefaults.MoneyIdCdf, fact.Taux);
            var cdf = CurrencyFormat.IsUsdMoneyId(snap.MoneyId) ? snap.MontantApresConversion : snap.Montant;
            pu = cdf / qte;
        }
        return CurrencyFormat.Cdf(CurrencyFormat.NormalizeLegacyFactCdf(pu, fact.Taux), "N0");
    }

    static string? ResolveLogo(InvoiceTemplate template, ShopProfile shop) =>
        !string.IsNullOrWhiteSpace(template.Logo) ? template.Logo : shop.PhotoShop;

    private void AppendTicketHeader(StringBuilder sb, string shopName, string? photo, string? adresse1, string? adresse2)
    {
        sb.Append("<header class=\"tk-brand\">");
        if (!string.IsNullOrWhiteSpace(photo))
            sb.Append("<div class=\"tk-logo\"><img src=\"").Append(H(photo)).Append("\" alt=\"Logo\"/></div>");
        sb.Append("<h1 class=\"tk-shop\">").Append(H(shopName)).Append("</h1>");
        if (!string.IsNullOrWhiteSpace(adresse1))
            sb.Append("<p class=\"tk-address\">").Append(H(adresse1)).Append("</p>");
        if (!string.IsNullOrWhiteSpace(adresse2))
            sb.Append("<p class=\"tk-address\">").Append(H(adresse2)).Append("</p>");
        sb.Append("</header><div class=\"tk-rule\"></div>");
    }

    private void AppendInfo(StringBuilder sb, Fact fact, int lineCount, string prefix)
    {
        sb.Append("<div class=\"").Append(prefix).Append("-doc\">");
        if (prefix == "a4")
            sb.Append("<h2 class=\"a4-doc-title\">Facture N° ").Append(fact.Id.ToString("D6")).Append("</h2>");
        else
        {
            sb.Append("<span class=\"tk-doc-type\">Facture</span>");
            sb.Append("<span class=\"tk-doc-num\">N° ").Append(fact.Id.ToString("D6")).Append("</span>");
        }
        sb.Append("</div>");
        sb.Append("<dl class=\"").Append(prefix).Append("-info\">");
        AppendInfoRow(sb, prefix, "Date", fact.DateSys.ToString("dd/MM/yyyy HH:mm"));
        AppendInfoRow(sb, prefix, "Client", fact.DescriptionName ?? "Client Anonyme");
        AppendInfoRow(sb, prefix, "Caissier", fact.User ?? "—");
        if (lineCount > 0)
            AppendInfoRow(sb, prefix, "Lignes", lineCount.ToString());
        sb.Append("</dl><div class=\"").Append(prefix).Append("-rule\"></div>");
    }

    private static void AppendInfoRow(StringBuilder sb, string prefix, string label, string value)
    {
        sb.Append("<div class=\"").Append(prefix).Append("-info-item\"><dt>").Append(H(label))
            .Append("</dt><dd>").Append(H(value)).Append("</dd></div>");
    }

    private void AppendLines(StringBuilder sb, Fact fact, List<FactDetail> details, string prefix)
    {
        sb.Append("<section class=\"").Append(prefix).Append("-lines\"><div class=\"").Append(prefix).Append("-lines-head\">");
        sb.Append("<span class=\"").Append(prefix).Append("-col-qty\">Qté</span><span class=\"").Append(prefix)
            .Append("-col-desc\">Article</span><span class=\"").Append(prefix).Append("-col-amt\">Montant</span>");
        sb.Append("</div>");

        foreach (var d in details)
        {
            var qte = d.Qte ?? 0;
            sb.Append("<article class=\"").Append(prefix).Append("-line\">");
            sb.Append("<span class=\"").Append(prefix).Append("-col-qty ").Append(prefix).Append("-qty\">")
                .Append(H(QtyFormat.Display(qte))).Append("</span>");
            sb.Append("<div class=\"").Append(prefix).Append("-col-desc ").Append(prefix).Append("-desc\"><span class=\"")
                .Append(prefix).Append("-name\">");
            sb.Append(H(d.Article?.Description ?? d.DescriptionLine ?? d.IdArticle ?? "—"));
            sb.Append("</span></div>");
            sb.Append("<div class=\"").Append(prefix).Append("-col-amt ").Append(prefix).Append("-amt\"><span>")
                .Append(H(FormatLineCdf(fact, d))).Append("</span></div>");
            sb.Append("</article>");
        }

        sb.Append("</section><div class=\"").Append(prefix).Append("-rule ").Append(prefix).Append("-rule--strong\"></div>");
    }

    private void AppendTotals(StringBuilder sb, Fact fact, string footer, string prefix)
    {
        sb.Append("<section class=\"").Append(prefix).Append("-summary\">");

        if ((fact.Reduction ?? 0) > 0 || (fact.Total ?? 0) != (fact.TotalApresReduction ?? fact.Total ?? 0))
        {
            sb.Append("<div class=\"").Append(prefix).Append("-sum-row\"><span>Sous-total</span><span>")
                .Append(H(FormatFactCdf(fact, fact.Total ?? 0))).Append("</span></div>");
        }

        if ((fact.Reduction ?? 0) > 0)
        {
            var redMontant = (fact.Total ?? 0) * (fact.Reduction ?? 0) / 100m;
            sb.Append("<div class=\"").Append(prefix).Append("-sum-row ").Append(prefix).Append("-sum-row--muted\"><span>Réduction ")
                .Append((fact.Reduction ?? 0).ToString("0.##")).Append("%</span><span>− ")
                .Append(H(FormatFactCdf(fact, redMontant))).Append("</span></div>");
        }

        sb.Append("<div class=\"").Append(prefix).Append("-payable\"><span class=\"").Append(prefix)
            .Append("-payable-label\">Net à payer</span>");
        sb.Append("<span class=\"").Append(prefix).Append("-payable-amt\">")
            .Append(H(FormatFactCdf(fact, fact.TotalApresReduction ?? fact.Total ?? 0)))
            .Append("</span></div></section>");

        var netCdf = NetCdf(fact);
        sb.Append("<p class=\"").Append(prefix).Append("-words\">")
            .Append(H(_numberToWords.Convert(netCdf, "francs", "centimes")))
            .Append("</p>");

        sb.Append("<div class=\"").Append(prefix).Append("-rule\"></div><p class=\"").Append(prefix)
            .Append("-closing\">").Append(H(footer)).Append("</p>");
    }

    private static decimal NetCdf(Fact fact) =>
        CurrencyFormat.NormalizeLegacyFactCdf(fact.TotalApresReduction ?? fact.Total ?? 0, fact.Taux);

    private static string FormatFactCdf(Fact fact, decimal amountFc) =>
        CurrencyFormat.Cdf(CurrencyFormat.NormalizeLegacyFactCdf(amountFc, fact.Taux), "N0");

    private static string FormatLineCdf(Fact fact, FactDetail d)
    {
        var qte = (decimal)(d.Qte ?? 0);
        var pu = (decimal)(d.Pu ?? 0);
        var snap = CurrencyFormat.ResolveMonetarySnapshot(
            d.Montant, d.MoneyId, d.Taux, d.MontantApresConversion,
            pu, qte > 0 ? qte : 1,
            fact.MoneyId ?? CurrencyDefaults.MoneyIdCdf, fact.Taux);
        var cdf = CurrencyFormat.IsUsdMoneyId(snap.MoneyId) ? snap.MontantApresConversion : snap.Montant;
        return CurrencyFormat.Cdf(cdf, "N0");
    }

    private static string H(string? value) => WebUtility.HtmlEncode(value ?? "");

    /// <summary>Attribut HTML (src) — n'encode pas tout le data URI comme background CSS.</summary>
    private static string Attr(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal);
    }

    private const string TicketCss = """
        *{box-sizing:border-box;margin:0;padding:0}
        body{font-family:"Segoe UI",system-ui,sans-serif;background:#f5f5f5;color:#111}
        .btn-bar{text-align:center;padding:18px 0 8px}
        .btn-print,.btn-fermer{display:inline-flex;align-items:center;gap:6px;padding:10px 28px;font-size:14px;font-weight:600;border-radius:6px;cursor:pointer;text-decoration:none}
        .btn-print{background:#1976d2;color:#fff;border:none}
        .btn-fermer{background:#e8e8e8;color:#333;margin-left:10px}
        .ticket-wrapper{display:flex;justify-content:center;padding:16px 0 48px}
        .tk{width:80mm;padding:5mm 4mm 4mm;font-size:11px;line-height:1.4;background:#fff;border:1px solid #e5e5e5;box-shadow:0 2px 16px rgba(0,0,0,.08)}
        .tk-brand{text-align:center;margin-bottom:2px}
        .tk-logo{margin-bottom:6px}
        .tk-logo img{max-height:36px;max-width:48mm;object-fit:contain}
        .tk-shop{font-size:17px;font-weight:700;letter-spacing:.04em;text-transform:uppercase;line-height:1.2}
        .tk-address{margin:2px 0 0;font-size:10px;color:#444;line-height:1.35}
        .tk-rule,.a4-rule{border:none;border-top:1px solid #222;margin:8px 0}
        .tk-rule--strong,.a4-rule--strong{border-top-width:1.5px;margin:10px 0 8px}
        .tk-doc{display:flex;justify-content:space-between;align-items:baseline;gap:8px;margin:2px 0 6px}
        .tk-doc-type{font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:.08em;color:#555}
        .tk-doc-num{font-size:12px;font-weight:700;font-variant-numeric:tabular-nums}
        .tk-info-item,.a4-info-item{display:flex;justify-content:space-between;align-items:baseline;gap:10px;padding:2px 0}
        .tk-info dt,.a4-info dt{font-size:10px;font-weight:600;color:#666;flex-shrink:0}
        .tk-info dd,.a4-info dd{text-align:right;font-size:10.5px;font-weight:500;word-break:break-word}
        .tk-lines-head,.tk-line,.a4-lines-head,.a4-line{display:grid;grid-template-columns:12mm 1fr 24mm;column-gap:2mm;align-items:start}
        .a4-lines-head,.a4-line{grid-template-columns:14mm 1fr 28mm}
        .tk-lines-head,.a4-lines-head{padding:0 0 5px;border-bottom:1px solid #222;font-size:9px;font-weight:700;text-transform:uppercase;letter-spacing:.06em;color:#333}
        .tk-line,.a4-line{padding:7px 0;border-bottom:1px solid #eee}
        .tk-line:last-child,.a4-line:last-child{border-bottom:none;padding-bottom:2px}
        .tk-col-qty,.a4-col-qty{text-align:center}
        .tk-col-amt,.a4-col-amt{text-align:right}
        .tk-qty,.a4-qty{font-size:11px;font-weight:700;font-variant-numeric:tabular-nums;padding-top:1px}
        .tk-name,.a4-name{font-size:11px;font-weight:600;line-height:1.3;word-break:break-word}
        .tk-amt,.a4-amt{font-size:11px;font-weight:700;font-variant-numeric:tabular-nums;white-space:nowrap;padding-top:1px}
        .tk-sum-row,.a4-sum-row{display:flex;justify-content:space-between;align-items:baseline;gap:8px;padding:2px 0;font-size:10.5px;font-variant-numeric:tabular-nums}
        .tk-sum-row--muted,.a4-sum-row--muted{color:#555}
        .tk-payable,.a4-payable{display:flex;justify-content:space-between;align-items:baseline;gap:8px;margin-top:6px;padding-top:8px;border-top:1.5px solid #111}
        .tk-payable-label,.a4-payable-label{font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.04em}
        .tk-payable-amt,.a4-payable-amt{font-size:15px;font-weight:800;font-variant-numeric:tabular-nums}
        .tk-words,.a4-words{margin:8px 0 0;font-size:9.5px;font-style:italic;text-align:center;color:#444;line-height:1.45}
        .tk-closing{margin:8px 0 0;text-align:center;font-size:10px;color:#444}
        @media print{
            .no-print{display:none!important}
            body{background:#fff;margin:0;padding:0}
            .ticket-wrapper{padding:0;margin:0}
            .tk{width:100%;max-width:80mm;margin:0 auto;border:none;box-shadow:none;font-size:10.5px;padding:2mm 3mm}
        }
        """;

    private const string A4Css = """
        *{box-sizing:border-box;margin:0;padding:0}
        body{font-family:"Segoe UI",system-ui,-apple-system,sans-serif;background:#eef1f5;color:#111827;-webkit-print-color-adjust:exact;print-color-adjust:exact}
        .btn-bar{text-align:center;padding:18px 0 8px}
        .btn-print,.btn-fermer{display:inline-flex;align-items:center;gap:6px;padding:10px 28px;font-size:14px;font-weight:600;border-radius:6px;cursor:pointer;text-decoration:none}
        .btn-print{background:#111827;color:#fff;border:none}
        .btn-fermer{background:#e5e7eb;color:#111;margin-left:10px}
        @page{size:A4 portrait;margin:12mm 14mm}
        .a4-sheet{position:relative;width:210mm;min-height:297mm;margin:16px auto;background:#fff;box-shadow:0 8px 30px rgba(15,23,42,.08);overflow:hidden}
        .a4-watermark{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;pointer-events:none;z-index:0}
        .a4-watermark__img{width:min(52%,420px);height:auto;opacity:.09;object-fit:contain;-webkit-print-color-adjust:exact;print-color-adjust:exact}
        .a4-content{position:relative;z-index:1;padding:14mm 16mm 12mm}
        .a4-top{display:flex;justify-content:space-between;align-items:flex-start;gap:24px;margin-bottom:22px}
        .a4-title{font-size:30px;font-weight:700;letter-spacing:-.025em;color:#111827;line-height:1.05}
        .a4-logo{max-height:44px;max-width:128px;object-fit:contain;flex-shrink:0}
        .a4-meta{display:flex;flex-wrap:wrap;gap:8px 56px;margin-bottom:26px}
        .a4-meta-item{min-width:128px;max-width:200px}
        .a4-meta-label{display:block;font-size:10px;line-height:1.3;color:#6b7280;margin-bottom:3px}
        .a4-meta-value{display:block;font-size:12px;font-weight:600;line-height:1.35;color:#111827;font-variant-numeric:tabular-nums}
        .a4-parties{display:grid;grid-template-columns:1fr 1fr;column-gap:72px;margin-bottom:22px;align-items:start}
        .a4-party h2{font-size:10px;font-weight:600;color:#6b7280;text-transform:uppercase;letter-spacing:.07em;margin:0 0 10px}
        .a4-party-name{font-size:13px;font-weight:700;color:#111827;margin:0 0 6px;line-height:1.35}
        .a4-party-body p{font-size:11px;line-height:1.55;color:#374151;margin:0 0 2px}
        .a4-summary{font-size:13px;line-height:1.45;color:#374151;margin:0 0 16px;padding:0 0 14px;border-bottom:1px solid #d1d5db}
        .a4-summary strong{font-weight:700;color:#111827}
        .a4-table{width:100%;table-layout:fixed;border-collapse:collapse;margin:0 0 4px;font-size:11px}
        .a4-col-desc{width:auto}
        .a4-col-qty{width:52px}
        .a4-col-pu{width:92px}
        .a4-col-amt{width:96px}
        .a4-table thead th{text-align:left;font-size:10px;font-weight:600;color:#6b7280;padding:0 8px 10px 0;border-bottom:1px solid #d1d5db;vertical-align:bottom}
        .a4-table thead th.num{padding-right:0;text-align:right}
        .a4-table tbody td{padding:11px 8px 11px 0;border-bottom:1px solid #f3f4f6;vertical-align:top;color:#111827;line-height:1.45}
        .a4-table tbody td.num{padding-right:0;text-align:right;font-variant-numeric:tabular-nums;white-space:nowrap}
        .a4-table tbody tr:last-child td{border-bottom:1px solid #d1d5db}
        .a4-bottom{display:flex;align-items:flex-start;justify-content:space-between;gap:32px;margin-top:6px}
        .a4-words{flex:1;min-width:0;max-width:52%;margin:8px 0 0;font-size:10px;font-style:italic;line-height:1.55;color:#6b7280}
        .a4-totals{width:220px;flex-shrink:0;margin-left:auto;font-size:11px}
        .a4-total-row{display:flex;justify-content:space-between;align-items:baseline;gap:16px;padding:5px 0;color:#374151;font-variant-numeric:tabular-nums}
        .a4-total-row.muted{color:#6b7280}
        .a4-total-row.strong{margin-top:4px;padding-top:10px;border-top:1.5px solid #111827;font-size:12px;font-weight:700;color:#111827}
        .a4-closing{margin:20px 0 0;font-size:10px;color:#6b7280;text-align:left}
        .a4-legal{margin-top:32px;padding-top:12px;border-top:1px solid #e5e7eb;display:flex;justify-content:space-between;align-items:flex-end;gap:16px;font-size:9px;line-height:1.45;color:#9ca3af}
        .a4-page{margin-left:auto;white-space:nowrap}
        @media print{
            .no-print{display:none!important}
            body{background:#fff;margin:0;padding:0}
            .a4-sheet{margin:0;box-shadow:none;width:auto;min-height:auto}
            .a4-content{padding:0}
            .a4-watermark__img{opacity:.1}
        }
        """;
}
