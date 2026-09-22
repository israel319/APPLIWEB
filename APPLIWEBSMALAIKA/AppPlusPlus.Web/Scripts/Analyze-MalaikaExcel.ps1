param(
    [Parameter(Mandatory = $true)]
    [string]$ExcelPath
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
    if (-not (Test-Path -LiteralPath $sstPath)) { return @() }
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
    if ($cell.t -eq 's') {
        $idx = [int]$cell.v
        if ($idx -lt 0 -or $idx -ge $shared.Count) { return $null }
        return $shared[$idx].Trim()
    }
    if ($cell.t -eq 'str') { return [string]$cell.v }
    if ($cell.v -ne $null -and $cell.v -ne '') { return [string]$cell.v }
    return $null
}

function Find-ColumnMap($headers) {
    $map = @{ Code = 0; Category = 0; Famille = 0; Designation = 0; PriceUsd = 0
        QteInit = 0; QtePhys = 0; StockActuel = 0; Obs = 0 }
    foreach ($col in $headers.Keys) {
        $h = ($headers[$col] -as [string])
        if (-not $h) { continue }
        $hn = $h.ToLowerInvariant() -replace '\s+', ' '
        if ($hn -match 'code.?bar|code barres|barcode') { $map.Code = $col }
        elseif ($hn -eq 'catégorie' -or $hn -eq 'categorie') { $map.Category = $col }
        elseif ($hn -eq 'famille') { $map.Famille = $col }
        elseif ($hn -match 'désignation|designation') { $map.Designation = $col }
        elseif ($hn -match 'prix unitaire' -and $map.PriceUsd -eq 0) { $map.PriceUsd = $col }
        elseif ($hn -match 'qté physique|qte physique|quantité physique|quantite physique') { $map.QtePhys = $col }
        elseif ($hn -match 'qté initiale|qte initiale|quantité initiale|quantite initiale') { $map.QteInit = $col }
        elseif ($hn -match 'stock actuel') { $map.StockActuel = $col }
        elseif ($hn -match 'observation') { $map.Obs = $col }
    }
    return $map
}

