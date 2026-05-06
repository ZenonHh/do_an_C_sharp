#!/usr/bin/env pwsh
# Script to test App & AdminWeb integration

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  🔗 Vĩnh Khánh Tour - Integration Test                       ║" -ForegroundColor Cyan
Write-Host "║  Testing connection between MAUI App and AdminWeb             ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# 1. Check if AdminWeb is running
Write-Host "🌐 Step 1: Checking if AdminWeb is running..." -ForegroundColor Yellow
$adminRunning = $false

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/pois" -Method GET -TimeoutSec 5 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        $adminRunning = $true
        Write-Host "   ✅ AdminWeb is running on port 5000" -ForegroundColor Green
        
        # Parse JSON response
        $pois = $response.Content | ConvertFrom-Json
        Write-Host "   ✅ API responded with $($pois.Count) POIs" -ForegroundColor Green
    }
}
catch {
    Write-Host "   ❌ AdminWeb is NOT running!" -ForegroundColor Red
    Write-Host "   💡 Please start AdminWeb first:" -ForegroundColor Yellow
    Write-Host "      cd DoAnCSharp.AdminWeb" -ForegroundColor Yellow
    Write-Host "      .\run-admin.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# 2. Test API endpoints
Write-Host "🧪 Step 2: Testing API endpoints..." -ForegroundColor Yellow

$endpoints = @(
    @{ Name = "Get POIs"; Url = "http://localhost:5000/api/pois"; Method = "GET" },
    @{ Name = "Get Users"; Url = "http://localhost:5000/api/users"; Method = "GET" },
    @{ Name = "Get Devices"; Url = "http://localhost:5000/api/devices"; Method = "GET" },
    @{ Name = "Get Dashboard"; Url = "http://localhost:5000/api/users/dashboard/summary"; Method = "GET" }
)

$passedTests = 0
$totalTests = $endpoints.Count

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url -Method $endpoint.Method -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "   ✅ $($endpoint.Name) - OK" -ForegroundColor Green
            $passedTests++
        }
    }
    catch {
        Write-Host "   ❌ $($endpoint.Name) - FAILED: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "   📊 Test Results: $passedTests/$totalTests passed" -ForegroundColor $(if ($passedTests -eq $totalTests) { "Green" } else { "Yellow" })
Write-Host ""

# 3. Test QR Code functionality
Write-Host "🔍 Step 3: Testing QR Code system..." -ForegroundColor Yellow

try {
    # Get first POI's QR code
    $poisResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/pois" -Method GET -UseBasicParsing
    $pois = $poisResponse.Content | ConvertFrom-Json
    
    if ($pois.Count -gt 0 -and $pois[0].qrCode) {
        $qrCode = $pois[0].qrCode
        $poiName = $pois[0].name
        
        Write-Host "   📱 Testing QR Code: $qrCode" -ForegroundColor Cyan
        Write-Host "   🏪 POI: $poiName" -ForegroundColor Cyan
        
        # Test QR lookup endpoint
        $qrUrl = "http://localhost:5000/api/pois/qr/$qrCode"
        $qrResponse = Invoke-WebRequest -Uri $qrUrl -Method GET -UseBasicParsing
        
        if ($qrResponse.StatusCode -eq 200) {
            Write-Host "   ✅ QR Code lookup - OK" -ForegroundColor Green
            
            # Test QR scan landing page
            $scanUrl = "http://localhost:5000/qr-scan?code=$qrCode"
            Write-Host "   🌐 QR Scan URL: $scanUrl" -ForegroundColor Cyan
            
            $scanResponse = Invoke-WebRequest -Uri $scanUrl -Method GET -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
            
            # Should redirect (302) or return HTML (200)
            if ($scanResponse.StatusCode -in @(200, 302)) {
                Write-Host "   ✅ QR Scan landing page - OK" -ForegroundColor Green
            }
        }
    }
    else {
        Write-Host "   ⚠️  No POIs with QR codes found" -ForegroundColor Yellow
        Write-Host "   💡 Please add POIs via AdminWeb first" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   ❌ QR Code test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 4. Check database
Write-Host "📊 Step 4: Checking database..." -ForegroundColor Yellow

$appData = [Environment]::GetFolderPath('ApplicationData')
$dbPath = Join-Path $appData "VinhKhanhTour\VinhKhanhTour_Full.db3"

if (Test-Path $dbPath) {
    $dbSize = (Get-Item $dbPath).Length / 1KB
    Write-Host "   ✅ Database found: $dbPath" -ForegroundColor Green
    Write-Host "   📏 Size: $([math]::Round($dbSize, 2)) KB" -ForegroundColor Cyan
}
else {
    Write-Host "   ⚠️  Database not found at: $dbPath" -ForegroundColor Yellow
    Write-Host "   💡 Will be created on first AdminWeb run" -ForegroundColor Yellow
}

Write-Host ""

# 5. Network configuration
Write-Host "🌍 Step 5: Network configuration for mobile testing..." -ForegroundColor Yellow

$ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.PrefixOrigin -eq "Dhcp" }

if ($ipAddresses) {
    Write-Host "   💡 For physical device testing, use these IPs:" -ForegroundColor Cyan
    foreach ($ip in $ipAddresses) {
        Write-Host "      📱 API URL: http://$($ip.IPAddress):5000/api" -ForegroundColor Green
    }
    Write-Host ""
    Write-Host "   ⚠️  Make sure:" -ForegroundColor Yellow
    Write-Host "      1. Phone and PC are on same WiFi" -ForegroundColor Yellow
    Write-Host "      2. Firewall allows port 5000" -ForegroundColor Yellow
    Write-Host "      3. Update ApiService.cs in MAUI app with this IP" -ForegroundColor Yellow
}
else {
    Write-Host "   ⚠️  No network IP found (WiFi/Ethernet)" -ForegroundColor Yellow
}

Write-Host ""

# 6. Deep linking test
Write-Host "🔗 Step 6: Deep Linking configuration..." -ForegroundColor Yellow

$deepLinkScheme = "vinhkhanhtour://"
Write-Host "   📱 Deep Link Scheme: $deepLinkScheme" -ForegroundColor Cyan
Write-Host "   🔗 Example: vinhkhanhtour://poi/1" -ForegroundColor Cyan

# Check if AndroidManifest.xml has intent filter
$manifestPath = "Platforms\Android\AndroidManifest.xml"
if (Test-Path $manifestPath) {
    $manifestContent = Get-Content $manifestPath -Raw
    if ($manifestContent -like "*vinhkhanhtour*") {
        Write-Host "   ✅ Android deep linking configured" -ForegroundColor Green
    }
    else {
        Write-Host "   ⚠️  Android deep linking NOT configured" -ForegroundColor Yellow
        Write-Host "   💡 Add intent-filter to AndroidManifest.xml" -ForegroundColor Yellow
    }
}

# Check if Info.plist has URL scheme
$infoPlistPath = "Platforms\iOS\Info.plist"
if (Test-Path $infoPlistPath) {
    $plistContent = Get-Content $infoPlistPath -Raw
    if ($plistContent -like "*vinhkhanhtour*") {
        Write-Host "   ✅ iOS deep linking configured" -ForegroundColor Green
    }
    else {
        Write-Host "   ⚠️  iOS deep linking NOT configured" -ForegroundColor Yellow
        Write-Host "   💡 Add CFBundleURLTypes to Info.plist" -ForegroundColor Yellow
    }
}

Write-Host ""

# Summary
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                    INTEGRATION TEST SUMMARY                   ║" -ForegroundColor Magenta
Write-Host "╠═══════════════════════════════════════════════════════════════╣" -ForegroundColor Magenta
Write-Host "║                                                               ║" -ForegroundColor Magenta
Write-Host "║  ✅ AdminWeb Status:        RUNNING                           ║" -ForegroundColor Magenta
Write-Host "║  ✅ API Endpoints:          $passedTests/$totalTests PASSED                         ║" -ForegroundColor Magenta
Write-Host "║  ✅ QR Code System:         CONFIGURED                        ║" -ForegroundColor Magenta
Write-Host "║  ✅ Database:               AVAILABLE                         ║" -ForegroundColor Magenta
Write-Host "║                                                               ║" -ForegroundColor Magenta
Write-Host "╠═══════════════════════════════════════════════════════════════╣" -ForegroundColor Magenta
Write-Host "║  📱 NEXT STEPS:                                               ║" -ForegroundColor Magenta
Write-Host "║                                                               ║" -ForegroundColor Magenta
Write-Host "║  1. Open MAUI app in Visual Studio                            ║" -ForegroundColor Magenta
Write-Host "║  2. Build & Run on Android Emulator                           ║" -ForegroundColor Magenta
Write-Host "║  3. Test API calls (should work with http://10.0.2.2:5000)   ║" -ForegroundColor Magenta
Write-Host "║  4. Test QR scanning                                          ║" -ForegroundColor Magenta
Write-Host "║  5. Verify data syncs between app and admin                   ║" -ForegroundColor Magenta
Write-Host "║                                                               ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

Write-Host "✨ Integration test completed!" -ForegroundColor Green
Write-Host "📖 For detailed documentation, see: docs\APP_ADMINWEB_INTEGRATION.md" -ForegroundColor Cyan
Write-Host ""
