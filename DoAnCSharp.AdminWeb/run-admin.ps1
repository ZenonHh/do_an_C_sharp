#!/usr/bin/env pwsh
# Script to run AdminWeb with proper configuration

Write-Host "🚀 Starting Vĩnh Khánh Tour - Admin Web..." -ForegroundColor Cyan
Write-Host ""

# Auto-detect project path
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir "DoAnCSharp.AdminWeb"

if (-not (Test-Path $projectPath)) {
    Write-Host "❌ ERROR: Cannot find project at: $projectPath" -ForegroundColor Red
    Write-Host "Please run this script from the DoAnCSharp.AdminWeb folder" -ForegroundColor Yellow
    Write-Host "Current location: $(Get-Location)" -ForegroundColor Cyan
    exit 1
}

Write-Host "📁 Project path: $projectPath" -ForegroundColor Cyan
Set-Location $projectPath

# Kill any existing dotnet processes
Write-Host "🛑 Stopping existing dotnet processes..." -ForegroundColor Yellow
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Delete old database to start fresh
Write-Host "🗑️  Removing old database (fake data)..." -ForegroundColor Yellow
$dbPath = Join-Path $env:APPDATA "VinhKhanhTour\VinhKhanhTour_Full.db3"
if(Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Old database deleted - starting fresh!" -ForegroundColor Green
} else {
    Write-Host "✅ No old database found - will create new empty database" -ForegroundColor Green
}

Write-Host ""
Write-Host "📦 Building project..." -ForegroundColor Yellow
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 Starting web server..." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║  🎉 Admin Dashboard URLs:                            ║" -ForegroundColor Magenta
    Write-Host "║                                                       ║" -ForegroundColor Magenta
    Write-Host "║  HTTP:  http://localhost:5000                        ║" -ForegroundColor Magenta
    Write-Host "║  HTTPS: https://localhost:5001  ← USE THIS           ║" -ForegroundColor Magenta
    Write-Host "║                                                       ║" -ForegroundColor Magenta
    Write-Host "║  ⚠️  If CSS looks broken, press Ctrl+F5 to refresh!  ║" -ForegroundColor Yellow
    Write-Host "║      (This clears browser cache)                     ║" -ForegroundColor Yellow
    Write-Host "║                                                       ║" -ForegroundColor Magenta
    Write-Host "║  Press Ctrl+C to stop the server                     ║" -ForegroundColor Magenta
    Write-Host "╚═══════════════════════════════════════════════════════╝" -ForegroundColor Magenta
    Write-Host ""
    
    # Run the app
    dotnet run --configuration Release
} else {
    Write-Host "❌ Build failed! Please fix errors and try again." -ForegroundColor Red
    exit 1
}
