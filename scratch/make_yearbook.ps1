Add-Type -AssemblyName System.Drawing

$width = 1000
$height = 600
$bmp = New-Object System.Drawing.Bitmap($width, $height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Background: Cream Paper Color
$bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(242, 238, 228))
$g.Clear([System.Drawing.Color]::FromArgb(242, 238, 228))

# Center Book Spine Shadow
$spineBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(190, 185, 175))
$g.FillRectangle($spineBrush, 490, 0, 20, 600)

# Borders around pages
$pagePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(160, 155, 145), 2)
$g.DrawRectangle($pagePen, 20, 20, 460, 560)
$g.DrawRectangle($pagePen, 520, 20, 460, 560)

# 6 Large Photo Frames (3 on left, 3 on right - very clean and spacious)
$coords = @(
    @(90, 80),  @(280, 80),  @(185, 320),
    @(590, 80), @(780, 80),  @(685, 320)
)

$photoBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(210, 210, 215))
$photoBorderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(70, 70, 75), 3)
$headBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(130, 130, 140))

$redPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(200, 15, 15), 4)
$blackPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(15, 15, 15), 4)

$rnd = New-Object Random(4567)

for ($i = 0; $i -lt $coords.Length; $i++) {
    $x = $coords[$i][0]
    $y = $coords[$i][1]

    # Photo background
    $g.FillRectangle($photoBrush, $x, $y, 150, 190)
    $g.DrawRectangle($photoBorderPen, $x, $y, 150, 190)

    # Student Silhouette (Head & Shoulders)
    $g.FillEllipse($headBrush, $x + 45, $y + 30, 60, 60)
    $g.FillEllipse($headBrush, $x + 25, $y + 105, 100, 95)

    # Deface ONLY student #4 (Top Right photo on the right page)
    if ($i -eq 4) {
        for ($k = 0; $k -lt 45; $k++) {
            $x1 = $x + $rnd.Next(5, 145)
            $y1 = $y + $rnd.Next(5, 185)
            $x2 = $x + $rnd.Next(5, 145)
            $y2 = $y + $rnd.Next(5, 185)

            if ($k % 2 -eq 0) {
                $g.DrawLine($redPen, $x1, $y1, $x2, $y2)
            } else {
                $g.DrawLine($blackPen, $x1, $y1, $x2, $y2)
            }
        }
    }
}

$outputPath = "c:\Users\Kimunet\Documents\The-bear\Assets\Sprite\yearbook_defaced_photo.png"
$bmp.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)

$g.Dispose()
$bmp.Dispose()
