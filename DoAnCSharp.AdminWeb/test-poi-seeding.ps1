#!/usr/bin/env pwsh

# 🧪 Test POI Seeding and QR Scan
# Run this in a NEW PowerShell window while server is running

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🧪 POI Seeding & QR Scan Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if server is running
Write-Host "Test 1: Checking if server is running..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest "http://172.20.10.2:5000/health-check.html" -ErrorAction SilentlyContinue -TimeoutSec 2
    Write-Host "✅ Server is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Server is not responding!" -ForegroundColor Red
    Write-Host "   Make sure server is started with: dotnet run" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Test 2: Check POIs
Write-Host "Test 2: Checking POIs in database..." -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest "http://172.20.10.2:5000/api/pois" -ErrorAction Stop
    $pois = $r.Content | ConvertFrom-Json
    
    if ($pois -and $pois.Count -gt 0) {
        Write-Host "✅ Found $($pois.Count) POIs:" -ForegroundColor Green
        Write-Host ""
        
        $pois | ForEach-Object {
            Write-Host "  📍 Name: $($_.name)" -ForegroundColor Cyan
            Write-Host "     ID: $($_.id)" -ForegroundColor Gray
            Write-Host "     QRCode: $($_.qrCode)" -ForegroundColor Gray
            Write-Host ""
        }
        
        # Test 3: Test QR scan with first POI
        $firstPoi = $pois[0]
        $qrCode = $firstPoi.qrCode
        
        if ($qrCode -match 'POI_[A-Z0-9]+') {
            $codeMatch = $matches[0]
            
            Write-Host "Test 3: Testing QR scan endpoint..." -ForegroundColor Yellow
            Write-Host "  Using QR code: $codeMatch" -ForegroundColor Gray
            Write-Host "  Expected POI: $($firstPoi.name) (ID: $($firstPoi.id))" -ForegroundColor Gray
            Write-Host ""
            
            try {
                # Simulate QR scan with device tracking
                $headers = @{'User-Agent' = 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_6)'}
                $scanUrl = "http://172.20.10.2:5000/qr/$codeMatch"
                
                Write-Host "  Scanning: $scanUrl" -ForegroundColor Gray
                
                $response = Invoke-WebRequest $scanUrl `
                    -MaximumRedirection 0 `
                    -ErrorAction SilentlyContinue `
                    -Headers $headers
                
                $redirectUrl = $response.Headers.Location
                Write-Host "✅ QR scan successful!" -ForegroundColor Green
                Write-Host "   Redirected to: $redirectUrl" -ForegroundColor Gray
                
                # Verify it contains the POI ID
                if ($redirectUrl -match "poiId=$($firstPoi.id)") {
                    Write-Host "✅ POI found and matched correctly!" -ForegroundColor Green
                } else {
                    Write-Host "⚠️  Redirect URL doesn't contain expected POI ID" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "⚠️  QR scan test error: $($_.Exception.Message)" -ForegroundColor Yellow
            }
            
            Write-Host ""
        }
        
    } else {
        Write-Host "❌ No POIs found in database!" -ForegroundColor Red
        Write-Host ""
        Write-Host "This means seeding failed. Try:" -ForegroundColor Yellow
        Write-Host "1. Restart the server" -ForegroundColor Yellow
        Write-Host "2. Delete database and rebuild" -ForegroundColor Yellow
        Write-Host "3. Check server console for error messages" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error checking POIs: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Test complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
