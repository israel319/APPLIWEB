-- Corrige TR_T_Inventaire_Reception_Detail_RefreshTotal
-- Erreur : « Nom d'objet 'Affected' non valide » — la CTE Affected n'est valable que pour une seule requête.
USE GlobalShoping;
GO

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

PRINT 'Trigger TR_T_Inventaire_Reception_Detail_RefreshTotal corrigé.';
GO
