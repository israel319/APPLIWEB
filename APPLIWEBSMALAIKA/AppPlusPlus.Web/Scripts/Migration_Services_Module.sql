-- ============================================================================
-- Module Services & Prestations — tables + extensions facturation
-- Exécuter sur la base GlobalShoping (même BDD que Ventes)
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ── T_Service_Projects ──
IF OBJECT_ID('dbo.T_Service_Projects', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Service_Projects (
        Id              INT            IDENTITY(1,1) NOT NULL,
        Title           NVARCHAR(120)  NOT NULL,
        Description     NVARCHAR(500)  NULL,
        CustomerId      INT            NULL,
        Status          INT            NOT NULL CONSTRAINT DF_SvcProj_Status DEFAULT (0),
        BillingMode     INT            NOT NULL CONSTRAINT DF_SvcProj_Billing DEFAULT (0),
        HourlyRate      DECIMAL(18,2)  NULL,
        FlatAmount      DECIMAL(18,2)  NULL,
        FlatInvoiced    BIT            NOT NULL CONSTRAINT DF_SvcProj_FlatInv DEFAULT (0),
        StartDate       DATE           NULL,
        EndDate         DATE           NULL,
        LocalisationId  INT            NULL,
        MoneyId         INT            NULL,
        DateSys         DATETIME       NOT NULL CONSTRAINT DF_SvcProj_DateSys DEFAULT (GETDATE()),
        [User]          VARCHAR(50)    NOT NULL,
        Cumputer        VARCHAR(50)    NOT NULL,
        CONSTRAINT PK_T_Service_Projects PRIMARY KEY (Id)
    );
    CREATE INDEX IX_SvcProj_Customer_Status ON dbo.T_Service_Projects (CustomerId, Status);
    PRINT 'Créé : T_Service_Projects';
END
GO

-- ── T_Service_Tasks ──
IF OBJECT_ID('dbo.T_Service_Tasks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Service_Tasks (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProjectId     INT            NOT NULL,
        Title         NVARCHAR(200)  NOT NULL,
        Description   NVARCHAR(500)  NULL,
        AssignedUser  VARCHAR(50)    NULL,
        PlannedStart  DATE           NULL,
        PlannedEnd    DATE           NULL,
        Status        INT            NOT NULL CONSTRAINT DF_SvcTask_Status DEFAULT (0),
        Priority      INT            NOT NULL CONSTRAINT DF_SvcTask_Priority DEFAULT (0),
        DateSys       DATETIME       NOT NULL CONSTRAINT DF_SvcTask_DateSys DEFAULT (GETDATE()),
        [User]        VARCHAR(50)    NOT NULL,
        CONSTRAINT PK_T_Service_Tasks PRIMARY KEY (Id),
        CONSTRAINT FK_SvcTask_Project FOREIGN KEY (ProjectId) REFERENCES dbo.T_Service_Projects (Id)
    );
    CREATE INDEX IX_SvcTask_Project ON dbo.T_Service_Tasks (ProjectId);
    PRINT 'Créé : T_Service_Tasks';
END
GO

-- ── T_Service_Timesheets ──
IF OBJECT_ID('dbo.T_Service_Timesheets', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Service_Timesheets (
        Id          INT            IDENTITY(1,1) NOT NULL,
        ProjectId   INT            NOT NULL,
        TaskId      INT            NULL,
        UserLogin   VARCHAR(50)    NOT NULL,
        WorkDate    DATE           NOT NULL,
        Hours       DECIMAL(18,3)  NOT NULL,
        Description NVARCHAR(500)  NULL,
        Status      INT            NOT NULL CONSTRAINT DF_SvcTs_Status DEFAULT (0),
        FactId      INT            NULL,
        DateSys     DATETIME       NOT NULL CONSTRAINT DF_SvcTs_DateSys DEFAULT (GETDATE()),
        [User]      VARCHAR(50)    NOT NULL,
        CONSTRAINT PK_T_Service_Timesheets PRIMARY KEY (Id),
        CONSTRAINT FK_SvcTs_Project FOREIGN KEY (ProjectId) REFERENCES dbo.T_Service_Projects (Id),
        CONSTRAINT FK_SvcTs_Task FOREIGN KEY (TaskId) REFERENCES dbo.T_Service_Tasks (Id)
    );
    CREATE INDEX IX_SvcTs_Project_Status_Fact ON dbo.T_Service_Timesheets (ProjectId, Status, FactId);
    PRINT 'Créé : T_Service_Timesheets';
END
GO

-- ── T_Service_Deliverables ──
IF OBJECT_ID('dbo.T_Service_Deliverables', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.T_Service_Deliverables (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProjectId     INT            NOT NULL,
        TaskId        INT            NULL,
        Title         NVARCHAR(200)  NOT NULL,
        Description   NVARCHAR(500)  NULL,
        DueDate       DATE           NULL,
        DeliveredDate DATE           NULL,
        Status        INT            NOT NULL CONSTRAINT DF_SvcDel_Status DEFAULT (0),
        DateSys       DATETIME       NOT NULL CONSTRAINT DF_SvcDel_DateSys DEFAULT (GETDATE()),
        [User]        VARCHAR(50)    NOT NULL,
        CONSTRAINT PK_T_Service_Deliverables PRIMARY KEY (Id),
        CONSTRAINT FK_SvcDel_Project FOREIGN KEY (ProjectId) REFERENCES dbo.T_Service_Projects (Id),
        CONSTRAINT FK_SvcDel_Task FOREIGN KEY (TaskId) REFERENCES dbo.T_Service_Tasks (Id)
    );
    CREATE INDEX IX_SvcDel_Project ON dbo.T_Service_Deliverables (ProjectId);
    PRINT 'Créé : T_Service_Deliverables';
END
GO

-- ── Extension T_Facts ──
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Facts') AND name = 'Id_Service_Project')
BEGIN
    ALTER TABLE dbo.T_Facts ADD Id_Service_Project INT NULL;
    PRINT 'Colonne ajoutée : T_Facts.Id_Service_Project';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Facts_ServiceProject')
BEGIN
    ALTER TABLE dbo.T_Facts ADD CONSTRAINT FK_T_Facts_ServiceProject
        FOREIGN KEY (Id_Service_Project) REFERENCES dbo.T_Service_Projects (Id);
    PRINT 'FK T_Facts → T_Service_Projects';
END
GO

-- ── Extension T_Fact_Details ──
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Fact_Details') AND name = 'Description_Line')
BEGIN
    ALTER TABLE dbo.T_Fact_Details ADD Description_Line NVARCHAR(200) NULL;
    PRINT 'Colonne ajoutée : T_Fact_Details.Description_Line';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Fact_Details') AND name = 'Id_Timesheet')
BEGIN
    ALTER TABLE dbo.T_Fact_Details ADD Id_Timesheet INT NULL;
    PRINT 'Colonne ajoutée : T_Fact_Details.Id_Timesheet';
END
GO

-- ── Fonction permissions ──
IF NOT EXISTS (SELECT 1 FROM dbo.T_Fonctions WHERE LOWER(LTRIM(RTRIM(Description_Fonction))) = 'services')
BEGIN
    INSERT INTO dbo.T_Fonctions (Description_Fonction) VALUES ('services');
    PRINT 'Fonction ajoutée : services';
END
GO

-- ── Licence module ──
IF OBJECT_ID('dbo.T_ModuleLicenses', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.T_ModuleLicenses WHERE ModuleCode = 'services')
BEGIN
    INSERT INTO dbo.T_ModuleLicenses (ModuleCode, ModuleName, Description, Icon, IsActive, IsCore, CreatedAt, UpdatedAt)
    VALUES (
        'services',
        N'Services & Prestations',
        N'Gestion des prestations immatérielles, feuilles de temps et facturation',
        'bi-briefcase',
        1, 0, GETUTCDATE(), GETUTCDATE()
    );
    PRINT 'Licence module ajoutée : services';
END
GO

PRINT 'Migration Services & Prestations terminée.';
GO
