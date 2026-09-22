$f2 = Get-ChildItem -LiteralPath 'e:\Malaika' -Filter '*.xlsx' | Where-Object { $_.Name -like '*Incomplet*' } | Select-Object -First 1
if (-not $f2) { throw 'Fichier Incomplet introuvable' }
Write-Host "======== FICHIER 2 (Incomplet) ========" -ForegroundColor Magenta
$r2 = & "$PSScriptRoot\Analyze-MalaikaExcel.ps1" -ExcelPath $f2.FullName

$f1 = Get-ChildItem -LiteralPath 'e:\Malaika' -Filter '*.xlsx' | Where-Object { $_.Name -like '*Boutique 1*' } | Select-Object -First 1
if ($f1) {
    Write-Host "`n======== FICHIER 1 (Boutique 1) ========" -ForegroundColor Magenta
    $r1 = & "$PSScriptRoot\Analyze-MalaikaExcel.ps1" -ExcelPath $f1.FullName
    $codes1 = @($r1 | ForEach-Object { $_.Codes } | Sort-Object -Unique)
    $codes2 = @($r2 | ForEach-Object { $_.Codes } | Sort-Object -Unique)
    $only1 = Compare-Object $codes1 $codes2 | Where-Object SideIndicator -eq '<=' | ForEach-Object InputObject
    $only2 = Compare-Object $codes1 $codes2 | Where-Object SideIndicator -eq '=>' | ForEach-Object InputObject
    $both = $codes1 | Where-Object { $codes2 -contains $_ }
    Write-Host "`n======== COMPARAISON CODES MKL ========" -ForegroundColor Green
    Write-Host "Fichier 1: $($codes1.Count) | Fichier 2: $($codes2.Count) | Communs: $($both.Count)"
    Write-Host "Uniquement fichier 1: $($only1.Count) | Uniquement fichier 2 (nouveaux?): $($only2.Count)"
    if ($only2.Count -gt 0) {
        $sample = $only2 | Select-Object -First 25
        Write-Host "Echantillon codes seulement fichier 2: $($sample -join ', ')"
    }
}
