using MudBlazor;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppPlusPlus.Web.Components;
using AppPlusPlus.Infrastructure;
using AppPlusPlus.Infrastructure.Persistence;
using AppPlusPlus.Application;
using AppPlusPlus.Application.Services.Parametres;
using AppPlusPlus.Application.Services.Administration;
using AppPlusPlus.Application.Services.Clients;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Web;
using AppPlusPlus.Web.Print;
using AppPlusPlus.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var appLogPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "app.log");
builder.Logging.AddProvider(new AppFileLoggerProvider(appLogPath));

// Clés antiforgie/cookies persistantes (obligatoire sur hébergement mutualisé IIS).
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("AppPlusPlus.Web");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<HttpConnectionDispatcherOptions>(options =>
{
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddGrhServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddMemoryCache();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 6000;
    config.SnackbarConfiguration.HideTransitionDuration = 250;
    config.SnackbarConfiguration.ShowTransitionDuration = 250;
});
builder.Services.AddScoped<HomeCatalogCoordinator>();
builder.Services.AddScoped<IClientCartService, ClientCartService>();
builder.Services.AddScoped<IPublicCatalogPricingService, PublicCatalogPricingService>();
builder.Services.AddScoped<ICatalogRefreshService, CatalogRefreshService>();
builder.Services.AddScoped<UiNavigationGate>();
builder.Services.AddScoped<FacturePrintHtmlRenderer>();
builder.Services.AddScoped<InventaireReceptionPrintHtmlRenderer>();
builder.Services.AddScoped<InventaireSessionPrintHtmlRenderer>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Blazor Server / SignalR — timeouts adaptés à l'hébergement mutualisé (SmarterASP, etc.)
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(3);
    })
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(15);
        options.HandshakeTimeout = TimeSpan.FromSeconds(60);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.MaximumReceiveMessageSize = 128 * 1024;
    });

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var settings = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();
    await settings.LoadAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Chargement T_AppSettings ignoré au démarrage.");
}

try
{
    using var scope = app.Services.CreateScope();
    var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
    await roleService.InitializeDefaultPermissionsAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Initialisation des permissions ignorée au démarrage.");
}

if (app.Environment.IsDevelopment())
{
    // Development-only middleware
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method)
        && context.Request.Path.Equals("/login", StringComparison.OrdinalIgnoreCase)
        && context.User.Identity?.IsAuthenticated == true)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapFacturePrintEndpoints();
app.MapInventairePrintEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/login", async (
    [FromForm] string login,
    [FromForm] string password,
    [FromForm] string? returnUrl,
    IDbContextFactory<AppDbContext> dbFactory,
    HttpContext httpContext) =>
{
    var safeReturnUrl = "/dashboard";
    if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/'))
    {
        safeReturnUrl = returnUrl;
    }

    await using var ctx = await dbFactory.CreateDbContextAsync();
    var user = await ctx.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Login == login && u.Password == password && u.Activated == true);

    if (user is null)
    {
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(safeReturnUrl)}");
    }

    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Login),
        new(ClaimTypes.Name, user.Name ?? user.Login),
        new("login", user.Login),
        new("roleId", (user.RoleId ?? 0).ToString())
    };

    if (!string.IsNullOrWhiteSpace(user.Role?.DescriptionRole))
    {
        claims.Add(new Claim(ClaimTypes.Role, user.Role.DescriptionRole));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties { IsPersistent = true });

    return Results.Redirect(safeReturnUrl);
}).AllowAnonymous().DisableAntiforgery();

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).AllowAnonymous();

app.MapPost("/auth/client/login", async (
    [FromForm] string contact,
    [FromForm] string? returnUrl,
    ICustomerService customerService,
    HttpContext httpContext) =>
{
    var safeReturnUrl = "/client/panier";
    if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/'))
        safeReturnUrl = returnUrl;

    var customer = await customerService.GetByContactAsync(contact);
    if (customer is null)
    {
        return Results.Redirect(
            $"/auth/connexion?error=unknown&returnUrl={Uri.EscapeDataString(safeReturnUrl)}");
    }

    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await SignInClientAsync(httpContext, customer);
    return Results.Redirect(safeReturnUrl);
}).AllowAnonymous().DisableAntiforgery();

app.MapPost("/auth/client/register", async (
    [FromForm] string name,
    [FromForm] string contact,
    [FromForm] string? adress,
    [FromForm] string? returnUrl,
    ICustomerService customerService,
    HttpContext httpContext) =>
{
    var safeReturnUrl = "/client/panier";
    if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/'))
        safeReturnUrl = returnUrl;

    try
    {
        var customer = await customerService.RegisterOnlineAsync(name, contact, adress);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await SignInClientAsync(httpContext, customer);
        return Results.Redirect(safeReturnUrl);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Redirect(
            $"/client/inscription?error={Uri.EscapeDataString(ex.Message)}&returnUrl={Uri.EscapeDataString(safeReturnUrl)}");
    }
}).AllowAnonymous().DisableAntiforgery();

app.MapGet("/auth/client/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).AllowAnonymous();

app.MapGet("/api/keepalive", () => Results.NoContent()).AllowAnonymous();

app.MapGet("/catalog/img/a/{id}", async (string id, IDbContextFactory<AppDbContext> dbFactory) =>
{
    await using var ctx = await dbFactory.CreateDbContextAsync();
    var stored = await ctx.Articles.AsNoTracking()
        .Where(a => a.IdArticle == id)
        .Select(a => a.ImageArticle)
        .FirstOrDefaultAsync();
    return ServeCatalogImage(stored);
}).AllowAnonymous();

app.MapGet("/catalog/img/c/{id:int}", async (int id, IDbContextFactory<AppDbContext> dbFactory) =>
{
    await using var ctx = await dbFactory.CreateDbContextAsync();
    var stored = await ctx.ArticleCategories.AsNoTracking()
        .Where(c => c.IdCategory == id)
        .Select(c => c.ImageCategory)
        .FirstOrDefaultAsync();
    return ServeCatalogImage(stored);
}).AllowAnonymous();

app.Run();

static IResult ServeCatalogImage(string? stored)
{
    if (string.IsNullOrWhiteSpace(stored))
        return Results.NotFound();

    if (stored.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
    {
        var comma = stored.IndexOf(',');
        if (comma < 0)
            return Results.BadRequest();

        var contentType = stored["data:".Length..comma].Split(';')[0];
        try
        {
            var bytes = Convert.FromBase64String(stored[(comma + 1)..]);
            return Results.File(bytes, contentType, enableRangeProcessing: true);
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    var path = stored.StartsWith('/') ? stored : $"/{stored}";
    return Results.Redirect(path);
}

static async Task SignInClientAsync(HttpContext httpContext, AppPlusPlus.Domain.Entities.Clients.Customer customer)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
        new(ClaimTypes.Name, customer.CustomerName ?? customer.Contact ?? "Client"),
        new(ClaimTypes.Role, ClientAuth.Role),
        new(ClientAuth.CustomerIdClaim, customer.CustomerId.ToString()),
        new(ClientAuth.ContactClaim, customer.Contact ?? "")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties { IsPersistent = true });
}
