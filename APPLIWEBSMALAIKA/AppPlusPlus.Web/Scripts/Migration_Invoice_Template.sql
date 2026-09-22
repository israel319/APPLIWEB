-- ============================================================================
-- Modèles de facture (ticket 80 mm / A4 portrait, en-tête, filigrane)
-- Exécuter sur GlobalShoping — idempotent
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.T_InvoiceTemplate', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_InvoiceTemplate (
        Id                      INT             IDENTITY(1,1) NOT NULL,
        Nom                     NVARCHAR(120)   NOT NULL,
        IsActive                BIT             NOT NULL CONSTRAINT DF_T_InvoiceTemplate_IsActive DEFAULT (0),
        PrintFormat             NVARCHAR(20)    NOT NULL CONSTRAINT DF_T_InvoiceTemplate_PrintFormat DEFAULT (N'Ticket80'),
        Logo                    NVARCHAR(MAX)   NULL,
        LetterheadBackground    NVARCHAR(MAX)   NULL,
        ShowLetterheadBackground BIT            NOT NULL CONSTRAINT DF_T_InvoiceTemplate_ShowBg DEFAULT (0),
        LegalRccm               NVARCHAR(120)   NULL,
        LegalIdNat              NVARCHAR(120)   NULL,
        LegalAdresse              NVARCHAR(500) NULL,
        LegalContact            NVARCHAR(250)   NULL,
        LegalEmail              NVARCHAR(250)   NULL,
        LegalWebsite            NVARCHAR(250)   NULL,
        FooterText              NVARCHAR(500)   NULL,
        ContentMarginTopMm      INT             NOT NULL CONSTRAINT DF_T_InvoiceTemplate_MarginTop DEFAULT (42),
        ContentMarginBottomMm   INT             NOT NULL CONSTRAINT DF_T_InvoiceTemplate_MarginBottom DEFAULT (32),
        CONSTRAINT PK_T_InvoiceTemplate PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table T_InvoiceTemplate créée.';
END
ELSE
    PRINT 'Table T_InvoiceTemplate déjà présente.';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.T_InvoiceTemplate)
BEGIN
    INSERT INTO dbo.T_InvoiceTemplate (
        Nom, IsActive, PrintFormat, ShowLetterheadBackground,
        LegalRccm, LegalIdNat, LegalAdresse, LegalContact, LegalEmail, LegalWebsite,
        FooterText, ContentMarginTopMm, ContentMarginBottomMm
    )
    VALUES
    (
        N'Ticket caisse (80 mm)', 1, N'Ticket80', 0,
        NULL, NULL, NULL, NULL, NULL, NULL,
        N'Merci de votre visite', 42, 32
    ),
    (
        N'Facture A4 portrait', 0, N'A4Portrait', 0,
        NULL, NULL, NULL, NULL, NULL, NULL,
        N'Merci de votre confiance',
        42, 32
    );
    PRINT 'Modèles de facture par défaut insérés (ticket actif + A4 vierge).';
END
ELSE
    PRINT 'T_InvoiceTemplate contient déjà des enregistrements — seed ignoré.';
GO

-- Anciennes seeds « IPS » : vider les mentions légales pré-remplies (app multi-clients)
UPDATE dbo.T_InvoiceTemplate
SET
    Nom = N'Facture A4 portrait',
    ShowLetterheadBackground = 0,
    LegalRccm = NULL,
    LegalIdNat = NULL,
    LegalAdresse = NULL,
    LegalContact = NULL,
    LegalEmail = NULL,
    LegalWebsite = NULL
WHERE Nom LIKE N'%IPS%'
   OR LegalEmail LIKE N'%itsp-dev%';
IF @@ROWCOUNT > 0
    PRINT 'Modèle A4 exemple IPS neutralisé (champs à renseigner par entreprise).';
GO
