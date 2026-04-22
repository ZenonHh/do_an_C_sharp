#!/usr/bin/env pwsh
# Run Web Admin - PowerShell Version

param(
    [int]$Port = 5000,
    [switch]$Release = $false
)

$config = if ($Release) { "Release" } else { "Debug" }

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗"
Write-Host "║  Vĩnh Khánh Tour - Admin Web Dashboard             ║"
Write-Host "║  Running on http://localhost:$Port"
Write-Host "║  Press Ctrl+C to stop"
Write-Host "╚════════════════════════════════════════════════════╝"
Write-Host ""

$projectPath = Join-Path $PSScriptRoot "DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"

Push-Location $projectPath

Write-Host "🚀 Starting Web Admin..." -ForegroundColor Green
Write-Host ""

dotnet run --configuration $config --urls "http://localhost:$Port"

Pop-Location
