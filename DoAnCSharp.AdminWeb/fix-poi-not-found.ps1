#!/usr/bin/env pwsh

# 🔧 Complete Fix Script for "Quán Không Tìm Thấy" Error
# Deletes old database, rebuilds, and tests POI seeding

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔧 POI Not Found - Complete Fix" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill processes
Write-Host "Step 1: Killing old processes..." -ForegroundColor Yellow
Get-Process "DoAnCSharp.AdminWeb" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "✅ Done" -ForegroundColor Green
Write-Host ""

# Step 2: Delete old database
Write-Host "Step 2: Deleting old database..." -ForegroundColor Yellow
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Deleted: $dbPath" -ForegroundColor Green
} else {
    Write-Host "ℹ️  No old database found" -ForegroundColor Cyan
}
Write-Host ""

# Step 3: Clean bin/obj
Write-Host "Step 3: Cleaning build folders..." -ForegroundColor Yellow
$projPath = "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
if (Test-Path "$projPath\bin") {
    Remove-Item "$projPath\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Cleaned bin folder" -ForegroundColor Green
}
if (Test-Path "$projPath\obj") {
    Remove-Item "$projPath\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Cleaned obj folder" -ForegroundColor Green
}
Write-Host ""

# Step 4: Clean build
Write-Host "Step 4: Running dotnet clean..." -ForegroundColor Yellow
cd $projPath
dotnet clean | Out-Null
Write-Host "✅ Clean complete" -ForegroundColor Green
Write-Host ""

# Step 5: Build
Write-Host "Step 5: Building project..." -ForegroundColor Yellow
$buildOutput = dotnet build 2>&1
if ($buildOutput -match "Build succeeded") {
    Write-Host "✅ Build succeeded" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    Write-Host $buildOutput
    exit 1
}
Write-Host ""

# Step 6: Start server
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ Build successful! Starting server..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "The server will start in 3 seconds..." -ForegroundColor Cyan
Write-Host "📌 It will initialize database and seed POIs" -ForegroundColor Cyan
Write-Host ""
Start-Sleep -Seconds 3

# Start server and show output
dotnet run
