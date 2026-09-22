# Import inventaire Excel Malaika Kids -> SQL (GlobalShoping)
# Colonnes utilisées : Catégorie (B), Désignation (C), Code-Barres (D), Prix USD (G), Observation (I) optionnelle
# Ignorées : Famille (A), Qté Initiale (E), Qté Physique (F), Valeur Physique (H)
param(
    [string]$ExcelPath = "e:\Malaika\Boutique 1_Fichier _D'Inventaire_MalaikaKids.xlsx",
    [string]$OutSql = "$PSScriptRoot\Migration_MalaikaKids_Import_Excel_Generated.sql",
    [int]$IdLocalisationStock = 1,
    [int]$IdMarque = 1,
    [int]$IdMesure = 1,
    [int]$IdMonais = 2,
    [int]$IdType = 1,
    [decimal]$Taux = 0,
    [string]$SqlServer = "localhost",
    [string]$Database = "GlobalShoping",
    [switch]$ApplyToDatabase
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-ColumnIndex([string]$cellRef) {
    if ($cellRef -match '^([A-Z]+)') {
        $letters = $Matches[1]
        $n = 0
        foreach ($c in $letters.ToCharArray()) { $n = $n * 26 + ([int][char]$c - [int][char]'A' + 1) }
        return $n
    }
    return 0
}

function Read-SharedStringsSimple([string]$sstPath) {
    [xml]$x = Get-Content -LiteralPath $sstPath -Encoding UTF8
    $list = @()
    foreach ($si in $x.sst.si) {
        if ($si.t -and $si.t.'#text') { $list += $si.t.'#text' }
        elseif ($si.t) { $list += [string]$si.t }
        elseif ($si.r) {
            $text = ($si.r | ForEach-Object { if ($_.t) { [string]$_.t } else { '' } }) -join ''
            $list += $text
        }
        else { $list += '' }
    }
    return $list
}

function Get-CellText($cell, $shared) {
    if (-not $cell) { return $null }
    if ($cell.t -eq 's') { return $shared[[int]$cell.v].Trim() }
    if ($cell.t -eq 'str') { return [string]$cell.v }
    if ($cell.v -ne $null -and $cell.v -ne '') { return [string]$cell.v }
    return $null
}

function Sql-Escape([string]$s) {
    if ($null -eq $s) { return '' }
    return ($s -replace "'", "''")
}

function Convert-UsdToFc([decimal]$usd, [decimal]$taux) {
    if ($usd -le 0 -or $taux -le 0) { return 0 }
    $fc = [decimal]::Ceiling($usd * $taux)
    if ($fc -le 0) { return 1 }
    return $fc
}

function Get-TauxFromDatabase([string]$server, [string]$database) {
    Add-Type -AssemblyName System.Data
    $cs = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC"
        $v = $cmd.ExecuteScalar()
        return [decimal]$v
    }
    finally {
        $conn.Close()
    }
}

