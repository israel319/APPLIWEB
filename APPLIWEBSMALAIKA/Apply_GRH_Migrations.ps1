# ============================================================================
# Script PowerShell: Appliquer les migrations GRH
# ============================================================================

# Paramètres de connexion SQL Server
$ServerName = "localhost"
$DatabaseName = "APW_GRH"
$ScriptPath = "E:\APPLIWEB\APPLIWEB\GRH_Apply_Migration.sql"

Write-Host "========================================"
Write-Host "Application des Migrations GRH"
Write-Host "========================================"
Write-Host ""
Write-Host "Serveur: $ServerName"
Write-Host "Base de données: $DatabaseName"
Write-Host ""

# Lire le contenu du script SQL
if (Test-Path $ScriptPath) {
    Write-Host "✓ Fichier SQL trouvé: $ScriptPath"
    Write-Host ""
    
    try {
        # Exécuter le script SQL
        Write-Host "Exécution du script SQL..."
        Invoke-SqlCmd -ServerInstance $ServerName `
                      -Database $DatabaseName `
                      -InputFile $ScriptPath `
                      -TrustServerCertificate
        
        Write-Host ""
        Write-Host "✓ Script exécuté avec succès!"
    }
    catch {
        Write-Host "✗ Erreur lors de l'exécution du script:"
        Write-Host $_.Exception.Message
        exit 1
    }
}
else {
    Write-Host "✗ Fichier SQL non trouvé: $ScriptPath"
    exit 1
}

Write-Host ""
Write-Host "========================================"
Write-Host "Migrations GRH appliquées avec succès!"
Write-Host "========================================"
