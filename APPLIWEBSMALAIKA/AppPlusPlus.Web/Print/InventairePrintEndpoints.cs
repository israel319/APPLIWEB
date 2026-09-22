namespace AppPlusPlus.Web.Print;

public static class InventairePrintEndpoints
{
    public static IEndpointRouteBuilder MapInventairePrintEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/stock/inventaire/reception/print/{id:int}/{autoPrint:bool}", RenderReceptionAsync).RequireAuthorization();
        app.MapGet("/stock/inventaire/reception/print/{id:int}", (int id, InventaireReceptionPrintHtmlRenderer r) =>
            RenderReceptionAsync(id, false, r)).RequireAuthorization();

        app.MapGet("/stock/inventaire/session/print/{id:int}/{autoPrint:bool}", RenderSessionAsync).RequireAuthorization();
        app.MapGet("/stock/inventaire/session/print/{id:int}", (int id, InventaireSessionPrintHtmlRenderer r) =>
            RenderSessionAsync(id, false, r)).RequireAuthorization();

        return app;
    }

    static async Task<IResult> RenderReceptionAsync(int id, bool autoPrint, InventaireReceptionPrintHtmlRenderer renderer)
    {
        var html = await renderer.RenderAsync(id, autoPrint);
        return html is null
            ? NotFound("Réception introuvable.")
            : Results.Content(html, "text/html; charset=utf-8");
    }

    static async Task<IResult> RenderSessionAsync(int id, bool autoPrint, InventaireSessionPrintHtmlRenderer renderer)
    {
        var html = await renderer.RenderAsync(id, autoPrint);
        return html is null
            ? NotFound("Session introuvable.")
            : Results.Content(html, "text/html; charset=utf-8");
    }

    static IResult NotFound(string message)
        => Results.Content($"<html><body><p style=\"color:red;padding:2rem;font-family:sans-serif\">{message}</p></body></html>", "text/html; charset=utf-8");
}
