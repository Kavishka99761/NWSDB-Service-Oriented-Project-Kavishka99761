Add-Type -Path "d:\My Project\NWSDB system\src\NWSDB.Services\bin\Debug\net9.0\ClosedXML.dll"
Add-Type -Path "d:\My Project\NWSDB system\src\NWSDB.Services\bin\Debug\net9.0\DocumentFormat.OpenXml.dll"

$excelFile = "d:\My Project\NWSDB system\screenshots\08_NWSDB_Payments_ClosedXML_Export.xlsx"
$wb = New-Object ClosedXML.Excel.XLWorkbook($excelFile)
$ws = $wb.Worksheet(1)

$htmlRows = ""
for ($r = 4; $r -le $ws.LastRowUsed().RowNumber(); $r++) {
    $tag = if ($r -eq 4) { "th" } else { "td" }
    $cells = ""
    for ($c = 1; $c -le 8; $c++) {
        $val = $ws.Cell($r, $c).Value.ToString()
        $cells += "<$tag>$val</$tag>"
    }
    $htmlRows += "<tr>$cells</tr>`n"
}

$html = @"
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>NWSDB Payments ClosedXML Export</title>
<style>
body { font-family: 'Segoe UI', Arial, sans-serif; background: #0f172a; padding: 40px; margin: 0; color: #1e293b; }
.sheet { background: white; border-radius: 12px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); padding: 35px; max-width: 1080px; margin: 0 auto; }
.header-badge { display: inline-block; background: #107c41; color: white; padding: 5px 14px; border-radius: 6px; font-weight: bold; font-size: 13px; margin-bottom: 12px; letter-spacing: 0.5px; }
h2 { margin: 0 0 6px 0; color: #002244; font-size: 22px; }
.subtitle { color: #64748b; font-size: 13px; margin-bottom: 25px; }
table { width: 100%; border-collapse: collapse; font-size: 13px; }
th { background: #003366; color: white; padding: 12px 14px; text-align: left; font-weight: 600; }
td { padding: 10px 14px; border-bottom: 1px solid #e2e8f0; color: #334155; }
tr:nth-child(even) td { background: #f8fafc; }
tr:last-child td { font-weight: bold; background: #e0f2fe; color: #0369a1; border-top: 2px solid #0284c7; }
</style>
</head>
<body>
<div class="sheet">
  <div class="header-badge">&#128202; ClosedXML .XLSX Engine Output</div>
  <h2>NATIONAL WATER SUPPLY & DRAINAGE BOARD - OFFICIAL PAYMENT REGISTER</h2>
  <div class="subtitle">Real-time payment settlements generated automatically via ClosedXML service</div>
  <table>
    $htmlRows
  </table>
</div>
</body>
</html>
"@

$htmlPath = "d:\My Project\NWSDB system\screenshots\excel_preview.html"
Set-Content -Path $htmlPath -Value $html -Encoding utf8
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
& $edge --headless --screenshot="d:\My Project\NWSDB system\screenshots\08_ClosedXML_Excel_Spreadsheet_View.png" --window-size=1200,750 $htmlPath
Write-Host "ClosedXML Excel preview screenshot rendered successfully!"
