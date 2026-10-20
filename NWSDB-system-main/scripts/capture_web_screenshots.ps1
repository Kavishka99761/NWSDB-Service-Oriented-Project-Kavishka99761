$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$outDir = "d:\My Project\NWSDB system\screenshots"
if (!(Test-Path $outDir)) {
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
}

$pages = @(
    @{ Name = "09_WebApp_Home.png"; Url = "http://localhost:5100/" },
    @{ Name = "10_WebApp_Features.png"; Url = "http://localhost:5100/Features" },
    @{ Name = "11_WebApp_Contact.png"; Url = "http://localhost:5100/Contact" },
    @{ Name = "12_WebApp_Online_Payment_Lookup.png"; Url = "http://localhost:5100/Payment?accountNumber=ACC10001" },
    @{ Name = "13_WebApp_Customer_Login.png"; Url = "http://localhost:5100/Account/Login?demoUser=customer" },
    @{ Name = "14_WebApp_Customer_Register.png"; Url = "http://localhost:5100/Account/Register" },
    @{ Name = "15_WebApp_Portal_Dashboard.png"; Url = "http://localhost:5100/Portal/Dashboard" },
    @{ Name = "16_WebApp_Portal_Usage.png"; Url = "http://localhost:5100/Portal/Usage" },
    @{ Name = "17_WebApp_Portal_Bills.png"; Url = "http://localhost:5100/Portal/Bills" },
    @{ Name = "18_WebApp_Portal_Payments.png"; Url = "http://localhost:5100/Portal/Payments" },
    @{ Name = "19_WebApi_Swagger_UI.png"; Url = "http://localhost:5000/swagger/index.html" }
)

foreach ($page in $pages) {
    $target = Join-Path $outDir $page.Name
    Write-Host "Capturing $($page.Name)..."
    & $edge --headless --screenshot="$target" --window-size=1280,950 --hide-scrollbars "$($page.Url)"
    Start-Sleep -Milliseconds 800
}

Write-Host "All WebApp & Swagger screenshots completed!"
