using System.Net;
using System.Text;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Parametres;

namespace AppPlusPlus.Web.Print;

static class InventairePrintHtmlStyles
{
    public static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

    public static string UserLabel(string? creePar, string? cloturePar = null)
        => UserDisplayNames.Format(!string.IsNullOrWhiteSpace(cloturePar) ? cloturePar : creePar);

    public static string Attr(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal);
    }

    public static void AppendDocumentStart(StringBuilder sb, string title)
    {
        sb.Append("<!DOCTYPE html><html lang=\"fr\"><head><meta charset=\"utf-8\"/>");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"/>");
        sb.Append("<title>").Append(H(title)).Append("</title>");
        sb.Append("<style>").Append(Css).Append("</style></head><body>");
    }

    public static void AppendButtonBar(StringBuilder sb)
    {
        sb.Append("<div class=\"btn-bar no-print\">");
        sb.Append("<button type=\"button\" class=\"btn-print\" onclick=\"window.print()\">Imprimer</button>");
        sb.Append("<button type=\"button\" class=\"btn-close\" onclick=\"window.close()\">Fermer</button>");
        sb.Append("</div>");
    }

    public static void AppendAutoPrintScript(StringBuilder sb)
        => sb.Append("<script>window.addEventListener('load',function(){setTimeout(function(){window.print();},300);});</script>");

    public static void AppendDocumentEnd(StringBuilder sb) => sb.Append("</body></html>");

    public static void AppendReceiptShellStart(StringBuilder sb)
        => sb.Append("<div class=\"rcpt-sheet\"><div class=\"rcpt-body\">");

    public static void AppendReceiptShellEnd(StringBuilder sb)
        => sb.Append("</div></div>");

    public static void AppendReceiptHeader(StringBuilder sb, string docTitle, ShopProfile shop, string? logoOverride = null)
    {
        var shopName = shop.AppNameSetting?.Value ?? "App++";
        var logo = logoOverride ?? shop.PhotoShop;

        sb.Append("<header class=\"rcpt-head\">");
        sb.Append("<h1 class=\"rcpt-head__title\">").Append(H(docTitle)).Append("</h1>");
        if (!string.IsNullOrWhiteSpace(logo))
            sb.Append("<img class=\"rcpt-head__logo\" src=\"").Append(Attr(logo)).Append("\" alt=\"Logo\"/>");
        sb.Append("</header>");
    }

    public static void AppendDocMeta(StringBuilder sb, IEnumerable<(string Label, string Value)> rows)
    {
        sb.Append("<dl class=\"rcpt-docmeta\">");
        foreach (var (label, value) in rows)
        {
            sb.Append("<div class=\"rcpt-docmeta__row\"><dt>").Append(H(label)).Append("</dt><dd>")
                .Append(H(value)).Append("</dd></div>");
        }
        sb.Append("</dl>");
    }

    public static void AppendParties(StringBuilder sb, string emitterTitle, IEnumerable<string?> emitterLines,
        string recipientTitle, IEnumerable<string?> recipientLines)
    {
        sb.Append("<div class=\"rcpt-parties\">");
        AppendParty(sb, emitterTitle, emitterLines);
        AppendParty(sb, recipientTitle, recipientLines);
        sb.Append("</div>");
    }

    static void AppendParty(StringBuilder sb, string title, IEnumerable<string?> lines)
    {
        var list = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        sb.Append("<div class=\"rcpt-party\"><h2>").Append(H(title)).Append("</h2>");
        if (list.Count > 0)
        {
            sb.Append("<p class=\"rcpt-party__name\">").Append(H(list[0])).Append("</p>");
            foreach (var line in list.Skip(1))
                sb.Append("<p>").Append(H(line)).Append("</p>");
        }
        sb.Append("</div>");
    }

    public static void AppendCompactSummary(StringBuilder sb, IEnumerable<(string Label, string Value)> rows)
    {
        sb.Append("<dl class=\"rcpt-summary\">");
        foreach (var (label, value) in rows)
        {
            sb.Append("<div class=\"rcpt-summary__row\"><dt>").Append(H(label)).Append("</dt><dd>")
                .Append(H(value)).Append("</dd></div>");
        }
        sb.Append("</dl>");
    }

    public static void AppendFormulaLine(StringBuilder sb, string text)
        => sb.Append("<p class=\"rcpt-formula\">").Append(text).Append("</p>");

    public static void AppendSectionTitle(StringBuilder sb, string title)
        => sb.Append("<h3 class=\"rcpt-section\">").Append(H(title)).Append("</h3>");

    public static void AppendDataTable(StringBuilder sb, string[] headers, bool[] rightAlign, IEnumerable<string[]> rows)
    {
        sb.Append("<table class=\"rcpt-table\"><thead><tr>");
        for (var i = 0; i < headers.Length; i++)
            sb.Append("<th").Append(rightAlign[i] ? " class=\"num\"" : "").Append(">").Append(H(headers[i])).Append("</th>");
        sb.Append("</tr></thead><tbody>");

        foreach (var cells in rows)
        {
            sb.Append("<tr>");
            for (var i = 0; i < cells.Length; i++)
            {
                sb.Append("<td").Append(rightAlign[i] ? " class=\"num\"" : "").Append(">").Append(cells[i]).Append("</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table>");
    }

    public static void AppendTotals(StringBuilder sb, IEnumerable<(string Label, string Value, bool Strong)> lines)
    {
        sb.Append("<div class=\"rcpt-totals\">");
        foreach (var (label, value, strong) in lines)
        {
            sb.Append("<div class=\"rcpt-total-row").Append(strong ? " rcpt-total-row--strong" : "").Append("\">");
            sb.Append("<span>").Append(H(label)).Append("</span><span>").Append(value).Append("</span></div>");
        }
        sb.Append("</div>");
    }

    public static void AppendNote(StringBuilder sb, string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        sb.Append("<p class=\"rcpt-note\">").Append(H(text)).Append("</p>");
    }

    public static void AppendFooter(StringBuilder sb, string left, string page = "Page 1 / 1")
    {
        sb.Append("<footer class=\"rcpt-foot\">");
        sb.Append("<span>").Append(H(left)).Append("</span>");
        sb.Append("<span>").Append(H(page)).Append("</span>");
        sb.Append("</footer>");
    }

    public static string DescCell(string title, string? subtitle = null)
    {
        if (string.IsNullOrWhiteSpace(subtitle))
            return $"<span class=\"rcpt-desc__title\">{H(title)}</span>";
        return $"<span class=\"rcpt-desc__title\">{H(title)}</span><span class=\"rcpt-desc__sub\">{H(subtitle)}</span>";
    }

    public const string Css = """
        *{box-sizing:border-box;margin:0;padding:0}
        body{font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Helvetica,Arial,sans-serif;background:#f4f4f5;color:#111;-webkit-print-color-adjust:exact;print-color-adjust:exact}
        .btn-bar{display:flex;gap:10px;justify-content:center;padding:16px;background:#fff;border-bottom:1px solid #e4e4e7}
        .btn-print,.btn-close{padding:10px 24px;font-size:14px;font-weight:500;border-radius:6px;cursor:pointer;border:1px solid #d4d4d8;background:#fff;color:#18181b}
        .btn-print{background:#18181b;color:#fff;border-color:#18181b}
        @page{size:A4 portrait;margin:14mm 16mm}
        .rcpt-sheet{width:210mm;min-height:297mm;margin:20px auto;background:#fff;box-shadow:0 1px 3px rgba(0,0,0,.08)}
        .rcpt-body{padding:18mm 20mm 16mm}
        .rcpt-head{display:flex;justify-content:space-between;align-items:flex-start;gap:24px;margin-bottom:28px}
        .rcpt-head__title{font-size:24px;font-weight:700;letter-spacing:-.03em;line-height:1.1;color:#09090b}
        .rcpt-head__logo{max-height:40px;max-width:120px;object-fit:contain;margin-top:4px}
        .rcpt-docmeta{margin-bottom:32px}
        .rcpt-docmeta__row{display:flex;gap:12px;padding:2px 0;font-size:13px;line-height:1.5}
        .rcpt-docmeta__row dt{min-width:118px;color:#71717a;flex-shrink:0}
        .rcpt-docmeta__row dd{color:#18181b;font-weight:500}
        .rcpt-parties{display:grid;grid-template-columns:1fr 1fr;gap:48px;margin-bottom:36px}
        .rcpt-party h2{font-size:13px;font-weight:600;color:#18181b;margin-bottom:10px}
        .rcpt-party__name{font-size:13px;font-weight:600;color:#18181b;margin-bottom:4px;line-height:1.45}
        .rcpt-party p{font-size:13px;line-height:1.55;color:#52525b;margin:0 0 2px}
        .rcpt-hero{font-size:15px;font-weight:600;color:#18181b;margin:0 0 20px;line-height:1.4;letter-spacing:-.01em}
        .rcpt-summary{margin:0 0 24px;padding:10px 0;border-top:1px solid #e4e4e7;border-bottom:1px solid #e4e4e7}
        .rcpt-summary__row{display:flex;justify-content:space-between;gap:16px;padding:3px 0;font-size:12px;line-height:1.45}
        .rcpt-summary__row dt{color:#71717a;font-weight:400;flex-shrink:0}
        .rcpt-summary__row dd{color:#18181b;font-weight:500;text-align:right;font-variant-numeric:tabular-nums}
        .rcpt-formula{margin:0 0 22px;font-size:11px;color:#71717a;text-align:right;font-variant-numeric:tabular-nums;line-height:1.45}
        .rcpt-section{font-size:12px;font-weight:600;color:#18181b;margin:24px 0 10px}
        .rcpt-table{width:100%;border-collapse:collapse;font-size:13px;margin-bottom:8px}
        .rcpt-table thead th{text-align:left;font-size:11px;font-weight:500;color:#71717a;padding:0 12px 10px 0;border-bottom:2px solid #18181b;vertical-align:bottom}
        .rcpt-table thead th.num{padding-right:0;text-align:right}
        .rcpt-table tbody td{padding:14px 12px 14px 0;border-bottom:1px solid #e4e4e7;vertical-align:top;line-height:1.45;color:#18181b}
        .rcpt-table tbody td.num{padding-right:0;text-align:right;font-variant-numeric:tabular-nums;white-space:nowrap}
        .rcpt-table tbody tr:last-child td{border-bottom:1px solid #d4d4d8}
        .rcpt-desc__title{display:block;font-weight:600;color:#18181b}
        .rcpt-desc__sub{display:block;font-size:12px;color:#71717a;margin-top:2px;font-weight:400}
        .rcpt-totals{width:240px;margin:6px 0 0 auto;font-size:12px}
        .rcpt-total-row{display:flex;justify-content:space-between;align-items:baseline;gap:16px;padding:4px 0;color:#52525b;font-variant-numeric:tabular-nums}
        .rcpt-total-row--strong{margin-top:2px;padding-top:8px;border-top:1px solid #18181b;font-size:12px;font-weight:600;color:#18181b}
        .rcpt-note{margin:20px 0 0;font-size:12px;line-height:1.55;color:#52525b;padding:12px 0;border-top:1px solid #e4e4e7}
        .rcpt-foot{margin-top:48px;padding-top:16px;border-top:1px solid #e4e4e7;display:flex;justify-content:space-between;align-items:center;font-size:11px;color:#a1a1aa;line-height:1.45}
        @media print{
            .no-print{display:none!important}
            body{background:#fff;margin:0;padding:0}
            .rcpt-sheet{margin:0;box-shadow:none;width:auto;min-height:auto}
            .rcpt-body{padding:0}
        }
        """;
}
