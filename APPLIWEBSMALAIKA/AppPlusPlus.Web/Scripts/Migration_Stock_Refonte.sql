-- ============================================================================
-- SCRIPT DE REFONTE - GESTION DES STOCKS ET APPROVISIONNEMENTS
-- Database: TestAPP
-- Date: 2026-03-21
-- ============================================================================
-- ORDRE D'EXÉCUTION:
-- 1. Créer nouvelles tables (T_Appro_Details, T_Mouvement_Stock)
-- 2. Renommer T_Art_Localisations → T_Stock
-- 3. Migrer les données existantes
-- 4. Restructurer T_Appros
-- 5. Nettoyer T_Arts
-- ============================================================================

USE TestAPP;
GO

-- ============================================================================
-- ÉTAPE 1: CRÉER T_APPRO_DETAILS (Lignes d'approvisionnement)
-- ============================================================================
PRINT 'Création de T_Appro_Details...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_Appro_Details')
BEGIN
    CREATE TABLE T_Appro_Details (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Id_Appro INT NOT NULL,                    -- FK vers T_Appros
        Id_Article VARCHAR(100) NOT NULL,         -- FK vers T_Arts
        Id_Localisation INT NOT NULL,             -- FK vers T_Localisations

        Qte DECIMAL(18,2) NOT NULL DEFAULT 0,     -- Quantité reçue
        PA DECIMAL(18,2),                         -- Prix d'achat unitaire
        PV DECIMAL(18,2),                         -- Prix de vente suggéré
        Ben DECIMAL(18,2),                        -- Bénéfice unitaire
        BenTotal DECIMAL(18,2),                   -- Bénéfice total
        Taux DECIMAL(18,2),                       -- Taux de change

        DateSys DATETIME DEFAULT GETDATE(),
        [User] VARCHAR(50),
        Cumputer VARCHAR(50)
    );

    PRINT 'Table T_Appro_Details créée avec succès.';
END
ELSE
    PRINT 'Table T_Appro_Details existe déjà.';
GO

-- ============================================================================
-- ÉTAPE 2: CRÉER T_MOUVEMENT_STOCK (Traçabilité des mouvements)
-- ============================================================================
PRINT 'Création de T_Mouvement_Stock...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_Mouvement_Stock')
BEGIN
    CREATE TABLE T_Mouvement_Stock (
        Id INT PRIMARY KEY IDENTITY(1,1),

        -- Article et Localisation
        Id_Article VARCHAR(100) NOT NULL,
        Id_Localisation INT NOT NULL,

        -- Type de mouvement
        Type_Mouvement VARCHAR(20) NOT NULL,      -- ENTREE / SORTIE / TRANSFERT / AJUSTEMENT
        Quantite DECIMAL(18,2) NOT NULL,
        Qte_Avant DECIMAL(18,2),                  -- Stock avant mouvement
        Qte_Apres DECIMAL(18,2),                  -- Stock après mouvement

        Date_Mouvement DATETIME DEFAULT GETDATE(),

        -- Référence au document source
        Type_Document VARCHAR(50) NOT NULL,       -- APPRO / FACTURE / LIVRAISON / INVENTAIRE / TRANSFERT
        Id_Document INT,                          -- ID du document source
        Id_Document_Detail INT,                   -- ID de la ligne du document
        Reference VARCHAR(100),                   -- Référence lisible (APPRO-2024-001)

        -- Source et destination (pour transferts)
        Id_Localisation_Source INT,               -- Pour les transferts
        Id_Localisation_Dest INT,                 -- Pour les transferts

        -- Valeurs
        Prix_Unitaire DECIMAL(18,2),
        Valeur_Totale AS (Quantite * ISNULL(Prix_Unitaire, 0)) PERSISTED,

        -- Audit
        Observation VARCHAR(255),
        Cree_Par VARCHAR(50) NOT NULL,
        Date_Creation DATETIME DEFAULT GETDATE(),
        Valide_Par VARCHAR(50),
        Date_Validation DATETIME,
        Annule BIT DEFAULT 0
    );

    -- Contrainte CHECK pour Type_Mouvement
    ALTER TABLE T_Mouvement_Stock
    ADD CONSTRAINT CK_Mouvement_Type
    CHECK (Type_Mouvement IN ('ENTREE', 'SORTIE', 'TRANSFERT', 'AJUSTEMENT'));

    -- Index pour performance
    CREATE INDEX IX_Mouvement_Article ON T_Mouvement_Stock(Id_Article);
    CREATE INDEX IX_Mouvement_Localisation ON T_Mouvement_Stock(Id_Localisation);
    CREATE INDEX IX_Mouvement_Date ON T_Mouvement_Stock(Date_Mouvement);
    CREATE INDEX IX_Mouvement_Document ON T_Mouvement_Stock(Type_Document, Id_Document);

    PRINT 'Table T_Mouvement_Stock créée avec succès.';
