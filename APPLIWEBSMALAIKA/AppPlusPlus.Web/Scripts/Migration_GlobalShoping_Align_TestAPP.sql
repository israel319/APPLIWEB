-- ============================================================================
-- MIGRATION PRODUCTION : GlobalShoping → alignement schéma TestAPP
-- ============================================================================
-- Objectif : ajouter tables/colonnes manquantes SANS supprimer ni modifier
--            les données existantes de façon destructive.
--
-- RÈGLES :
--   • Script idempotent (IF NOT EXISTS partout)
--   • Aucune DROP TABLE / DROP COLUMN
--   • Tables legacy prod (T_Importations, items_shop, …) conservées
--   • Exécuter UNIQUEMENT après sauvegarde complète (.bak)
--
-- Ordre :
--   1. Tables de référence / sécurité
--   2. Tables métier manquantes
--   3. Colonnes manquantes sur tables existantes
--   4. Migration de données (stock, appros, renommages logiques)
--   5. Données de référence (si tables vides)
--   6. Index et clés étrangères
-- ============================================================================

USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
GO

-- ============================================================================
-- PHASE 1 — TABLES MANQUANTES (20 tables)
-- ============================================================================

IF OBJECT_ID('dbo.T_YesNo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_YesNo (
        Id          BIT          NOT NULL CONSTRAINT PK_T_YesNo PRIMARY KEY,
        Description VARCHAR(50)  NULL
    );
    PRINT 'Créé : T_YesNo';
END
GO

IF OBJECT_ID('dbo.T_Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Roles (
        RoleId           INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Roles PRIMARY KEY,
        Description_Role NVARCHAR(200)  NOT NULL,
        IsActive         BIT            NOT NULL CONSTRAINT DF_T_Roles_IsActive DEFAULT (1)
    );
    PRINT 'Créé : T_Roles';
END
GO

IF OBJECT_ID('dbo.T_Activities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Activities (
        ActivityId          INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Activities PRIMARY KEY,
        FonctionId          INT            NOT NULL,
        Code                NVARCHAR(80)   NOT NULL,
        Description_Activity NVARCHAR(150) NOT NULL,
        IsActive            BIT            NOT NULL CONSTRAINT DF_T_Activities_IsActive DEFAULT (1),
        CONSTRAINT UQ_T_Activities_Code UNIQUE (Code),
        CONSTRAINT UQ_T_Activities_Fonction_Description UNIQUE (FonctionId, Description_Activity)
    );
    PRINT 'Créé : T_Activities';
END
GO

IF OBJECT_ID('dbo.T_Permissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Permissions (
        PermissionId INT NOT NULL IDENTITY(1,1) CONSTRAINT PK_T_Permissions PRIMARY KEY,
        RoleId       INT NOT NULL,
        FonctionId   INT NOT NULL,
        CanRead      BIT NOT NULL CONSTRAINT DF_T_Permissions_CanRead DEFAULT (1),
        CanWrite     BIT NOT NULL CONSTRAINT DF_T_Permissions_CanWrite DEFAULT (0),
        CanDelete    BIT NOT NULL CONSTRAINT DF_T_Permissions_CanDelete DEFAULT (0),
        CONSTRAINT UQ_Role_Fonction UNIQUE (RoleId, FonctionId)
    );
    PRINT 'Créé : T_Permissions';
END
GO

IF OBJECT_ID('dbo.T_User_Activities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_User_Activities (
        UserActivityId INT          IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_User_Activities PRIMARY KEY,
        UserLogin      VARCHAR(50)  NOT NULL,
        ActivityId     INT          NOT NULL,
        IsGranted      BIT          NOT NULL CONSTRAINT DF_T_User_Activities_IsGranted DEFAULT (1),
        AssignedDate   DATETIME2(7) NOT NULL CONSTRAINT DF_T_User_Activities_AssignedDate DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_T_User_Activities_User_Activity UNIQUE (UserLogin, ActivityId)
    );
    PRINT 'Créé : T_User_Activities';
END
GO

IF OBJECT_ID('dbo.T_AppSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_AppSettings (
        [Key]   VARCHAR(100) NOT NULL CONSTRAINT PK_T_AppSettings PRIMARY KEY,
        [Value] VARCHAR(500) NULL
    );
    PRINT 'Créé : T_AppSettings';
