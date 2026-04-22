#!/usr/bin/env pwsh
# Universal Admin Web Launcher - Run from anywhere

Write-Host "🔍 Finding Admin Web project..." -ForegroundColor Cyan

# Try to find workspace root
$currentDir = Get-Location
$workspaceRoot = $null

# Method 1: Check if we're in workspace
if (Test-Path "DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb") {
    $workspaceRoot = $currentDir
} 
# Method 2: Check parent directories
elseif (Test-Path "..\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb") {
    $workspaceRoot = (Get-Item "..").FullName
}
# Method 3: Look for .git folder (workspace root marker)
else {
    $searchDir = $currentDir
    for ($i = 0; $i -lt 5; $i++) {
        if (Test-Path (Join-Path $searchDir ".git")) {
            if (Test-Path (Join-Path $searchDir "DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb")) {
                $workspaceRoot = $searchDir
                break
            }
        }
        $searchDir = Split-Path $searchDir -Parent
        if (-not $searchDir) { break }
    }
}

# If still not found, use hardcoded path
if (-not $workspaceRoot) {
    $workspaceRoot = "C:\Users\LENOVO\source\repos\do_an_C_sharp"
}

$projectPath = Join-Path $workspaceRoot "DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"

if (-not (Test-Path $projectPath)) {
    Write-Host ""
    Write-Host "❌ ERROR: Cannot find Admin Web project!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Searched in:" -ForegroundColor Yellow
    Write-Host "  $projectPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Current directory:" -ForegroundColor Yellow
    Write-Host "  $currentDir" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "SOLUTION:" -ForegroundColor Green
    Write-Host "  cd C:\Users\LENOVO\source\repos\do_an_C_sharp"
    Write-Host "  .\DoAnCSharp.AdminWeb\run-admin.ps1"
    Write-Host ""
    exit 1
}

Write-Host "✅ Found project at: $projectPath" -ForegroundColor Green
Write-Host ""

Set-Location $projectPath

# Kill any existing dotnet processes
Write-Host "🛑 Stopping existing dotnet processes..." -ForegroundColor Yellow
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Delete old database
Write-Host "🗑️  Removing old database..." -ForegroundColor Yellow
$dbPath = Join-Path $env:APPDATA "VinhKhanhTour\VinhKhanhTour_Full.db3"
if(Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Database deleted" -ForegroundColor Green
} else {
    Write-Host "✅ No database to delete" -ForegroundColor Green
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
    Write-Host "║  HTTPS: https://localhost:5001                       ║" -ForegroundColor Magenta
    Write-Host "║                                                       ║" -ForegroundColor Magenta
    Write-Host "║  ⚠️  If CSS broken, press Ctrl+F5 to refresh!        ║" -ForegroundColor Yellow
    Write-Host "║                                                       ║" -ForegroundColor Magenta
    Write-Host "║  Press Ctrl+C to stop the server                     ║" -ForegroundColor Magenta
    Write-Host "╚═══════════════════════════════════════════════════════╝" -ForegroundColor Magenta
    Write-Host ""
    
    dotnet run --configuration Release
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
