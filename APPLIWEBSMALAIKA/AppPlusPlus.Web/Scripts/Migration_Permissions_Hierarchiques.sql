/*
  Permissions hiérarchiques — stockage en base par utilisateur.

  Tables utilisées :
  - T_Activities          : catalogue (codes articles.catalogue.read, dashboard, etc.)
  - T_User_Activities     : SOURCE DE VÉRITÉ par login (cocher/décocher dans la modale)
  - T_Role_Activities     : miroir sur le rôle dédié de l'utilisateur
  - T_Permissions         : legacy — vidé à la sauvegarde profil hiérarchique

  L'application synchronise T_Activities via EnsureCatalogActivitiesAsync au premier enregistrement.
*/
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'T_Role_Activities')
BEGIN
    CREATE TABLE dbo.T_Role_Activities
    (
        RoleActivityId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_Role_Activities PRIMARY KEY,
        RoleId         int NOT NULL,
        ActivityId     int NOT NULL,
        IsGranted      bit NOT NULL CONSTRAINT DF_T_Role_Activities_IsGranted DEFAULT (1)
    );

    ALTER TABLE dbo.T_Role_Activities
        ADD CONSTRAINT FK_T_Role_Activities_Role
        FOREIGN KEY (RoleId) REFERENCES dbo.T_Roles (RoleId);

    ALTER TABLE dbo.T_Role_Activities
        ADD CONSTRAINT FK_T_Role_Activities_Activity
        FOREIGN KEY (ActivityId) REFERENCES dbo.T_Activities (ActivityId);

    CREATE UNIQUE INDEX UX_T_Role_Activities_Role_Activity
        ON dbo.T_Role_Activities (RoleId, ActivityId);

    PRINT 'Table T_Role_Activities créée.';
END
ELSE
BEGIN
    PRINT 'Table T_Role_Activities déjà présente.';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'T_User_Activities')
BEGIN
    CREATE TABLE dbo.T_User_Activities (
        UserActivityId INT          IDENTITY(1,1) NOT NULL CONSTRAINT PK_T_User_Activities PRIMARY KEY,
        UserLogin      VARCHAR(50)  NOT NULL,
        ActivityId     INT          NOT NULL,
        IsGranted      BIT          NOT NULL CONSTRAINT DF_T_User_Activities_IsGranted DEFAULT (1),
        AssignedDate   DATETIME2(7) NOT NULL CONSTRAINT DF_T_User_Activities_AssignedDate DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_T_User_Activities_User_Activity UNIQUE (UserLogin, ActivityId),
        CONSTRAINT FK_T_User_Activities_User FOREIGN KEY (UserLogin) REFERENCES dbo.T_Users (login),
        CONSTRAINT FK_T_User_Activities_Activity FOREIGN KEY (ActivityId) REFERENCES dbo.T_Activities (ActivityId)
    );
    PRINT 'Table T_User_Activities créée.';
END
ELSE
BEGIN
    PRINT 'Table T_User_Activities déjà présente.';
END

GO

/*
  Diagnostic après configuration d'un utilisateur dans l'app
  (Configurer les accès → Valider la modale ; en édition, écriture immédiate en base).

  1) Permissions par utilisateur (source de vérité) :
*/
-- SELECT u.login, a.Code, ua.IsGranted, ua.AssignedDate
-- FROM dbo.T_User_Activities ua
-- JOIN dbo.T_Users u ON u.login = ua.UserLogin
-- JOIN dbo.T_Activities a ON a.ActivityId = ua.ActivityId
-- WHERE ua.IsGranted = 1
-- ORDER BY u.login, a.Code;

/*
  2) Miroir rôle (doit exister si T_User_Activities est rempli) :
*/
-- SELECT r.Description_Role, a.Code
-- FROM dbo.T_Role_Activities ra
-- JOIN dbo.T_Roles r ON r.RoleId = ra.RoleId
-- JOIN dbo.T_Activities a ON a.ActivityId = ra.ActivityId
-- WHERE ra.IsGranted = 1
-- ORDER BY r.Description_Role, a.Code;

/*
  3) Catalogue hiérarchique (codes articles.catalogue.read, dashboard, etc.) :
     doit contenir des lignes après le premier enregistrement d'accès.
*/
-- SELECT Code, Description_Activity, IsActive FROM dbo.T_Activities
-- WHERE Code LIKE '%.%' OR Code IN ('dashboard','articles','vente','commandes','finance','rapports','administration')
-- ORDER BY Code;
