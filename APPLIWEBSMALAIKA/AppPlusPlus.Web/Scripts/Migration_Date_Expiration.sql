/*
  Date d'expiration — niveau ligne (lot) sur réceptions appro et inventaire magasin.
  Optionnel : traçabilité sur les mouvements de stock entrée.
*/
SET NOCOUNT ON;

IF COL_LENGTH('T_Appro_Details', 'Date_Expiration') IS NULL
BEGIN
    ALTER TABLE T_Appro_Details
        ADD Date_Expiration date NULL;
    PRINT 'T_Appro_Details.Date_Expiration ajoutée.';
END

IF COL_LENGTH('T_Inventaire_Reception_Detail', 'Date_Expiration') IS NULL
BEGIN
    ALTER TABLE T_Inventaire_Reception_Detail
        ADD Date_Expiration date NULL;
    PRINT 'T_Inventaire_Reception_Detail.Date_Expiration ajoutée.';
END

IF COL_LENGTH('T_Mouvement_Stock', 'Date_Expiration') IS NULL
BEGIN
    ALTER TABLE T_Mouvement_Stock
        ADD Date_Expiration date NULL;
    PRINT 'T_Mouvement_Stock.Date_Expiration ajoutée.';
END

GO
