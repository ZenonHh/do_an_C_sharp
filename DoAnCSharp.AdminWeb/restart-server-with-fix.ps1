#!/usr/bin/env powershell

# QUICK FIX: Restart server to apply seeding fix

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  QR SCAN FIX - QUICK START" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build
Write-Host "1️⃣  Building project..." -ForegroundColor Green
cd C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb
dotnet build 2>&1 | Select-String -Pattern "error|warning|successfully" -NotMatch | Select-String -Pattern "Build"

# Check build success
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "   ❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "2️⃣  Kill any existing server processes..." -ForegroundColor Green
Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" } | ForEach-Object {
    Write-Host "   Killing: $($_.ProcessName)"
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "3️⃣  Starting server..." -ForegroundColor Green
Write-Host "   URL: http://172.20.10.2:5000" -ForegroundColor Cyan
Write-Host "   Waiting 3 seconds for database seeding..." -ForegroundColor Yellow
Write-Host ""

# Start server in background
$serverProcess = Start-Process dotnet -ArgumentList "run" -PassThru -NoNewWindow

Start-Sleep -Seconds 3

Write-Host "4️⃣  Server started (PID: $($serverProcess.Id))" -ForegroundColor Green
Write-Host ""
Write-Host "✅ READY TO TEST!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Open http://172.20.10.2:5000 in browser" -ForegroundColor White
Write-Host "2. Check tab '🏪 Quán Ăn' to see 5 sample restaurants" -ForegroundColor White
Write-Host "3. Scan a QR code (or test with /qr/{code})" -ForegroundColor White
Write-Host "4. Should see restaurant info page (NOT error)" -ForegroundColor White
Write-Host ""
