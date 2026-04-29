#!/usr/bin/env pwsh
# Script to clean old database and rebuild the application

Write-Host "🧹 Cleaning Database for Language Switching Fix" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Define possible database paths
$dbPaths = @(
    "C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\VinhKhanhTour_Full.db3",
    "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb\bin\Debug\net8.0\data\VinhKhanhTour_Full.db3",
    "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb\data\VinhKhanhTour_Full.db3"
)

$dataFolder = "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb\bin\Debug\net8.0\data"

# Remove database files
$removed = $false
foreach ($dbPath in $dbPaths) {
    if (Test-Path $dbPath) {
        Write-Host "🗑️  Removing database: $dbPath" -ForegroundColor Yellow
        Remove-Item $dbPath -Force -ErrorAction SilentlyContinue
        if ($?) { 
            Write-Host "✅ Successfully removed: $dbPath" -ForegroundColor Green
            $removed = $true
        } else {
            Write-Host "⚠️  Could not remove: $dbPath" -ForegroundColor Red
        }
    }
}

# Remove data folder if empty
if (Test-Path $dataFolder) {
    try {
        Get-ChildItem -Path $dataFolder -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
        Remove-Item $dataFolder -Force -ErrorAction SilentlyContinue
        Write-Host "✅ Cleaned up data folder: $dataFolder" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  Could not clean data folder" -ForegroundColor Red
    }
}

if ($removed) {
    Write-Host "`n✅ Database cleanup complete!" -ForegroundColor Green
    Write-Host "📌 Next steps:" -ForegroundColor Cyan
    Write-Host "   1. Close Visual Studio or stop the app"
    Write-Host "   2. Do a 'Clean Solution' in Visual Studio"
    Write-Host "   3. Do a 'Build Solution' in Visual Studio"
    Write-Host "   4. Run the app - it will create new database with multi-language data"
    Write-Host "   5. Test language switching in browser"
} else {
    Write-Host "`n⚠️  No database files found to clean" -ForegroundColor Yellow
    Write-Host "   You may need to manually locate and delete:" -ForegroundColor Yellow
    Write-Host "   - VinhKhanhTour_Full.db3" -ForegroundColor Yellow
}

Write-Host "`n================================================" -ForegroundColor Cyan