END
GO

IF OBJECT_ID('dbo.T_Profile', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Profile (
        Id                INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Profile PRIMARY KEY,
        AppNameSettingKey VARCHAR(100)   NOT NULL CONSTRAINT DF_T_Profile_AppName DEFAULT ('AppName'),
        PhotoShop         NVARCHAR(MAX)  NULL,
        Adresse1          VARCHAR(250)   NULL,
        Adresse2          VARCHAR(250)   NULL
    );
    PRINT 'Créé : T_Profile';
END
GO

IF OBJECT_ID('dbo.T_Customer_Type', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Customer_Type (
        CustomerTypeId INT          IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Customer_Type PRIMARY KEY,
        Description    VARCHAR(50)  NULL
    );
    PRINT 'Créé : T_Customer_Type';
END
GO

IF OBJECT_ID('dbo.T_Expense_Source', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Expense_Source (
        Id      INT         IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Expense_Source PRIMARY KEY,
        Sources VARCHAR(50) NULL
    );
    PRINT 'Créé : T_Expense_Source';
END
GO

IF OBJECT_ID('dbo.T_Supplier_Service', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Supplier_Service (
        ServiceId          INT         IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Supplier_Service PRIMARY KEY,
        ServiceDescription VARCHAR(50) NULL
    );
    PRINT 'Créé : T_Supplier_Service';
END
GO

IF OBJECT_ID('dbo.T_Supplier', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Supplier (
        SupplierId      INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Supplier PRIMARY KEY,
        ServiceId       INT           NULL,
        SupplierNumber  VARCHAR(10)   NULL,
        SupplierName    VARCHAR(50)   NULL,
        Contact         VARCHAR(50)   NULL,
        Email           VARCHAR(50)   NULL,
        Currency        INT           NULL,
        Adresse         VARCHAR(50)   NULL,
        CcAmountToPay   DECIMAL(18,2) NULL,
        CcAmountPay     DECIMAL(18,2) NULL,
        UserLogin       VARCHAR(50)   NULL,
        CreationDate    DATE          NULL,
        CONSTRAINT UQ_Supplier UNIQUE (SupplierName)
    );
    PRINT 'Créé : T_Supplier';
END
GO

IF OBJECT_ID('dbo.T_Caisse', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Caisse (
        CaisseId      INT           IDENTITY(1,1) NOT NULL,
        Description   VARCHAR(50)   NOT NULL,
        CurrencyId    INT           NOT NULL,
        MinAmount     DECIMAL(18,2) NULL,
        MaxAmount     DECIMAL(18,2) NULL,
        DayLimit      DECIMAL(18,2) NULL,
        UserLogin     VARCHAR(50)   NULL,
        CreationDate  DATETIME      NULL,
        Solde         DECIMAL(18,2) NOT NULL,
        TotalDueDay   DECIMAL(18,2) NULL,
        TotalRestDay  DECIMAL(19,2) NULL,
        CONSTRAINT PK_T_Caisse PRIMARY KEY (Description, CurrencyId)
    );
    PRINT 'Créé : T_Caisse';
END
GO

IF OBJECT_ID('dbo.T_Users_Caisse', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Users_Caisse (
        Id         INT           IDENTITY(1,1) NOT NULL,
        [User]     VARCHAR(50)   NOT NULL,
        CaisseId   INT           NOT NULL,
        Begin_Date DATE          NULL,
        End_Date   DATE          NULL,
        MaxAmount  DECIMAL(18,2) NULL,
        Activate   BIT           NULL,
        CONSTRAINT PK_T_User_Caisse PRIMARY KEY ([User], CaisseId)
    );
    PRINT 'Créé : T_Users_Caisse';
END
GO

IF OBJECT_ID('dbo.T_Versements', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Versements (
        Id                INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Versements PRIMARY KEY,
        DateCloture       DATE           NOT NULL,
        HeureCloture      DATETIME2(7)   NOT NULL,
        LocalisationId    INT            NOT NULL,
        TypeOperation     NVARCHAR(50)   NOT NULL,
        Montant           DECIMAL(18,2)  NOT NULL,
        ModeVersement     NVARCHAR(50)   NOT NULL,
        NombreOperations  INT            NOT NULL CONSTRAINT DF_T_Versements_NbOps DEFAULT (0),
        Observation       NVARCHAR(500)  NULL,
        UserLogin         NVARCHAR(100)  NOT NULL,
        DateSys           DATETIME2(7)   NOT NULL,
        StatutCloture     INT            NOT NULL CONSTRAINT DF_T_Versements_Statut DEFAULT (0),
        MotifRejet        NVARCHAR(500)  NULL,
        TraitePar         NVARCHAR(100)  NULL,
        DateTraitement    DATETIME2(7)   NULL
    );
    PRINT 'Créé : T_Versements';
END
GO

IF OBJECT_ID('dbo.T_Appro_Expense', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Appro_Expense (
        Id            INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Appro PRIMARY KEY,
        SourceId      INT           NOT NULL,
        CaisseId      INT           NULL,
        CurrencyId    INT           NULL,
        AmountUSD     DECIMAL(18,2) NULL,
        AmountCDF     DECIMAL(18,2) NULL,
        Depositeur    VARCHAR(50)   NULL,
        Description   VARCHAR(50)   NULL,
        Comment       VARCHAR(50)   NULL,
        UserLogin     VARCHAR(50)   NULL,
        CreationDate  DATETIME      NULL,
        VersementId   INT           NULL,
        Statut        NVARCHAR(20)  NOT NULL CONSTRAINT DF_T_ApproExpense_Statut DEFAULT (N'En attente')
    );
    PRINT 'Créé : T_Appro_Expense';
END
GO

IF OBJECT_ID('dbo.T_Payments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Payments (
        Id      INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Payments PRIMARY KEY,
        Id_Fact INT           NOT NULL,
        [Date]  DATE          NOT NULL,
        Amount  DECIMAL(18,2) NOT NULL,
        Note    NVARCHAR(200) NULL,
        DateSys DATETIME      NOT NULL CONSTRAINT DF_T_Payments_DateSys DEFAULT (GETDATE()),
        [User]  NVARCHAR(100) NULL
    );
    PRINT 'Créé : T_Payments';
END
GO

IF OBJECT_ID('dbo.T_ModuleLicenses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_ModuleLicenses (
        Id           INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_ModuleLicenses PRIMARY KEY,
        ModuleCode   VARCHAR(50)    NOT NULL,
        ModuleName   NVARCHAR(100)  NOT NULL,
        Description  NVARCHAR(500)  NULL,
        Icon         VARCHAR(50)    NULL,
        IsActive     BIT            NOT NULL CONSTRAINT DF_T_ModuleLicenses_IsActive DEFAULT (1),
        IsCore       BIT            NOT NULL CONSTRAINT DF_T_ModuleLicenses_IsCore DEFAULT (0),
        Price        DECIMAL(18,2)  NULL,
        ActivatedAt  DATETIME       NULL,
        ExpiresAt    DATETIME       NULL,
        LicenseNotes NVARCHAR(500)  NULL,
        CreatedAt    DATETIME       NOT NULL CONSTRAINT DF_T_ModuleLicenses_CreatedAt DEFAULT (GETDATE()),
        UpdatedAt    DATETIME       NOT NULL CONSTRAINT DF_T_ModuleLicenses_UpdatedAt DEFAULT (GETDATE()),
        CONSTRAINT UQ_T_ModuleLicenses_ModuleCode UNIQUE (ModuleCode)
    );
    PRINT 'Créé : T_ModuleLicenses';
END
GO

IF OBJECT_ID('dbo.T_Appro_Details', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Appro_Details (
        CONSTRAINT PK_T_Appro_Details_Id PRIMARY KEY (Id)
        Id_Appro        INT           NOT NULL,
        Id_Article      VARCHAR(100)  NOT NULL,
        Id_Localisation INT           NOT NULL,
        Qte             DECIMAL(18,2) NOT NULL CONSTRAINT DF_T_ApproDetails_Qte DEFAULT (0),
        PA              DECIMAL(18,2) NULL,
        PV              DECIMAL(18,2) NULL,
        Ben             DECIMAL(18,2) NULL,
        BenTotal        DECIMAL(18,2) NULL,
        Taux            DECIMAL(18,2) NULL,
        DateSys         DATETIME      NULL CONSTRAINT DF_T_ApproDetails_DateSys DEFAULT (GETDATE()),
        [User]          VARCHAR(50)   NULL,
        Cumputer        VARCHAR(50)   NULL
    );
    PRINT 'Créé : T_Appro_Details';
END
GO

IF OBJECT_ID('dbo.T_Stock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Stock (
        Id              INT           IDENTITY(1,1) NOT NULL,
        Id_Article      VARCHAR(100)  NOT NULL,
        Id_Localisation INT           NOT NULL,
        DateSys         DATE          NULL,
        UserLogin       VARCHAR(50)   NULL,
        Qte             DECIMAL(18,2) NULL,
        CodeBar         VARCHAR(50)   NULL,
        Seuil           INT           NULL CONSTRAINT DF_T_Stock_Seuil DEFAULT (0),
        CONSTRAINT PK_T_Art_Localisations PRIMARY KEY (Id_Article, Id_Localisation),
        CONSTRAINT UQ_Stock_Article_Loc UNIQUE (Id_Article, Id_Localisation)
    );
    PRINT 'Créé : T_Stock';
END
GO

IF OBJECT_ID('dbo.T_Mouvement_Stock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Mouvement_Stock (
        Id                     INT           IDENTITY(1,1) NOT NULL,
        Id_Article             VARCHAR(100)  NOT NULL,
        Id_Localisation        INT           NOT NULL,
        Type_Mouvement         VARCHAR(20)   NOT NULL,
        Quantite               DECIMAL(18,2) NOT NULL,
        Qte_Avant              DECIMAL(18,2) NULL,
        Qte_Apres              DECIMAL(18,2) NULL,
        Date_Mouvement         DATETIME      NULL CONSTRAINT DF_T_Mvt_Date DEFAULT (GETDATE()),
        Type_Document          VARCHAR(50)   NOT NULL,
        Id_Document            INT           NULL,
        Id_Document_Detail     INT           NULL,
        Reference              VARCHAR(100)  NULL,
        Id_Localisation_Source INT           NULL,
        Id_Localisation_Dest   INT           NULL,
        Prix_Unitaire          DECIMAL(18,2) NULL,
        Valeur_Totale          AS (Quantite * ISNULL(Prix_Unitaire, 0)) PERSISTED,
        Observation            VARCHAR(255)  NULL,
        Cree_Par               VARCHAR(50)   NOT NULL,
        Date_Creation          DATETIME      NULL CONSTRAINT DF_T_Mvt_Creation DEFAULT (GETDATE()),
        Valide_Par             VARCHAR(50)   NULL,
        Date_Validation        DATETIME      NULL,
        Annule                 BIT           NULL CONSTRAINT DF_T_Mvt_Annule DEFAULT (0),
        CONSTRAINT PK_T_Mouvement_Stock_Id PRIMARY KEY (Id),
        CONSTRAINT CK_Mouvement_Type CHECK (Type_Mouvement IN ('ENTREE','SORTIE','TRANSFERT','AJUSTEMENT'))
    );
    PRINT 'Créé : T_Mouvement_Stock';
END
GO

-- ============================================================================
-- PHASE 2 — COLONNES MANQUANTES (tables existantes, ADDITIF UNIQUEMENT)
-- ============================================================================

-- T_Arts
IF COL_LENGTH('dbo.T_Arts', 'isTransferable') IS NULL
    ALTER TABLE dbo.T_Arts ADD isTransferable BIT NULL;
GO

-- T_Appros (colonnes en-tête TestAPP — anciennes colonnes ligne conservées)
IF COL_LENGTH('dbo.T_Appros', 'SupplierId') IS NULL     ALTER TABLE dbo.T_Appros ADD SupplierId INT NULL;
IF COL_LENGTH('dbo.T_Appros', 'LocalisationId') IS NULL ALTER TABLE dbo.T_Appros ADD LocalisationId INT NULL;
IF COL_LENGTH('dbo.T_Appros', 'StatusId') IS NULL       ALTER TABLE dbo.T_Appros ADD StatusId INT NULL;
IF COL_LENGTH('dbo.T_Appros', 'Id_Cmd') IS NULL         ALTER TABLE dbo.T_Appros ADD Id_Cmd INT NULL;
IF COL_LENGTH('dbo.T_Appros', 'Reference') IS NULL       ALTER TABLE dbo.T_Appros ADD Reference VARCHAR(50) NULL;
GO

-- T_Cmds
IF COL_LENGTH('dbo.T_Cmds', 'SupplierId') IS NULL ALTER TABLE dbo.T_Cmds ADD SupplierId INT NULL;
GO

-- T_Commande
IF COL_LENGTH('dbo.T_Commande', 'Status') IS NULL ALTER TABLE dbo.T_Commande ADD Status INT NULL;
GO

-- T_Customer (prod a CustomerDescription → app attend CustomerName)
IF COL_LENGTH('dbo.T_Customer', 'CustomerName') IS NULL
    ALTER TABLE dbo.T_Customer ADD CustomerName VARCHAR(50) NULL;
IF COL_LENGTH('dbo.T_Customer', 'IsPermanent') IS NULL
    ALTER TABLE dbo.T_Customer ADD IsPermanent BIT NULL;
IF COL_LENGTH('dbo.T_Customer', 'CustomerTypeId') IS NULL
    ALTER TABLE dbo.T_Customer ADD CustomerTypeId INT NULL;
GO

-- T_Facts
IF COL_LENGTH('dbo.T_Facts', 'CustomerId') IS NULL       ALTER TABLE dbo.T_Facts ADD CustomerId INT NULL;
IF COL_LENGTH('dbo.T_Facts', 'CommandeId') IS NULL      ALTER TABLE dbo.T_Facts ADD CommandeId INT NULL;
IF COL_LENGTH('dbo.T_Facts', 'LivraisonId') IS NULL     ALTER TABLE dbo.T_Facts ADD LivraisonId INT NULL;
IF COL_LENGTH('dbo.T_Facts', 'MontantEnLettres') IS NULL  ALTER TABLE dbo.T_Facts ADD MontantEnLettres NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.T_Facts', 'MoneyId') IS NULL         ALTER TABLE dbo.T_Facts ADD MoneyId INT NULL;
GO

-- T_Fact_Details
IF COL_LENGTH('dbo.T_Fact_Details', 'Localisationid') IS NULL
    ALTER TABLE dbo.T_Fact_Details ADD Localisationid INT NULL;
GO

-- T_Livraison
IF COL_LENGTH('dbo.T_Livraison', 'CommandeId') IS NULL ALTER TABLE dbo.T_Livraison ADD CommandeId INT NULL;
IF COL_LENGTH('dbo.T_Livraison', 'Status') IS NULL     ALTER TABLE dbo.T_Livraison ADD Status INT NULL;
GO

-- T_Livraison_Detail
IF COL_LENGTH('dbo.T_Livraison_Detail', 'Localisationid') IS NULL
    ALTER TABLE dbo.T_Livraison_Detail ADD Localisationid INT NULL;
GO

-- T_Taux
IF COL_LENGTH('dbo.T_Taux', 'RateLow') IS NULL ALTER TABLE dbo.T_Taux ADD RateLow DECIMAL(18,2) NULL;
IF COL_LENGTH('dbo.T_Taux', 'RateUp') IS NULL  ALTER TABLE dbo.T_Taux ADD RateUp DECIMAL(18,2) NULL;
GO

-- T_Transformation
IF COL_LENGTH('dbo.T_Transformation', 'ToQte') IS NULL              ALTER TABLE dbo.T_Transformation ADD ToQte DECIMAL(18,2) NULL;
IF COL_LENGTH('dbo.T_Transformation', 'FromLocalisationId') IS NULL ALTER TABLE dbo.T_Transformation ADD FromLocalisationId INT NULL;
IF COL_LENGTH('dbo.T_Transformation', 'ToLocalisationId') IS NULL   ALTER TABLE dbo.T_Transformation ADD ToLocalisationId INT NULL;
IF COL_LENGTH('dbo.T_Transformation', 'Status') IS NULL           ALTER TABLE dbo.T_Transformation ADD Status INT NULL;
GO

-- T_Users
IF COL_LENGTH('dbo.T_Users', 'Email') IS NULL     ALTER TABLE dbo.T_Users ADD Email VARCHAR(50) NULL;
IF COL_LENGTH('dbo.T_Users', 'Activated') IS NULL ALTER TABLE dbo.T_Users ADD Activated BIT NULL;
IF COL_LENGTH('dbo.T_Users', 'RoleId') IS NULL    ALTER TABLE dbo.T_Users ADD RoleId INT NULL;
GO

-- T_User_Localisations (prod : User / Localisation → app : Id_User / Id_Localisation)
IF COL_LENGTH('dbo.T_User_Localisations', 'Id_User') IS NULL
    ALTER TABLE dbo.T_User_Localisations ADD Id_User VARCHAR(50) NULL;
IF COL_LENGTH('dbo.T_User_Localisations', 'Id_Localisation') IS NULL
    ALTER TABLE dbo.T_User_Localisations ADD Id_Localisation INT NULL;
GO

-- Extension longueur ArticleId (TestAPP = 100, prod parfois 50)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='T_Commande_Details' AND COLUMN_NAME='ArticleId' AND CHARACTER_MAXIMUM_LENGTH < 100)
    ALTER TABLE dbo.T_Commande_Details ALTER COLUMN ArticleId VARCHAR(100) NULL;
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='T_Livraison_Detail' AND COLUMN_NAME='ArticleId' AND CHARACTER_MAXIMUM_LENGTH < 100)
    ALTER TABLE dbo.T_Livraison_Detail ALTER COLUMN ArticleId VARCHAR(100) NULL;
GO

-- T_Livraison.ClientId : int attendu par l'app (table vide en prod → migration sûre)
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'T_Livraison' AND COLUMN_NAME = 'ClientId'
      AND DATA_TYPE IN ('varchar','nvarchar')
)
AND NOT EXISTS (SELECT 1 FROM dbo.T_Livraison)
BEGIN
    ALTER TABLE dbo.T_Livraison ALTER COLUMN ClientId INT NULL;
    PRINT 'T_Livraison.ClientId converti en INT (table vide).';
END
ELSE IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'T_Livraison' AND COLUMN_NAME = 'ClientId'
      AND DATA_TYPE IN ('varchar','nvarchar')
)
BEGIN
    PRINT 'ATTENTION : T_Livraison contient des données — ClientId varchar NON converti automatiquement.';
END
GO

-- Rendre Id_Localisation nullable sur T_Arts (TestAPP) sans perdre les valeurs existantes
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'T_Arts' AND COLUMN_NAME = 'Id_Localisation' AND IS_NULLABLE = 'NO'
)
BEGIN
    ALTER TABLE dbo.T_Arts ALTER COLUMN Id_Localisation INT NULL;
    PRINT 'T_Arts.Id_Localisation → nullable (aligné TestAPP).';
END
GO

-- ============================================================================
-- PHASE 3 — MIGRATION DE DONNÉES EXISTANTES
-- ============================================================================

PRINT '--- Migration logique colonnes renommées ---';

-- CustomerDescription → CustomerName
IF COL_LENGTH('dbo.T_Customer', 'CustomerDescription') IS NOT NULL
BEGIN
    UPDATE dbo.T_Customer
    SET CustomerName = CustomerDescription
    WHERE CustomerName IS NULL AND CustomerDescription IS NOT NULL;
    PRINT CONCAT('CustomerName rempli depuis CustomerDescription : ', @@ROWCOUNT, ' ligne(s).');
END
GO

-- User / Localisation → Id_User / Id_Localisation
UPDATE dbo.T_User_Localisations
SET Id_User = [User]
WHERE Id_User IS NULL AND [User] IS NOT NULL;
UPDATE dbo.T_User_Localisations
SET Id_Localisation = Localisation
WHERE Id_Localisation IS NULL AND Localisation IS NOT NULL;
PRINT 'T_User_Localisations : Id_User / Id_Localisation synchronisés.';
GO

-- Attribuer rôle par défaut aux utilisateurs existants (Administrateur = 1)
IF EXISTS (SELECT 1 FROM dbo.T_Roles WHERE RoleId = 1)
BEGIN
    UPDATE dbo.T_Users SET RoleId = 1 WHERE RoleId IS NULL;
    UPDATE dbo.T_Users SET Activated = 1 WHERE Activated IS NULL;
    PRINT 'T_Users : RoleId=1 et Activated=1 par défaut.';
END
GO

-- T_Appros → T_Appro_Details (5204 appros prod)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Appros') AND name = 'Id_Article')
BEGIN
    INSERT INTO dbo.T_Appro_Details (Id_Appro, Id_Article, Id_Localisation, Qte, PA, PV, Ben, BenTotal, Taux, DateSys, [User], Cumputer)
    SELECT
        a.Id,
        a.Id_Article,
        ISNULL(COALESCE(a.LocalisationId, art.Id_Localisation), 1),
        a.Qte,
        a.PA, a.PV, a.Ben, a.BenTotal, a.Taux,
        a.DateSys, a.[User], a.Cumputer
    FROM dbo.T_Appros a
    LEFT JOIN dbo.T_Arts art ON art.Id_Article = a.Id_Article
    WHERE a.Id_Article IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM dbo.T_Appro_Details d
          WHERE d.Id_Appro = a.Id AND d.Id_Article = a.Id_Article
      );
    PRINT CONCAT('T_Appro_Details : ', @@ROWCOUNT, ' ligne(s) migrées depuis T_Appros.');
END
GO

-- T_Arts → T_Stock (stock par localisation)
INSERT INTO dbo.T_Stock (Id_Article, Id_Localisation, Qte, Seuil, DateSys, UserLogin)
SELECT
    a.Id_Article,
    a.Id_Localisation,
    ISNULL(a.Qte, 0),
    ISNULL(a.Soeuil, 0),
    a.DateSys,
    a.[User]
FROM dbo.T_Arts a
WHERE a.Id_Localisation IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM dbo.T_Stock s
      WHERE s.Id_Article = a.Id_Article AND s.Id_Localisation = a.Id_Localisation
  );
PRINT CONCAT('T_Stock : ', @@ROWCOUNT, ' ligne(s) créées depuis T_Arts.');
GO

-- ============================================================================
-- PHASE 4 — DONNÉES DE RÉFÉRENCE (uniquement si tables vides)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM dbo.T_YesNo)
BEGIN
    INSERT INTO dbo.T_YesNo (Id, Description) VALUES (0, 'Non'), (1, 'Oui');
    PRINT 'Seed : T_YesNo';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.T_Roles)
BEGIN
    SET IDENTITY_INSERT dbo.T_Roles ON;
    INSERT INTO dbo.T_Roles (RoleId, Description_Role, IsActive) VALUES
        (1, N'Administrateur', 1),
        (2, N'Gérant', 1),
        (3, N'Vendeur', 1),
        (4, N'Caissier', 1),
        (5, N'Magasinier', 1);
    SET IDENTITY_INSERT dbo.T_Roles OFF;
    PRINT 'Seed : T_Roles';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.T_Customer_Type)
