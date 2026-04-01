$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$assetsDir = Join-Path $root "coursework_assets"
if (Test-Path $assetsDir) {
    Remove-Item $assetsDir -Recurse -Force
}
New-Item -ItemType Directory -Path $assetsDir | Out-Null

$msoTrue = -1
$msoFalse = 0
$rect = 1
$round = 5

function Style-Shape($shape, [string]$fillHex = "F8FAFC", [string]$lineHex = "334155") {
    $shape.Fill.ForeColor.RGB = [Convert]::ToInt32($fillHex, 16)
    $shape.Line.ForeColor.RGB = [Convert]::ToInt32($lineHex, 16)
    $shape.Line.Weight = 1.5
}

function Add-Label($slide, [string]$text, [double]$left, [double]$top, [double]$width, [double]$height, [int]$fontSize = 18, [bool]$bold = $false, [int]$align = 2) {
    $shape = $slide.Shapes.AddTextbox(1, $left, $top, $width, $height)
    $shape.TextFrame.TextRange.Text = $text
    $shape.TextFrame.TextRange.Font.Name = "Times New Roman"
    $shape.TextFrame.TextRange.Font.Size = $fontSize
    $shape.TextFrame.TextRange.Font.Bold = if ($bold) { $msoTrue } else { $msoFalse }
    $shape.TextFrame.TextRange.ParagraphFormat.Alignment = $align
    $shape.Line.Visible = $msoFalse
    return $shape
}

function Add-Card($slide, [string]$text, [double]$left, [double]$top, [double]$width, [double]$height, [string]$fillHex = "FFFFFF", [string]$lineHex = "CBD5E1") {
    $shape = $slide.Shapes.AddShape($round, $left, $top, $width, $height)
    Style-Shape $shape $fillHex $lineHex
    $shape.TextFrame.TextRange.Text = $text
    $shape.TextFrame.TextRange.Font.Name = "Times New Roman"
    $shape.TextFrame.TextRange.Font.Size = 17
    $shape.TextFrame.TextRange.Font.Bold = $msoTrue
    $shape.TextFrame.TextRange.ParagraphFormat.Alignment = 2
    return $shape
}

$ppt = New-Object -ComObject PowerPoint.Application
$ppt.Visible = $msoTrue
$presentation = $ppt.Presentations.Add()
$presentation.PageSetup.SlideWidth = 1280
$presentation.PageSetup.SlideHeight = 720

