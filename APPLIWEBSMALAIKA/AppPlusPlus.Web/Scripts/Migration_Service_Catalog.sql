-- Catalogue des prestations (équivalent T_Arts pour les services)
USE GlobalShoping;
GO

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.T_Service_Catalog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Service_Catalog (
        Id          INT            IDENTITY(1,1) NOT NULL,
        Code        NVARCHAR(50)   NOT NULL,
        Description NVARCHAR(120)  NOT NULL,
        Detail      NVARCHAR(500)  NULL,
        Price       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_SvcCat_Price DEFAULT (0),
        MoneyId     INT            NOT NULL,
        UnitType    INT            NOT NULL CONSTRAINT DF_SvcCat_Unit DEFAULT (0),
        Category    NVARCHAR(80)   NULL,
        IsActive    BIT            NOT NULL CONSTRAINT DF_SvcCat_Active DEFAULT (1),
        DateSys     DATETIME       NOT NULL CONSTRAINT DF_SvcCat_DateSys DEFAULT (GETDATE()),
        DateEditing DATETIME       NULL,
        [User]      VARCHAR(50)    NOT NULL,
        Cumputer    VARCHAR(50)    NOT NULL,
        CONSTRAINT PK_T_Service_Catalog PRIMARY KEY (Id),
        CONSTRAINT UQ_T_Service_Catalog_Code UNIQUE (Code)
    );
    CREATE INDEX IX_SvcCat_Active ON dbo.T_Service_Catalog (IsActive, Description);
    PRINT 'Créé : T_Service_Catalog';
END
GO

IF OBJECT_ID('dbo.T_ModuleLicenses', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.T_ModuleLicenses
    SET ModuleName = N'Catalogue prestations',
        Description = N'Catalogue des prestations et services facturables (équivalent articles)',
        UpdatedAt = GETUTCDATE()
    WHERE ModuleCode = 'services';
END
GO

PRINT 'Migration catalogue prestations terminée.';
GO
