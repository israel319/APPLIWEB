-- ============================================================================
-- PATCH migration GlobalShoping (complète T_Appro_Details + T_Mouvement_Stock)
-- Exécuter après Migration_GlobalShoping_Align_TestAPP.sql
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

-- T_Appro_Details (PK nom explicite pour éviter conflit)
IF OBJECT_ID('dbo.T_Appro_Details', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Appro_Details (
        Id              INT           IDENTITY(1,1) NOT NULL,
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
        Cumputer        VARCHAR(50)   NULL,
        CONSTRAINT PK_T_Appro_Details_Id PRIMARY KEY (Id)
    );
    PRINT 'Créé : T_Appro_Details';
END
GO

-- T_Mouvement_Stock (QUOTED_IDENTIFIER requis pour colonne calculée)
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

-- Migration T_Appros → T_Appro_Details
IF OBJECT_ID('dbo.T_Appro_Details', 'U') IS NOT NULL
AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Appros') AND name = 'Id_Article')
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
    PRINT CONCAT('T_Appro_Details migrées : ', @@ROWCOUNT);
END
GO

-- Index Mouvement + FK manquantes
IF OBJECT_ID('dbo.T_Mouvement_Stock', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mouvement_Article' AND object_id = OBJECT_ID('dbo.T_Mouvement_Stock'))
        CREATE INDEX IX_Mouvement_Article ON dbo.T_Mouvement_Stock (Id_Article);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mouvement_Localisation' AND object_id = OBJECT_ID('dbo.T_Mouvement_Stock'))
        CREATE INDEX IX_Mouvement_Localisation ON dbo.T_Mouvement_Stock (Id_Localisation);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Mouvement_Date' AND object_id = OBJECT_ID('dbo.T_Mouvement_Stock'))
        CREATE INDEX IX_Mouvement_Date ON dbo.T_Mouvement_Stock (Date_Mouvement);
END
GO

IF OBJECT_ID('dbo.T_Appro_Details', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Appro')
        ALTER TABLE dbo.T_Appro_Details ADD CONSTRAINT FK_ApproDetails_Appro
            FOREIGN KEY (Id_Appro) REFERENCES dbo.T_Appros (Id);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Article')
        ALTER TABLE dbo.T_Appro_Details ADD CONSTRAINT FK_ApproDetails_Article
            FOREIGN KEY (Id_Article) REFERENCES dbo.T_Arts (Id_Article);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Localisation')
        ALTER TABLE dbo.T_Appro_Details ADD CONSTRAINT FK_ApproDetails_Localisation
            FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation);
END
GO

IF OBJECT_ID('dbo.T_Mouvement_Stock', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Mouvement_Article')
        ALTER TABLE dbo.T_Mouvement_Stock ADD CONSTRAINT FK_Mouvement_Article
            FOREIGN KEY (Id_Article) REFERENCES dbo.T_Arts (Id_Article);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Mouvement_Localisation')
        ALTER TABLE dbo.T_Mouvement_Stock ADD CONSTRAINT FK_Mouvement_Localisation
            FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation);
END
GO

SELECT 'T_Stock' AS [Table], COUNT(*) AS Lignes FROM dbo.T_Stock
UNION ALL SELECT 'T_Appro_Details', COUNT(*) FROM dbo.T_Appro_Details
UNION ALL SELECT 'T_Appros', COUNT(*) FROM dbo.T_Appros
UNION ALL SELECT 'T_Mouvement_Stock', COUNT(*) FROM dbo.T_Mouvement_Stock;
GO