try {
    for ($i = 1; $i -le 11; $i++) {
        $null = $presentation.Slides.Add($i, 12)
    }

    $s = $presentation.Slides.Item(1)
    Add-Label $s "Use Case Diagram" 400 20 480 28 24 $true 2 | Out-Null
    Add-Card $s "Tutor" 80 280 160 80 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "Student" 1040 280 160 80 "DCFCE7" "15803D" | Out-Null
    Add-Card $s "Add student" 360 120 220 70 | Out-Null
    Add-Card $s "Track lessons" 360 230 220 70 | Out-Null
    Add-Card $s "Track balance" 360 340 220 70 | Out-Null
    Add-Card $s "Risk analytics" 360 450 220 70 | Out-Null
    Add-Card $s "View progress" 700 230 220 70 | Out-Null
    Add-Card $s "Get advice" 700 340 220 70 | Out-Null

    $s = $presentation.Slides.Item(2)
    Add-Label $s "Comparison of solutions" 360 20 560 28 24 $true 2 | Out-Null
    $labels = @("Sheets","CRM","LMS","ORLtrack")
    $values = @(38, 54, 61, 92)
    for ($i = 0; $i -lt 4; $i++) {
        Add-Label $s $labels[$i] 120 (150 + $i * 110) 160 24 18 $true 1 | Out-Null
        $bar = $s.Shapes.AddShape($rect, 320, (145 + $i * 110), $values[$i] * 7, 40)
        Style-Shape $bar @("CBD5E1","93C5FD","FDE68A","86EFAC")[$i] "0F172A"
        Add-Label $s "$($values[$i])%" 980 (150 + $i * 110) 120 24 18 $true 1 | Out-Null
    }

    $s = $presentation.Slides.Item(3)
    Add-Label $s "Database schema" 420 20 440 28 24 $true 2 | Out-Null
    Add-Card $s "User`nId`nEmail" 110 140 220 130 "E0F2FE" "0369A1" | Out-Null
    Add-Card $s "Student`nId`nName`nBalance`nLessonRate" 390 110 250 190 | Out-Null
    Add-Card $s "StudentLesson`nId`nDate`nType`nPrice" 760 110 250 190 | Out-Null
    Add-Card $s "StudentPayment`nId`nDate`nAmount" 760 360 250 160 | Out-Null

    $s = $presentation.Slides.Item(4)
    Add-Label $s "App architecture" 430 20 420 28 24 $true 2 | Out-Null
    Add-Card $s "Browser" 90 270 160 80 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "ASP.NET Core MVC" 350 240 220 130 "E2E8F0" "334155" | Out-Null
    Add-Card $s "Analytics service" 650 150 220 90 "FDE68A" "B45309" | Out-Null
    Add-Card $s "PostgreSQL" 650 320 220 90 "DCFCE7" "15803D" | Out-Null
    Add-Card $s "GigaChat" 960 150 220 90 "FBCFE8" "BE185D" | Out-Null
    Add-Card $s "Razor UI" 960 320 220 90 "E0E7FF" "4338CA" | Out-Null

    $s = $presentation.Slides.Item(5)
    Add-Label $s "Analytics flow" 450 20 380 28 24 $true 2 | Out-Null
    Add-Card $s "Lessons and payments" 90 280 220 90 "E0F2FE" "0284C7" | Out-Null
    Add-Card $s "Risk score" 390 280 180 90 "FEF3C7" "B45309" | Out-Null
    Add-Card $s "GigaChat" 660 280 180 90 "FCE7F3" "BE185D" | Out-Null
    Add-Card $s "Advice for tutor" 930 260 250 130 "DCFCE7" "15803D" | Out-Null

    $s = $presentation.Slides.Item(6)
    Add-Label $s "Dashboard mockup" 430 20 420 28 24 $true 2 | Out-Null
    $bg = $s.Shapes.AddShape($rect, 40, 80, 1200, 600)
    Style-Shape $bg "F8FAFC" "CBD5E1"
    Add-Card $s "Students: 18" 110 220 220 120 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "Income: 82 000" 370 220 220 120 "DCFCE7" "15803D" | Out-Null
    Add-Card $s "Skips: 6" 630 220 220 120 "FDE68A" "B45309" | Out-Null
    Add-Card $s "High risk: 4" 890 220 260 120 "FBCFE8" "BE185D" | Out-Null
    Add-Card $s "Recent activity" 110 400 420 180 | Out-Null
    Add-Card $s "AI insights" 580 400 570 180 | Out-Null

    $s = $presentation.Slides.Item(7)
    Add-Label $s "Students page mockup" 400 20 500 28 24 $true 2 | Out-Null
    $bg = $s.Shapes.AddShape($rect, 40, 80, 1200, 600)
    Style-Shape $bg "F8FAFC" "CBD5E1"
    Add-Card $s "Anna Petrova`nBalance: 4500`nRate: 1500" 90 150 320 170 | Out-Null
    Add-Card $s "Ilya Smirnov`nBalance: 1000`nRate: 1200" 460 150 320 170 | Out-Null
    Add-Card $s "Maria Orlova`nBalance: 0`nRate: 1800" 830 150 320 170 | Out-Null
    Add-Card $s "Actions: lesson / skip / payment" 90 370 500 110 "E0E7FF" "4338CA" | Out-Null
    Add-Card $s "History block" 650 370 500 180 | Out-Null

    $s = $presentation.Slides.Item(8)
    Add-Label $s "Add student form" 450 20 380 28 24 $true 2 | Out-Null
    $card = $s.Shapes.AddShape($round, 290, 110, 700, 500)
    Style-Shape $card "FFFFFF" "CBD5E1"
    Add-Label $s "Name" 360 225 100 20 18 $true 1 | Out-Null
    Add-Card $s "Input field" 360 245 560 45 "F8FAFC" "CBD5E1" | Out-Null
    Add-Label $s "Contact" 360 315 100 20 18 $true 1 | Out-Null
    Add-Card $s "Input field" 360 335 560 45 "F8FAFC" "CBD5E1" | Out-Null
    Add-Label $s "Rate" 360 405 100 20 18 $true 1 | Out-Null
    Add-Card $s "1500" 360 425 260 45 "F8FAFC" "CBD5E1" | Out-Null
    Add-Label $s "Start balance" 660 405 160 20 18 $true 1 | Out-Null
    Add-Card $s "3000" 660 425 260 45 "F8FAFC" "CBD5E1" | Out-Null
    Add-Card $s "Save" 510 510 180 60 "DCFCE7" "15803D" | Out-Null

    $s = $presentation.Slides.Item(9)
    Add-Label $s "Analytics page mockup" 390 20 500 28 24 $true 2 | Out-Null
    $bg = $s.Shapes.AddShape($rect, 40, 80, 1200, 600)
    Style-Shape $bg "F8FAFC" "CBD5E1"
    Add-Card $s "High risk`nMaria Orlova" 90 150 260 140 "FECACA" "B91C1C" | Out-Null
    Add-Card $s "Medium risk`nIlya Smirnov" 390 150 260 140 "FDE68A" "B45309" | Out-Null
    Add-Card $s "Low risk`nAnna Petrova" 690 150 260 140 "BBF7D0" "15803D" | Out-Null
    Add-Card $s "AI reason: low activity, no payment for 21 days" 90 360 520 180 | Out-Null
    Add-Card $s "Advice: contact student and offer a new slot" 660 360 490 180 | Out-Null

    $s = $presentation.Slides.Item(10)
    Add-Label $s "User scenarios" 450 20 380 28 24 $true 2 | Out-Null
    Add-Card $s "1. Add student" 90 170 250 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "2. Add payment" 390 170 250 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "3. Mark lesson" 690 170 250 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "4. View analytics" 990 170 220 110 "DBEAFE" "1D4ED8" | Out-Null
    Add-Card $s "Result: data updated, metrics recalculated, risk visible in analytics" 180 390 920 140 "DCFCE7" "15803D" | Out-Null

    $s = $presentation.Slides.Item(11)
    Add-Label $s "Usage statistics" 450 20 380 28 24 $true 2 | Out-Null
    Add-Label $s "Mock data for 4 months" 460 70 360 20 18 $false 2 | Out-Null
    $months = @("Jan", "Feb", "Mar", "Apr")
    $students = @(9, 12, 15, 18)
    $income = @(38, 51, 67, 82)
    for ($i = 0; $i -lt 4; $i++) {
        Add-Label $s $months[$i] (180 + $i * 220) 590 120 24 18 $true 2 | Out-Null
        $bar1 = $s.Shapes.AddShape($rect, (200 + $i * 220), (500 - $students[$i] * 15), 55, ($students[$i] * 15))
        Style-Shape $bar1 "93C5FD" "1D4ED8"
        $bar2 = $s.Shapes.AddShape($rect, (270 + $i * 220), (530 - $income[$i] * 4), 55, ($income[$i] * 4))
        Style-Shape $bar2 "86EFAC" "15803D"
    }
    Add-Label $s "Blue - students, green - income (thousand RUB)" 320 640 640 20 16 $false 2 | Out-Null

    for ($i = 1; $i -le 11; $i++) {
        $presentation.Slides.Item($i).Export((Join-Path $assetsDir ("figure_{0:D2}.png" -f $i)), "PNG", 1280, 720)
    }
}
finally {
    $presentation.Close()
    $ppt.Quit()
}

Write-Output "CREATED_ASSETS: $assetsDir"