function Invoke-MalaikaImportDatabase {
    param(
        $Items, $Categories, [decimal]$TauxEffectif,
        [int]$IdLocalisationStock, [int]$IdMarque, [int]$IdMesure, [int]$IdMonais, [int]$IdType,
        [string]$Server, [string]$Database
    )
    Add-Type -AssemblyName System.Data
    $cs = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
    try {
        $tx = $conn.BeginTransaction()
        try {
            foreach ($cat in $Categories) {
                $cmd = $conn.CreateCommand()
                $cmd.Transaction = $tx
                $cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Categorys WHERE Description_Category = @cat)
    INSERT INTO dbo.T_Art_Categorys (Description_Category, Visible_Catalog) VALUES (@cat, 1);
"@
                $cmd.Parameters.AddWithValue("@cat", $cat) | Out-Null
                [void]$cmd.ExecuteNonQuery()
            }

            foreach ($it in $Items) {
                $parts = Split-Description $it.Designation
                if ($it.Observation -and -not $parts.III) {
                    $obs = $it.Observation.Trim()
                    if ($obs.Length -gt 50) { $obs = $obs.Substring(0, 50) }
                    $parts.III = $obs
                }
                $priceFc = Convert-UsdToFc $it.PriceUsd $TauxEffectif

                $cmd = $conn.CreateCommand()
                $cmd.Transaction = $tx
                $cmd.CommandText = @"
DECLARE @IdCat INT = (SELECT Id_Category FROM dbo.T_Art_Categorys WHERE Description_Category = @cat);
IF @IdCat IS NULL
BEGIN
    INSERT INTO dbo.T_Art_Categorys (Description_Category, Visible_Catalog) VALUES (@cat, 1);
    SET @IdCat = SCOPE_IDENTITY();
END

IF EXISTS (SELECT 1 FROM dbo.T_Arts WHERE Id_Article = @code)
    UPDATE dbo.T_Arts SET
        Id_Category = @IdCat, Id_Type = @idType, Id_Monais = @idMonais,
        Description = @d, Description_I = @di, Description_II = @dii, Description_III = @diii,
        Price = @priceFc, Can_Insert_After_0 = 0
    WHERE Id_Article = @code;
ELSE
    INSERT INTO dbo.T_Arts (
        Id_Article, Id_Marque, Id_Type, Id_Category, Id_Mesure,
        Description, Description_I, Description_II, Description_III,
        Price, Qte, Id_Monais, Soeuil, Internal, Can_Insert_After_0,
        [User], DateSys, Cumputer, isTransferable)
    VALUES (
        @code, @idMarque, @idType, @IdCat, @idMesure,
        @d, @di, @dii, @diii,
        @priceFc, 0, @idMonais, 0, 0, 0,
        N'IMPORT', CAST('2026-09-17' AS DATE), N'MALAIKA-IMPORT', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.T_Stock WHERE Id_Article = @code AND Id_Localisation = @loc)
    INSERT INTO dbo.T_Stock (Id_Article, Id_Localisation, Qte, Seuil, Qte_max, CodeBar, DateSys, UserLogin)
    VALUES (@code, @loc, 0, 0, 100, @code, CAST('2026-09-17' AS DATE), N'IMPORT');
"@
                [void]$cmd.Parameters.AddWithValue("@cat", $it.Category)
                [void]$cmd.Parameters.AddWithValue("@code", $it.Code)
                [void]$cmd.Parameters.AddWithValue("@idType", $IdType)
                [void]$cmd.Parameters.AddWithValue("@idMonais", $IdMonais)
                [void]$cmd.Parameters.AddWithValue("@idMarque", $IdMarque)
                [void]$cmd.Parameters.AddWithValue("@idMesure", $IdMesure)
                [void]$cmd.Parameters.AddWithValue("@loc", $IdLocalisationStock)
                [void]$cmd.Parameters.AddWithValue("@d", $parts.D)
                [void]$cmd.Parameters.Add("@di", [System.Data.SqlDbType]::NVarChar, 50)
                $cmd.Parameters["@di"].Value = if ($parts.I) { $parts.I } else { [DBNull]::Value }
                [void]$cmd.Parameters.Add("@dii", [System.Data.SqlDbType]::NVarChar, 50)
                $cmd.Parameters["@dii"].Value = if ($parts.II) { $parts.II } else { [DBNull]::Value }
                [void]$cmd.Parameters.Add("@diii", [System.Data.SqlDbType]::NVarChar, 50)
                $cmd.Parameters["@diii"].Value = if ($parts.III) { $parts.III } else { [DBNull]::Value }
                [void]$cmd.Parameters.AddWithValue("@priceFc", $priceFc)
                [void]$cmd.ExecuteNonQuery()
            }
            $tx.Commit()
            Write-Host "ApplyToDatabase OK: $($Items.Count) articles (prix FC, taux $TauxEffectif)."
        }
        catch {
            $tx.Rollback()
            throw
        }
    }
    finally {
        $conn.Close()
    }
}

function Split-Description([string]$full) {
    if ($null -eq $full) { $full = '' }
    $full = $full.Trim()
    if ($full.Length -le 50) { return @{ D = $full; I = $null; II = $null; III = $null } }
    $d = $full.Substring(0, 50)
    $rest = $full.Substring(50)
    $i = if ($rest.Length -gt 50) { $rest.Substring(0, 50) } else { $rest }; $rest = if ($rest.Length -gt 50) { $rest.Substring(50) } else { '' }
    $ii = if ($rest.Length -gt 50) { $rest.Substring(0, 50) } else { $rest }; $rest = if ($rest.Length -gt 50) { $rest.Substring(50) } else { '' }
    $iii = if ($rest.Length -gt 0) { if ($rest.Length -gt 50) { $rest.Substring(0, 50) } else { $rest } } else { $null }
    return @{ D = $d; I = $i; II = $ii; III = $iii }
}

