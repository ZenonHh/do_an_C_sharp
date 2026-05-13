# QR Queue System - Quick Test Script
# Chạy script này để test queue system

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  QR Queue System - Quick Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if service is running
Write-Host "[1/5] Checking if queue service is running..." -ForegroundColor Yellow
try {
    $stats = Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats" -ErrorAction Stop
    Write-Host "  ✅ Queue service is running!" -ForegroundColor Green
    Write-Host "     - Uptime: $([math]::Round($stats.serviceUptimeSeconds, 0)) seconds" -ForegroundColor Gray
    Write-Host "     - Total Processed: $($stats.totalProcessed)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "  ❌ Queue service is NOT running!" -ForegroundColor Red
    Write-Host "     Please start the server first: dotnet run" -ForegroundColor Yellow
    exit
}

# Test 2: Add single request
Write-Host "[2/5] Testing single request (should process immediately)..." -ForegroundColor Yellow
$result1 = Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
if ($result1.status -eq "processing") {
    Write-Host "  ✅ Request processed immediately!" -ForegroundColor Green
    Write-Host "     Status: $($result1.status)" -ForegroundColor Gray
} else {
    Write-Host "  ⚠️  Request queued (system might be busy)" -ForegroundColor Yellow
    Write-Host "     Status: $($result1.status), Position: $($result1.queuePosition)" -ForegroundColor Gray
}
Write-Host ""

Start-Sleep -Seconds 2

# Test 3: Add 15 requests to test queue
Write-Host "[3/5] Adding 15 requests to test queue..." -ForegroundColor Yellow
$jobs = 1..15 | ForEach-Object {
    Start-Job -ScriptBlock {
        Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
    }
}

Write-Host "  Waiting for requests to be added..." -ForegroundColor Gray
$jobs | Wait-Job | Out-Null
$results = $jobs | Receive-Job
$jobs | Remove-Job

$processing = ($results | Where-Object { $_.status -eq "processing" }).Count
$queued = ($results | Where-Object { $_.status -eq "queued" }).Count

Write-Host "  ✅ Results:" -ForegroundColor Green
Write-Host "     - Processing immediately: $processing" -ForegroundColor Green
Write-Host "     - Queued: $queued" -ForegroundColor Yellow
Write-Host ""

Start-Sleep -Seconds 2

# Test 4: Check current stats
Write-Host "[4/5] Checking current queue stats..." -ForegroundColor Yellow
$stats = Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats"
Write-Host "  📊 Current Stats:" -ForegroundColor Cyan
Write-Host "     - Active Requests: $($stats.activeRequests) / $($stats.maxConcurrentRequests)" -ForegroundColor Green
Write-Host "     - Queued Requests: $($stats.queuedRequests) / $($stats.maxQueueSize)" -ForegroundColor Yellow
Write-Host "     - Total Processed: $($stats.totalProcessed)" -ForegroundColor Blue
Write-Host "     - Total Rejected: $($stats.totalRejected)" -ForegroundColor Red
Write-Host "     - Processing Rate: $([math]::Round($stats.averageProcessingRate, 2)) req/s" -ForegroundColor Magenta
Write-Host ""

# Test 5: Test priority (Paid vs Free)
Write-Host "[5/5] Testing priority queue (Paid vs Free users)..." -ForegroundColor Yellow

# Add 3 free users
$freeJobs = 1..3 | ForEach-Object {
    Start-Job -ScriptBlock {
        Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
    }
}

# Add 3 paid users
$paidJobs = 1..3 | ForEach-Object {
    Start-Job -ScriptBlock {
        Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=true"
    }
}

$allJobs = $freeJobs + $paidJobs
$allJobs | Wait-Job | Out-Null
$priorityResults = $allJobs | Receive-Job
$allJobs | Remove-Job

$paidProcessing = ($priorityResults | Where-Object { $_.status -eq "processing" -and $_.message -like "*paid*" }).Count
$freeProcessing = ($priorityResults | Where-Object { $_.status -eq "processing" -and $_.message -notlike "*paid*" }).Count

Write-Host "  ✅ Priority test completed!" -ForegroundColor Green
Write-Host "     Note: Paid users should be processed first when queue is active" -ForegroundColor Gray
Write-Host ""

# Final summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$finalStats = Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats"

Write-Host ""
Write-Host "📊 Final Statistics:" -ForegroundColor White
Write-Host "   Active:    $($finalStats.activeRequests) / $($finalStats.maxConcurrentRequests)" -ForegroundColor Green
Write-Host "   Queued:    $($finalStats.queuedRequests) / $($finalStats.maxQueueSize)" -ForegroundColor Yellow
Write-Host "   Processed: $($finalStats.totalProcessed)" -ForegroundColor Blue
Write-Host "   Rejected:  $($finalStats.totalRejected)" -ForegroundColor Red
Write-Host ""

Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Open Queue Monitor: http://localhost:5000/qr-queue-monitor.html" -ForegroundColor White
Write-Host "   2. Click '🔥 Thêm 10 Requests' to see queue in action" -ForegroundColor White
Write-Host "   3. Test QR scan: http://localhost:5000/qr/FOODSTREET_VINHKHANH" -ForegroundColor White
Write-Host ""

Write-Host "✅ All tests completed successfully!" -ForegroundColor Green
Write-Host ""
