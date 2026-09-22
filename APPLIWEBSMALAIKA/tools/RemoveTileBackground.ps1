# Removes baked checkerboard / light studio backgrounds from universe tile PNGs.
param(
    [Parameter(Mandatory = $true)]
    [string[]]$InputPaths
)

Add-Type -AssemblyName System.Drawing

function Test-BackgroundPixel([System.Drawing.Color]$c) {
    $mx = [Math]::Max($c.R, [Math]::Max($c.G, $c.B))
    $mn = [Math]::Min($c.R, [Math]::Min($c.G, $c.B))
    $chroma = $mx - $mn
    if ($chroma -gt 20) { return $false }
    $avg = ($c.R + $c.G + $c.B) / 3.0
    if ($avg -ge 246) { return $true }
    if ($avg -ge 188 -and $avg -le 232) { return $true }
    return $false
}

function Remove-LightBackground([string]$path) {
    $src = [System.Drawing.Bitmap]::FromFile($path)
    $w = $src.Width
    $h = $src.Height
    $visited = New-Object 'bool[,]' $h, $w
    $queue = [System.Collections.Generic.Queue[System.Drawing.Point]]::new()

    function Enqueue([int]$x, [int]$y) {
        if ($x -lt 0 -or $y -lt 0 -or $x -ge $w -or $y -ge $h) { return }
        if ($visited[$y, $x]) { return }
        $c = $src.GetPixel($x, $y)
        if (-not (Test-BackgroundPixel $c)) { return }
        $visited[$y, $x] = $true
        $null = $queue.Enqueue([System.Drawing.Point]::new($x, $y))
    }

    for ($x = 0; $x -lt $w; $x++) {
        Enqueue $x 0
        Enqueue $x ($h - 1)
    }
    for ($y = 0; $y -lt $h; $y++) {
        Enqueue 0 $y
        Enqueue ($w - 1) $y
    }

    while ($queue.Count -gt 0) {
        $p = $queue.Dequeue()
        Enqueue ($p.X + 1) $p.Y
        Enqueue ($p.X - 1) $p.Y
        Enqueue $p.X ($p.Y + 1)
        Enqueue $p.X ($p.Y - 1)
    }

    $out = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            if ($visited[$y, $x]) {
                $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
            }
            else {
                $c = $src.GetPixel($x, $y)
                $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $c.R, $c.G, $c.B))
            }
        }
    }

    $tmp = "$path.tmp.png"
    $out.Save($tmp, [System.Drawing.Imaging.ImageFormat]::Png)
    $src.Dispose()
    $out.Dispose()
    Move-Item -Force $tmp $path
    Write-Host "Processed $path"
}

foreach ($p in $InputPaths) {
    if (-not (Test-Path $p)) {
        Write-Warning "Missing: $p"
        continue
    }
    Remove-LightBackground (Resolve-Path $p).Path
}
