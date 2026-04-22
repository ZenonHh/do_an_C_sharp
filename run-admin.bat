@echo off
REM Script chạy Web Admin nhanh
REM Usage: run-admin.bat [port] (default: 5000)

setlocal enabledelayedexpansion

set PORT=%1
if "%PORT%"=="" (
    set PORT=5000
)

echo.
echo ========================================
echo   Vĩnh Khánh Tour - Admin Web
echo ========================================
echo.
echo Starting on http://localhost:%PORT%
echo Press Ctrl+C to stop
echo.

cd DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb
dotnet run --urls "http://localhost:%PORT%"

pause
