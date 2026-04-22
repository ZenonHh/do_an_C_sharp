# 🚀 DEVICE TRACKING - QUICK START & TESTING

## ⚡ 5-Minute Setup

### Step 1: Rebuild Project
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet clean
dotnet build
```

**Expected:** ✅ Build successful (0 errors)

### Step 2: Restart Server
```powershell
Stop-Process -Name dotnet -Force
dotnet run
```

**Expected:** 
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5000
```

### Step 3: Test Device Tracking

Open **PowerShell** and run:

```powershell
# Test online devices endpoint
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
Write-Host "Online devices: $($data.totalOnlineDevices)"
```

**Expected:** Returns number of online devices

---

## 🧪 Testing Device Tracking

### Test 1: Scan QR Code & Track Device

**Step 1: Open Admin Dashboard**
```
http://172.20.10.2:5000
```

**Step 2: Generate QR Code**
- Click POIs tab
- Click "View QR" for any restaurant
- Note the QR code

**Step 3: Scan QR (Simulator - Use Browser DevTools)**

In browser console (F12 → Console):
```javascript
// Simulate scanning QR from different device
fetch('/qr/POI_ABC123', {
    headers: {
        'User-Agent': 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X)'
    }
}).then(r => r.text()).then(t => console.log('QR scanned!'));
```

**Step 4: Check Device Registered**
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
$data.devices | ForEach-Object {
    Write-Host "$($_.deviceName) - $($_.deviceModel) ($($_.deviceOS)) - Last seen: $($_.lastOnlineAt)"
}
```

**Expected:** Device appears in list with iOS/Android info

---

### Test 2: Real Phone Test

**On Phone:**
1. Connect to same WiFi as PC
2. Open browser on phone
3. Visit: `http://172.20.10.2:5000`
4. Click POI link
5. View QR code
6. Scan from camera → redirects to `/qr/...`

**Server logs should show:**
```
✅ Device registered: device_192.168.1.105 (iPhone)
✅ Device updated online: device_192.168.1.105
```

**Check dashboard:**
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
Write-Host "Total online: $($data.totalOnlineDevices)"
```

---

### Test 3: Multiple Devices

**Simulate 3 different devices:**

```powershell
# Device 1: iPhone
$headers1 = @{'User-Agent' = 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_6)'}
Invoke-WebRequest -Uri "http://172.20.10.2:5000/qr/POI_ABC123" -Headers $headers1 -MaximumRedirection 0 -ErrorAction SilentlyContinue

# Device 2: Android
$headers2 = @{'User-Agent' = 'Mozilla/5.0 (Linux; Android 11)'}
Invoke-WebRequest -Uri "http://172.20.10.2:5000/qr/POI_ABC123" -Headers $headers2 -MaximumRedirection 0 -ErrorAction SilentlyContinue

# Device 3: Windows
$headers3 = @{'User-Agent' = 'Mozilla/5.0 (Windows NT 10.0)'}
Invoke-WebRequest -Uri "http://172.20.10.2:5000/qr/POI_ABC123" -Headers $headers3 -MaximumRedirection 0 -ErrorAction SilentlyContinue

# Check how many registered
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
Write-Host "Total devices: $($data.totalOnlineDevices)"
```

**Expected:** 3 devices registered with different OS

---

### Test 4: Dashboard Stats

```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/dashboard-stats"
$stats = $response.Content | ConvertFrom-Json

Write-Host "=== Dashboard Stats ==="
Write-Host "Total Users: $($stats.totalRegisteredUsers)"
Write-Host "Online Users: $($stats.totalOnlineUsers)"
Write-Host "Online Devices: $($stats.onlineDevices)"
Write-Host "Today QR Scans: $($stats.todayQRScans)"
Write-Host ""
Write-Host "Online Devices List:"
$stats.onlineDevicesList | ForEach-Object {
    Write-Host "  - $($_.deviceName) ($($_.deviceOS)) @ $($_.ipAddress)"
}
```

**Expected:** Shows all stats with device list

---

### Test 5: Device Heartbeat

```powershell
# Send heartbeat to keep device online
$deviceId = "device_192.168.1.100"
Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/device-heartbeat?deviceId=$deviceId" -Method POST
Write-Host "Heartbeat sent for device: $deviceId"