END
ELSE
    PRINT 'Table T_Mouvement_Stock existe déjà.';
GO

-- ============================================================================
-- ÉTAPE 3: RENOMMER T_Art_Localisations → T_Stock
-- ============================================================================
PRINT 'Renommage de T_Art_Localisations → T_Stock...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'T_Art_Localisations')
   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'T_Stock')
BEGIN
    EXEC sp_rename 'T_Art_Localisations', 'T_Stock';
    PRINT 'Table renommée en T_Stock.';
END
ELSE IF EXISTS (SELECT * FROM sys.tables WHERE name = 'T_Stock')
    PRINT 'Table T_Stock existe déjà.';
ELSE
    PRINT 'ATTENTION: Table T_Art_Localisations introuvable!';
GO

-- Ajouter colonne Seuil à T_Stock
PRINT 'Ajout de la colonne Seuil à T_Stock...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'T_Stock')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Stock') AND name = 'Seuil')
BEGIN
    ALTER TABLE T_Stock ADD Seuil INT DEFAULT 0;
    PRINT 'Colonne Seuil ajoutée.';
END
ELSE
    PRINT 'Colonne Seuil existe déjà ou table introuvable.';
GO

-- Contrainte UNIQUE sur (Id_Article, Id_Localisation)
PRINT 'Ajout de la contrainte UNIQUE sur T_Stock...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Stock_Article_Loc')
BEGIN
    -- Vérifier s'il y a des doublons avant d'ajouter la contrainte
    IF NOT EXISTS (
        SELECT Id_Article, Id_Localisation, COUNT(*)
        FROM T_Stock
        GROUP BY Id_Article, Id_Localisation
        HAVING COUNT(*) > 1
    )
    BEGIN
        ALTER TABLE T_Stock ADD CONSTRAINT UQ_Stock_Article_Loc UNIQUE (Id_Article, Id_Localisation);
        PRINT 'Contrainte UNIQUE ajoutée.';
    END
    ELSE
        PRINT 'ATTENTION: Doublons détectés! Nettoyez les données avant d''ajouter la contrainte.';
END
ELSE
    PRINT 'Contrainte UNIQUE existe déjà.';
GO

-- Index pour performance sur T_Stock
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stock_Article')
BEGIN
    CREATE INDEX IX_Stock_Article ON T_Stock(Id_Article);
    PRINT 'Index IX_Stock_Article créé.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stock_Localisation')
BEGIN
    CREATE INDEX IX_Stock_Localisation ON T_Stock(Id_Localisation);
    PRINT 'Index IX_Stock_Localisation créé.';
END
GO

-- ============================================================================
-- ÉTAPE 4: MIGRER LES DONNÉES DE T_APPROS VERS T_APPRO_DETAILS
-- ============================================================================
PRINT 'Migration des données de T_Appros vers T_Appro_Details...';

-- Vérifier si T_Appros a encore la colonne Id_Article (données à migrer)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Appros') AND name = 'Id_Article')
BEGIN
    -- Migrer les données existantes
    INSERT INTO T_Appro_Details (Id_Appro, Id_Article, Id_Localisation, Qte, PA, PV, Ben, BenTotal, Taux, DateSys, [User], Cumputer)
    SELECT
        Id,
        Id_Article,
        ISNULL(LocalisationId, 1),  -- Localisation par défaut si NULL
        Qte,
        PA,
        PV,
        Ben,
        BenTotal,
        Taux,
        DateSys,
        [User],
        Cumputer
    FROM T_Appros
    WHERE Id_Article IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM T_Appro_Details ad
          WHERE ad.Id_Appro = T_Appros.Id
            AND ad.Id_Article = T_Appros.Id_Article
      );

    PRINT CONCAT(@@ROWCOUNT, ' lignes migrées vers T_Appro_Details.');
