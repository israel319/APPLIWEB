-- Réinitialise ou crée l'utilisateur admin / 1234 (connexion application)
SET NOCOUNT ON;

DECLARE @RoleId INT = (
    SELECT TOP 1 RoleId
    FROM dbo.T_Roles
    WHERE Description_Role = N'Administrateur'
       OR RoleId = 1
    ORDER BY RoleId
);

IF @RoleId IS NULL
BEGIN
    RAISERROR('Aucun rôle Administrateur trouvé dans T_Roles.', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.T_Users WHERE login = N'admin')
BEGIN
    UPDATE dbo.T_Users
    SET Password  = N'1234',
        Name      = ISNULL(Name, N'Administrateur'),
        Activated = 1,
        RoleId    = @RoleId
    WHERE login = N'admin';

    PRINT 'Utilisateur admin mis à jour (mot de passe : 1234).';
END
ELSE
BEGIN
    INSERT INTO dbo.T_Users (login, Password, Name, Email, Activated, RoleId)
    VALUES (N'admin', N'1234', N'Administrateur', NULL, 1, @RoleId);

    PRINT 'Utilisateur admin créé (login : admin, mot de passe : 1234).';
END