# Verify device still online
Start-Sleep -Seconds 2
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
$device = $data.devices | Where-Object { $_.deviceId -eq $deviceId }
if ($device) {
    Write-Host "✅ Device still online, last seen: $($device.lastOnlineAt)"
}
```

**Expected:** Device `lastOnlineAt` updates to current time

---

### Test 6: Device Offline

```powershell
$deviceId = "device_192.168.1.100"

# Mark device offline
Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/device-offline?deviceId=$deviceId" -Method POST
Write-Host "Device marked offline: $deviceId"

# Check updated status
Start-Sleep -Seconds 1
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
$device = $data.devices | Where-Object { $_.deviceId -eq $deviceId }
if ($device -and -not $device.isOnline) {
    Write-Host "✅ Device confirmed offline"
} else {
    Write-Host "❌ Device still showing online"
}
```

**Expected:** Device `isOnline` changes to false

---

## 📊 Expected Results

### ✅ Successful Device Tracking:

```
Online Devices (3):
├─ iPhone 14 (iOS)
│  ├─ Last seen: 2024-01-15 10:30:45
│  ├─ IP: 192.168.1.105
│  └─ Registered: 2024-01-15 10:15:00
│
├─ Samsung Galaxy A12 (Android)
│  ├─ Last seen: 2024-01-15 10:32:10
│  ├─ IP: 192.168.1.106
│  └─ Registered: 2024-01-15 10:20:00
│
└─ Windows PC (Windows)
   ├─ Last seen: 2024-01-15 10:25:30
   ├─ IP: 192.168.1.107
   └─ Registered: 2024-01-15 10:05:00
```

### ✅ Dashboard Shows:
```
┌─────────────────────────────┐
│ Dashboard Summary           │
├─────────────────────────────┤
│ Total Users: 5              │
│ Online Users: 3             │
│ Online Devices: 3           │
│ Today QR Scans: 12          │
└─────────────────────────────┘
```

---

## 📝 Database Check

### Verify Device Records

```powershell
# PowerShell script to check database
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Write-Host "✅ Database found: $dbPath"
} else {
    Write-Host "❌ Database not found"
}
```

### Query Devices (SQL)
```sql
SELECT 
    id, 
    device_id, 
    device_name, 
    device_model, 
    device_os, 
    is_online, 
    last_online_at,
    ip_address
FROM user_device
ORDER BY last_online_at DESC;
```

---

## 🔍 Debugging

### If Devices Not Tracking:

**Check 1: Server Logs**
```
Look for: "✅ Device registered:" or "✅ Device updated online:"
If missing: Device tracking code may not be executing
```

**Check 2: QR Scan**
```
Manually test: http://172.20.10.2:5000/qr/POI_ABC123
Check: Redirects to poi-public.html?poiId=X
If fails: QR code issue, not device tracking
```

**Check 3: Endpoint**
```powershell
# Test if endpoint exists
Try {
    $response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
    Write-Host "✅ Endpoint working"
} Catch {
    Write-Host "❌ Endpoint error: $_"
}
```

**Check 4: Database**
```powershell
# Verify UserDevice table exists and has data
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/qrscans/online-devices"
$data = $response.Content | ConvertFrom-Json
if ($data.devices.Count -eq 0) {
    Write-Host "⚠️  No devices in database yet"
} else {
    Write-Host "✅ $($data.devices.Count) devices found"
}
```

---

## 🎯 Success Criteria

✅ QR scan registers device info
✅ Device appears in online devices list
✅ Device OS detected correctly (iOS/Android/Windows)
✅ Device model captured
✅ IP address stored
✅ LastOnlineAt timestamp updates
✅ Device marked offline on request
✅ Dashboard stats show online device count
✅ Heartbeat endpoint keeps device alive
✅ Multiple devices tracking separately

---

## 🚀 Next Steps

1. **Run all tests above** ✓
2. **Verify device data in database** ✓
3. **Check admin dashboard** ✓
4. **Add device list UI to dashboard** (Next)
5. **Implement heartbeat in frontend** (Next)
6. **Add real-time updates with WebSocket** (Future)

---

## 📄 Commands Reference

```powershell
# Build & Run
dotnet clean && dotnet build && dotnet run

# Test Online Devices
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/online-devices"

# Test Dashboard Stats
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/dashboard-stats"

# Send Heartbeat
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/device-heartbeat?deviceId=test" -Method POST

# Mark Offline
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/device-offline?deviceId=test" -Method POST
```

---

**Status: ✅ READY FOR TESTING**

Run the tests above and report results!
