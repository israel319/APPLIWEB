-- Corrige les approvisionnements directs enregistrés avec StatusId=1 (transfert)
-- alors qu'ils sont des réceptions comptabilisées (Id_Cmd_Detail = 0, hors transfert interne).
-- À exécuter une fois sur la base de production.

UPDATE T_Appros
SET StatusId = 0
WHERE StatusId = 1
  AND (Id_Cmd_Detail = 0 OR Id_Cmd_Detail IS NULL)
  AND (Id_Cmd IS NULL OR Id_Cmd = 0);

-- Les réceptions depuis commande (StatusId NULL) restent comptabilisées (filtre : != 1 et != 3).

PRINT 'Migration StatusId approvisionnements terminée.';
