namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Catalogue hiérarchique des permissions : Onglet → Sous-onglet → Actions.
/// Les codes sont persistés dans T_Activities (ex. articles.approvisionnement.write).
/// </summary>
public static class PermissionCatalog
{
    public enum NodeKind { Tab, SubTab, Action }

    public sealed record PermissionNode(
        string Code,
        string Label,
        NodeKind Kind,
        string? ParentCode,
        string? LegacyModule,
        string? RoutePrefix);

    public static IReadOnlyList<PermissionNode> All { get; } = Build();

    public static IEnumerable<PermissionNode> Tabs =>
        All.Where(n => n.Kind == NodeKind.Tab);

    public static IEnumerable<PermissionNode> ChildrenOf(string? parentCode) =>
        All.Where(n => string.Equals(n.ParentCode, parentCode, StringComparison.OrdinalIgnoreCase));

    public static PermissionNode? Find(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var norm = NormalizeCode(code);
        return All.FirstOrDefault(n => n.Code == norm);
    }

    public static string NormalizeCode(string code) =>
        code.Trim().ToLowerInvariant().Replace(' ', '.').Replace('_', '.');

    /// <summary>Tous les codes du catalogue — preset accès complet (administrateur).</summary>
    public static HashSet<string> BuildFullAccessGrants() =>
        All.Select(n => NormalizeCode(n.Code))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Libellé unique pour persistance en base (contrainte FonctionId + Description).</summary>
    public static string GetStorageLabel(string code)
    {
        var node = Find(code);
        if (node is null) return NormalizeCode(code);

        var parts = new List<string> { node.Label };
        var parentCode = node.ParentCode;
        while (!string.IsNullOrEmpty(parentCode))
        {
            var parent = Find(parentCode);
            if (parent is null) break;
            parts.Insert(0, parent.Label);
            parentCode = parent.ParentCode;
        }

        var label = string.Join(" · ", parts);
        return label.Length <= 150 ? label : label[..150];
    }

    public static bool IsGranted(IReadOnlySet<string> grants, string code)
    {
        var norm = NormalizeCode(code);
        return grants.Contains(norm);
    }

    /// <summary>Accès onglet : grant explicite ou sous-élément accordé.</summary>
    public static bool CanAccessTab(IReadOnlySet<string> grants, string tabCode)
    {
        var norm = NormalizeCode(tabCode);
        if (grants.Contains(norm)) return true;
        var prefix = norm + ".";
        return grants.Any(g => g.StartsWith(prefix, StringComparison.Ordinal));
    }

    /// <summary>
    /// Accès sous-onglet : grant explicite du sous-onglet ou d'une action en dessous.
    /// Cocher l'onglet parent seul n'ouvre PAS tous les sous-onglets.
    /// </summary>
    public static bool CanAccessSubTab(IReadOnlySet<string> grants, string subTabCode)
    {
        var norm = NormalizeCode(subTabCode);
        if (grants.Contains(norm)) return true;

        var prefix = norm + ".";
        return grants.Any(g => g.StartsWith(prefix, StringComparison.Ordinal));
    }

    /// <summary>Onglet parent coché seul → tous les sous-onglets (accès complet).</summary>
    public static bool IsFullTabGrant(IReadOnlySet<string> grants, string tabCode)
    {
        if (!IsGranted(grants, tabCode)) return false;
        var prefix = tabCode + ".";
        return !grants.Any(g => g.StartsWith(prefix, StringComparison.Ordinal));
    }

    public static bool CanAccessSubTabOrFullTab(IReadOnlySet<string> grants, string subTabCode)
    {
        if (CanAccessSubTab(grants, subTabCode)) return true;
        var node = Find(subTabCode);
        return node?.ParentCode is string parent && IsFullTabGrant(grants, parent);
    }

