/*
  ═══════════════════════════════════════════════════════════════════════════════
  DÉPLOIEMENT COMPLET — Permissions hiérarchiques + enregistrement utilisateurs
  ═══════════════════════════════════════════════════════════════════════════════

  À exécuter UNE SEULE FOIS (ou autant que nécessaire — idempotent) dans SSMS.

  1. Décommentez la ligne USE ci-dessous (base Glomyra production).
  2. Exécutez TOUT le script (F5) — ne lancez pas seulement la fin du fichier.

  Erreur typique sans ce script :
    Invalid object name 'T_Role_Activities' / 'T_User_Activities'

  Sans T_Role_Activities / T_User_Activities, « Enregistrer » un utilisateur échoue.
*/
-- USE db_acb256_glomyrashopping;   /* ← PRODUCTION Glomyra : décommentez cette ligne */
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '════════════════════════════════════════════════════════════════════';
PRINT CONCAT('Base active : ', DB_NAME());
PRINT CONCAT('Heure       : ', CONVERT(varchar(30), SYSDATETIME(), 120));
PRINT '════════════════════════════════════════════════════════════════════';
GO

/* ── 0. Contrôles prérequis ─────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.T_Users', N'U') IS NULL
BEGIN
    RAISERROR('KO — Table dbo.T_Users absente. Déployez d''abord le schéma GlobalShoping.', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.T_Roles', N'U') IS NULL
BEGIN
    RAISERROR('KO — Table dbo.T_Roles absente. Déployez d''abord le schéma GlobalShoping.', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.T_Fonctions', N'U') IS NULL
BEGIN
    RAISERROR('KO — Table dbo.T_Fonctions absente. Déployez d''abord le schéma GlobalShoping.', 16, 1);
END
GO

/* ── 1. Au moins une fonction (obligatoire pour T_Activities) ───────────── */
IF NOT EXISTS (SELECT 1 FROM dbo.T_Fonctions)
BEGIN
    INSERT INTO dbo.T_Fonctions (Description_Fonction)
    VALUES (N'Administration');
    PRINT 'OK — 1 ligne insérée dans T_Fonctions (Administration).';
END
ELSE
    PRINT 'OK — T_Fonctions contient déjà des lignes.';
GO

/* ── 2. Table T_Activities (schéma hiérarchique) ────────────────────────── */
IF OBJECT_ID(N'dbo.T_Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Activities (
        ActivityId           INT            IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_T_Activities PRIMARY KEY,
        FonctionId           INT            NOT NULL,
        Code                 NVARCHAR(80)   NOT NULL,
        Description_Activity NVARCHAR(150)  NOT NULL,
        IsActive             BIT            NOT NULL
            CONSTRAINT DF_T_Activities_IsActive DEFAULT (1),
        CONSTRAINT UQ_T_Activities_Code UNIQUE (Code),
        CONSTRAINT UQ_T_Activities_Fonction_Description UNIQUE (FonctionId, Description_Activity)
    );
    PRINT 'OK — Table T_Activities créée.';
