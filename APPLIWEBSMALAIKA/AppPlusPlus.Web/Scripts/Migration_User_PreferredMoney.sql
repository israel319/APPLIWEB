-- Préférence de devise par utilisateur (USD / CDF)
USE GlobalShoping;
GO

IF COL_LENGTH('T_Users', 'Id_Monais_Pref') IS NULL
BEGIN
    ALTER TABLE T_Users ADD Id_Monais_Pref INT NULL;
    PRINT 'Colonne Id_Monais_Pref ajoutée à T_Users';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Users_Moneys_Pref')
BEGIN
    ALTER TABLE T_Users
        ADD CONSTRAINT FK_T_Users_Moneys_Pref
        FOREIGN KEY (Id_Monais_Pref) REFERENCES T_Moneys(Id_Monais);
    PRINT 'FK FK_T_Users_Moneys_Pref créée';
END
GO

-- Défaut CDF (2) pour les utilisateurs existants sans préférence
UPDATE T_Users
SET Id_Monais_Pref = 2
WHERE Id_Monais_Pref IS NULL;
GO

PRINT 'Migration User PreferredMoney terminée.';
GO