END
ELSE
    PRINT 'Colonnes déjà migrées ou inexistantes.';
GO

-- ============================================================================
-- ÉTAPE 5: MIGRER SEUIL DE T_ARTS VERS T_STOCK
-- ============================================================================
PRINT 'Migration du Seuil de T_Arts vers T_Stock...';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Arts') AND name = 'Soeuil')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Stock') AND name = 'Seuil')
BEGIN
    -- Mettre à jour le seuil dans T_Stock depuis T_Arts
    UPDATE s
    SET s.Seuil = ISNULL(a.Soeuil, 0)
    FROM T_Stock s
    INNER JOIN T_Arts a ON s.Id_Article = a.Id_Article
    WHERE s.Seuil IS NULL OR s.Seuil = 0;

    PRINT CONCAT(@@ROWCOUNT, ' seuils mis à jour dans T_Stock.');
END
GO

-- ============================================================================
-- ÉTAPE 6: AJOUTER LES CLÉS ÉTRANGÈRES
-- ============================================================================
PRINT 'Ajout des clés étrangères...';

-- FK T_Appro_Details → T_Appros
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Appro')
BEGIN
    ALTER TABLE T_Appro_Details
    ADD CONSTRAINT FK_ApproDetails_Appro
    FOREIGN KEY (Id_Appro) REFERENCES T_Appros(Id);
    PRINT 'FK_ApproDetails_Appro créée.';
END

-- FK T_Appro_Details → T_Arts
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Article')
BEGIN
    ALTER TABLE T_Appro_Details
    ADD CONSTRAINT FK_ApproDetails_Article
    FOREIGN KEY (Id_Article) REFERENCES T_Arts(Id_Article);
    PRINT 'FK_ApproDetails_Article créée.';
END

-- FK T_Appro_Details → T_Localisations
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ApproDetails_Localisation')
BEGIN
    ALTER TABLE T_Appro_Details
    ADD CONSTRAINT FK_ApproDetails_Localisation
    FOREIGN KEY (Id_Localisation) REFERENCES T_Localisations(Id_Localisation);
    PRINT 'FK_ApproDetails_Localisation créée.';
END

-- FK T_Mouvement_Stock → T_Arts
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Mouvement_Article')
BEGIN
    ALTER TABLE T_Mouvement_Stock
    ADD CONSTRAINT FK_Mouvement_Article
    FOREIGN KEY (Id_Article) REFERENCES T_Arts(Id_Article);
    PRINT 'FK_Mouvement_Article créée.';
END

-- FK T_Mouvement_Stock → T_Localisations
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Mouvement_Localisation')
BEGIN
    ALTER TABLE T_Mouvement_Stock
    ADD CONSTRAINT FK_Mouvement_Localisation
    FOREIGN KEY (Id_Localisation) REFERENCES T_Localisations(Id_Localisation);
    PRINT 'FK_Mouvement_Localisation créée.';
END

-- FK T_Stock → T_Arts
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Stock_Article')
BEGIN
    ALTER TABLE T_Stock
    ADD CONSTRAINT FK_Stock_Article
    FOREIGN KEY (Id_Article) REFERENCES T_Arts(Id_Article);
    PRINT 'FK_Stock_Article créée.';
END

-- FK T_Stock → T_Localisations
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Stock_Localisation')
BEGIN
    ALTER TABLE T_Stock
    ADD CONSTRAINT FK_Stock_Localisation
    FOREIGN KEY (Id_Localisation) REFERENCES T_Localisations(Id_Localisation);
    PRINT 'FK_Stock_Localisation créée.';
END
GO

-- ============================================================================
-- ÉTAPE 7: RESTRUCTURER T_APPROS (Optionnel - à exécuter après validation)
-- ============================================================================
-- ATTENTION: Exécutez cette section UNIQUEMENT après avoir validé la migration!
-- Décommentez les lignes ci-dessous pour supprimer les colonnes de T_Appros