END
ELSE
BEGIN
    PRINT 'OK — T_Activities déjà présente.';
    IF COL_LENGTH(N'dbo.T_Activities', N'Code') IS NULL
    BEGIN
        ALTER TABLE dbo.T_Activities ADD Code NVARCHAR(80) NULL;
        PRINT 'OK — Colonne Code ajoutée à T_Activities.';
    END
    IF COL_LENGTH(N'dbo.T_Activities', N'IsActive') IS NULL
    BEGIN
        ALTER TABLE dbo.T_Activities ADD IsActive BIT NOT NULL
            CONSTRAINT DF_T_Activities_IsActive_Legacy DEFAULT (1);
        PRINT 'OK — Colonne IsActive ajoutée à T_Activities.';
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_T_Activities_Fonctions')
   AND OBJECT_ID(N'dbo.T_Activities', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.T_Fonctions', N'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.T_Activities
        ADD CONSTRAINT FK_T_Activities_Fonctions
        FOREIGN KEY (FonctionId) REFERENCES dbo.T_Fonctions (Id_Fonction);
    PRINT 'OK — FK T_Activities → T_Fonctions.';
END
GO

/* ── 3. Table T_Role_Activities (OBLIGATOIRE pour SaveUserProfileAccess) ── */
IF OBJECT_ID(N'dbo.T_Role_Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Role_Activities (
        RoleActivityId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_T_Role_Activities PRIMARY KEY,
        RoleId         INT NOT NULL,
        ActivityId     INT NOT NULL,
        IsGranted      BIT NOT NULL
            CONSTRAINT DF_T_Role_Activities_IsGranted DEFAULT (1)
    );

    ALTER TABLE dbo.T_Role_Activities
        ADD CONSTRAINT FK_T_Role_Activities_Role
        FOREIGN KEY (RoleId) REFERENCES dbo.T_Roles (RoleId);

    ALTER TABLE dbo.T_Role_Activities
        ADD CONSTRAINT FK_T_Role_Activities_Activity
        FOREIGN KEY (ActivityId) REFERENCES dbo.T_Activities (ActivityId);

    CREATE UNIQUE INDEX UX_T_Role_Activities_Role_Activity
        ON dbo.T_Role_Activities (RoleId, ActivityId);

    PRINT 'OK — Table T_Role_Activities créée.';
END
ELSE
    PRINT 'OK — T_Role_Activities déjà présente.';
GO

/* ── 4. Table T_User_Activities (source de vérité par login) ────────────── */
IF OBJECT_ID(N'dbo.T_User_Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_User_Activities (
        UserActivityId INT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_T_User_Activities PRIMARY KEY,
        UserLogin      VARCHAR(50)  NOT NULL,
        ActivityId     INT          NOT NULL,
        IsGranted      BIT          NOT NULL
            CONSTRAINT DF_T_User_Activities_IsGranted DEFAULT (1),
        AssignedDate   DATETIME2(7) NOT NULL
            CONSTRAINT DF_T_User_Activities_AssignedDate DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_T_User_Activities_User_Activity UNIQUE (UserLogin, ActivityId)
    );

    ALTER TABLE dbo.T_User_Activities
        ADD CONSTRAINT FK_T_User_Activities_User
        FOREIGN KEY (UserLogin) REFERENCES dbo.T_Users (login);

    ALTER TABLE dbo.T_User_Activities
        ADD CONSTRAINT FK_T_User_Activities_Activity
        FOREIGN KEY (ActivityId) REFERENCES dbo.T_Activities (ActivityId);

    PRINT 'OK — Table T_User_Activities créée.';
END
ELSE
    PRINT 'OK — T_User_Activities déjà présente.';
GO

/* FK manquantes sur tables legacy déjà existantes */
IF OBJECT_ID(N'dbo.T_User_Activities', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_T_User_Activities_User')
    BEGIN
        ALTER TABLE dbo.T_User_Activities
            ADD CONSTRAINT FK_T_User_Activities_User
            FOREIGN KEY (UserLogin) REFERENCES dbo.T_Users (login);
        PRINT 'OK — FK FK_T_User_Activities_User ajoutée.';
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_T_User_Activities_Activity')
    BEGIN
        ALTER TABLE dbo.T_User_Activities
            ADD CONSTRAINT FK_T_User_Activities_Activity
            FOREIGN KEY (ActivityId) REFERENCES dbo.T_Activities (ActivityId);
        PRINT 'OK — FK FK_T_User_Activities_Activity ajoutée.';
    END
END
GO

IF OBJECT_ID(N'dbo.T_Role_Activities', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_T_Role_Activities_Role')
    BEGIN
        ALTER TABLE dbo.T_Role_Activities
            ADD CONSTRAINT FK_T_Role_Activities_Role
            FOREIGN KEY (RoleId) REFERENCES dbo.T_Roles (RoleId);
        PRINT 'OK — FK FK_T_Role_Activities_Role ajoutée.';
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_T_Role_Activities_Activity')
    BEGIN
        ALTER TABLE dbo.T_Role_Activities
            ADD CONSTRAINT FK_T_Role_Activities_Activity
            FOREIGN KEY (ActivityId) REFERENCES dbo.T_Activities (ActivityId);
        PRINT 'OK — FK FK_T_Role_Activities_Activity ajoutée.';
    END
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_T_Role_Activities_Role_Activity')
    BEGIN
        CREATE UNIQUE INDEX UX_T_Role_Activities_Role_Activity
            ON dbo.T_Role_Activities (RoleId, ActivityId);
        PRINT 'OK — Index UX_T_Role_Activities_Role_Activity créé.';
    END
END
GO

/* ── 4b. Nettoyage doublons T_Activities (même Code → crash .NET) ─────── */
IF OBJECT_ID(N'dbo.T_Activities', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'tempdb..#ActDup', N'U') IS NOT NULL DROP TABLE #ActDup;

    SELECT
        ActivityId,
        LOWER(LTRIM(RTRIM(Code))) AS NormCode,
        ROW_NUMBER() OVER (
            PARTITION BY LOWER(LTRIM(RTRIM(Code)))
            ORDER BY ActivityId
        ) AS rn,
        MIN(ActivityId) OVER (
            PARTITION BY LOWER(LTRIM(RTRIM(Code)))
        ) AS KeepId
    INTO #ActDup
    FROM dbo.T_Activities
    WHERE Code IS NOT NULL AND LTRIM(RTRIM(Code)) <> N'';

    IF EXISTS (SELECT 1 FROM #ActDup WHERE rn > 1)
    BEGIN
        IF OBJECT_ID(N'dbo.T_User_Activities', N'U') IS NOT NULL
        BEGIN
            UPDATE ua
            SET ua.ActivityId = d.KeepId
            FROM dbo.T_User_Activities ua
            INNER JOIN #ActDup d ON d.ActivityId = ua.ActivityId
            WHERE d.rn > 1
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.T_User_Activities x
                  WHERE x.UserLogin = ua.UserLogin AND x.ActivityId = d.KeepId
              );

            DELETE ua
            FROM dbo.T_User_Activities ua
            INNER JOIN #ActDup d ON d.ActivityId = ua.ActivityId
            WHERE d.rn > 1;
        END

        IF OBJECT_ID(N'dbo.T_Role_Activities', N'U') IS NOT NULL
        BEGIN
            UPDATE ra
            SET ra.ActivityId = d.KeepId
            FROM dbo.T_Role_Activities ra
            INNER JOIN #ActDup d ON d.ActivityId = ra.ActivityId
            WHERE d.rn > 1
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.T_Role_Activities x
                  WHERE x.RoleId = ra.RoleId AND x.ActivityId = d.KeepId
              );

            DELETE ra
            FROM dbo.T_Role_Activities ra
            INNER JOIN #ActDup d ON d.ActivityId = ra.ActivityId
            WHERE d.rn > 1;
        END

        DELETE a
        FROM dbo.T_Activities a
        INNER JOIN #ActDup d ON d.ActivityId = a.ActivityId
        WHERE d.rn > 1;

        PRINT CONCAT('OK — Doublons T_Activities supprimés : ', @@ROWCOUNT, ' ligne(s).');
    END
    ELSE
        PRINT 'OK — Aucun doublon Code dans T_Activities.';

    DROP TABLE #ActDup;
END
GO

/* ── 5. Catalogue PermissionCatalog (67 codes) ──────────────────────────── */
DECLARE @FonctionId INT = (
    SELECT TOP (1) Id_Fonction FROM dbo.T_Fonctions ORDER BY Id_Fonction
);

IF @FonctionId IS NULL OR @FonctionId = 0
BEGIN
    RAISERROR('KO — T_Fonctions est vide après tentative de remplissage.', 16, 1);
END

DECLARE @Catalog TABLE (
    Code                 NVARCHAR(80)  NOT NULL PRIMARY KEY,
    Description_Activity NVARCHAR(150) NOT NULL
);

INSERT INTO @Catalog (Code, Description_Activity) VALUES
    (N'dashboard', N'Tableau de bord'),
    (N'vente', N'Ventes'),
    (N'articles', N'Articles'),
    (N'commandes', N'Commandes'),
    (N'services', N'Prestations'),
    (N'grh', N'GRH'),
    (N'rapports', N'Rapports'),
    (N'administration', N'Administration'),
    (N'parametres', N'Paramètres'),
    (N'vente.facturation', N'Ventes · Facturation'),
    (N'vente.facturation.read', N'Ventes · Facturation · Consulter'),
    (N'vente.facturation.write', N'Ventes · Facturation · Créer / modifier'),
    (N'vente.facturation.delete', N'Ventes · Facturation · Supprimer / annuler'),
    (N'vente.livraisons', N'Ventes · Livraisons'),
    (N'vente.livraisons.read', N'Ventes · Livraisons · Consulter'),
    (N'vente.livraisons.write', N'Ventes · Livraisons · Enregistrer'),
    (N'articles.catalogue', N'Articles · Catalogue'),
    (N'articles.catalogue.read', N'Articles · Catalogue · Consulter'),
    (N'articles.catalogue.write', N'Articles · Catalogue · Créer / modifier'),
    (N'articles.catalogue.delete', N'Articles · Catalogue · Supprimer'),
    (N'articles.approvisionnement', N'Articles · Approvisionnement'),
    (N'articles.approvisionnement.read', N'Articles · Approvisionnement · Consulter'),
    (N'articles.approvisionnement.write', N'Articles · Approvisionnement · Créer / modifier'),
    (N'articles.approvisionnement.delete', N'Articles · Approvisionnement · Annuler'),
    (N'articles.transformation', N'Articles · Transformation'),
    (N'articles.transformation.read', N'Articles · Transformation · Consulter'),
    (N'articles.transformation.write', N'Articles · Transformation · Enregistrer'),
    (N'articles.localisations', N'Articles · Localisations'),
    (N'articles.localisations.read', N'Articles · Localisations · Consulter'),
    (N'articles.localisations.write', N'Articles · Localisations · Gérer'),
    (N'articles.inventaire', N'Articles · Inventaire magasin'),
    (N'articles.inventaire.read', N'Articles · Inventaire magasin · Consulter'),
    (N'articles.inventaire.write', N'Articles · Inventaire magasin · Saisir / clôturer'),
    (N'commandes.internes', N'Commandes · Commandes internes'),
    (N'commandes.internes.read', N'Commandes · Commandes internes · Consulter'),
    (N'commandes.internes.write', N'Commandes · Commandes internes · Créer / réceptionner'),
    (N'commandes.clients', N'Commandes · Commandes clients'),
    (N'commandes.clients.read', N'Commandes · Commandes clients · Consulter'),
    (N'commandes.clients.write', N'Commandes · Commandes clients · Gérer'),
    (N'commandes.demandes', N'Commandes · Demandes magasin'),
    (N'commandes.demandes.read', N'Commandes · Demandes magasin · Consulter'),
    (N'commandes.demandes.write', N'Commandes · Demandes magasin · Traiter'),
    (N'commandes.livraisons', N'Commandes · Livraisons commandes'),
    (N'commandes.livraisons.read', N'Commandes · Livraisons commandes · Consulter'),
    (N'commandes.livraisons.write', N'Commandes · Livraisons commandes · Enregistrer'),
    (N'administration.utilisateurs', N'Administration · Utilisateurs'),
    (N'administration.utilisateurs.read', N'Administration · Utilisateurs · Consulter'),
    (N'administration.utilisateurs.write', N'Administration · Utilisateurs · Créer / modifier'),
    (N'administration.utilisateurs.delete', N'Administration · Utilisateurs · Désactiver'),
    (N'administration.roles', N'Administration · Rôles & permissions'),
    (N'administration.roles.read', N'Administration · Rôles & permissions · Consulter'),
    (N'administration.roles.write', N'Administration · Rôles & permissions · Gérer'),
    (N'administration.fournisseurs', N'Administration · Fournisseurs'),
    (N'administration.fournisseurs.read', N'Administration · Fournisseurs · Consulter'),
    (N'administration.fournisseurs.write', N'Administration · Fournisseurs · Gérer'),
    (N'administration.localisations', N'Administration · Localisations'),
    (N'administration.localisations.read', N'Administration · Localisations · Consulter'),
    (N'administration.localisations.write', N'Administration · Localisations · Gérer'),
    (N'parametres.general', N'Paramètres · Configuration'),
    (N'parametres.general.read', N'Paramètres · Configuration · Consulter'),
    (N'parametres.general.write', N'Paramètres · Configuration · Modifier'),
    (N'grh.read', N'GRH · Consulter'),
    (N'grh.write', N'GRH · Gérer'),
    (N'rapports.read', N'Rapports · Consulter'),
    (N'rapports.write', N'Rapports · Gérer'),
    (N'services.read', N'Prestations · Consulter'),
    (N'services.write', N'Prestations · Gérer');

DECLARE @ExpectedCatalog INT = (SELECT COUNT(*) FROM @Catalog);
DECLARE @Inserted INT;

INSERT INTO dbo.T_Activities (FonctionId, Code, Description_Activity, IsActive)
SELECT @FonctionId, c.Code, c.Description_Activity, 1
FROM @Catalog c
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.T_Activities a WHERE a.Code = c.Code
)
AND NOT EXISTS (
    SELECT 1 FROM dbo.T_Activities a
    WHERE a.FonctionId = @FonctionId
      AND a.Description_Activity = c.Description_Activity
);

SET @Inserted = @@ROWCOUNT;
PRINT CONCAT('OK — Catalogue : ', @Inserted, ' activité(s) ajoutée(s). Attendu max ', @ExpectedCatalog, '.');

/* Codes encore absents (conflit description legacy) */
IF EXISTS (
    SELECT 1
    FROM @Catalog c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.T_Activities a WHERE a.Code = c.Code)
)
BEGIN
    PRINT 'ATTENTION — Certains codes catalogue sont encore absents (conflit Description_Activity legacy) :';
    SELECT c.Code, c.Description_Activity
    FROM @Catalog c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.T_Activities a WHERE a.Code = c.Code);