    /// <summary>
    /// Accès Inventaire seul (sans Catalogue / Appro / etc.) → la page /stock doit rediriger vers /stock/inventaire.
    /// </summary>
    public static bool ShouldRedirectStockToInventaire(IReadOnlySet<string> grants) =>
        CanAccessTab(grants, "articles")
        && CanAccessSubTabOrFullTab(grants, "articles.inventaire")
        && !CanAccessSubTabOrFullTab(grants, "articles.catalogue")
        && !CanAccessSubTabOrFullTab(grants, "articles.approvisionnement")
        && !CanAccessSubTabOrFullTab(grants, "articles.transformation")
        && !CanAccessSubTabOrFullTab(grants, "articles.localisations");

    public static bool CanPerformAction(IReadOnlySet<string> grants, string actionCode)
    {
        if (IsGranted(grants, actionCode)) return true;

        var node = Find(actionCode);
        if (node?.Kind != NodeKind.Action || node.ParentCode is null)
            return false;

        if (!CanAccessSubTab(grants, node.ParentCode))
            return false;

        return actionCode.EndsWith(".read", StringComparison.Ordinal);
    }

    public static string? FirstAccessibleArticlesRoute(IReadOnlySet<string> grants)
    {
        if (!CanAccessTab(grants, "articles")) return null;

        foreach (var (subCode, route) in new (string Code, string Route)[]
        {
            ("articles.catalogue", "/stock"),
            ("articles.approvisionnement", "/stock"),
            ("articles.transformation", "/stock"),
            ("articles.localisations", "/stock"),
            ("articles.inventaire", "/stock/inventaire")
        })
        {
            if (CanAccessSubTabOrFullTab(grants, subCode))
                return route;
        }

        return null;
    }

    public static string? FirstAccessibleVenteRoute(IReadOnlySet<string> grants)
    {
        if (!CanAccessTab(grants, "vente")) return null;
        if (CanAccessSubTabOrFullTab(grants, "vente.facturation")) return "/vente";
        if (CanAccessSubTabOrFullTab(grants, "vente.livraisons")) return "/vente";
        return null;
    }

    public static string? FirstAccessibleCommandesRoute(IReadOnlySet<string> grants)
    {
        if (!CanAccessTab(grants, "commandes")) return null;
        foreach (var sub in new[] { "commandes.internes", "commandes.demandes", "commandes.clients", "commandes.livraisons" })
        {
            if (CanAccessSubTabOrFullTab(grants, sub))
                return "/commandes";
        }
        return null;
    }

    public static string? FirstAccessibleAdminRoute(IReadOnlySet<string> grants)
    {
        if (!CanAccessTab(grants, "administration")) return null;
        foreach (var sub in new[] {
            "administration.utilisateurs", "administration.roles",
            "administration.fournisseurs", "administration.localisations" })
        {
            if (CanAccessSubTabOrFullTab(grants, sub))
                return "/administration";
        }
        return null;
    }

    /// <summary>Première route accessible selon les grants (équivalent menu principal).</summary>
    public static string FirstAccessibleRoute(IReadOnlySet<string> grants)
    {
        if (CanAccessTab(grants, "dashboard")) return "/dashboard";
        var vente = FirstAccessibleVenteRoute(grants);
        if (vente is not null) return vente;
        var articles = FirstAccessibleArticlesRoute(grants);
        if (articles is not null) return articles;
        var commandes = FirstAccessibleCommandesRoute(grants);
        if (commandes is not null) return commandes;
        if (CanAccessTab(grants, "grh")) return "/grh";
        if (CanAccessTab(grants, "rapports")) return "/rapports";
        if (CanAccessTab(grants, "services")) return "/services";
        var admin = FirstAccessibleAdminRoute(grants);
        if (admin is not null) return admin;
        if (CanAccessSubTabOrFullTab(grants, "parametres.general")) return "/settings";
        return "/profile";
    }

