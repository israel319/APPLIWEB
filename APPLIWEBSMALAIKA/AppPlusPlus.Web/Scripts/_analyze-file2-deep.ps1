Add-Type -AssemblyName System.IO.Compression.FileSystem
$ErrorActionPreference = 'Stop'

function Get-ColumnIndex([string]$cellRef) {
    if ($cellRef -match '^([A-Z]+)') {
        $letters = $Matches[1]; $n = 0
        foreach ($c in $letters.ToCharArray()) { $n = $n * 26 + ([int][char]$c - [int][char]'A' + 1) }
        return $n
    }
    return 0
}

function Read-Shared($path) {
    if (-not (Test-Path $path)) { return @() }
    [xml]$x = Get-Content -LiteralPath $path -Encoding UTF8
    $list = @()
    foreach ($si in $x.sst.si) {
        if ($si.t.'#text') { $list += $si.t.'#text' }
        elseif ($si.t) { $list += [string]$si.t }
        elseif ($si.r) { $list += (($si.r | ForEach-Object { if ($_.t) { [string]$_.t } else { '' } }) -join '') }
        else { $list += '' }
    }
    return $list
}

function Get-Cell($cell, $shared) {
    if (-not $cell) { return $null }
    if ($cell.t -eq 's') { return $shared[[int]$cell.v].Trim() }
    if ($cell.v -ne $null -and $cell.v -ne '') { return [string]$cell.v }
    return $null
}

function Parse-Sheet($sheetPath, $shared) {
    [xml]$sheet = Get-Content -LiteralPath $sheetPath -Encoding UTF8
    $headers = @{}
    $rows = @{}
    foreach ($row in $sheet.worksheet.sheetData.row) {
        $rIdx = [int]$row.r
        $cells = @{}
        foreach ($c in $row.c) { $cells[(Get-ColumnIndex $c.r)] = $c }
        $rows[$rIdx] = $cells
    }
    if ($rows[1]) {
        foreach ($col in $rows[1].Keys) { $headers[$col] = Get-Cell $rows[1][$col] $shared }
    }
    # detect columns
    $codeCol = 0; $famCol = 0; $desCol = 0; $priceCol = 0; $stockCol = 0; $qtePhysCol = 0
    foreach ($col in $headers.Keys) {
        $h = ($headers[$col] -as [string]).ToLower()
        if ($h -match 'code-bar|code bar') { $codeCol = $col }
        if ($h -eq 'famille') { $famCol = $col }
        if ($h -match 'signation') { $desCol = $col }
        if ($h -match 'prix unitaire') { $priceCol = $col }
        if ($h -match 'stock actuel') { $stockCol = $col }
        if ($h -match 'quantit. physique|qt. physique') { $qtePhysCol = $col }
    }
    if ($codeCol -eq 0) { return @() }
    if ($desCol -eq 0) { $desCol = 3 }

    $items = @()
    foreach ($rIdx in ($rows.Keys | Sort-Object)) {
        if ($rIdx -le 1) { continue }
        $cells = $rows[$rIdx]
        $code = Get-Cell $cells[$codeCol] $shared
        if ([string]::IsNullOrWhiteSpace($code)) { continue }
        $stock = if ($stockCol -gt 0) { Get-Cell $cells[$stockCol] $shared } else { $null }
        $qphys = if ($qtePhysCol -gt 0) { Get-Cell $cells[$qtePhysCol] $shared } else { $null }
        $items += [pscustomobject]@{
            Code = $code.Trim()
            Famille = if ($famCol -gt 0) { Get-Cell $cells[$famCol] $shared } else { $null }
            Designation = Get-Cell $cells[$desCol] $shared
            PriceUsd = if ($priceCol -gt 0) { Get-Cell $cells[$priceCol] $shared } else { $null }
            StockActuel = $stock
            QtePhys = $qphys
        }
    }
    return $items
}

