-- ============================================================================
-- Migration : Demandes génériques (réappro interne dépôt → agents)
-- - Is_Depot sur T_Localisations
-- - T_Demandes + T_Demande_Details
-- - sp_GenererDemandesGeneriques (job quotidien 18:30)
-- Database: GlobalShoping
-- ============================================================================

USE GlobalShoping;
GO
SET NOCOUNT ON;

-- ── Dépôt central sur localisations ──
IF COL_LENGTH('T_Localisations', 'Is_Depot') IS NULL
BEGIN
    ALTER TABLE T_Localisations
        ADD Is_Depot BIT NOT NULL CONSTRAINT DF_T_Localisations_Is_Depot DEFAULT 0;
    PRINT 'Colonne Is_Depot ajoutée à T_Localisations.';
END
ELSE
    PRINT 'Colonne Is_Depot existe déjà sur T_Localisations.';
GO

-- ── T_Demandes ──
IF OBJECT_ID('T_Demandes', 'U') IS NULL
BEGIN
    CREATE TABLE T_Demandes (
        Id_Demande                INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Statut                    INT NOT NULL CONSTRAINT DF_T_Demandes_Statut DEFAULT 0,
        Id_Localisation_Demandeur INT NOT NULL,
        Id_Localisation_Source    INT NOT NULL,
        Commentaire               NVARCHAR(255) NULL,
        Commentaire_Admin         NVARCHAR(255) NULL,
        Cree_Par                  NVARCHAR(50) NOT NULL CONSTRAINT DF_T_Demandes_Cree_Par DEFAULT 'SYSTEM',
        Date_Creation             DATETIME NOT NULL CONSTRAINT DF_T_Demandes_Date_Creation DEFAULT GETDATE(),
        Admin_Approuve_Par        NVARCHAR(50) NULL,
        Date_Admin_Approuve       DATETIME NULL,
        Agent_Approuve_Par        NVARCHAR(50) NULL,
        Date_Agent_Approuve       DATETIME NULL,
        CONSTRAINT FK_T_Demandes_Loc_Demandeur
            FOREIGN KEY (Id_Localisation_Demandeur) REFERENCES T_Localisations(Id_Localisation),
        CONSTRAINT FK_T_Demandes_Loc_Source
            FOREIGN KEY (Id_Localisation_Source) REFERENCES T_Localisations(Id_Localisation)
    );
    CREATE INDEX IX_T_Demandes_Statut ON T_Demandes(Statut);
    CREATE INDEX IX_T_Demandes_Loc_Demandeur ON T_Demandes(Id_Localisation_Demandeur);
    PRINT 'Table T_Demandes créée.';
END
ELSE
    PRINT 'Table T_Demandes existe déjà.';
GO

-- ── T_Demande_Details ──
IF OBJECT_ID('T_Demande_Details', 'U') IS NULL
BEGIN
    CREATE TABLE T_Demande_Details (
        Id                   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Id_Demande           INT NOT NULL,
        Id_Article           VARCHAR(100) NOT NULL,
        Qte_Demandee         DECIMAL(18,2) NOT NULL,
        Qte_Approuvee        DECIMAL(18,2) NULL,
        Id_Stock_Demandeur   INT NULL,
        CONSTRAINT FK_T_Demande_Details_Demande
            FOREIGN KEY (Id_Demande) REFERENCES T_Demandes(Id_Demande) ON DELETE CASCADE,
        CONSTRAINT FK_T_Demande_Details_Article
            FOREIGN KEY (Id_Article) REFERENCES T_Arts(Id_Article)
    );
    CREATE INDEX IX_T_Demande_Details_Demande ON T_Demande_Details(Id_Demande);
    PRINT 'Table T_Demande_Details créée.';
END
ELSE
    PRINT 'Table T_Demande_Details existe déjà.';
GO

-- ── Procédure : génération automatique des demandes ──
CREATE OR ALTER PROCEDURE dbo.sp_GenererDemandesGeneriques
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DepotId INT;
    SELECT TOP 1 @DepotId = Id_Localisation
    FROM T_Localisations
    WHERE Is_Depot = 1
    ORDER BY Id_Localisation;

    IF @DepotId IS NULL
    BEGIN
        RAISERROR(N'Aucune localisation dépôt (Is_Depot = 1). Configurez un dépôt dans Administration.', 16, 1);
        RETURN;
    END

    ;WITH OpenArticles AS (
        SELECT d.Id_Localisation_Demandeur, dd.Id_Article
        FROM T_Demandes d
        INNER JOIN T_Demande_Details dd ON dd.Id_Demande = d.Id_Demande
        WHERE d.Statut IN (0, 1)
    ),
    Eligible AS (
        SELECT
            s.Id              AS Id_Stock,
            s.Id_Article,
            s.Id_Localisation AS Id_Loc_Demandeur,
            CAST(CASE WHEN s.Qte_max > s.Qte THEN s.Qte_max - s.Qte ELSE 0 END AS DECIMAL(18,2)) AS Qte_Demandee
        FROM T_Stock s
        INNER JOIN T_Localisations l ON l.Id_Localisation = s.Id_Localisation
        WHERE s.Seuil > 0
          AND s.Qte <= s.Seuil
          AND ISNULL(l.Is_Depot, 0) = 0
          AND s.Qte_max > s.Qte
          AND NOT EXISTS (
              SELECT 1 FROM OpenArticles oa
              WHERE oa.Id_Localisation_Demandeur = s.Id_Localisation
                AND oa.Id_Article = s.Id_Article
          )
    )
    SELECT * INTO #Eligible FROM Eligible WHERE Qte_Demandee > 0;

    IF NOT EXISTS (SELECT 1 FROM #Eligible)
    BEGIN
        PRINT 'Aucun article éligible pour une nouvelle demande.';
        RETURN;
    END

    DECLARE @LocId INT;
    DECLARE @DemandeId INT;

    DECLARE loc_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT Id_Loc_Demandeur FROM #Eligible ORDER BY Id_Loc_Demandeur;

    OPEN loc_cursor;
    FETCH NEXT FROM loc_cursor INTO @LocId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO T_Demandes (
            Statut, Id_Localisation_Demandeur, Id_Localisation_Source,
            Commentaire, Cree_Par, Date_Creation
        )
        VALUES (
            0, @LocId, @DepotId,
            N'Demande auto — stock sous seuil', N'SYSTEM', GETDATE()
        );

        SET @DemandeId = SCOPE_IDENTITY();

        INSERT INTO T_Demande_Details (Id_Demande, Id_Article, Qte_Demandee, Id_Stock_Demandeur)
        SELECT @DemandeId, Id_Article, Qte_Demandee, Id_Stock
        FROM #Eligible
        WHERE Id_Loc_Demandeur = @LocId;

        FETCH NEXT FROM loc_cursor INTO @LocId;
    END

    CLOSE loc_cursor;
    DEALLOCATE loc_cursor;

    DROP TABLE #Eligible;

    PRINT 'Génération des demandes génériques terminée.';
END
GO

PRINT 'Migration Demandes génériques terminée.';
PRINT 'Configurez une localisation comme dépôt (Is_Depot = 1) via Administration.';
PRINT 'Exécutez Migration_DemandesGeneriques_Job.sql pour créer le job SQL Agent (18:30).';
GO