END
GO

/* ── 6. Rapport de contrôle (doit afficher OK partout) ──────────────────── */
DECLARE @CatalogCount INT;
DECLARE @HasRoleAct BIT = CASE WHEN OBJECT_ID(N'dbo.T_Role_Activities', N'U') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @HasUserAct BIT = CASE WHEN OBJECT_ID(N'dbo.T_User_Activities', N'U') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @FonctionCount INT = (SELECT COUNT(*) FROM dbo.T_Fonctions);

SELECT @CatalogCount = COUNT(*)
FROM dbo.T_Activities
WHERE Code IN (
    N'dashboard', N'vente', N'articles', N'commandes', N'services',
    N'grh', N'rapports', N'administration', N'parametres'
)
OR Code LIKE N'%.%';

SELECT
    CONCAT('T_Fonctions (>=1)', CASE WHEN @FonctionCount >= 1 THEN ' OK' ELSE ' KO' END) AS Controle,
    @FonctionCount AS Valeur
UNION ALL
SELECT
    CONCAT('T_Role_Activities', CASE WHEN @HasRoleAct = 1 THEN ' OK' ELSE ' KO MANQUANT' END),
    CASE WHEN @HasRoleAct = 1 THEN (SELECT COUNT(*) FROM dbo.T_Role_Activities) END
