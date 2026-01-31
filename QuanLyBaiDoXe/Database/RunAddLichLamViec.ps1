# PowerShell Script ?? ch?y SQL Script thêm b?ng LichLamViec
# Ch?y script này t? th? m?c g?c c?a project

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Thêm b?ng LichLamViec vào Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ??c connection string t? appsettings.json
$appsettingsPath = ".\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    Write-Host "? ?ã ??c connection string t? appsettings.json" -ForegroundColor Green
} else {
    Write-Host "? Không tìm th?y file appsettings.json" -ForegroundColor Red
    exit 1
}

# Path to SQL script
$sqlScriptPath = ".\Database\Add_LichLamViec_Table.sql"
if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "? Không tìm th?y file SQL script: $sqlScriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "? ?ã tìm th?y SQL script" -ForegroundColor Green
Write-Host ""

# Ch?y SQL script
try {
    Write-Host "?ang ch?y SQL script..." -ForegroundColor Yellow
    
    # S? d?ng sqlcmd ?? ch?y script
    $result = sqlcmd -S "(localdb)\MSSQLLocalDB" -d "QuanLyBaiDoXe" -U "sa" -P "1234" -i $sqlScriptPath
    
    Write-Host $result
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "   Hoàn t?t!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "B?n có th? ch?y l?i ?ng d?ng ngay bây gi?." -ForegroundColor Cyan
    
} catch {
    Write-Host "? L?i khi ch?y SQL script:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Vui lòng ch?y SQL script th? công trong SQL Server Management Studio" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
