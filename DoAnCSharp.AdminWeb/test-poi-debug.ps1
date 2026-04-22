#!/usr/bin/env pwsh

# Test endpoints để debug vấn đề POI not found

$SERVER = "http://172.20.10.2:5000"

Write-Host "========== POI DEBUG TEST ==========" -ForegroundColor Cyan
Write-Host "Server: $SERVER" -ForegroundColor Yellow
Write-Host ""

# 1. Check all POIs
Write-Host "1️⃣  Checking all POIs in database..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "$SERVER/api/pois/debug/all" -Method GET -ErrorAction Stop
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   Total POIs: $($data.totalCount)" -ForegroundColor Yellow
    if ($data.pois) {
        $data.pois | ForEach-Object {
            Write-Host "   - ID: $($_.id), Name: $($_.name), QRCode: $($_.qrCode)" -ForegroundColor Cyan
        }
    }
} catch {
    Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 2. Create a test POI
Write-Host "2️⃣  Creating a test POI..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "$SERVER/api/pois/debug/create-test" -Method POST -ErrorAction Stop
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   ✅ Test POI created!" -ForegroundColor Green
    Write-Host "   - POI ID: $($data.poiId)" -ForegroundColor Cyan
    Write-Host "   - QR Code: $($data.qrCode)" -ForegroundColor Cyan
    Write-Host "   - Scan URL: $($data.scanUrl)" -ForegroundColor Cyan
    
    # Store the QR code for next step
    $testQRCode = $data.qrCode
} catch {
    Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 3. Check all POIs again
Write-Host "3️⃣  Checking all POIs again..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "$SERVER/api/pois/debug/all" -Method GET -ErrorAction Stop
    $data = $response.Content | ConvertFrom-Json
    Write-Host "   Total POIs: $($data.totalCount)" -ForegroundColor Yellow
    if ($data.pois) {
        $data.pois | ForEach-Object {
            Write-Host "   - ID: $($_.id), Name: $($_.name), QRCode: $($_.qrCode)" -ForegroundColor Cyan
        }
    }
} catch {
    Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 4. Test QR code lookup by code
if ($testQRCode) {
    Write-Host "4️⃣  Testing QR code lookup..." -ForegroundColor Green
    try {
        $response = Invoke-WebRequest -Uri "$SERVER/api/pois/qr/$testQRCode" -Method GET -ErrorAction Stop
        $poi = $response.Content | ConvertFrom-Json
        Write-Host "   ✅ POI found!" -ForegroundColor Green
        Write-Host "   - Name: $($poi.name)" -ForegroundColor Cyan
        Write-Host "   - Address: $($poi.address)" -ForegroundColor Cyan
    } catch {
        Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
    
    # 5. Test the QR scan endpoint
    Write-Host "5️⃣  Testing QR scan redirect..." -ForegroundColor Green
    $qrUrl = "$SERVER/qr/$testQRCode"
    Write-Host "   URL: $qrUrl" -ForegroundColor Cyan
    Write-Host "   (Open this in browser or scan the QR code)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========== TEST COMPLETE ==========" -ForegroundColor Cyan