    public static string TabHubRoute(string tabCode, IReadOnlySet<string> grants) => tabCode switch
    {
        "dashboard" => CanAccessTab(grants, "dashboard") ? "/dashboard" : FirstAccessibleRoute(grants),
        "vente" => FirstAccessibleVenteRoute(grants) ?? FirstAccessibleRoute(grants),
        "articles" => FirstAccessibleArticlesRoute(grants) ?? FirstAccessibleRoute(grants),
        "commandes" => FirstAccessibleCommandesRoute(grants) ?? FirstAccessibleRoute(grants),
        "administration" => FirstAccessibleAdminRoute(grants) ?? FirstAccessibleRoute(grants),
        "grh" => CanAccessTab(grants, "grh") ? "/grh" : FirstAccessibleRoute(grants),
        "rapports" => CanAccessTab(grants, "rapports") ? "/rapports" : FirstAccessibleRoute(grants),
        "services" => CanAccessTab(grants, "services") ? "/services" : FirstAccessibleRoute(grants),
        "parametres" => CanAccessSubTabOrFullTab(grants, "parametres.general") ? "/settings" : FirstAccessibleRoute(grants),
        _ => FirstAccessibleRoute(grants)
    };

    public static string NormalizeRoutePath(string path)
    {
        var raw = (path ?? "/").Split('?')[0].Trim();
        if (raw.Length == 0) return "/";
        return raw.TrimEnd('/').ToLowerInvariant();
    }

    /// <summary>
    /// Retourne la route cible si autorisée, sinon la première route accessible du même module, sinon l'accueil permis.
    /// </summary>
    public static string ResolveSafeRoute(IReadOnlySet<string> grants, string targetRoute)
    {
        var path = NormalizeRoutePath(targetRoute);
        if (CanAccessRoutePath(grants, path))
        {
            var qIdx = targetRoute.IndexOf('?');
            return qIdx >= 0 ? targetRoute[..qIdx] + targetRoute[qIdx..] : targetRoute;
        }

        if (path.StartsWith("/vente") || path.StartsWith("/gestion/vente"))
            return FirstAccessibleVenteRoute(grants) ?? FirstAccessibleRoute(grants);
        if (path.StartsWith("/stock") || path.StartsWith("/gestion/article")
            || path.StartsWith("/gestion/approvisionnement") || path.StartsWith("/gestion/transformation"))
            return FirstAccessibleArticlesRoute(grants) ?? FirstAccessibleRoute(grants);
        if (path.StartsWith("/commandes") || path.StartsWith("/gestion/commande") || path.StartsWith("/gestion/client"))
            return FirstAccessibleCommandesRoute(grants) ?? FirstAccessibleRoute(grants);
        if (path.StartsWith("/administration") || path.StartsWith("/admin/") || path.StartsWith("/localisation"))
            return FirstAccessibleAdminRoute(grants) ?? FirstAccessibleRoute(grants);
        if (path.StartsWith("/dashboard"))
            return FirstAccessibleRoute(grants);
        if (path.StartsWith("/rapports") || path.StartsWith("/rapport/"))
            return CanAccessTab(grants, "rapports") ? "/rapports" : FirstAccessibleRoute(grants);
        if (path.StartsWith("/grh"))
            return CanAccessTab(grants, "grh") ? "/grh" : FirstAccessibleRoute(grants);
        if (path.StartsWith("/services"))
            return CanAccessTab(grants, "services") ? "/services" : FirstAccessibleRoute(grants);
        if (path.StartsWith("/settings"))
            return CanAccessSubTabOrFullTab(grants, "parametres.general") ? "/settings" : FirstAccessibleRoute(grants);

        return FirstAccessibleRoute(grants);
    }

