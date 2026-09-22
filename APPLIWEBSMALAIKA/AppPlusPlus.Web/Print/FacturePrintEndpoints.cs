using AppPlusPlus.Web.Print;

namespace AppPlusPlus.Web;

public static class FacturePrintEndpoints
{
    public static IEndpointRouteBuilder MapFacturePrintEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/vente/facture/print/{id:int}/{autoPrint:bool}", RenderAsync).RequireAuthorization();
        app.MapGet("/vente/facture/print/{id:int}", (int id, FacturePrintHtmlRenderer r, HttpContext ctx) =>
            RenderAsync(id, false, r, ctx)).RequireAuthorization();

        app.MapGet("/services/facturation/imprimer/{id:int}/{autoPrint:bool}", RenderAsync).RequireAuthorization();
        app.MapGet("/services/facturation/imprimer/{id:int}", (int id, FacturePrintHtmlRenderer r, HttpContext ctx) =>
            RenderAsync(id, false, r, ctx)).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RenderAsync(
        int id,
        bool autoPrint,
        FacturePrintHtmlRenderer renderer,
        HttpContext ctx)
    {
        var html = await renderer.RenderAsync(id, autoPrint, ctx.Request.Path.Value ?? "");
        return html is null
            ? Results.Content($"<html><body><p style=\"color:red;padding:2rem\">Facture N°{id} introuvable.</p></body></html>", "text/html; charset=utf-8")
            : Results.Content(html, "text/html; charset=utf-8");
    }
}
