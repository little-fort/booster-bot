<#
.SYNOPSIS
    Builds a synthetic-resolution copy of the labeled detection corpus.

.DESCRIPTION
    Renders every 1920x1080 screenshot in the corpus at ContentH (keeping 16:9), centered on a
    black CanvasW x CanvasH canvas. A taller canvas simulates letterboxing (16:10 fullscreen), a
    wider one pillarboxing (ultrawide). Screenshots of any other size are skipped, and their
    manifest lines dropped. Run `dev eval <Dst>\manifest.txt` on the result.

.EXAMPLE
    .\rescale.ps1 -Src . -Dst $env:TEMP\corpus-1440 -CanvasW 2560 -CanvasH 1440 -ContentH 1440
    .\rescale.ps1 -Src . -Dst $env:TEMP\corpus-1920x1200 -CanvasW 1920 -CanvasH 1200 -ContentH 1080
#>
param(
    [Parameter(Mandatory)][string]$Src,
    [Parameter(Mandatory)][string]$Dst,
    [Parameter(Mandatory)][int]$CanvasW,
    [Parameter(Mandatory)][int]$CanvasH,
    [Parameter(Mandatory)][int]$ContentH
)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $Dst | Out-Null

$contentW = [int][math]::Round($ContentH * 16 / 9)
$ox = [int](($CanvasW - $contentW) / 2); $oy = [int](($CanvasH - $ContentH) / 2)
$skipped = @()

foreach ($f in Get-ChildItem (Join-Path $Src '*.png')) {
    $img = [System.Drawing.Image]::FromFile($f.FullName)
    try {
        if ($img.Width -ne 1920 -or $img.Height -ne 1080) { $skipped += $f.Name; continue }
        $bmp = New-Object System.Drawing.Bitmap $CanvasW, $CanvasH
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.Clear([System.Drawing.Color]::Black)
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.DrawImage($img, $ox, $oy, $contentW, $ContentH)
        $bmp.Save((Join-Path $Dst $f.Name))
        $g.Dispose(); $bmp.Dispose()
    }
    finally { $img.Dispose() }
}

# Copy the manifest without lines for skipped screenshots
Get-Content (Join-Path $Src 'manifest.txt') |
    Where-Object { $line = $_; -not ($skipped | Where-Object { $line.StartsWith("${_}:") }) } |
    Set-Content (Join-Path $Dst 'manifest.txt')

"$((Get-ChildItem (Join-Path $Dst '*.png')).Count) images -> $Dst (skipped $($skipped.Count) non-1080p)"