    /// <summary>Vérifie si un chemin est accessible en mode hiérarchique strict.</summary>
    public static bool CanAccessRoutePath(IReadOnlySet<string> grants, string normalizedPath)
    {
        var path = NormalizeRoutePath(normalizedPath);

        if (path.StartsWith("/login") || path.StartsWith("/profile")
            || path.StartsWith("/client/") || path.StartsWith("/catalogue"))
            return true;

        if (path.StartsWith("/administration") || path.StartsWith("/admin/"))
            return HasAnySubTabInTab(grants, "administration");

        if (path.StartsWith("/localisation/"))
            return CanAccessSubTabOrFullTab(grants, "administration.localisations");

        if (path.StartsWith("/settings"))
            return CanAccessSubTabOrFullTab(grants, "parametres.general");

        if (path.StartsWith("/rapports") || path.StartsWith("/rapport/"))
            return CanAccessTab(grants, "rapports");

        if (path.StartsWith("/grh"))
            return CanAccessTab(grants, "grh");

        if (path.StartsWith("/services"))
            return CanAccessTab(grants, "services");

        if (path.StartsWith("/vente/facturation") || path.StartsWith("/vente/facture/print"))
            return CanAccessSubTabOrFullTab(grants, "vente.facturation");

        if (path.StartsWith("/vente/livraison") || path.StartsWith("/vente/payement"))
            return CanAccessSubTabOrFullTab(grants, "vente.livraisons")
                || CanAccessSubTabOrFullTab(grants, "vente.facturation");

        if (path.StartsWith("/vente"))
            return HasAnySubTabInTab(grants, "vente");

        if (path.StartsWith("/stock/inventaire"))
            return CanAccessSubTabOrFullTab(grants, "articles.inventaire");

        if (path.StartsWith("/gestion/approvisionnement") || path.StartsWith("/stock/appro"))
            return CanAccessSubTabOrFullTab(grants, "articles.approvisionnement");

        if (path.StartsWith("/gestion/transformation") || path.StartsWith("/stock/transformation"))
            return CanAccessSubTabOrFullTab(grants, "articles.transformation");

        if (path.StartsWith("/gestion/article"))
            return CanAccessSubTabOrFullTab(grants, "articles.catalogue");

        if (path == "/stock" || path.StartsWith("/stock/"))
            return HasAnyArticlesSubTabAccess(grants);

        if (path.StartsWith("/gestion/commande") || path.StartsWith("/commandes/interne"))
            return CanAccessSubTabOrFullTab(grants, "commandes.internes");

        if (path.StartsWith("/gestion/client") || path.StartsWith("/gestion/vente")
            || path.StartsWith("/commandes/client") || path.StartsWith("/commandes/nouveau"))
            return CanAccessSubTabOrFullTab(grants, "commandes.clients");

        if (path.StartsWith("/commandes"))
            return HasAnySubTabInTab(grants, "commandes");

        if (path == "/dashboard")
            return CanAccessTab(grants, "dashboard");

        return false;
    }

    public static bool HasAnyArticlesSubTabAccess(IReadOnlySet<string> grants) =>
        IsFullTabGrant(grants, "articles")
        || CanAccessSubTabOrFullTab(grants, "articles.catalogue")
        || CanAccessSubTabOrFullTab(grants, "articles.approvisionnement")
        || CanAccessSubTabOrFullTab(grants, "articles.transformation")
        || CanAccessSubTabOrFullTab(grants, "articles.localisations")
        || CanAccessSubTabOrFullTab(grants, "articles.inventaire");

    /// <summary>Onglet coché si grant direct ou descendant (articles.inventaire.* → onglet articles).</summary>
    public static bool IsNodeGranted(IReadOnlySet<string> grants, string code)
    {
        var norm = NormalizeCode(code);
        if (grants.Contains(norm)) return true;
        var prefix = norm + ".";
        return grants.Any(g => g.StartsWith(prefix, StringComparison.Ordinal));
    }

    public static bool HasAnySubTabInTab(IReadOnlySet<string> grants, string tabCode) =>
        IsFullTabGrant(grants, tabCode)
        || ChildrenOf(tabCode)
            .Where(n => n.Kind == NodeKind.SubTab)
            .Any(n => CanAccessSubTab(grants, n.Code));

    public static IEnumerable<string> LegacyModulesFromGrants(IReadOnlySet<string> grants)
    {
        foreach (var node in All.Where(n => !string.IsNullOrEmpty(n.LegacyModule)))
        {
            if (IsGranted(grants, node.Code) || HasGrantedChild(grants, node.Code))
                yield return node.LegacyModule!;
        }
    }

    public static void ApplyActionGrant(
        IReadOnlySet<string> grants,
        string actionCode,
        HashSet<string> read,
        HashSet<string> write,
        HashSet<string> delete)
    {
        if (!IsGranted(grants, actionCode)) return;

        var node = Find(actionCode);
        if (node?.LegacyModule is null) return;

        var module = AppFunctions.Normalize(node.LegacyModule);
        if (actionCode.EndsWith(".read", StringComparison.Ordinal))
            read.Add(module);
        else if (actionCode.EndsWith(".write", StringComparison.Ordinal))
        {
            read.Add(module);
            write.Add(module);
        }
        else if (actionCode.EndsWith(".delete", StringComparison.Ordinal))
        {
            read.Add(module);
            write.Add(module);
            delete.Add(module);
        }
    }

