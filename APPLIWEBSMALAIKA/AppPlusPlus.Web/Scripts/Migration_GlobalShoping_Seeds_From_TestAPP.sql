-- ============================================================================
-- Copie des données de référence TestAPP → GlobalShoping
-- Tables : T_AppSettings, T_Activities, T_ModuleLicenses, T_Permissions, T_Profile
-- Idempotent : n'insère que si la cible est vide (ou clé absente)
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
GO

-- T_AppSettings
IF NOT EXISTS (SELECT 1 FROM dbo.T_AppSettings)
BEGIN
    INSERT INTO dbo.T_AppSettings ([Key], [Value])
    SELECT [Key], [Value] FROM TestAPP.dbo.T_AppSettings;
    PRINT CONCAT('T_AppSettings : ', @@ROWCOUNT, ' ligne(s) copiées.');
END
ELSE
    PRINT 'T_AppSettings : déjà peuplé, ignoré.';
GO

-- T_Activities
IF NOT EXISTS (SELECT 1 FROM dbo.T_Activities)
BEGIN
    SET IDENTITY_INSERT dbo.T_Activities ON;
    INSERT INTO dbo.T_Activities (ActivityId, FonctionId, Code, Description_Activity, IsActive)
    SELECT ActivityId, FonctionId, Code, Description_Activity, IsActive
    FROM TestAPP.dbo.T_Activities;
    SET IDENTITY_INSERT dbo.T_Activities OFF;
    PRINT CONCAT('T_Activities : ', @@ROWCOUNT, ' ligne(s) copiées.');
END
ELSE
    PRINT 'T_Activities : déjà peuplé, ignoré.';
GO

-- T_ModuleLicenses
IF NOT EXISTS (SELECT 1 FROM dbo.T_ModuleLicenses)
BEGIN
    SET IDENTITY_INSERT dbo.T_ModuleLicenses ON;
    INSERT INTO dbo.T_ModuleLicenses (
        Id, ModuleCode, ModuleName, Description, Icon, IsActive, IsCore,
        Price, ActivatedAt, ExpiresAt, LicenseNotes, CreatedAt, UpdatedAt
    )
    SELECT
        Id, ModuleCode, ModuleName, Description, Icon, IsActive, IsCore,
        Price, ActivatedAt, ExpiresAt, LicenseNotes, CreatedAt, UpdatedAt
    FROM TestAPP.dbo.T_ModuleLicenses;
    SET IDENTITY_INSERT dbo.T_ModuleLicenses OFF;
    PRINT CONCAT('T_ModuleLicenses : ', @@ROWCOUNT, ' ligne(s) copiées.');
END
ELSE
    PRINT 'T_ModuleLicenses : déjà peuplé, ignoré.';
GO

-- T_Permissions (nécessaire avec T_Roles pour la sécurité)
IF NOT EXISTS (SELECT 1 FROM dbo.T_Permissions)
BEGIN
    SET IDENTITY_INSERT dbo.T_Permissions ON;
    INSERT INTO dbo.T_Permissions (PermissionId, RoleId, FonctionId, CanRead, CanWrite, CanDelete)
    SELECT PermissionId, RoleId, FonctionId, CanRead, CanWrite, CanDelete
    FROM TestAPP.dbo.T_Permissions;
    SET IDENTITY_INSERT dbo.T_Permissions OFF;
    PRINT CONCAT('T_Permissions : ', @@ROWCOUNT, ' ligne(s) copiées.');
END
ELSE
    PRINT 'T_Permissions : déjà peuplé, ignoré.';
GO

-- T_Profile (profil boutique)
IF NOT EXISTS (SELECT 1 FROM dbo.T_Profile)
BEGIN
    SET IDENTITY_INSERT dbo.T_Profile ON;
    INSERT INTO dbo.T_Profile (Id, AppNameSettingKey, PhotoShop, Adresse1, Adresse2)
    SELECT Id, AppNameSettingKey, PhotoShop, Adresse1, Adresse2
    FROM TestAPP.dbo.T_Profile;
    SET IDENTITY_INSERT dbo.T_Profile OFF;
    PRINT CONCAT('T_Profile : ', @@ROWCOUNT, ' ligne(s) copiées.');
END
ELSE
    PRINT 'T_Profile : déjà peuplé, ignoré.';
GO

-- FK T_Profile → T_AppSettings (si possible)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Profile_T_AppSettings_AppNameSettingKey')
   AND EXISTS (SELECT 1 FROM dbo.T_Profile)
   AND EXISTS (SELECT 1 FROM dbo.T_AppSettings)
BEGIN
    ALTER TABLE dbo.T_Profile ADD CONSTRAINT FK_T_Profile_T_AppSettings_AppNameSettingKey
        FOREIGN KEY (AppNameSettingKey) REFERENCES dbo.T_AppSettings ([Key]);
    PRINT 'FK T_Profile → T_AppSettings créée.';
END
GO

-- FK T_Activities → T_Fonctions
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Activities_Fonctions')
   AND EXISTS (SELECT 1 FROM dbo.T_Activities)
BEGIN
    ALTER TABLE dbo.T_Activities ADD CONSTRAINT FK_T_Activities_Fonctions
        FOREIGN KEY (FonctionId) REFERENCES dbo.T_Fonctions (Id_Fonction);
    PRINT 'FK T_Activities → T_Fonctions créée.';
END
GO

-- FK T_Permissions
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Permissions_Role')
   AND EXISTS (SELECT 1 FROM dbo.T_Permissions)
BEGIN
    ALTER TABLE dbo.T_Permissions ADD CONSTRAINT FK_Permissions_Role
        FOREIGN KEY (RoleId) REFERENCES dbo.T_Roles (RoleId);
    ALTER TABLE dbo.T_Permissions ADD CONSTRAINT FK_Permissions_Fonction
        FOREIGN KEY (FonctionId) REFERENCES dbo.T_Fonctions (Id_Fonction);
    PRINT 'FK T_Permissions créées.';
END
GO

SELECT 'T_AppSettings' AS [Table], COUNT(*) AS Lignes FROM dbo.T_AppSettings
UNION ALL SELECT 'T_Activities', COUNT(*) FROM dbo.T_Activities
UNION ALL SELECT 'T_ModuleLicenses', COUNT(*) FROM dbo.T_ModuleLicenses
UNION ALL SELECT 'T_Permissions', COUNT(*) FROM dbo.T_Permissions
UNION ALL SELECT 'T_Profile', COUNT(*) FROM dbo.T_Profile;
GO