function Analyze-Sheet([string]$sheetPath, $shared, [string]$sheetName) {
    [xml]$sheet = Get-Content -LiteralPath $sheetPath -Encoding UTF8
    $rows = @{}
    $maxCol = 0
    foreach ($row in $sheet.worksheet.sheetData.row) {
        $rIdx = [int]$row.r
        $cells = @{}
        foreach ($c in $row.c) {
            $col = Get-ColumnIndex $c.r
            if ($col -gt $maxCol) { $maxCol = $col }
            $cells[$col] = $c
        }
        $rows[$rIdx] = $cells
    }

    $headerRow = $rows[1]
    $headers = @{}
    if ($headerRow) {
        foreach ($col in ($headerRow.Keys | Sort-Object)) {
            $headers[$col] = Get-CellText $headerRow[$col] $shared
        }
    }

    Write-Host ""
    Write-Host "=== Feuille: $sheetName ===" -ForegroundColor Cyan
    Write-Host ("Colonnes detectees (ligne 1): {0}" -f $maxCol)
    foreach ($col in ($headers.Keys | Sort-Object)) {
        $letter = [char]([int][char]'A' + $col - 1)
        if ($col -gt 26) { $letter = "col$col" }
        Write-Host ("  {0,2} ({1}): {2}" -f $col, $letter, $headers[$col])
    }

    $colMap = Find-ColumnMap $headers
    if ($colMap.Code -gt 0) {
        Write-Host ("Mapping: Code=col{0} Cat=col{1} Fam=col{2} Des=col{3} Prix=col{4} Stock=col{5}/{6}" -f `
            $colMap.Code, $colMap.Category, $colMap.Famille, $colMap.Designation, $colMap.PriceUsd, $colMap.StockActuel, $colMap.QtePhys)
    }

    $dataRows = @()
    foreach ($rIdx in ($rows.Keys | Sort-Object)) {
        if ($rIdx -le 1) { continue }
        $cells = $rows[$rIdx]
        $codeCol = if ($colMap.Code -gt 0) { $colMap.Code } else { 4 }
        $code = Get-CellText $cells[$codeCol] $shared
        if ([string]::IsNullOrWhiteSpace($code)) { continue }
        $catCol = if ($colMap.Category -gt 0) { $colMap.Category } else { 0 }
        $famCol = if ($colMap.Famille -gt 0) { $colMap.Famille } else { 1 }
        $desCol = if ($colMap.Designation -gt 0) { $colMap.Designation } else { 3 }
        if ($desCol -eq 0) { $desCol = 3 }
        $dataRows += [pscustomobject]@{
            Row      = $rIdx
            Famille  = if ($famCol -gt 0) { Get-CellText $cells[$famCol] $shared } else { $null }
            Category = if ($catCol -gt 0) { Get-CellText $cells[$catCol] $shared } else { $null }
            Designation = if ($desCol -gt 0) { Get-CellText $cells[$desCol] $shared } else { $null }
            Code     = $code.Trim()
            QteInit  = if ($colMap.QteInit -gt 0) { Get-CellText $cells[$colMap.QteInit] $shared } else { $null }
            QtePhys  = if ($colMap.QtePhys -gt 0) { Get-CellText $cells[$colMap.QtePhys] $shared } else { $null }
            StockActuel = if ($colMap.StockActuel -gt 0) { Get-CellText $cells[$colMap.StockActuel] $shared } else { $null }
            PriceUsd = if ($colMap.PriceUsd -gt 0) { Get-CellText $cells[$colMap.PriceUsd] $shared } else { $null }
            Obs      = if ($colMap.Obs -gt 0) { Get-CellText $cells[$colMap.Obs] $shared } else { $null }
        }
    }

    $mkl = $dataRows | Where-Object { $_.Code -match '^MKL\d' }
    $allCodes = $dataRows | Where-Object { $_.Code -match '\S' }
    Write-Host ("Lignes avec code-barres: {0}" -f $allCodes.Count)
    Write-Host ("Lignes MKL*: {0}" -f $mkl.Count)
    if ($allCodes.Count -gt 0 -and $mkl.Count -eq 0) {
        $samples = $allCodes | Select-Object -First 8 -ExpandProperty Code
        Write-Host ("Echantillon codes (non MKL?): {0}" -f ($samples -join ', '))
    }

    if ($mkl.Count -gt 0) {
        $cats = $mkl | ForEach-Object { $_.Category } | Where-Object { $_ } | Sort-Object -Unique
        Write-Host ("Categories (col B) uniques: {0}" -f $cats.Count)
        $cats | ForEach-Object { Write-Host "  - $_" }

        $fam = $mkl | ForEach-Object { $_.Famille } | Where-Object { $_ } | Sort-Object -Unique
        Write-Host ("Familles (col A) uniques: {0}" -f $fam.Count)
        $fam | ForEach-Object { Write-Host "  - $_" }

        $noCat = ($mkl | Where-Object { -not $_.Category -and -not $_.Famille }).Count
        $noDes = ($mkl | Where-Object { -not $_.Designation }).Count
        $noPrice = ($mkl | Where-Object { -not $_.PriceUsd -or [double]$_.PriceUsd -eq 0 }).Count
        $withStock = ($mkl | Where-Object { $_.StockActuel -and [double]$_.StockActuel -gt 0 }).Count
        $withQtePhys = ($mkl | Where-Object { $_.QtePhys -and [double]$_.QtePhys -gt 0 }).Count
        Write-Host ("Lignes avec stock actuel > 0: {0} | qte physique > 0: {1}" -f $withStock, $withQtePhys)
        $dupCodes = $mkl | Group-Object Code | Where-Object { $_.Count -gt 1 }
        Write-Host ("Sans categorie: {0} | Sans designation: {1} | Prix USD vide/0: {2}" -f $noCat, $noDes, $noPrice)
        Write-Host ("Codes MKL dupliques: {0}" -f $dupCodes.Count)
        if ($dupCodes.Count -gt 0) {
            $dupCodes | Select-Object -First 5 | ForEach-Object { Write-Host ("  {0} x{1}" -f $_.Name, $_.Count) }
        }

        Write-Host "Echantillon (5 premieres lignes MKL):"
        $mkl | Select-Object -First 5 | Format-Table Row, Code, Category, Designation, QtePhys, PriceUsd -AutoSize | Out-String | Write-Host
    }

    return [pscustomobject]@{
        SheetName = $sheetName
        SheetFile = [System.IO.Path]::GetFileName($sheetPath)
        MklCount  = $mkl.Count
        Categories = ($mkl | ForEach-Object { $_.Category } | Sort-Object -Unique)
        Codes = ($mkl | ForEach-Object { $_.Code })
        Items = $mkl
    }
}

if (-not (Test-Path -LiteralPath $ExcelPath)) {
    throw "Fichier introuvable: $ExcelPath"
}

$tmp = Join-Path $env:TEMP ("xlsx_analyze_{0}" -f [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp | Out-Null
try {
    [System.IO.Compression.ZipFile]::ExtractToDirectory($ExcelPath, $tmp)
    $shared = Read-SharedStringsSimple (Join-Path $tmp 'xl\sharedStrings.xml')

    [xml]$wb = Get-Content -LiteralPath (Join-Path $tmp 'xl\workbook.xml') -Encoding UTF8
    $relsPath = Join-Path $tmp 'xl\_rels\workbook.xml.rels'
    [xml]$rels = Get-Content -LiteralPath $relsPath -Encoding UTF8
    $relMap = @{}
    foreach ($r in $rels.Relationships.Relationship) {
        $relMap[$r.Id] = $r.Target
    }

    $results = @()
    $sheetIdx = 0
    $sheetNodes = @($wb.workbook.sheets.sheet)
    foreach ($sh in $sheetNodes) {
        $sheetIdx++
        $name = [string]$sh.name
        $rid = $sh.GetAttribute('id', 'http://schemas.openxmlformats.org/officeDocument/2006/relationships')
        if (-not $rid) { $rid = $sh.'r:id' }
        $target = $relMap[$rid] -replace '^\.\./', ''
        $sheetPath = Join-Path $tmp ("xl\{0}" -f ($target -replace '/', '\'))
        if (Test-Path $sheetPath) {
            $results += Analyze-Sheet $sheetPath $shared $name
        }
        else {
            Write-Host "Feuille non trouvee: $name -> $sheetPath" -ForegroundColor Yellow
        }
    }

    # Feuilles orphelines (sheet1.xml, sheet2.xml...) si workbook incomplet
    $wsDir = Join-Path $tmp 'xl\worksheets'
    if (Test-Path $wsDir) {
        foreach ($sf in Get-ChildItem $wsDir -Filter 'sheet*.xml') {
            $already = $results | Where-Object { $_.SheetFile -eq $sf.Name }
            if (-not $already) {
                $results += Analyze-Sheet $sf.FullName $shared $sf.Name
            }
        }
    }

    Write-Host ""
    Write-Host "=== RESUME FICHIER ===" -ForegroundColor Green
    Write-Host ("Chemin: {0}" -f $ExcelPath)
    Write-Host ("Feuilles analysees: {0}" -f $results.Count)
    $allCodes = $results | ForEach-Object { $_.Codes } | Where-Object { $_ }
    Write-Host ("Total codes MKL toutes feuilles: {0}" -f $allCodes.Count)
    return ,$results
}
finally {
    Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}