    static bool HasGrantedChild(IReadOnlySet<string> grants, string code)
    {
        var prefix = NormalizeCode(code) + ".";
        return grants.Any(g => g.StartsWith(prefix, StringComparison.Ordinal));
    }

    static List<PermissionNode> Build()
    {
        var nodes = new List<PermissionNode>();

        void Tab(string code, string label, string? legacy = null, string? route = null) =>
            nodes.Add(new PermissionNode(code, label, NodeKind.Tab, null, legacy, route));

        void Sub(string code, string label, string parent, string? legacy = null) =>
            nodes.Add(new PermissionNode(code, label, NodeKind.SubTab, parent, legacy, null));

        void Act(string code, string label, string parent, string? legacy = null) =>
            nodes.Add(new PermissionNode(code, label, NodeKind.Action, parent, legacy, null));

        // ── Onglets principaux ──
        Tab("dashboard", "Tableau de bord", AppFunctions.Dashboard, "/dashboard");
        Tab("vente", "Ventes", AppFunctions.Vente, "/vente");
        Tab("articles", "Articles", AppFunctions.Stock, "/stock");
        Tab("commandes", "Commandes", AppFunctions.CommandesInternes, "/commandes");
        Tab("services", "Prestations", AppFunctions.Services, "/services");
        Tab("grh", "GRH", AppFunctions.GRH, "/grh");
        Tab("rapports", "Rapports", AppFunctions.Rapports, "/rapports");
        Tab("administration", "Administration", AppFunctions.Administration, "/administration");
        Tab("parametres", "Paramètres", AppFunctions.Parametres, "/settings");

        // ── Ventes ──
        Sub("vente.facturation", "Facturation", "vente", AppFunctions.Facturation);
        Act("vente.facturation.read", "Consulter", "vente.facturation", AppFunctions.Facturation);
        Act("vente.facturation.write", "Créer / modifier", "vente.facturation", AppFunctions.Facturation);
        Act("vente.facturation.delete", "Supprimer / annuler", "vente.facturation", AppFunctions.Facturation);

        Sub("vente.livraisons", "Livraisons", "vente", AppFunctions.Livraison);
        Act("vente.livraisons.read", "Consulter", "vente.livraisons", AppFunctions.Livraison);
        Act("vente.livraisons.write", "Enregistrer", "vente.livraisons", AppFunctions.Livraison);

        // ── Articles ──
        Sub("articles.catalogue", "Catalogue", "articles", AppFunctions.Stock);
        Act("articles.catalogue.read", "Consulter", "articles.catalogue", AppFunctions.Stock);
        Act("articles.catalogue.write", "Créer / modifier", "articles.catalogue", AppFunctions.Stock);
        Act("articles.catalogue.delete", "Supprimer", "articles.catalogue", AppFunctions.Stock);

        Sub("articles.approvisionnement", "Approvisionnement", "articles", AppFunctions.Approvisionnement);
        Act("articles.approvisionnement.read", "Consulter", "articles.approvisionnement", AppFunctions.Approvisionnement);
        Act("articles.approvisionnement.write", "Créer / modifier", "articles.approvisionnement", AppFunctions.Approvisionnement);
        Act("articles.approvisionnement.delete", "Annuler", "articles.approvisionnement", AppFunctions.Approvisionnement);

        Sub("articles.transformation", "Transformation", "articles", AppFunctions.Stock);
        Act("articles.transformation.read", "Consulter", "articles.transformation", AppFunctions.Stock);
        Act("articles.transformation.write", "Enregistrer", "articles.transformation", AppFunctions.Stock);

        Sub("articles.localisations", "Localisations", "articles", AppFunctions.Stock);
        Act("articles.localisations.read", "Consulter", "articles.localisations", AppFunctions.Stock);
        Act("articles.localisations.write", "Gérer", "articles.localisations", AppFunctions.Stock);

        Sub("articles.inventaire", "Inventaire magasin", "articles", AppFunctions.Stock);
        Act("articles.inventaire.read", "Consulter", "articles.inventaire", AppFunctions.Stock);
        Act("articles.inventaire.write", "Saisir / clôturer", "articles.inventaire", AppFunctions.Stock);

        // ── Commandes ──
        Sub("commandes.internes", "Commandes internes", "commandes", AppFunctions.CommandesInternes);
        Act("commandes.internes.read", "Consulter", "commandes.internes", AppFunctions.CommandesInternes);
        Act("commandes.internes.write", "Créer / réceptionner", "commandes.internes", AppFunctions.CommandesInternes);

        Sub("commandes.clients", "Commandes clients", "commandes", AppFunctions.CommandesClients);
        Act("commandes.clients.read", "Consulter", "commandes.clients", AppFunctions.CommandesClients);
        Act("commandes.clients.write", "Gérer", "commandes.clients", AppFunctions.CommandesClients);

        Sub("commandes.demandes", "Demandes magasin", "commandes", AppFunctions.CommandesInternes);
        Act("commandes.demandes.read", "Consulter", "commandes.demandes", AppFunctions.CommandesInternes);
        Act("commandes.demandes.write", "Traiter", "commandes.demandes", AppFunctions.CommandesInternes);

        Sub("commandes.livraisons", "Livraisons commandes", "commandes", AppFunctions.Livraison);
        Act("commandes.livraisons.read", "Consulter", "commandes.livraisons", AppFunctions.Livraison);
        Act("commandes.livraisons.write", "Enregistrer", "commandes.livraisons", AppFunctions.Livraison);

        // ── Administration ──
        Sub("administration.utilisateurs", "Utilisateurs", "administration", AppFunctions.Administration);
        Act("administration.utilisateurs.read", "Consulter", "administration.utilisateurs", AppFunctions.Administration);
        Act("administration.utilisateurs.write", "Créer / modifier", "administration.utilisateurs", AppFunctions.Administration);
        Act("administration.utilisateurs.delete", "Désactiver", "administration.utilisateurs", AppFunctions.Administration);

        Sub("administration.roles", "Rôles & permissions", "administration", AppFunctions.Administration);
        Act("administration.roles.read", "Consulter", "administration.roles", AppFunctions.Administration);
        Act("administration.roles.write", "Gérer", "administration.roles", AppFunctions.Administration);

        Sub("administration.fournisseurs", "Fournisseurs", "administration", AppFunctions.Administration);
        Act("administration.fournisseurs.read", "Consulter", "administration.fournisseurs", AppFunctions.Administration);
        Act("administration.fournisseurs.write", "Gérer", "administration.fournisseurs", AppFunctions.Administration);

        Sub("administration.localisations", "Localisations", "administration", AppFunctions.Administration);
        Act("administration.localisations.read", "Consulter", "administration.localisations", AppFunctions.Administration);
        Act("administration.localisations.write", "Gérer", "administration.localisations", AppFunctions.Administration);

        // ── Paramètres ──
        Sub("parametres.general", "Configuration", "parametres", AppFunctions.Parametres);
        Act("parametres.general.read", "Consulter", "parametres.general", AppFunctions.Parametres);
        Act("parametres.general.write", "Modifier", "parametres.general", AppFunctions.Parametres);

        // ── GRH / Rapports / Services (actions directes) ──
        foreach (var tab in new[] { "grh", "rapports", "services" })
        {
            Act($"{tab}.read", "Consulter", tab, tab switch
            {
                "grh" => AppFunctions.GRH,
                "rapports" => AppFunctions.Rapports,
                "services" => AppFunctions.Services,
                _ => null
            });
            Act($"{tab}.write", "Gérer", tab, tab switch
            {
                "grh" => AppFunctions.GRH,
                "rapports" => AppFunctions.Rapports,
                "services" => AppFunctions.Services,
                _ => null
            });
        }

        return nodes;
    }
}
