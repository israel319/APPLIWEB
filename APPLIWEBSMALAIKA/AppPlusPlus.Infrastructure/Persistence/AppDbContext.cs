using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Entities.Stock;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Domain.Entities.CommandesInternes;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Domain.Entities.Fournisseurs;
using AppPlusPlus.Domain.Entities.Parametres;
using AppPlusPlus.Domain.Entities.Shared;
using AppPlusPlus.Domain.Entities.Prestations;
using AppPlusPlus.Domain.Entities.Demandes;
using AppPlusPlus.Domain.Entities.Inventaire;

namespace AppPlusPlus.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Tables de référence ──
    public DbSet<Localisation> Localisations => Set<Localisation>();
    public DbSet<ArticleType> ArticleTypes => Set<ArticleType>();
    public DbSet<ArticleMarque> ArticleMarques => Set<ArticleMarque>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<Mesure> Mesures => Set<Mesure>();
    public DbSet<Money> Moneys => Set<Money>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<StatusFactDetail> StatusFactDetails => Set<StatusFactDetail>();
    public DbSet<StatusCmd> StatusCmds => Set<StatusCmd>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Fonction> Fonctions => Set<Fonction>();
    public DbSet<Reduction> Reductions => Set<Reduction>();
    public DbSet<ExpenseSource> ExpenseSources => Set<ExpenseSource>();
    public DbSet<SupplierService> SupplierServices => Set<SupplierService>();

    // ── Entités métier ──
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerType> CustomerTypes => Set<CustomerType>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserFonction> UserFonctions => Set<UserFonction>();
    public DbSet<UserLocalisation> UserLocalisations => Set<UserLocalisation>();
    public DbSet<UserCaisse> UserCaisses => Set<UserCaisse>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<RoleActivity> RoleActivities => Set<RoleActivity>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    // ── Facturation / Commandes / Livraisons ──
    public DbSet<Fact> Facts => Set<Fact>();
    public DbSet<FactDetail> FactDetails => Set<FactDetail>();
    public DbSet<Cmd> Cmds => Set<Cmd>();
    public DbSet<CmdDetail> CmdDetails => Set<CmdDetail>();
    public DbSet<Commande> Commandes => Set<Commande>();
    public DbSet<CommandeDetail> CommandeDetails => Set<CommandeDetail>();
    public DbSet<Livraison> Livraisons => Set<Livraison>();
    public DbSet<LivraisonDetail> LivraisonDetails => Set<LivraisonDetail>();

    // ── Approvisionnement / Finance ──
    public DbSet<Appro> Appros => Set<Appro>();
    public DbSet<ApproDetail> ApproDetails => Set<ApproDetail>();
    public DbSet<ApproExpense> ApproExpenses => Set<ApproExpense>();
    public DbSet<Transformation> Transformations => Set<Transformation>();
    public DbSet<Taux> TauxChanges => Set<Taux>();
    public DbSet<Periode> Periodes => Set<Periode>();
    public DbSet<Caisse> Caisses => Set<Caisse>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Versement> Versements => Set<Versement>();

    // ── Stock / Mouvements ──
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<MouvementStock> MouvementsStock => Set<MouvementStock>();

    // ── Paramètres application ──
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<ShopProfile> ShopProfiles => Set<ShopProfile>();
    public DbSet<InvoiceTemplate> InvoiceTemplates => Set<InvoiceTemplate>();

    // ── Licences modules ──
    public DbSet<ModuleLicense> ModuleLicenses => Set<ModuleLicense>();

    // ── Services & Prestations ──
    public DbSet<ServiceProject> ServiceProjects => Set<ServiceProject>();
    public DbSet<ServiceTask> ServiceTasks => Set<ServiceTask>();
    public DbSet<ServiceTimesheet> ServiceTimesheets => Set<ServiceTimesheet>();
    public DbSet<ServiceDeliverable> ServiceDeliverables => Set<ServiceDeliverable>();
    public DbSet<ServiceCatalogItem> ServiceCatalogItems => Set<ServiceCatalogItem>();

    // ── Demandes génériques ──
    public DbSet<Demande> Demandes => Set<Demande>();
    public DbSet<DemandeDetail> DemandeDetails => Set<DemandeDetail>();

    // ── Inventaire magasin ──
    public DbSet<InventaireMagasin> InventairesMagasin => Set<InventaireMagasin>();
    public DbSet<InventaireReception> InventaireReceptions => Set<InventaireReception>();
    public DbSet<InventaireReceptionDetail> InventaireReceptionDetails => Set<InventaireReceptionDetail>();
    public DbSet<InventaireVente> InventaireVentes => Set<InventaireVente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly,
            type => type.Namespace?.Contains(".GRH.") != true);
    }
}
