@echo off
chcp 65001 >nul
echo ========================================
echo    Thêm b?ng LichLamViec vào Database
echo ========================================
echo.

echo ?ang ch?y SQL script...
echo.

sqlcmd -S "(localdb)\MSSQLLocalDB" -d "QuanLyBaiDoXe" -U "sa" -P "1234" -i "%~dp0Add_LichLamViec_Table.sql"

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo    Hoàn t?t!
    echo ========================================
    echo.
    echo B?n có th? ch?y l?i ?ng d?ng ngay bây gi?.
) else (
    echo.
    echo ========================================
    echo    L?i!
    echo ========================================
    echo.
    echo Vui lòng ki?m tra:
    echo 1. LocalDB ?ang ch?y
    echo 2. Database QuanLyBaiDoXe t?n t?i
    echo 3. Ho?c ch?y SQL script th? công trong SSMS
)

echo.
pause
