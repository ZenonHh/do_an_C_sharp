# PowerShell Script to clean up and prepare Android build

Write-Host ""
Write-Host "========================================"
Write-Host "   ANDROID BUILD PREPARATION SCRIPT" -ForegroundColor Cyan
Write-Host "========================================"
Write-Host ""

# Check if in correct directory
if (-not (Test-Path "DoAnCSharp.csproj")) {
    Write-Host "ERROR: DoAnCSharp.csproj not found!" -ForegroundColor Red
    Write-Host "Run this script from: C:\Users\LENOVO\source\repos\do_an_C_sharp\"
    exit 1
}

# Step 1: Clean
Write-Host "[1/5] Cleaning previous build..." -ForegroundColor Yellow
dotnet clean DoAnCSharp.csproj -f net8.0-android -q
Write-Host "✓ Clean completed" -ForegroundColor Green

# Step 2: Restore
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore DoAnCSharp.csproj -q
Write-Host "✓ Restore completed" -ForegroundColor Green

# Step 3: Build
Write-Host "[3/5] Building project..." -ForegroundColor Yellow
$buildResult = & dotnet build DoAnCSharp.csproj -f net8.0-android -q 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green

# Step 4: Uninstall old APK
Write-Host "[4/5] Attempting to uninstall old APK..." -ForegroundColor Yellow
$adbPath = Get-Command adb -ErrorAction SilentlyContinue
if ($adbPath) {
    adb uninstall com.companyname.doancsharp_clean 2>$null | Out-Null
    Write-Host "✓ Old APK uninstalled (or wasn't installed)" -ForegroundColor Green
} else {
    Write-Host "⚠ ADB not found - please manually uninstall from device" -ForegroundColor Yellow
    Write-Host "  Settings > Apps > VinhKhanhFoodTour > Uninstall"
}

# Step 5: Check device
Write-Host "[5/5] Checking for connected devices..." -ForegroundColor Yellow
$adbPath = Get-Command adb -ErrorAction SilentlyContinue
if ($adbPath) {
    $devices = adb devices -l 2>$null | Select-String "device" | Where-Object { $_ -notmatch "^List of attached" }
    if ($devices) {
        Write-Host "✓ Device(s) found:" -ForegroundColor Green
        $devices | ForEach-Object { Write-Host "  $_" }
    } else {
        Write-Host "⚠ No devices found" -ForegroundColor Yellow
        Write-Host "  Start Android Emulator or connect USB device"
    }
} else {
    Write-Host "⚠ ADB not found - can't check devices" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================"
Write-Host "   ✓ PREPARATION COMPLETE" -ForegroundColor Green
Write-Host "========================================"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Make sure Android Emulator is running or Device is connected"
Write-Host "2. Deploy using one of these methods:"
Write-Host "   a) Visual Studio: F5 or Build > Deploy Solution"
Write-Host "   b) PowerShell: dotnet build DoAnCSharp.csproj -f net8.0-android -t Install"
Write-Host ""
Write-Host "For help: See FIX_ALL_ERRORS.md in project root"
Write-Host ""

Read-Host "Press Enter to exit"
