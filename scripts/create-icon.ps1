Add-Type -AssemblyName System.Drawing

$outputPath = "$PSScriptRoot\..\assets\DashLook.ico"

function New-IcoFile {
    param([string]$Path, [System.Drawing.Bitmap[]]$Bitmaps)

    $imageDataList = foreach ($bmp in $Bitmaps) {
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        , $ms.ToArray()
        $ms.Dispose()
    }

    $out = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create)
    $w = New-Object System.IO.BinaryWriter($out)

    # ICO header
    $w.Write([uint16]0)       # reserved
    $w.Write([uint16]1)       # type: icon
    $w.Write([uint16]$Bitmaps.Count)

    $dataOffset = 6 + $Bitmaps.Count * 16

    for ($i = 0; $i -lt $Bitmaps.Count; $i++) {
        $bmp  = $Bitmaps[$i]
        $data = $imageDataList[$i]
        $ww   = if ($bmp.Width  -eq 256) { 0 } else { [byte]$bmp.Width  }
        $hh   = if ($bmp.Height -eq 256) { 0 } else { [byte]$bmp.Height }
        $w.Write([byte]$ww)
        $w.Write([byte]$hh)
        $w.Write([byte]0)
        $w.Write([byte]0)
        $w.Write([uint16]1)
        $w.Write([uint16]32)
        $w.Write([uint32]$data.Length)
        $w.Write([uint32]$dataOffset)
        $dataOffset += $data.Length
    }

    foreach ($data in $imageDataList) {
        $w.Write($data)
    }

    $w.Close()
    $out.Close()
}

function New-IconBitmap {
    param([int]$Size)

    $bmp = New-Object System.Drawing.Bitmap($Size, $Size)
    $g   = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode       = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint   = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

    # Transparent background
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = [int]($Size * 0.06)
    $r   = [int]($Size * 0.22)
    $x   = $pad
    $y   = $pad
    $w   = $Size - $pad * 2
    $h   = $Size - $pad * 2

    # Rounded rectangle path
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($x,       $y,       $r, $r, 180, 90)
    $path.AddArc($x+$w-$r, $y,       $r, $r, 270, 90)
    $path.AddArc($x+$w-$r, $y+$h-$r, $r, $r, 0,   90)
    $path.AddArc($x,       $y+$h-$r, $r, $r, 90,  90)
    $path.CloseFigure()

    # Gradient fill: purple to blue
    $c1   = [System.Drawing.Color]::FromArgb(255, 203, 166, 247)  # #CBA6F7
    $c2   = [System.Drawing.Color]::FromArgb(255, 137, 180, 250)  # #89B4FA
    $rect = New-Object System.Drawing.Rectangle($x, $y, $w, $h)
    $gb   = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
                $rect, $c1, $c2,
                [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal)
    $g.FillPath($gb, $path)

    # Letter "D" in dark color
    $dark     = [System.Drawing.Color]::FromArgb(255, 24, 24, 37)
    $fontSize = [float]($Size * 0.54)
    $font     = New-Object System.Drawing.Font(
                    "Segoe UI",
                    $fontSize,
                    [System.Drawing.FontStyle]::Bold,
                    [System.Drawing.GraphicsUnit]::Pixel)
    $brush    = New-Object System.Drawing.SolidBrush($dark)
    $sf       = New-Object System.Drawing.StringFormat
    $sf.Alignment     = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $rectF    = New-Object System.Drawing.RectangleF(0, 0, $Size, $Size)
    $g.DrawString("D", $font, $brush, $rectF, $sf)

    $font.Dispose()
    $brush.Dispose()
    $gb.Dispose()
    $g.Dispose()

    return $bmp
}

$bitmaps = @(16, 32, 48, 256) | ForEach-Object { New-IconBitmap -Size $_ }

New-IcoFile -Path $outputPath -Bitmaps $bitmaps

foreach ($b in $bitmaps) { $b.Dispose() }

Write-Host "Icon created: $outputPath"
