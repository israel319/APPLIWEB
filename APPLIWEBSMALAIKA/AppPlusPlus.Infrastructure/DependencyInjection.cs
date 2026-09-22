using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppPlusPlus.Infrastructure.Persistence;
using AppPlusPlus.Infrastructure.Persistence.Repositories;
using AppPlusPlus.Infrastructure.Services;
using AppPlusPlus.Infrastructure.ExternalServices;
using AppPlusPlus.Infrastructure.Identity;
using AppPlusPlus.Application.Interfaces;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.Services.Dashboard;
using AppPlusPlus.Application.Services.Vente;
using AppPlusPlus.Application.Services.Commandes;
using AppPlusPlus.Application.Services.Prestations;
using AppPlusPlus.Application.Services.Finance;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Application.Services.Approvisionnement;
using AppPlusPlus.Application.Services.Inventaire;
using AppPlusPlus.Application.Services.Rapports;
using AppPlusPlus.Application.Services.Shared;
using AppPlusPlus.Infrastructure.QueryServices;
using AppPlusPlus.Infrastructure.QueryServices.Vente;
using AppPlusPlus.Infrastructure.QueryServices.Commandes;
using AppPlusPlus.Infrastructure.QueryServices.Finance;
using AppPlusPlus.Infrastructure.QueryServices.Prestations;

namespace AppPlusPlus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core ──
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ── Repositories ──
        services.AddScoped<ICatalogueRepository, CatalogueRepository>();
        services.AddScoped<IFactureRepository, FactureRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IApproRepository, ApproRepository>();
        services.AddScoped<ICommandeRepository, CommandeRepository>();
        services.AddScoped<ICmdRepository, CmdRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IFinanceRepository, FinanceRepository>();
        services.AddSingleton<IParametresRepository, ParametresRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IDemandeRepository, DemandeRepository>();
        services.AddScoped<IInventaireRepository, InventaireRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<StockMovementService>();
        services.AddScoped<IStockDocumentReversal, StockDocumentReversalService>();
        services.AddScoped<IInternalTransferService, InternalTransferService>();
        services.AddScoped<IStockAvailabilityService, StockAvailabilityService>();
        services.AddScoped<IStockCatalogQueryService, QueryServices.StockCatalogQueryService>();
        services.AddScoped<IApproQueryService, QueryServices.ApproQueryService>();
        services.AddScoped<IStockTransferService, StockTransferService>();
        services.AddScoped<IApproPostingService, ApproPostingService>();
        services.AddScoped<ILivraisonPostingService, LivraisonPostingService>();
        services.AddScoped<ITransformationPostingService, TransformationPostingService>();
        services.AddScoped<IInventairePostingService, InventairePostingService>();

        // ── External Services ──
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<INumberToWordsConverter, NumberToWordsConverter>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── Query Services ──
        services.AddScoped<IVendorSecurityService, QueryServices.VendorSecurityService>();
        services.AddScoped<IModuleLicenseService, QueryServices.ModuleLicenseService>();
        services.AddScoped<IDashboardService, QueryServices.DashboardService>();
        services.AddScoped<IFacturationService, FacturationQueryService>();
        services.AddScoped<IPaymentService, PaymentQueryService>();
        services.AddScoped<ILivraisonService, LivraisonQueryService>();
        services.AddScoped<IClotureService, ClotureQueryService>();
        services.AddScoped<IRapportQueryService, QueryServices.Rapports.RapportQueryService>();
        services.AddScoped<IServiceManagementService, ServiceManagementQueryService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogQueryService>();

        return services;
    }
}
