# 🧹 Cleanup Database and Restart Server

Write-Host "=" * 60
Write-Host "🧹 QR Code Fix - Database Cleanup & Restart" -ForegroundColor Cyan
Write-Host "=" * 60

# Step 1: Kill existing server process
Write-Host "`n1️⃣  Stopping existing server process..."
$proc = Get-Process dotnet -ErrorAction SilentlyContinue
if ($proc) {
    Stop-Process -Force -InputObject $proc
    Write-Host "✅ Stopped dotnet process"
    Start-Sleep -Seconds 2
} else {
    Write-Host "ℹ️  No running dotnet process found"
}

# Step 2: Backup old database
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Write-Host "`n2️⃣  Backing up old database..."
    Rename-Item $dbPath "$dbPath.bak" -Force
    Write-Host "✅ Database backed up to: $dbPath.bak"
} else {
    Write-Host "`n2️⃣  Database file not found (will be created fresh)"
}

# Step 3: Start server
Write-Host "`n3️⃣  Starting fresh server..."
Write-Host "📂 Working directory: $(Get-Location)"

cd "DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
Write-Host "🚀 Running: dotnet run"
Write-Host "=" * 60
Write-Host "Server will start in a moment..."
Write-Host "Once running, go to: http://192.168.0.125:5000"
Write-Host "=" * 60

dotnet run
