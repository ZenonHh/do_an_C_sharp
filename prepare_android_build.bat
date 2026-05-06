@echo off
REM Script to clean up and prepare Android build

echo.
echo ========================================
echo   ANDROID BUILD PREPARATION SCRIPT
echo ========================================
echo.

REM Check if we're in correct directory
if not exist "DoAnCSharp.csproj" (
    echo ERROR: DoAnCSharp.csproj not found in current directory
    echo Please run this script from project root: C:\Users\LENOVO\source\repos\do_an_C_sharp\
    exit /b 1
)

echo [1/4] Cleaning previous build...
dotnet clean DoAnCSharp.csproj -f net8.0-android -q
echo ✓ Clean completed

echo.
echo [2/4] Restoring NuGet packages...
dotnet restore DoAnCSharp.csproj -q
echo ✓ Restore completed

echo.
echo [3/4] Building project...
dotnet build DoAnCSharp.csproj -f net8.0-android -q
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed
    exit /b 1
)
echo ✓ Build successful

echo.
echo [4/4] Attempting to uninstall old APK...
where adb >nul 2>&1
if %ERRORLEVEL% equ 0 (
    adb uninstall com.companyname.doancsharp_clean
    echo ✓ Old APK uninstalled (or didn't exist)
) else (
    echo ⚠ ADB not found - please manually uninstall from device/emulator
    echo   Settings ^> Apps ^> VinhKhanhFoodTour ^> Uninstall
)

echo.
echo ========================================
echo   ✓ PREPARATION COMPLETE
echo ========================================
echo.
echo Next steps:
echo 1. Make sure Android Emulator is running or Device is connected
echo 2. Run from Visual Studio: F5 or Build ^> Deploy Solution
echo    OR from PowerShell: dotnet build DoAnCSharp.csproj -f net8.0-android -t Install
echo.
pause