BEGIN
    SET IDENTITY_INSERT dbo.T_Customer_Type ON;
    INSERT INTO dbo.T_Customer_Type (CustomerTypeId, Description) VALUES
        (0, 'Anonyme'), (1, 'Particulier'), (2, 'Entreprise');
    SET IDENTITY_INSERT dbo.T_Customer_Type OFF;
    PRINT 'Seed : T_Customer_Type';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.T_Expense_Source)
BEGIN
    INSERT INTO dbo.T_Expense_Source (Sources) VALUES ('Caisse'), ('Banque'), (N'Clôture journalière');
    PRINT 'Seed : T_Expense_Source';
END
GO

-- ============================================================================
-- PHASE 5 — INDEX (performance)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mouvement_Article' AND object_id = OBJECT_ID('dbo.T_Mouvement_Stock'))
    CREATE INDEX IX_Mouvement_Article ON dbo.T_Mouvement_Stock (Id_Article);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mouvement_Localisation' AND object_id = OBJECT_ID('dbo.T_Mouvement_Stock'))
    CREATE INDEX IX_Mouvement_Localisation ON dbo.T_Mouvement_Stock (Id_Localisation);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mouvement_Date' AND object_id = OBJECT_ID('dbo.T_Mouvement_Stock'))
    CREATE INDEX IX_Mouvement_Date ON dbo.T_Mouvement_Stock (Date_Mouvement);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stock_Article' AND object_id = OBJECT_ID('dbo.T_Stock'))
    CREATE INDEX IX_Stock_Article ON dbo.T_Stock (Id_Article);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stock_Localisation' AND object_id = OBJECT_ID('dbo.T_Stock'))
    CREATE INDEX IX_Stock_Localisation ON dbo.T_Stock (Id_Localisation);
