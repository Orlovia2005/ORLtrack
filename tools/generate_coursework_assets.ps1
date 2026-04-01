$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$assetsDir = Join-Path $root "coursework_assets"
if (Test-Path $assetsDir) {
    Remove-Item $assetsDir -Recurse -Force
}
New-Item -ItemType Directory -Path $assetsDir | Out-Null

$msoTrue = -1
$msoFalse = 0
$msoShapeRectangle = 1
$msoShapeRoundedRectangle = 5
$msoShapeOval = 9
$msoShapeChevron = 52

function Set-ShapeStyle {
    param(
        $shape,
        [string]$fillHex = "F5F7FB",
        [string]$lineHex = "334155",
        [double]$lineWeight = 1.5
    )

    $shape.Fill.ForeColor.RGB = [Convert]::ToInt32($fillHex, 16)
    $shape.Line.ForeColor.RGB = [Convert]::ToInt32($lineHex, 16)
    $shape.Line.Weight = $lineWeight
}

function Add-TextBox {
    param(
        $slide,
        [string]$text,
        [double]$left,
        [double]$top,
        [double]$width,
        [double]$height,
        [int]$fontSize = 18,
        [bool]$bold = $false,
        [int]$align = 2
    )

    $shape = $slide.Shapes.AddTextbox(1, $left, $top, $width, $height)
    $shape.TextFrame.TextRange.Text = $text
    $shape.TextFrame.TextRange.Font.Name = "Times New Roman"
    $shape.TextFrame.TextRange.Font.Size = $fontSize
    $shape.TextFrame.TextRange.Font.Bold = if ($bold) { $msoTrue } else { $msoFalse }
    $shape.TextFrame.TextRange.ParagraphFormat.Alignment = $align
    $shape.Line.Visible = $msoFalse
    return $shape
}

function Add-Box {
    param(
        $slide,
        [string]$text,
        [double]$left,
        [double]$top,
        [double]$width,
        [double]$height,
        [string]$fillHex = "F8FAFC",
        [string]$lineHex = "1F2937"
    )

    $shape = $slide.Shapes.AddShape($msoShapeRoundedRectangle, $left, $top, $width, $height)
    Set-ShapeStyle -shape $shape -fillHex $fillHex -lineHex $lineHex
    $shape.TextFrame.TextRange.Text = $text
    $shape.TextFrame.TextRange.Font.Name = "Times New Roman"
    $shape.TextFrame.TextRange.Font.Size = 17
    $shape.TextFrame.TextRange.Font.Bold = $msoTrue
    $shape.TextFrame.TextRange.ParagraphFormat.Alignment = 2
    return $shape
}

function Add-Caption {
    param($slide, [string]$title)
    Add-TextBox -slide $slide -text $title -left 30 -top 18 -width 1220 -height 28 -fontSize 22 -bold $true -align 2 | Out-Null
}

$ppt = New-Object -ComObject PowerPoint.Application
$ppt.Visible = $msoFalse
$presentation = $ppt.Presentations.Add()
$presentation.PageSetup.SlideWidth = 1280
$presentation.PageSetup.SlideHeight = 720