$tmp = Join-Path $env:TEMP ("xlsx_import_{0}" -f [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp | Out-Null
try {
    [System.IO.Compression.ZipFile]::ExtractToDirectory($ExcelPath, $tmp)
    $shared = Read-SharedStringsSimple (Join-Path $tmp 'xl\sharedStrings.xml')
    [xml]$sheet = Get-Content -LiteralPath (Join-Path $tmp 'xl\worksheets\sheet2.xml') -Encoding UTF8

    $rows = @{}
    foreach ($row in $sheet.worksheet.sheetData.row) {
        $rIdx = [int]$row.r
        $cells = @{}
        foreach ($c in $row.c) {
            $col = Get-ColumnIndex $c.r
            $cells[$col] = $c
        }
        $rows[$rIdx] = $cells
    }

    $items = @()
    foreach ($rIdx in ($rows.Keys | Sort-Object)) {
        if ($rIdx -le 1) { continue }
        $cells = $rows[$rIdx]
        $code = Get-CellText $cells[4] $shared
        if ([string]::IsNullOrWhiteSpace($code)) { continue }
        if ($code -notmatch '^MKL\d') { continue }

        $category = Get-CellText $cells[2] $shared
        if (-not $category) { throw "Ligne $rIdx : Catégorie (col. B) manquante pour $code." }
        $designation = Get-CellText $cells[3] $shared
        if (-not $designation) { $designation = $code }
        $priceStr = Get-CellText $cells[7] $shared
        $observation = Get-CellText $cells[9] $shared

        $price = 0.0
        [void][double]::TryParse($priceStr, [System.Globalization.NumberStyles]::Any, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$price)

        $items += [pscustomobject]@{
            Code        = $code.Trim()
            Category    = $category.Trim()
            Designation = $designation.Trim()
            PriceUsd    = $price
            Observation = $observation
        }
    }

    if ($items.Count -eq 0) { throw 'Aucune ligne MKL trouvée dans Inventaire général.' }

    $categories = $items | ForEach-Object { $_.Category } | Sort-Object -Unique
    Write-Host ("Categories ({0}): {1}" -f $categories.Count, ($categories -join ' | '))

    $tauxEffectif = $Taux
    if ($tauxEffectif -le 0) {
        try { $tauxEffectif = Get-TauxFromDatabase -server $SqlServer -database $Database }
        catch { $tauxEffectif = 2810 }
    }
    Write-Host "Taux USD->FC : $tauxEffectif (Price stocke en FC, Id_Monais=$IdMonais)"

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine("-- Generated by Import-MalaikaKidsFromExcel.ps1 $(Get-Date -Format 'yyyy-MM-dd HH:mm')")
    [void]$sb.AppendLine("-- Source: $ExcelPath")
    [void]$sb.AppendLine("-- Prix Excel USD -> Price FC (taux $tauxEffectif), Id_Monais=$IdMonais")
    [void]$sb.AppendLine("-- sqlcmd -f 65001 -i ...")
    [void]$sb.AppendLine('USE GlobalShoping;')
    [void]$sb.AppendLine('GO')
    [void]$sb.AppendLine('SET NOCOUNT ON;')
    [void]$sb.AppendLine('SET XACT_ABORT ON;')
    [void]$sb.AppendLine('SET QUOTED_IDENTIFIER ON;')
    [void]$sb.AppendLine('BEGIN TRY')
    [void]$sb.AppendLine('    BEGIN TRANSACTION;')
    [void]$sb.AppendLine('')

    foreach ($cat in $categories) {
        $esc = Sql-Escape $cat
        [void]$sb.AppendLine("IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Categorys WHERE Description_Category = N'$esc')")
        [void]$sb.AppendLine("    INSERT INTO dbo.T_Art_Categorys (Description_Category, Visible_Catalog) VALUES (N'$esc', 1);")
    }
    [void]$sb.AppendLine('')

    foreach ($it in $items) {
        $catEsc = Sql-Escape $it.Category
        $parts = Split-Description $it.Designation
        if ($it.Observation -and -not $parts.III) {
            $obs = $it.Observation.Trim()
            if ($obs.Length -gt 50) { $obs = $obs.Substring(0, 50) }
            $parts.III = $obs
        }
        $d = Sql-Escape $parts.D
        $code = Sql-Escape $it.Code
        $iSql = if ($parts.I) { "N'$(Sql-Escape $parts.I)'" } else { 'NULL' }
        $iiSql = if ($parts.II) { "N'$(Sql-Escape $parts.II)'" } else { 'NULL' }
        $iiiSql = if ($parts.III) { "N'$(Sql-Escape $parts.III)'" } else { 'NULL' }
        $priceFc = Convert-UsdToFc $it.PriceUsd $tauxEffectif
        $price = $priceFc.ToString([System.Globalization.CultureInfo]::InvariantCulture)

        [void]$sb.AppendLine(@"
IF NOT EXISTS (SELECT 1 FROM dbo.T_Arts WHERE Id_Article = N'$code')
BEGIN
    INSERT INTO dbo.T_Arts (
        Id_Article, Id_Marque, Id_Type, Id_Category, Id_Mesure,
        Description, Description_I, Description_II, Description_III,
        Price, Qte, Id_Monais, Soeuil, Internal, Can_Insert_After_0,
        [User], DateSys, Cumputer, isTransferable
    )
    SELECT
        N'$code', $IdMarque, $IdType,
        c.Id_Category, $IdMesure,
        N'$d', $iSql, $iiSql, $iiiSql,
        $price, 0, $IdMonais, 0, 0, 0,
        N'IMPORT', CAST('2026-09-17' AS DATE), N'MALAIKA-IMPORT', 1
    FROM dbo.T_Art_Categorys c WHERE c.Description_Category = N'$catEsc';

    IF NOT EXISTS (SELECT 1 FROM dbo.T_Stock WHERE Id_Article = N'$code' AND Id_Localisation = $IdLocalisationStock)
        INSERT INTO dbo.T_Stock (Id_Article, Id_Localisation, Qte, Seuil, Qte_max, CodeBar, DateSys, UserLogin)
        VALUES (N'$code', $IdLocalisationStock, 0, 0, 100, N'$code', CAST('2026-09-17' AS DATE), N'IMPORT');
END
ELSE
BEGIN
    UPDATE a SET
        a.Id_Category = c.Id_Category,
        a.Description = N'$d',
        a.Description_I = $iSql,
        a.Description_II = $iiSql,
        a.Description_III = $iiiSql,
        a.Price = $price,
        a.Id_Monais = $IdMonais,
        a.Id_Type = $IdType,
        a.Can_Insert_After_0 = 0
    FROM dbo.T_Arts a
    INNER JOIN dbo.T_Art_Categorys c ON c.Description_Category = N'$catEsc'
    WHERE a.Id_Article = N'$code';
END
"@)
    }

    [void]$sb.AppendLine(@"
    COMMIT TRANSACTION;
    PRINT CONCAT('Import OK — ', $($items.Count), ' articles, ', $($categories.Count), ' catégories.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

SELECT c.Description_Category, COUNT(*) AS NbArticles
FROM dbo.T_Arts a
INNER JOIN dbo.T_Art_Categorys c ON c.Id_Category = a.Id_Category
GROUP BY c.Description_Category
ORDER BY c.Description_Category;

SELECT COUNT(*) AS Articles FROM dbo.T_Arts;
"@)

    [System.IO.File]::WriteAllText($OutSql, $sb.ToString(), [System.Text.UTF8Encoding]::new($true))
    Write-Host "Generated $($items.Count) articles -> $OutSql (UTF-8 BOM; sqlcmd -f 65001)"

    if ($ApplyToDatabase) {
        Invoke-MalaikaImportDatabase -Items $items -Categories $categories -TauxEffectif $tauxEffectif `
            -IdLocalisationStock $IdLocalisationStock -IdMarque $IdMarque -IdMesure $IdMesure `
            -IdMonais $IdMonais -IdType $IdType -Server $SqlServer -Database $Database
    }

    return $OutSql
}
finally {
    Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}
