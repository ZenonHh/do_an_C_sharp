# QR Queue System - Demo & Test Script

## ✅ Xác Nhận: Queue Service Đang Hoạt Động

API Response từ `/api/qrqueue/stats`:
```json
{
  "activeRequests": 0,
  "queuedRequests": 0,
  "maxConcurrentRequests": 10,
  "maxQueueSize": 100,
  "totalProcessed": 1,
  "totalQueued": 0,
  "totalRejected": 0,
  "averageProcessingRate": 0.0037,
  "serviceUptimeSeconds": 266.48
}
```

✅ Service đã chạy được 266 giây (4 phút 26 giây)
✅ Đã xử lý 1 request thành công

## 🎯 Demo Scenarios

### Scenario 1: Test Single Request (Xử lý ngay lập tức)

```bash
# Thêm 1 request
curl -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"

# Kết quả mong đợi:
{
  "success": true,
  "status": "processing",
  "message": "Đang xử lý request của bạn...",
  "queuePosition": 0
}
```

**Giải thích:** Vì có < 10 active requests, request được xử lý ngay lập tức.

---

### Scenario 2: Test Queue (Thêm 15 requests cùng lúc)

```bash
# Thêm 15 requests liên tục
for i in {1..15}; do
  curl -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false" &
done
wait

# Kiểm tra stats
curl http://localhost:5000/api/qrqueue/stats
```

**Kết quả mong đợi:**
- 10 requests đầu → `status: "processing"` (đang xử lý)
- 5 requests còn lại → `status: "queued"` (trong hàng đợi)

---

### Scenario 3: Test Priority (Paid vs Free Users)

```bash
# Thêm 5 free users
for i in {1..5}; do
  curl -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false" &
done

# Thêm 5 paid users
for i in {1..5}; do
  curl -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=true" &
done

wait

# Xem queue
curl http://localhost:5000/api/qrqueue/queued
```

**Kết quả mong đợi:**
- Paid users (⭐) xuất hiện ở đầu queue
- Free users (🆓) xuất hiện ở cuối queue

---

### Scenario 4: Test Queue Full (Thêm > 100 requests)

```bash
# Thêm 110 requests
for i in {1..110}; do
  curl -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false" &
done
wait

# Kiểm tra rejected count
curl http://localhost:5000/api/qrqueue/stats | grep totalRejected
```

**Kết quả mong đợi:**
- 10 requests → Processing
- 100 requests → Queued
- 10 requests → Rejected (queue full)

---

### Scenario 5: Test Real QR Scan Flow

```bash
# Simulate user quét QR code
curl "http://localhost:5000/qr/POI_TEST123?deviceId=test_device_001"

# Kết quả:
# - Nếu < 10 active → Redirect ngay đến POI page
# - Nếu ≥ 10 active → Hiển thị trang chờ với countdown
```

---

## 🖥️ PowerShell Test Scripts

### Test 1: Thêm 20 Requests và Monitor

```powershell
# Thêm 20 requests
1..20 | ForEach-Object {
    Start-Job -ScriptBlock {
        Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
    }
}

# Đợi tất cả jobs hoàn thành
Get-Job | Wait-Job

# Xem kết quả
Get-Job | Receive-Job

# Cleanup
Get-Job | Remove-Job

# Kiểm tra stats
Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats"
```

### Test 2: Monitor Real-time

```powershell
# Monitor queue mỗi 2 giây
while ($true) {
    Clear-Host
    Write-Host "=== QR Queue Monitor ===" -ForegroundColor Cyan
    Write-Host "Time: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
    
    $stats = Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats"
    
    Write-Host "Active Requests: $($stats.activeRequests) / $($stats.maxConcurrentRequests)" -ForegroundColor Green
    Write-Host "Queued Requests: $($stats.queuedRequests) / $($stats.maxQueueSize)" -ForegroundColor Yellow
    Write-Host "Total Processed: $($stats.totalProcessed)" -ForegroundColor Blue
    Write-Host "Total Rejected:  $($stats.totalRejected)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Processing Rate: $([math]::Round($stats.averageProcessingRate, 2)) req/s" -ForegroundColor Magenta
    Write-Host "Uptime: $([math]::Round($stats.serviceUptimeSeconds, 0)) seconds" -ForegroundColor Gray
    
    Start-Sleep -Seconds 2
}
```

### Test 3: Stress Test (100 Concurrent Requests)

```powershell
Write-Host "Starting stress test with 100 requests..." -ForegroundColor Yellow

$jobs = 1..100 | ForEach-Object {
    $isPaid = if ($_ % 3 -eq 0) { "true" } else { "false" }
    Start-Job -ScriptBlock {
        param($isPaid)
        Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=$isPaid"
    } -ArgumentList $isPaid
}

Write-Host "Waiting for all requests to complete..." -ForegroundColor Yellow
$jobs | Wait-Job | Out-Null

Write-Host "`nResults:" -ForegroundColor Green
$results = $jobs | Receive-Job