/*
PRINT 'Restructuration de T_Appros...';

-- Supprimer la FK existante vers T_Cmd_Details
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_T_Appros_T_Cmd_Details')
BEGIN
    ALTER TABLE T_Appros DROP CONSTRAINT FK_T_Appros_T_Cmd_Details;
    PRINT 'FK_T_Appros_T_Cmd_Details supprimée.';
END

-- Ajouter colonne Id_Cmd (lien vers commande fournisseur)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Appros') AND name = 'Id_Cmd')
BEGIN
    ALTER TABLE T_Appros ADD Id_Cmd INT NULL;
    PRINT 'Colonne Id_Cmd ajoutée.';
END

-- Ajouter colonne Reference
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Appros') AND name = 'Reference')
BEGIN
    ALTER TABLE T_Appros ADD Reference VARCHAR(50);
    PRINT 'Colonne Reference ajoutée.';
END

-- Supprimer les colonnes migrées vers T_Appro_Details
-- ATTENTION: Assurez-vous que les données sont bien migrées!
ALTER TABLE T_Appros DROP COLUMN Id_Article;
ALTER TABLE T_Appros DROP COLUMN Qte;
ALTER TABLE T_Appros DROP COLUMN PA;
ALTER TABLE T_Appros DROP COLUMN PV;
ALTER TABLE T_Appros DROP COLUMN Ben;
ALTER TABLE T_Appros DROP COLUMN BenTotal;
ALTER TABLE T_Appros DROP COLUMN Taux;
ALTER TABLE T_Appros DROP COLUMN LocalisationId;
ALTER TABLE T_Appros DROP COLUMN Id_Cmd_Detail;

PRINT 'Colonnes supprimées de T_Appros.';

-- FK T_Appros → T_Cmds
ALTER TABLE T_Appros
ADD CONSTRAINT FK_Appros_Cmd
FOREIGN KEY (Id_Cmd) REFERENCES T_Cmds(Id);
PRINT 'FK_Appros_Cmd créée.';
*/

-- ============================================================================
-- ÉTAPE 8: NETTOYER T_ARTS (Optionnel - à exécuter après validation)
-- ============================================================================
-- ATTENTION: Exécutez cette section UNIQUEMENT après avoir validé la migration!
-- Décommentez les lignes ci-dessous pour supprimer les colonnes de T_Arts

/*
PRINT 'Nettoyage de T_Arts...';

-- Supprimer Qte (maintenant dans T_Stock)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Arts') AND name = 'Qte')
BEGIN
    ALTER TABLE T_Arts DROP COLUMN Qte;
    PRINT 'Colonne Qte supprimée de T_Arts.';
END

-- Supprimer Soeuil (maintenant dans T_Stock)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Arts') AND name = 'Soeuil')
BEGIN
    ALTER TABLE T_Arts DROP COLUMN Soeuil;
    PRINT 'Colonne Soeuil supprimée de T_Arts.';
END

-- Supprimer Id_Localisation (legacy)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('T_Arts') AND name = 'Id_Localisation')
BEGIN
    ALTER TABLE T_Arts DROP COLUMN Id_Localisation;
    PRINT 'Colonne Id_Localisation supprimée de T_Arts.';
END
*/

-- ============================================================================
-- VÉRIFICATION FINALE
-- ============================================================================
PRINT '';
PRINT '============================================================================';
PRINT 'VÉRIFICATION FINALE';
PRINT '============================================================================';

SELECT 'T_Appro_Details' AS [Table], COUNT(*) AS [Lignes] FROM T_Appro_Details
UNION ALL
SELECT 'T_Mouvement_Stock', COUNT(*) FROM T_Mouvement_Stock
UNION ALL
SELECT 'T_Stock', COUNT(*) FROM T_Stock
UNION ALL
SELECT 'T_Appros', COUNT(*) FROM T_Appros;

PRINT '';
PRINT 'Migration terminée avec succès!';
PRINT 'N''oubliez pas de mettre à jour les modèles C# correspondants.';
GO
