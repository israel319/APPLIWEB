-- Complète Migration_Demandes_Generiques.sql si T_Demande_Details n'a pas été créée
USE GlobalShoping;
GO

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