$processing = ($results | Where-Object { $_.status -eq "processing" }).Count
$queued = ($results | Where-Object { $_.status -eq "queued" }).Count
$rejected = ($results | Where-Object { $_.status -eq "queue_full" }).Count

Write-Host "  Processing: $processing" -ForegroundColor Green
Write-Host "  Queued:     $queued" -ForegroundColor Yellow
Write-Host "  Rejected:   $rejected" -ForegroundColor Red

$jobs | Remove-Job

Write-Host "`nFinal Stats:" -ForegroundColor Cyan
Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats" | ConvertTo-Json
```

---

## 🌐 Browser Test URLs

### 1. Queue Monitor Dashboard
```
http://localhost:5000/qr-queue-monitor.html
```
**Features:**
- Real-time stats (auto-refresh mỗi 2s)
- Active requests list
- Queued requests list
- Test buttons (1 request, 10 requests)

### 2. Test QR Scan
```
http://localhost:5000/qr/FOODSTREET_VINHKHANH
```
**Expected:**
- Nếu < 10 active → Redirect ngay
- Nếu ≥ 10 active → Trang chờ với countdown

### 3. API Endpoints

```bash
# Get stats
curl http://localhost:5000/api/qrqueue/stats

# Get active requests
curl http://localhost:5000/api/qrqueue/active

# Get queued requests
curl http://localhost:5000/api/qrqueue/queued

# Get metrics
curl http://localhost:5000/api/qrqueue/metrics

# Check device status
curl http://localhost:5000/api/qrqueue/status/test_device_001
```

---

## 📊 Expected Behavior

### Normal Load (< 10 requests)
```
User quét QR → Xử lý ngay → Redirect đến POI page
Time: ~100ms
```

### Medium Load (10-100 requests)
```
User quét QR → Vào queue → Trang chờ → Auto-redirect khi đến lượt
Time: ~3s per position in queue
```

### High Load (> 100 requests)
```
User quét QR → Queue full → Trang "Hệ thống quá tải" → Đề xuất thử lại
```

### Priority Handling
```
Queue: [Paid #1] [Paid #2] [Free #1] [Free #2] [Free #3]
       ↑ Xử lý trước          ↑ Xử lý sau
```

---

## 🔍 Verification Checklist

- [x] ✅ Queue service khởi động thành công
- [x] ✅ API `/api/qrqueue/stats` trả về dữ liệu
- [x] ✅ Background workers đang chạy
- [ ] 🧪 Test single request
- [ ] 🧪 Test multiple requests (queue)
- [ ] 🧪 Test priority (paid vs free)
- [ ] 🧪 Test queue full scenario
- [ ] 🧪 Test timeout cleanup
- [ ] 🧪 Test real QR scan flow

---

## 🐛 Debug Commands

### Check if service is running
```bash
curl http://localhost:5000/api/qrqueue/stats
```

### Check logs
```bash
# Server logs sẽ hiển thị:
🚀 Queue processor started
🧹 Cleanup worker started
✅ Request processing immediately: device_xxx -> POI_xxx
📥 Request queued: device_xxx -> POI_xxx (Position: 5, Paid: false)
⚡ Processing queued request: device_xxx (Waited: 2.5s, Paid: true)
✅ Request completed: device_xxx (Duration: 1.2s)
⏱️ Request timeout: device_xxx (Age: 30.5s)
```

### Force complete a request (for testing)
```bash
curl -X POST "http://localhost:5000/api/qrqueue/test/complete?deviceId=test_device_001"
```

---

## 🎉 Quick Start

**Cách nhanh nhất để test:**

1. Mở dashboard:
   ```
   http://localhost:5000/qr-queue-monitor.html
   ```

2. Click "🔥 Thêm 10 Requests"

3. Quan sát:
   - 10 requests → Active (màu xanh)
   - Sau 30s → Tự động complete
   - Queue tự động xử lý tiếp

**Hoặc dùng PowerShell:**
```powershell
# Test nhanh
1..15 | ForEach-Object {
    Invoke-RestMethod -Method POST -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
}

# Xem kết quả
Invoke-RestMethod -Uri "http://localhost:5000/api/qrqueue/stats" | ConvertTo-Json
```

---

## 📝 Notes

- Queue service là **Singleton** → State được giữ trong suốt lifetime của app
- Background workers chạy liên tục để xử lý queue
- Timeout: 30s (requests quá 30s sẽ bị cleanup tự động)
- Priority: Paid users (10) > Free users (1)
- Max concurrent: 10 requests
- Max queue size: 100 requests

Hệ thống đã sẵn sàng! 🚀
