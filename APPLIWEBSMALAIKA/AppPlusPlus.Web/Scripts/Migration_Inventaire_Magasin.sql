-- ============================================================================
-- Migration : Inventaire magasin — registre de valeur + mouvements stock
--
-- Objectif métier :
--   Suivre la situation réelle de chaque magasin en VALEUR (FC) et en STOCK.
--   Valeur théorique = Montant_Initial
--                    + SUM(réceptions clôturées)
--                    - SUM(ventes journalières)
--
-- Tables :
--   T_Inventaire_Magasin          Session / période par magasin
--   T_Inventaire_Reception        Documents de réception (modifiables tant qu'ouverts)
--   T_Inventaire_Reception_Detail Lignes articles des réceptions
--   T_Inventaire_Vente            Montants de vente journaliers (saisie globale)
--
-- Statuts session (T_Inventaire_Magasin.Statut) :
--   0 = Brouillon | 1 = Ouvert | 2 = Clôturé
--
-- Statuts réception (T_Inventaire_Reception.Statut) :
--   0 = Ouvert (modifiable) | 1 = Clôturé (figé, impacte la valeur)
--
-- Intégration stock (couche applicative à la clôture réception) :
--   T_Mouvement_Stock Type_Document = 'INVENTAIRE', Type_Mouvement = 'ENTREE'
--
-- Database: GlobalShoping  (adapter USE si TestAPP)
-- ============================================================================

USE GlobalShoping;
GO
SET NOCOUNT ON;

-- ── Nettoyage éventuel du premier brouillon de script (comptage physique) ───
IF OBJECT_ID('dbo.TR_T_Inventaire_Details_RefreshTotal', 'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_T_Inventaire_Details_RefreshTotal;
GO

IF OBJECT_ID('dbo.T_Inventaire_Details', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.T_Inventaire_Details;
    PRINT 'Ancienne table T_Inventaire_Details (comptage) supprimée.';
END
GO

IF OBJECT_ID('dbo.T_Inventaire_Magasin', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.T_Inventaire_Magasin;
    PRINT 'Ancienne table T_Inventaire_Magasin supprimée (remplacée par le nouveau modèle).';
END
GO

-- ── T_Inventaire_Magasin : session / période ────────────────────────────────
IF OBJECT_ID('dbo.T_Inventaire_Magasin', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Inventaire_Magasin (
        Id_Inventaire       INT IDENTITY(1,1) NOT NULL,
        Id_Localisation     INT NOT NULL,
        Nom                 NVARCHAR(100) NOT NULL,
        Description         NVARCHAR(500) NULL,
        -- 0 = Journée | 1 = Mois | 2 = Exercice
        Type_Periode        INT NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Type_Periode DEFAULT 0,
        Date_Debut          DATE NOT NULL,
        Date_Fin            DATE NULL,
        Montant_Initial     DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Montant_Initial DEFAULT 0,
        Montant_Receptions  DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Montant_Receptions DEFAULT 0,
        Montant_Ventes      DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Montant_Ventes DEFAULT 0,
        Montant_Theorique   AS (Montant_Initial + Montant_Receptions - Montant_Ventes) PERSISTED,
        Statut              INT NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Statut DEFAULT 0,
        Cree_Par            NVARCHAR(50) NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Cree_Par DEFAULT 'SYSTEM',
        Date_Creation       DATETIME NOT NULL
            CONSTRAINT DF_T_Inventaire_Magasin_Date_Creation DEFAULT GETDATE(),
        Date_Cloture        DATETIME NULL,
        Cloture_Par         NVARCHAR(50) NULL,
        CONSTRAINT PK_T_Inventaire_Magasin
            PRIMARY KEY CLUSTERED (Id_Inventaire),
        CONSTRAINT FK_T_Inventaire_Magasin_Localisation
            FOREIGN KEY (Id_Localisation) REFERENCES dbo.T_Localisations (Id_Localisation),
        CONSTRAINT CK_T_Inventaire_Magasin_Statut
            CHECK (Statut IN (0, 1, 2)),
        CONSTRAINT CK_T_Inventaire_Magasin_Type_Periode
            CHECK (Type_Periode IN (0, 1, 2)),
        CONSTRAINT CK_T_Inventaire_Magasin_Montant_Initial
            CHECK (Montant_Initial >= 0),
        CONSTRAINT CK_T_Inventaire_Magasin_Montant_Receptions
            CHECK (Montant_Receptions >= 0),
        CONSTRAINT CK_T_Inventaire_Magasin_Montant_Ventes
            CHECK (Montant_Ventes >= 0)
    );

    -- Une seule session ouverte par magasin
    CREATE UNIQUE INDEX UQ_T_Inventaire_Magasin_Localisation_Ouvert
        ON dbo.T_Inventaire_Magasin (Id_Localisation)
        WHERE Statut IN (0, 1);

    CREATE INDEX IX_T_Inventaire_Magasin_Statut
        ON dbo.T_Inventaire_Magasin (Statut);

    CREATE INDEX IX_T_Inventaire_Magasin_Date_Debut
        ON dbo.T_Inventaire_Magasin (Date_Debut DESC);

    PRINT 'Table T_Inventaire_Magasin créée.';
END
ELSE
    PRINT 'Table T_Inventaire_Magasin existe déjà.';
GO

-- ── T_Inventaire_Reception : documents de réception ─────────────────────────
IF OBJECT_ID('dbo.T_Inventaire_Reception', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Inventaire_Reception (
        Id_Reception        INT IDENTITY(1,1) NOT NULL,
        Id_Inventaire       INT NOT NULL,
        Numero_Reception    NVARCHAR(30) NULL,
        Reference           NVARCHAR(50) NULL,
        -- 0 = Ouvert (modifiable) | 1 = Clôturé
        Statut              INT NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Statut DEFAULT 0,
        Montant_Total       DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Montant_Total DEFAULT 0,
        Observation         NVARCHAR(255) NULL,
        Cree_Par            NVARCHAR(50) NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Cree_Par DEFAULT 'SYSTEM',
        Date_Creation       DATETIME NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Date_Creation DEFAULT GETDATE(),
        Date_Jour           DATE NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Date_Jour DEFAULT (CAST(GETDATE() AS DATE)),
        Date_Cloture        DATETIME NULL,
        Cloture_Par         NVARCHAR(50) NULL,
        CONSTRAINT PK_T_Inventaire_Reception
            PRIMARY KEY CLUSTERED (Id_Reception),
        CONSTRAINT FK_T_Inventaire_Reception_Inventaire
            FOREIGN KEY (Id_Inventaire)
            REFERENCES dbo.T_Inventaire_Magasin (Id_Inventaire),
        CONSTRAINT CK_T_Inventaire_Reception_Statut
            CHECK (Statut IN (0, 1)),
        CONSTRAINT CK_T_Inventaire_Reception_Montant_Total
            CHECK (Montant_Total >= 0)
    );

    CREATE INDEX IX_T_Inventaire_Reception_Inventaire
        ON dbo.T_Inventaire_Reception (Id_Inventaire);

    CREATE INDEX IX_T_Inventaire_Reception_Statut
        ON dbo.T_Inventaire_Reception (Statut);

    CREATE INDEX IX_T_Inventaire_Reception_Date_Creation
        ON dbo.T_Inventaire_Reception (Date_Creation DESC);

    CREATE UNIQUE INDEX UQ_T_Inventaire_Reception_Inventaire_Jour
        ON dbo.T_Inventaire_Reception (Id_Inventaire, Date_Jour);

    PRINT 'Table T_Inventaire_Reception créée.';
END
ELSE
    PRINT 'Table T_Inventaire_Reception existe déjà.';
GO

-- ── T_Inventaire_Reception_Detail : lignes articles ─────────────────────────
IF OBJECT_ID('dbo.T_Inventaire_Reception_Detail', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Inventaire_Reception_Detail (
        Id                  INT IDENTITY(1,1) NOT NULL,
        Id_Reception        INT NOT NULL,
        Id_Article          VARCHAR(100) NOT NULL,
        Quantite            DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Detail_Quantite DEFAULT 0,
        Prix_Unitaire       DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Detail_Prix_Unitaire DEFAULT 0,
        Montant_Ligne       AS (Quantite * Prix_Unitaire) PERSISTED,
        Observation         NVARCHAR(255) NULL,
        Date_Ligne          DATETIME NOT NULL
            CONSTRAINT DF_T_Inventaire_Reception_Detail_Date_Ligne DEFAULT GETDATE(),
        CONSTRAINT PK_T_Inventaire_Reception_Detail
            PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_T_Inventaire_Reception_Detail_Reception
            FOREIGN KEY (Id_Reception)
            REFERENCES dbo.T_Inventaire_Reception (Id_Reception)
            ON DELETE CASCADE,
        CONSTRAINT FK_T_Inventaire_Reception_Detail_Article
            FOREIGN KEY (Id_Article)
            REFERENCES dbo.T_Arts (Id_Article),
        CONSTRAINT CK_T_Inventaire_Reception_Detail_Quantite
            CHECK (Quantite >= 0),
        CONSTRAINT CK_T_Inventaire_Reception_Detail_Prix_Unitaire
            CHECK (Prix_Unitaire >= 0)
    );

    CREATE INDEX IX_T_Inventaire_Reception_Detail_Reception
        ON dbo.T_Inventaire_Reception_Detail (Id_Reception);

    CREATE INDEX IX_T_Inventaire_Reception_Detail_Article
        ON dbo.T_Inventaire_Reception_Detail (Id_Article);

    PRINT 'Table T_Inventaire_Reception_Detail créée.';
END
ELSE
    PRINT 'Table T_Inventaire_Reception_Detail existe déjà.';
GO

-- ── T_Inventaire_Vente : montants journaliers globaux ───────────────────────
IF OBJECT_ID('dbo.T_Inventaire_Vente', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Inventaire_Vente (
        Id                  INT IDENTITY(1,1) NOT NULL,
        Id_Inventaire       INT NOT NULL,
        Date_Vente          DATE NOT NULL,
        Montant             DECIMAL(18, 2) NOT NULL,
        Observation         NVARCHAR(255) NULL,
        Cree_Par            NVARCHAR(50) NOT NULL
            CONSTRAINT DF_T_Inventaire_Vente_Cree_Par DEFAULT 'SYSTEM',
        Date_Creation       DATETIME NOT NULL
            CONSTRAINT DF_T_Inventaire_Vente_Date_Creation DEFAULT GETDATE(),
        CONSTRAINT PK_T_Inventaire_Vente
            PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_T_Inventaire_Vente_Inventaire
            FOREIGN KEY (Id_Inventaire)
            REFERENCES dbo.T_Inventaire_Magasin (Id_Inventaire),
        CONSTRAINT UQ_T_Inventaire_Vente_Inventaire_Date
            UNIQUE (Id_Inventaire, Date_Vente),
        CONSTRAINT CK_T_Inventaire_Vente_Montant
            CHECK (Montant >= 0)
    );

    CREATE INDEX IX_T_Inventaire_Vente_Inventaire
        ON dbo.T_Inventaire_Vente (Id_Inventaire);

    CREATE INDEX IX_T_Inventaire_Vente_Date
        ON dbo.T_Inventaire_Vente (Date_Vente DESC);

    PRINT 'Table T_Inventaire_Vente créée.';
END
ELSE
    PRINT 'Table T_Inventaire_Vente existe déjà.';
GO

-- ── Trigger : total d'une réception depuis ses lignes ───────────────────────
CREATE OR ALTER TRIGGER dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal
ON dbo.T_Inventaire_Reception_Detail
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Affected AS (
        SELECT Id_Reception FROM inserted
        UNION
        SELECT Id_Reception FROM deleted
    )
    UPDATE r
    SET Montant_Total = ISNULL(agg.Somme, 0)
    FROM dbo.T_Inventaire_Reception r
    INNER JOIN Affected a ON a.Id_Reception = r.Id_Reception
    OUTER APPLY (
        SELECT SUM(d.Montant_Ligne) AS Somme
        FROM dbo.T_Inventaire_Reception_Detail d
        WHERE d.Id_Reception = r.Id_Reception
    ) agg;

    DECLARE @Sessions TABLE (Id_Inventaire INT NOT NULL PRIMARY KEY);

    INSERT INTO @Sessions (Id_Inventaire)
    SELECT DISTINCT r.Id_Inventaire
    FROM dbo.T_Inventaire_Reception r
    INNER JOIN (
        SELECT Id_Reception FROM inserted
        UNION
        SELECT Id_Reception FROM deleted
    ) a ON a.Id_Reception = r.Id_Reception
    WHERE r.Statut = 1;

    UPDATE inv
    SET Montant_Receptions = ISNULL(rec.Somme, 0)
    FROM dbo.T_Inventaire_Magasin inv
    INNER JOIN @Sessions s ON s.Id_Inventaire = inv.Id_Inventaire
    OUTER APPLY (
        SELECT SUM(r.Montant_Total) AS Somme
        FROM dbo.T_Inventaire_Reception r
        WHERE r.Id_Inventaire = inv.Id_Inventaire
          AND r.Statut = 1
    ) rec;
END;
GO

-- ── Trigger : agrégats session quand une réception change de statut ─────────
CREATE OR ALTER TRIGGER dbo.TR_T_Inventaire_Reception_RefreshSession
ON dbo.T_Inventaire_Reception
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Affected AS (
        SELECT Id_Inventaire FROM inserted
        UNION
        SELECT Id_Inventaire FROM deleted
    )
    UPDATE inv
    SET Montant_Receptions = ISNULL(rec.Somme, 0)
    FROM dbo.T_Inventaire_Magasin inv
    INNER JOIN Affected a ON a.Id_Inventaire = inv.Id_Inventaire
    OUTER APPLY (
        SELECT SUM(r.Montant_Total) AS Somme
        FROM dbo.T_Inventaire_Reception r
        WHERE r.Id_Inventaire = inv.Id_Inventaire
          AND r.Statut = 1
    ) rec;
END;
GO

-- ── Trigger : total ventes session ──────────────────────────────────────────
CREATE OR ALTER TRIGGER dbo.TR_T_Inventaire_Vente_RefreshSession
ON dbo.T_Inventaire_Vente
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Affected AS (
        SELECT Id_Inventaire FROM inserted
        UNION
        SELECT Id_Inventaire FROM deleted
    )
    UPDATE inv
    SET Montant_Ventes = ISNULL(v.Somme, 0)
    FROM dbo.T_Inventaire_Magasin inv
    INNER JOIN Affected a ON a.Id_Inventaire = inv.Id_Inventaire
    OUTER APPLY (
        SELECT SUM(vt.Montant) AS Somme
        FROM dbo.T_Inventaire_Vente vt
        WHERE vt.Id_Inventaire = inv.Id_Inventaire
    ) v;
END;
GO

-- ── Vue : situation magasin en temps réel ───────────────────────────────────
CREATE OR ALTER VIEW dbo.V_Inventaire_Situation_Magasin
AS
SELECT
    inv.Id_Inventaire,
    inv.Id_Localisation,
    loc.Description_Localisation   AS Nom_Magasin,
    inv.Nom,
    inv.Type_Periode,
    inv.Date_Debut,
    inv.Date_Fin,
    inv.Statut,
    inv.Montant_Initial,
    inv.Montant_Receptions,
    inv.Montant_Ventes,
    inv.Montant_Theorique,
    inv.Cree_Par,
    inv.Date_Creation,
    (SELECT COUNT(*) FROM dbo.T_Inventaire_Reception r
     WHERE r.Id_Inventaire = inv.Id_Inventaire AND r.Statut = 1) AS Nb_Receptions_Cloturees,
    (SELECT COUNT(*) FROM dbo.T_Inventaire_Reception r
     WHERE r.Id_Inventaire = inv.Id_Inventaire AND r.Statut = 0) AS Nb_Receptions_Ouvertes,
    (SELECT COUNT(*) FROM dbo.T_Inventaire_Vente v
     WHERE v.Id_Inventaire = inv.Id_Inventaire) AS Nb_Jours_Vente
FROM dbo.T_Inventaire_Magasin inv
INNER JOIN dbo.T_Localisations loc ON loc.Id_Localisation = inv.Id_Localisation;
GO

PRINT 'Migration Inventaire Magasin (registre de valeur) terminée.';
GO

-- ── Exemple de contrôle (décommenter pour test) ─────────────────────────────
/*
DECLARE @IdInv INT;

INSERT INTO dbo.T_Inventaire_Magasin (Id_Localisation, Nom, Date_Debut, Montant_Initial, Statut, Cree_Par)
VALUES (1, N'Inventaire test juillet', '2026-07-31', 1000000, 1, 'TEST');
SET @IdInv = SCOPE_IDENTITY();

DECLARE @IdRec INT;
INSERT INTO dbo.T_Inventaire_Reception (Id_Inventaire, Numero_Reception, Cree_Par)
VALUES (@IdInv, N'REC-001', 'TEST');
SET @IdRec = SCOPE_IDENTITY();

INSERT INTO dbo.T_Inventaire_Reception_Detail (Id_Reception, Id_Article, Quantite, Prix_Unitaire)
VALUES (@IdRec, 'ART001', 10, 25000);  -- +250 000 FC

UPDATE dbo.T_Inventaire_Reception SET Statut = 1, Date_Cloture = GETDATE(), Cloture_Par = 'TEST'
WHERE Id_Reception = @IdRec;

INSERT INTO dbo.T_Inventaire_Vente (Id_Inventaire, Date_Vente, Montant, Cree_Par)
VALUES (@IdInv, '2026-07-31', 350000, 'TEST');

SELECT * FROM dbo.V_Inventaire_Situation_Magasin WHERE Id_Inventaire = @IdInv;
-- Attendu : Initial=1 000 000 | Réceptions=250 000 | Ventes=350 000 | Théorique=900 000
*/