try {
    for ($i = 1; $i -le 11; $i++) {
        $null = $presentation.Slides.Add($i, 12)
    }

    $slide = $presentation.Slides.Item(1)
    Add-Caption $slide "Р”РёР°РіСЂР°РјРјР° РІР°СЂРёР°РЅС‚РѕРІ РёСЃРїРѕР»СЊР·РѕРІР°РЅРёСЏ ORLtrack"
    Add-Box $slide "Р РµРїРµС‚РёС‚РѕСЂ" 60 280 160 80 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "РЈС‡РµРЅРёРє" 1060 280 160 80 "DCFCE7" "15803D" | Out-Null
    Add-Box $slide "Р”РѕР±Р°РІР»РµРЅРёРµ СѓС‡РµРЅРёРєР°" 350 120 240 70 | Out-Null
    Add-Box $slide "РЈС‡РµС‚ Р·Р°РЅСЏС‚РёР№" 350 230 240 70 | Out-Null
    Add-Box $slide "РЈС‡РµС‚ РѕРїР»Р°С‚" 350 340 240 70 | Out-Null
    Add-Box $slide "РђРЅР°Р»РёС‚РёРєР° СЂРёСЃРєР°" 350 450 240 70 | Out-Null
    Add-Box $slide "РџСЂРѕСЃРјРѕС‚СЂ РїСЂРѕРіСЂРµСЃСЃР°" 690 230 240 70 | Out-Null
    Add-Box $slide "РџРѕР»СѓС‡РµРЅРёРµ СЂРµРєРѕРјРµРЅРґР°С†РёР№" 690 340 240 70 | Out-Null

    $slide = $presentation.Slides.Item(2)
    Add-Caption $slide "РЎСЂР°РІРЅРµРЅРёРµ РїРѕРґС…РѕРґРѕРІ Рє Р°РІС‚РѕРјР°С‚РёР·Р°С†РёРё"
    Add-TextBox $slide "РЈСЃР»РѕРІРЅР°СЏ РѕС†РµРЅРєР° СѓРґРѕР±СЃС‚РІР° Рё РїРѕР»РµР·РЅРѕСЃС‚Рё" 370 70 540 24 18 $false 2 | Out-Null
    $labels = @("РўР°Р±Р»РёС†С‹", "РЈРЅРёРІРµСЂСЃР°Р»СЊРЅС‹Рµ CRM", "LMS", "ORLtrack")
    $values = @(38, 54, 61, 92)
    for ($i = 0; $i -lt $labels.Count; $i++) {
        Add-TextBox $slide $labels[$i] 120 (140 + $i * 120) 180 24 18 $true 1 | Out-Null
        $bar = $slide.Shapes.AddShape($msoShapeRectangle, 320, (135 + $i * 120), $values[$i] * 7, 42)
        Set-ShapeStyle -shape $bar -fillHex @("CBD5E1","93C5FD","FDE68A","86EFAC")[$i] -lineHex "0F172A"
        Add-TextBox $slide "$($values[$i])%" (980) (140 + $i * 120) 120 24 18 $true 1 | Out-Null
    }

    $slide = $presentation.Slides.Item(3)
    Add-Caption $slide "ER-РґРёР°РіСЂР°РјРјР° Р±Р°Р·С‹ РґР°РЅРЅС‹С…"
    Add-Box $slide "User`nId`nEmail" 110 140 220 130 "E0F2FE" "0369A1" | Out-Null
    Add-Box $slide "Student`nId`nName`nBalance`nLessonRate" 390 110 250 190 "F8FAFC" "0F172A" | Out-Null
    Add-Box $slide "StudentLesson`nId`nDate`nType`nPrice" 760 110 250 190 "F8FAFC" "0F172A" | Out-Null
    Add-Box $slide "StudentPayment`nId`nDate`nAmount" 760 360 250 160 "F8FAFC" "0F172A" | Out-Null
    Add-TextBox $slide "1" 330 200 30 20 18 $true 2 | Out-Null
    Add-TextBox $slide "M" 660 200 30 20 18 $true 2 | Out-Null
    Add-TextBox $slide "M" 660 430 30 20 18 $true 2 | Out-Null

    $slide = $presentation.Slides.Item(4)
    Add-Caption $slide "РђСЂС…РёС‚РµРєС‚СѓСЂР° РїСЂРёР»РѕР¶РµРЅРёСЏ"
    Add-Box $slide "Р’РµР±-Р±СЂР°СѓР·РµСЂ" 80 260 180 90 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "ASP.NET Core MVC" 360 240 220 130 "E2E8F0" "334155" | Out-Null
    Add-Box $slide "РЎРµСЂРІРёСЃ Р°РЅР°Р»РёС‚РёРєРё" 680 150 220 90 "FDE68A" "B45309" | Out-Null
    Add-Box $slide "PostgreSQL" 680 320 220 90 "DCFCE7" "15803D" | Out-Null
    Add-Box $slide "GigaChat API" 980 150 220 90 "FBCFE8" "BE185D" | Out-Null
    Add-Box $slide "Razor Views / CSS / JS" 980 320 220 90 "E0E7FF" "4338CA" | Out-Null

    $slide = $presentation.Slides.Item(5)
    Add-Caption $slide "РЎС…РµРјР° СЂР°Р±РѕС‚С‹ Р°РЅР°Р»РёС‚РёС‡РµСЃРєРѕРіРѕ РјРѕРґСѓР»СЏ"
    Add-Box $slide "РСЃС‚РѕСЂРёСЏ Р·Р°РЅСЏС‚РёР№ Рё РѕРїР»Р°С‚" 90 280 220 90 "E0F2FE" "0284C7" | Out-Null
    Add-Box $slide "Р›РѕРєР°Р»СЊРЅС‹Р№ risk score" 390 280 220 90 "FEF3C7" "B45309" | Out-Null
    Add-Box $slide "GigaChat" 690 280 180 90 "FCE7F3" "BE185D" | Out-Null
    Add-Box $slide "РџРѕСЏСЃРЅРµРЅРёРµ Рё СЂРµРєРѕРјРµРЅРґР°С†РёСЏ" 950 260 240 130 "DCFCE7" "15803D" | Out-Null
    Add-TextBox $slide "РџСЂРёР·РЅР°РєРё: РїСЂРѕРїСѓСЃРєРё, РїР°СѓР·С‹, Р±Р°Р»Р°РЅСЃ" 320 410 360 24 18 $false 2 | Out-Null

    $slide = $presentation.Slides.Item(6)
    Add-Caption $slide "РњР°РєРµС‚ РіР»Р°РІРЅРѕР№ СЃС‚СЂР°РЅРёС†С‹ ORLtrack"
    $bg = $slide.Shapes.AddShape($msoShapeRectangle, 40, 80, 1200, 600)
    Set-ShapeStyle -shape $bg -fillHex "F8FAFC" -lineHex "CBD5E1"
    Add-Box $slide "ORLtrack" 70 105 160 50 "0F172A" "0F172A" | Out-Null
    Add-Box $slide "РЈС‡РµРЅРёРєРѕРІ: 18" 110 220 220 120 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "Р”РѕС…РѕРґ Р·Р° РјРµСЃСЏС†: 82 000" 370 220 220 120 "DCFCE7" "15803D" | Out-Null
    Add-Box $slide "РџСЂРѕРїСѓСЃРєРё: 6" 630 220 220 120 "FDE68A" "B45309" | Out-Null
    Add-Box $slide "Р РёСЃРєРѕРІР°РЅРЅС‹С… СѓС‡РµРЅРёРєРѕРІ: 4" 890 220 260 120 "FBCFE8" "BE185D" | Out-Null
    Add-Box $slide "Р›РµРЅС‚Р° Р°РєС‚РёРІРЅРѕСЃС‚Рё" 110 400 420 180 "FFFFFF" "CBD5E1" | Out-Null
    Add-Box $slide "AI-СЂРµРєРѕРјРµРЅРґР°С†РёРё" 580 400 570 180 "FFFFFF" "CBD5E1" | Out-Null

    $slide = $presentation.Slides.Item(7)
    Add-Caption $slide "РњР°РєРµС‚ СЃС‚СЂР°РЅРёС†С‹ СѓС‡РµРЅРёРєРѕРІ"
    $bg = $slide.Shapes.AddShape($msoShapeRectangle, 40, 80, 1200, 600)
    Set-ShapeStyle -shape $bg -fillHex "F8FAFC" -lineHex "CBD5E1"
    Add-Box $slide "РђРЅРЅР° РџРµС‚СЂРѕРІР°`nР‘Р°Р»Р°РЅСЃ: 4 500`nРЎС‚Р°РІРєР°: 1 500" 90 150 320 170 "FFFFFF" "CBD5E1" | Out-Null
    Add-Box $slide "РР»СЊСЏ РЎРјРёСЂРЅРѕРІ`nР‘Р°Р»Р°РЅСЃ: 1 000`nРЎС‚Р°РІРєР°: 1 200" 460 150 320 170 "FFFFFF" "CBD5E1" | Out-Null
    Add-Box $slide "РњР°СЂРёСЏ РћСЂР»РѕРІР°`nР‘Р°Р»Р°РЅСЃ: 0`nРЎС‚Р°РІРєР°: 1 800" 830 150 320 170 "FFFFFF" "CBD5E1" | Out-Null
    Add-Box $slide "РљРЅРѕРїРєРё: Р·Р°РЅСЏС‚РёРµ / РїСЂРѕРїСѓСЃРє / РїРѕРїРѕР»РЅРµРЅРёРµ" 90 370 500 110 "E0E7FF" "4338CA" | Out-Null
    Add-Box $slide "РСЃС‚РѕСЂРёСЏ СѓСЂРѕРєРѕРІ Рё РѕРїР»Р°С‚" 650 370 500 180 "FFFFFF" "CBD5E1" | Out-Null

    $slide = $presentation.Slides.Item(8)
    Add-Caption $slide "РњР°РєРµС‚ С„РѕСЂРјС‹ РґРѕР±Р°РІР»РµРЅРёСЏ СѓС‡РµРЅРёРєР° Рё РїРѕРїРѕР»РЅРµРЅРёСЏ Р±Р°Р»Р°РЅСЃР°"
    $card = $slide.Shapes.AddShape($msoShapeRoundedRectangle, 290, 110, 700, 500)
    Set-ShapeStyle -shape $card -fillHex "FFFFFF" -lineHex "CBD5E1"
    Add-TextBox $slide "Р”РѕР±Р°РІР»РµРЅРёРµ СѓС‡РµРЅРёРєР°" 390 150 500 30 24 $true 2 | Out-Null
    Add-Box $slide "РРјСЏ СѓС‡РµРЅРёРєР°" 360 220 560 55 "F8FAFC" "CBD5E1" | Out-Null
    Add-Box $slide "РљРѕРЅС‚Р°РєС‚" 360 300 560 55 "F8FAFC" "CBD5E1" | Out-Null
    Add-Box $slide "РЎС‚Р°РІРєР° Р·Р° Р·Р°РЅСЏС‚РёРµ" 360 380 260 55 "F8FAFC" "CBD5E1" | Out-Null
    Add-Box $slide "РќР°С‡Р°Р»СЊРЅС‹Р№ Р±Р°Р»Р°РЅСЃ" 660 380 260 55 "F8FAFC" "CBD5E1" | Out-Null
    Add-Box $slide "РЎРѕС…СЂР°РЅРёС‚СЊ" 510 470 180 60 "DCFCE7" "15803D" | Out-Null

    $slide = $presentation.Slides.Item(9)
    Add-Caption $slide "РњР°РєРµС‚ СЃС‚СЂР°РЅРёС†С‹ Р°РЅР°Р»РёС‚РёРєРё"
    $bg = $slide.Shapes.AddShape($msoShapeRectangle, 40, 80, 1200, 600)
    Set-ShapeStyle -shape $bg -fillHex "F8FAFC" -lineHex "CBD5E1"
    Add-Box $slide "Р’С‹СЃРѕРєРёР№ СЂРёСЃРє`nРњР°СЂРёСЏ РћСЂР»РѕРІР°" 90 150 260 140 "FECACA" "B91C1C" | Out-Null
    Add-Box $slide "РЎСЂРµРґРЅРёР№ СЂРёСЃРє`nРР»СЊСЏ РЎРјРёСЂРЅРѕРІ" 390 150 260 140 "FDE68A" "B45309" | Out-Null
    Add-Box $slide "РќРёР·РєРёР№ СЂРёСЃРє`nРђРЅРЅР° РџРµС‚СЂРѕРІР°" 690 150 260 140 "BBF7D0" "15803D" | Out-Null
    Add-Box $slide "AI-РѕР±СЉСЏСЃРЅРµРЅРёРµ: СЃРЅРёР·РёР»Р°СЃСЊ С‡Р°СЃС‚РѕС‚Р° Р·Р°РЅСЏС‚РёР№, Р±Р°Р»Р°РЅСЃ РЅРµ РїРѕРїРѕР»РЅСЏР»СЃСЏ 21 РґРµРЅСЊ" 90 360 520 180 "FFFFFF" "CBD5E1" | Out-Null
    Add-Box $slide "Р РµРєРѕРјРµРЅРґР°С†РёСЏ: СЃРІСЏР·Р°С‚СЊСЃСЏ СЃ СѓС‡РµРЅРёРєРѕРј Рё РїСЂРµРґР»РѕР¶РёС‚СЊ РЅРѕРІРѕРµ РѕРєРЅРѕ РІ СЂР°СЃРїРёСЃР°РЅРёРё" 660 360 490 180 "FFFFFF" "CBD5E1" | Out-Null

    $slide = $presentation.Slides.Item(10)
    Add-Caption $slide "РџСЂРёРјРµСЂС‹ РїРѕР»СЊР·РѕРІР°С‚РµР»СЊСЃРєРёС… СЃС†РµРЅР°СЂРёРµРІ"
    Add-Box $slide "1. Р”РѕР±Р°РІРёС‚СЊ СѓС‡РµРЅРёРєР°" 90 170 250 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "2. РџРѕРїРѕР»РЅРёС‚СЊ Р±Р°Р»Р°РЅСЃ" 390 170 250 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "3. РћС‚РјРµС‚РёС‚СЊ Р·Р°РЅСЏС‚РёРµ" 690 170 250 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "4. РџРѕР»СѓС‡РёС‚СЊ Р°РЅР°Р»РёС‚РёРєСѓ" 990 170 220 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Box $slide "Р РµР·СѓР»СЊС‚Р°С‚: РґР°РЅРЅС‹Рµ РѕР±РЅРѕРІР»РµРЅС‹, РјРµС‚СЂРёРєРё РїРµСЂРµСЃС‡РёС‚Р°РЅС‹, СЂРёСЃРє РїРѕРєР°Р·Р°РЅ РІ Р°РЅР°Р»РёС‚РёРєРµ" 180 390 920 140 "DCFCE7" "15803D" | Out-Null

    $slide = $presentation.Slides.Item(11)
    Add-Caption $slide "РЎС‚Р°С‚РёСЃС‚РёРєР° РёСЃРїРѕР»СЊР·РѕРІР°РЅРёСЏ ORLtrack"
    Add-TextBox $slide "РЈСЃР»РѕРІРЅС‹Рµ РґР°РЅРЅС‹Рµ Р·Р° 4 РјРµСЃСЏС†Р°" 430 80 420 24 18 $false 2 | Out-Null
    $months = @("РЇРЅРІ", "Р¤РµРІ", "РњР°СЂ", "РђРїСЂ")
    $students = @(9, 12, 15, 18)
    $income = @(38, 51, 67, 82)
    for ($i = 0; $i -lt 4; $i++) {
        Add-TextBox $slide $months[$i] (180 + $i * 220) 590 120 24 18 $true 2 | Out-Null
        $bar1 = $slide.Shapes.AddShape($msoShapeRectangle, (200 + $i * 220), (500 - $students[$i] * 15), 55, ($students[$i] * 15))
        Set-ShapeStyle -shape $bar1 -fillHex "93C5FD" -lineHex "1D4ED8"
        $bar2 = $slide.Shapes.AddShape($msoShapeRectangle, (270 + $i * 220), (530 - $income[$i] * 4), 55, ($income[$i] * 4))
        Set-ShapeStyle -shape $bar2 -fillHex "86EFAC" -lineHex "15803D"
    }
    Add-TextBox $slide "РЎРёРЅРёР№ - СѓС‡РµРЅРёРєРё, Р·РµР»РµРЅС‹Р№ - РґРѕС…РѕРґ (С‚С‹СЃ. СЂСѓР±.)" 350 640 600 24 16 $false 2 | Out-Null

    for ($i = 1; $i -le 11; $i++) {
        $presentation.Slides.Item($i).Export((Join-Path $assetsDir ("figure_{0:D2}.png" -f $i)), "PNG", 1280, 720)
    }
}
finally {
    $presentation.Close()
    $ppt.Quit()
}

Write-Output "CREATED_ASSETS: $assetsDir"