$f2 = Get-ChildItem 'e:\Malaika\*.xlsx' | Where-Object { $_.Name -like '*Incomplet*' } | Select-Object -First 1
$f1 = Get-ChildItem 'e:\Malaika\*.xlsx' | Where-Object { $_.Name -like '*Boutique 1*' } | Select-Object -First 1
$tmp = Join-Path $env:TEMP "deep_$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory $tmp | Out-Null
[IO.Compression.ZipFile]::ExtractToDirectory($f2.FullName, $tmp)
$shared = Read-Shared (Join-Path $tmp 'xl\sharedStrings.xml')

$catalogSheets = @(
    @{ Name = 'Inventaire General'; File = 'sheet2.xml' },
    @{ Name = 'Arrivage 20 Juillet'; File = 'sheet3.xml' },
    @{ Name = '03 Aout'; File = 'sheet4.xml' },
    @{ Name = '24 Aout'; File = 'sheet5.xml' }
)

$all = @()
foreach ($s in $catalogSheets) {
    $path = Join-Path $tmp "xl\worksheets\$($s.File)"
    if (-not (Test-Path $path)) { continue }
    $items = Parse-Sheet $path $shared
    Write-Host "`n--- $($s.Name) --- rows: $($items.Count)"
    $mkl = $items | Where-Object { $_.Code -match '^MKL\d' }
    Write-Host "Codes MKLxxx (Boutique1): $($mkl.Count)"
    $stockPos = ($items | Where-Object { $_.StockActuel -and [double]$_.StockActuel -gt 0 }).Count
    $physPos = ($items | Where-Object { $_.QtePhys -and [double]$_.QtePhys -gt 0 }).Count
    Write-Host "Stock actuel > 0: $stockPos | Qte physique > 0: $physPos"
    foreach ($it in ($items | Select-Object -First 3)) {
        Write-Host "  $($it.Code) | $($it.Famille) | stock=$($it.StockActuel) prix=$($it.PriceUsd)"
    }
    $items | ForEach-Object { $_ | Add-Member -NotePropertyName Sheet -NotePropertyValue $s.Name -Force; $all += $_ }
}

$unique = $all | Sort-Object Code -Unique
Write-Host "`n=== SYNTHESE FICHIER 2 (feuilles catalogue/arrivage) ==="
Write-Host "Lignes totales: $($all.Count) | Codes uniques: $($unique.Count)"
$prefix = $unique | ForEach-Object { if ($_.Code -match '^([A-Za-z]+)') { $Matches[1] } else { '?' } } | Group-Object | Sort-Object Count -Descending
Write-Host "Prefixes:"
$prefix | Select-Object -First 12 | ForEach-Object { Write-Host "  $($_.Name): $($_.Count)" }

# Compare file1 MKL
$tmp1 = Join-Path $env:TEMP "deep1_$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory $tmp1 | Out-Null
[IO.Compression.ZipFile]::ExtractToDirectory($f1.FullName, $tmp1)
$shared1 = Read-Shared (Join-Path $tmp1 'xl\sharedStrings.xml')
$f1items = Parse-Sheet (Join-Path $tmp1 'xl\worksheets\sheet2.xml') $shared1
# file1 has code col 4
[xml]$sh1 = Get-Content (Join-Path $tmp1 'xl\worksheets\sheet2.xml') -Encoding UTF8
$f1codes = @()
foreach ($row in $sh1.worksheet.sheetData.row) {
    if ([int]$row.r -le 1) { continue }
    foreach ($c in $row.c) {
        if ((Get-ColumnIndex $c.r) -eq 4) {
            $v = Get-Cell $c $shared1
            if ($v -match '^MKL') { $f1codes += $v.Trim() }
        }
    }
}
$codes2 = $unique.Code
$overlap = $f1codes | Where-Object { $codes2 -contains $_ }
Write-Host "`n=== vs FICHIER 1 (MKL) ==="
Write-Host "Fichier1 MKL: $($f1codes.Count) | Fichier2 uniques: $($codes2.Count) | Meme code MKL: $($overlap.Count)"
if ($overlap.Count -gt 0) { Write-Host "Exemples communs: $(($overlap | Select-Object -First 10) -join ', ')" }

Remove-Item $tmp, $tmp1 -Recurse -Force -ErrorAction SilentlyContinue