UNION ALL
SELECT
    CONCAT('T_User_Activities', CASE WHEN @HasUserAct = 1 THEN ' OK' ELSE ' KO MANQUANT' END),
    CASE WHEN @HasUserAct = 1 THEN (SELECT COUNT(*) FROM dbo.T_User_Activities) END
UNION ALL
SELECT
    CONCAT('Catalogue hiérarchique (>=60)', CASE WHEN @CatalogCount >= 60 THEN ' OK' ELSE ' KO INCOMPLET' END),
    @CatalogCount;

IF @HasRoleAct = 0 OR @HasUserAct = 0 OR @FonctionCount < 1
BEGIN
    RAISERROR('KO — Déploiement incomplet. Relisez les messages PRINT ci-dessus.', 16, 1);
END

IF EXISTS (
    SELECT 1
    FROM dbo.T_Activities
    WHERE Code IS NOT NULL AND LTRIM(RTRIM(Code)) <> N''
    GROUP BY LOWER(LTRIM(RTRIM(Code)))
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR('KO — Doublons Code encore présents dans T_Activities. Relancez la section 4b.', 16, 1);
END
GO

PRINT '════════════════════════════════════════════════════════════════════';
PRINT 'SUCCÈS — Base prête pour enregistrer des utilisateurs avec accès hiérarchiques.';
PRINT 'Redéployez l''application web puis testez.';
PRINT '';
PRINT 'Rappel : si le login existe déjà (ex. ize), utilisez Modifier, pas Nouveau.';
PRINT '════════════════════════════════════════════════════════════════════';
GO

/*
  ── Diagnostic après enregistrement d''un utilisateur dans l''app ─────────────

  SELECT u.login, a.Code, ua.IsGranted, ua.AssignedDate
  FROM dbo.T_User_Activities ua
  JOIN dbo.T_Users u ON u.login = ua.UserLogin
  JOIN dbo.T_Activities a ON a.ActivityId = ua.ActivityId
  WHERE u.login = N'ize'
  ORDER BY a.Code;

  SELECT r.Description_Role, a.Code
  FROM dbo.T_Users u
  JOIN dbo.T_Roles r ON r.RoleId = u.RoleId
  JOIN dbo.T_Role_Activities ra ON ra.RoleId = r.RoleId AND ra.IsGranted = 1
  JOIN dbo.T_Activities a ON a.ActivityId = ra.ActivityId
  WHERE u.login = N'ize'
  ORDER BY a.Code;
*/
