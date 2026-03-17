# Generates DashLook.ico from the same vector path as assets/DashLook.svg.
# Edit the SVG first, then run this script to regenerate the ICO.
# Usage:  powershell scripts/create-icon.ps1

Add-Type -AssemblyName System.Drawing

$outputPath    = "$PSScriptRoot\..\assets\DashLook.ico"
$trayDestPath  = "$PSScriptRoot\..\src\DashLook\Resources\dashlook_tray.ico"

# ── Write ICO file ────────────────────────────────────────────────────────────
function New-IcoFile {
    param([string]$Path, [System.Drawing.Bitmap[]]$Bitmaps)

    $imageDataList = foreach ($bmp in $Bitmaps) {
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        , $ms.ToArray()
        $ms.Dispose()
    }

    $out = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create)
    $w   = New-Object System.IO.BinaryWriter($out)

    $w.Write([uint16]0)
    $w.Write([uint16]1)
    $w.Write([uint16]$Bitmaps.Count)

    $dataOffset = 6 + $Bitmaps.Count * 16
    for ($i = 0; $i -lt $Bitmaps.Count; $i++) {
        $bmp  = $Bitmaps[$i]
        $data = $imageDataList[$i]
        $ww = if ($bmp.Width  -eq 256) { [byte]0 } else { [byte]$bmp.Width  }
        $hh = if ($bmp.Height -eq 256) { [byte]0 } else { [byte]$bmp.Height }
        $w.Write($ww)
        $w.Write($hh)
        $w.Write([byte]0); $w.Write([byte]0)
        $w.Write([uint16]1); $w.Write([uint16]32)
        $w.Write([uint32]$data.Length)
        $w.Write([uint32]$dataOffset)
        $dataOffset += $data.Length
    }
    foreach ($data in $imageDataList) { $w.Write($data) }
    $w.Close(); $out.Close()
}

# ── Render one size — mirrors assets/DashLook.svg exactly ────────────────────
#
#  SVG viewBox = 0 0 256 256  →  scale = $Size / 256
#
#  Background tile : rect x=12 y=12 w=232 h=232 rx=56
#  Gradient        : #CBA6F7 top-left → #89B4FA bottom-right
#  D lettermark (even-odd):
#    Outer  : M 58,46  L 95,46
#             C 95,46  198,46  198,128   (right-side arc top)
#             C 198,210 95,210 95,210    (right-side arc bottom)
#             L 58,210  Z
#    Cutout : M 95,84
#             C 95,84  163,84  163,128  (inner arc top)
#             C 163,172 95,172 95,172   (inner arc bottom)
#             Z

function New-IconBitmap {
    param([int]$Size)

    $bmp = New-Object System.Drawing.Bitmap($Size, $Size)
    $g   = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode     = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.Clear([System.Drawing.Color]::Transparent)

    $k = $Size / 256.0  # scale factor

    # Helper: scale a SVG coordinate
    function S([float]$v) { [float]($v * $k) }

    # ── Background rounded tile ───────────────────────────────────────────────
    $tx = S(12); $ty = S(12); $tw = S(232); $th = S(232); $tr = S(56)
    $tile = New-Object System.Drawing.Drawing2D.GraphicsPath
    $tile.AddArc($tx,         $ty,         $tr, $tr, 180, 90)
    $tile.AddArc($tx+$tw-$tr, $ty,         $tr, $tr, 270, 90)
    $tile.AddArc($tx+$tw-$tr, $ty+$th-$tr, $tr, $tr, 0,   90)
    $tile.AddArc($tx,         $ty+$th-$tr, $tr, $tr, 90,  90)
    $tile.CloseFigure()

    $c1   = [System.Drawing.Color]::FromArgb(255, 203, 166, 247)  # #CBA6F7
    $c2   = [System.Drawing.Color]::FromArgb(255, 137, 180, 250)  # #89B4FA
    $rect = New-Object System.Drawing.Rectangle([int]$tx, [int]$ty, [int]$tw, [int]$th)
    $gb   = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
                $rect, $c1, $c2,
                [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal)
    $g.FillPath($gb, $tile)

    # Inner shine (radial highlight — approximate with a white semi-transparent ellipse)
    if ($Size -ge 32) {
        $shineW = [int](S(100)); $shineH = [int](S(80))
        $shineX = [int](S(20));  $shineY = [int](S(18))
        $shinePath = New-Object System.Drawing.Drawing2D.GraphicsPath
        $shinePath.AddEllipse($shineX, $shineY, $shineW, $shineH)
        $shineBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(30, 255, 255, 255))
        $g.FillPath($shineBrush, $shinePath)
        $shineBrush.Dispose(); $shinePath.Dispose()
    }

    # ── "D" lettermark (even-odd, mirrors SVG path exactly) ──────────────────
    $d = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d.FillMode = [System.Drawing.Drawing2D.FillMode]::Alternate

    # Outer D
    $d.StartFigure()
    $d.AddLine(  (S 58),(S 46),  (S 95),(S 46))
    $d.AddBezier((S 95),(S 46),  (S 95),(S 46),  (S 198),(S 46),  (S 198),(S 128))
    $d.AddBezier((S 198),(S 128),(S 198),(S 210),(S 95),(S 210),   (S 95),(S 210))
    $d.AddLine(  (S 95),(S 210), (S 58),(S 210))
    $d.CloseFigure()

    # Inner cutout
    $d.StartFigure()
    $d.AddBezier((S 95),(S 84),  (S 95),(S 84),  (S 163),(S 84),  (S 163),(S 128))
    $d.AddBezier((S 163),(S 128),(S 163),(S 172),(S 95),(S 172),   (S 95),(S 172))
    $d.CloseFigure()

    $dark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 30, 46))
    $g.FillPath($dark, $d)

    $dark.Dispose(); $d.Dispose(); $gb.Dispose(); $tile.Dispose(); $g.Dispose()
    return $bmp
}

# ── Generate sizes and write ICO ──────────────────────────────────────────────
$bitmaps = @(16, 32, 48, 256) | ForEach-Object { New-IconBitmap -Size $_ }

New-IcoFile -Path $outputPath -Bitmaps $bitmaps

# Also copy to the embedded tray resource so it stays in sync
Copy-Item -Path $outputPath -Destination $trayDestPath -Force

foreach ($b in $bitmaps) { $b.Dispose() }
Write-Host "Icon written: $outputPath"
Write-Host "Tray icon updated: $trayDestPath"