GO

-- ============================================================================
-- PHASE 6 — CLÉS ÉTRANGÈRES (après migration données)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Appro')
    ALTER TABLE dbo.T_Appro_Details ADD CONSTRAINT FK_ApproDetails_Appro
        FOREIGN KEY (Id_Appro) REFERENCES dbo.T_Appros (Id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Article')
    ALTER TABLE dbo.T_Appro_Details ADD CONSTRAINT FK_ApproDetails_Article
        FOREIGN KEY (Id_Article) REFERENCES dbo.T_Arts (Id_Article);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Localisation')
    ALTER TABLE dbo.T_Appro_Details ADD CONSTRAINT FK_ApproDetails_Localisation
        FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Stock_Article')
    ALTER TABLE dbo.T_Stock ADD CONSTRAINT FK_Stock_Article
        FOREIGN KEY (Id_Article) REFERENCES dbo.T_Arts (Id_Article);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Stock_Localisation')
    ALTER TABLE dbo.T_Stock ADD CONSTRAINT FK_Stock_Localisation
        FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Mouvement_Article')
    ALTER TABLE dbo.T_Mouvement_Stock ADD CONSTRAINT FK_Mouvement_Article
        FOREIGN KEY (Id_Article) REFERENCES dbo.T_Arts (Id_Article);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Mouvement_Localisation')
    ALTER TABLE dbo.T_Mouvement_Stock ADD CONSTRAINT FK_Mouvement_Localisation
        FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Payments_Facts')
    ALTER TABLE dbo.T_Payments ADD CONSTRAINT FK_Payments_Facts
        FOREIGN KEY (Id_Fact) REFERENCES dbo.T_Facts (Id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Roles')
    ALTER TABLE dbo.T_Users ADD CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.T_Roles (RoleId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserLocalisation_User')
    ALTER TABLE dbo.T_User_Localisations ADD CONSTRAINT FK_UserLocalisation_User
        FOREIGN KEY (Id_User) REFERENCES dbo.T_Users (login);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserLocalisation_Localisation')
    ALTER TABLE dbo.T_User_Localisations ADD CONSTRAINT FK_UserLocalisation_Localisation
        FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Customer_CustomerType')
    ALTER TABLE dbo.T_Customer ADD CONSTRAINT FK_Customer_CustomerType
        FOREIGN KEY (CustomerTypeId) REFERENCES dbo.T_Customer_Type (CustomerTypeId);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproExpense_Versement')
    ALTER TABLE dbo.T_Appro_Expense ADD CONSTRAINT FK_ApproExpense_Versement
        FOREIGN KEY (VersementId) REFERENCES dbo.T_Versements (Id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Versements_T_Localisations')
    ALTER TABLE dbo.T_Versements ADD CONSTRAINT FK_T_Versements_T_Localisations
        FOREIGN KEY (LocalisationId) REFERENCES dbo.T_Localisations (Id_Localisation);
GO

-- ============================================================================
-- VÉRIFICATION FINALE
-- ============================================================================
PRINT '';
PRINT '=== Vérification post-migration ===';

SELECT 'Tables GlobalShoping' AS Info, COUNT(*) AS Valeur
FROM sys.tables WHERE is_ms_shipped = 0;

SELECT 'T_Stock' AS [Table], COUNT(*) AS Lignes FROM dbo.T_Stock
UNION ALL SELECT 'T_Appro_Details', COUNT(*) FROM dbo.T_Appro_Details
UNION ALL SELECT 'T_Appros', COUNT(*) FROM dbo.T_Appros
UNION ALL SELECT 'T_Facts', COUNT(*) FROM dbo.T_Facts
UNION ALL SELECT 'T_Roles', COUNT(*) FROM dbo.T_Roles;

PRINT '';
PRINT '=== Migration terminée. Vérifiez les comptages ci-dessus. ===';
PRINT 'Prochaine étape : pointer appsettings DefaultConnection vers GlobalShoping et tester l''application.';
GO
