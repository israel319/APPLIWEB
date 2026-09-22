Add-Type -AssemblyName System.IO.Compression.FileSystem
$ErrorActionPreference = 'Stop'
$f = Get-ChildItem -LiteralPath 'e:\Malaika' -Filter '*.xlsx' | Where-Object { $_.Name -like '*Incomplet*' } | Select-Object -First 1
$tmp = Join-Path $env:TEMP 'x2s'
if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }
New-Item -ItemType Directory $tmp | Out-Null
[IO.Compression.ZipFile]::ExtractToDirectory($f.FullName, $tmp)
[xml]$sst = Get-Content (Join-Path $tmp 'xl\sharedStrings.xml') -Encoding UTF8
$shared = @(); foreach ($si in $sst.sst.si) { if ($si.t.'#text') { $shared += $si.t.'#text' } elseif ($si.t) { $shared += [string]$si.t } else { $shared += '' } }
[xml]$sh = Get-Content (Join-Path $tmp 'xl\worksheets\sheet2.xml') -Encoding UTF8
$codes = @()
foreach ($row in $sh.worksheet.sheetData.row) {
    if ([int]$row.r -le 1) { continue }
    foreach ($c in $row.c) {
        if ($c.r -match '^B(\d+)$') {
            $v = if ($c.t -eq 's') { $shared[[int]$c.v] } else { [string]$c.v }
            if ($v) { $codes += $v.Trim() }
        }
    }
}
Write-Host "Total B column: $($codes.Count)"
Write-Host "Sample 20: $($codes | Select-Object -First 20 | ForEach-Object { $_ })"
Write-Host "MKL regex: $(($codes | Where-Object { $_ -match '^MKL\d' }).Count)"
Write-Host "Contains MKL: $(($codes | Where-Object { $_ -match 'MKL' }).Count)"
$codes | Group-Object { if ($_ -match '^([A-Za-z]+)') { $Matches[1] } else { 'OTHER' } } | Sort-Object Count -Descending | Select-Object -First 10 | Format-Table Name, Count
