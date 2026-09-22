-- Ventes et dépenses journalières séparées (T_Inventaire_Vente)
USE GlobalShoping;
GO

IF COL_LENGTH('dbo.T_Inventaire_Vente', 'Montant_Ventes') IS NULL
BEGIN
    ALTER TABLE dbo.T_Inventaire_Vente
        ADD Montant_Ventes DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Vente_Montant_Ventes DEFAULT 0;

    PRINT 'Colonne Montant_Ventes ajoutée.';
END
GO

IF COL_LENGTH('dbo.T_Inventaire_Vente', 'Montant_Ventes') IS NOT NULL
BEGIN
    UPDATE dbo.T_Inventaire_Vente
    SET Montant_Ventes = Montant
    WHERE Montant_Ventes = 0 AND Montant > 0;
END
GO

IF COL_LENGTH('dbo.T_Inventaire_Vente', 'Montant_Depenses') IS NULL
BEGIN
    ALTER TABLE dbo.T_Inventaire_Vente
        ADD Montant_Depenses DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_T_Inventaire_Vente_Montant_Depenses DEFAULT 0;

    PRINT 'Colonne Montant_Depenses ajoutée.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_T_Inventaire_Vente_Montant_Ventes'
)
BEGIN
    ALTER TABLE dbo.T_Inventaire_Vente
        ADD CONSTRAINT CK_T_Inventaire_Vente_Montant_Ventes CHECK (Montant_Ventes >= 0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_T_Inventaire_Vente_Montant_Depenses'
)
BEGIN
    ALTER TABLE dbo.T_Inventaire_Vente
        ADD CONSTRAINT CK_T_Inventaire_Vente_Montant_Depenses CHECK (Montant_Depenses >= 0);
END
GO

PRINT 'Migration ventes/dépenses terminée.';
GO
