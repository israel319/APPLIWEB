/*
  CORRECTION URGENTE — doublons T_Activities (code permission en double)
  ====================================================================
  Symptôme app : "An item with the same key has already been added.
                  Key: commandes.internes.read"

  1. Décommentez USE ci-dessous
  2. Exécutez TOUT ce script (F5)
  3. Redéployez aussi l'application web (correctif code)
*/
-- USE db_acb256_glomyrashopping;
GO

SET NOCOUNT ON;
PRINT CONCAT('Base : ', DB_NAME());
GO

IF OBJECT_ID(N'dbo.T_Activities', N'U') IS NULL
BEGIN
    RAISERROR('KO — Table T_Activities absente.', 16, 1);
END
GO

/* Afficher les doublons avant nettoyage */
PRINT '--- Doublons détectés (avant) ---';
SELECT LOWER(LTRIM(RTRIM(Code))) AS CodeNorm, COUNT(*) AS Nb, STRING_AGG(CAST(ActivityId AS varchar(12)), ', ') AS ActivityIds
FROM dbo.T_Activities
WHERE Code IS NOT NULL AND LTRIM(RTRIM(Code)) <> N''
GROUP BY LOWER(LTRIM(RTRIM(Code)))
HAVING COUNT(*) > 1
ORDER BY CodeNorm;
GO

IF OBJECT_ID(N'tempdb..#ActDup', N'U') IS NOT NULL DROP TABLE #ActDup;

SELECT
    ActivityId,
    LOWER(LTRIM(RTRIM(Code))) AS NormCode,
    ROW_NUMBER() OVER (
        PARTITION BY LOWER(LTRIM(RTRIM(Code)))
        ORDER BY ActivityId
    ) AS rn,
    MIN(ActivityId) OVER (
        PARTITION BY LOWER(LTRIM(RTRIM(Code)))
    ) AS KeepId
INTO #ActDup
FROM dbo.T_Activities
WHERE Code IS NOT NULL AND LTRIM(RTRIM(Code)) <> N'';

IF NOT EXISTS (SELECT 1 FROM #ActDup WHERE rn > 1)
BEGIN
    PRINT 'OK — Aucun doublon Code dans T_Activities.';
    DROP TABLE #ActDup;
END
ELSE
BEGIN
    IF OBJECT_ID(N'dbo.T_User_Activities', N'U') IS NOT NULL
    BEGIN
        UPDATE ua
        SET ua.ActivityId = d.KeepId
        FROM dbo.T_User_Activities ua
        INNER JOIN #ActDup d ON d.ActivityId = ua.ActivityId
        WHERE d.rn > 1
          AND NOT EXISTS (
              SELECT 1 FROM dbo.T_User_Activities x
              WHERE x.UserLogin = ua.UserLogin AND x.ActivityId = d.KeepId
          );

        DELETE ua
        FROM dbo.T_User_Activities ua
        INNER JOIN #ActDup d ON d.ActivityId = ua.ActivityId
        WHERE d.rn > 1;
    END

    IF OBJECT_ID(N'dbo.T_Role_Activities', N'U') IS NOT NULL
    BEGIN
        UPDATE ra
        SET ra.ActivityId = d.KeepId
        FROM dbo.T_Role_Activities ra
        INNER JOIN #ActDup d ON d.ActivityId = ra.ActivityId
        WHERE d.rn > 1
          AND NOT EXISTS (
              SELECT 1 FROM dbo.T_Role_Activities x
              WHERE x.RoleId = ra.RoleId AND x.ActivityId = d.KeepId
          );

        DELETE ra
        FROM dbo.T_Role_Activities ra
        INNER JOIN #ActDup d ON d.ActivityId = ra.ActivityId
        WHERE d.rn > 1;
    END

    DELETE a
    FROM dbo.T_Activities a
    INNER JOIN #ActDup d ON d.ActivityId = a.ActivityId
    WHERE d.rn > 1;

    PRINT CONCAT('OK — Doublons supprimés : ', @@ROWCOUNT, ' activité(s).');
    DROP TABLE #ActDup;
END
GO

PRINT '--- Contrôle final (doit être vide) ---';
SELECT LOWER(LTRIM(RTRIM(Code))) AS CodeNorm, COUNT(*) AS Nb
FROM dbo.T_Activities
WHERE Code IS NOT NULL
GROUP BY LOWER(LTRIM(RTRIM(Code)))
HAVING COUNT(*) > 1;

IF @@ROWCOUNT = 0
    PRINT 'SUCCÈS — Plus de doublon. Redéployez l''application web puis retestez Enregistrer.';
GO
