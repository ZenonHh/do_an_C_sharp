# ⚡ DEVICE TRACKING - QUICK REFERENCE

## 🎯 What's New

When someone scans a QR code on their phone:
1. System **captures device info** (iPhone/Android/Windows)
2. **Stores in database** (device name, model, OS, IP)
3. **Marks as online** on dashboard
4. **Updates last seen time**
5. Shows **online device count** on admin page

---

## 🚀 Commands to Run

### Build & Start
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet clean && dotnet build && dotnet run
```

### Test Online Devices
```powershell
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/online-devices" | ConvertFrom-Json | ConvertTo-Json
```

### Test Dashboard Stats
```powershell
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/dashboard-stats" | ConvertFrom-Json | ConvertTo-Json
```

### Send Device Heartbeat (Keep Online)
```powershell
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/device-heartbeat?deviceId=device_192.168.1.100" -Method POST
```

### Mark Device Offline
```powershell
Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/device-offline?deviceId=device_192.168.1.100" -Method POST
```

---

## 📊 API Endpoints (4 New)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/qrscans/online-devices` | GET | List of online devices |
| `/api/qrscans/dashboard-stats` | GET | Complete dashboard stats |
| `/api/qrscans/device-heartbeat?deviceId=X` | POST | Keep device online |
| `/api/qrscans/device-offline?deviceId=X` | POST | Mark device offline |

---

## 💾 Database Changes

**UserDevice table** gets updated:
- New device registered on first scan
- `IsOnline` = true
- `LastOnlineAt` = current time
- Device info captured (model, OS, IP)

---

## 📱 Device Detection

```
iPhone        → iOS
Samsung       → Android
Windows PC    → Windows
Mac           → macOS
Unknown       → From User-Agent
```

---

## 🧪 Quick Test (5 minutes)

```powershell
# 1. Start server
dotnet run

# 2. In new PowerShell, test endpoint
$r = Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/online-devices"
($r.Content | ConvertFrom-Json).totalOnlineDevices

# Expected: Number of online devices

# 3. Test stats
$s = Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/dashboard-stats"
($s.Content | ConvertFrom-Json).onlineDevices

# Expected: Online device count
```

---

## ✅ What's Tracked

✅ Device type (iPhone, Android, Windows)
✅ Device model (iPhone 14, Galaxy A12, etc)
✅ Operating system (iOS, Android, Windows)
✅ IP address
✅ Last online time
✅ Registration time
✅ Online/offline status

---

## ❌ What's NOT Tracked

❌ Personal data
❌ Browsing history
❌ App data
❌ Device location (just IP)
❌ IMEI/Serial numbers

---

## 🔄 Workflow

```
User scans QR
    ↓
Server captures: Device type, OS, IP
    ↓
Database records device info
    ↓
Marks device as online
    ↓
Admin sees on dashboard: "5 devices online"
    ↓
When user leaves, send offline request
    ↓
Dashboard updates: "4 devices online"
```

---

## 💡 Usage Examples

### JavaScript Heartbeat (Keep Online)
```javascript
// Send heartbeat every 30 seconds
setInterval(() => {
    fetch('/api/qrscans/device-heartbeat?deviceId=' + deviceId, 
        {method: 'POST'});
}, 30000);
```

### Mark Offline on Leave
```javascript
window.addEventListener('beforeunload', () => {
    fetch('/api/qrscans/device-offline?deviceId=' + deviceId, 
        {method: 'POST'});
});
```

### Get Online Count
```javascript
async function getOnlineCount() {
    const r = await fetch('/api/qrscans/online-devices');
    const data = await r.json();
    console.log('Online: ' + data.totalOnlineDevices);
}
```

---

## 📝 Files Modified

- ✏️ **QRScansController.cs** - Added device tracking

## 🆕 Methods Added

```
TrackDeviceInfoAsync()    → Main tracking method
ExtractDeviceInfo()       → Parse User-Agent
GetLocationFromIP()       → Get location
GetOnlineDevices()        → API endpoint
GetDashboardStats()       → API endpoint
DeviceHeartbeat()         → API endpoint
DeviceOffline()           → API endpoint
```

---

## 📋 Response Format

### Online Devices
```json
{
  "totalOnlineDevices": 3,
  "devices": [
    {
      "deviceId": "device_192.168.1.100",
      "deviceName": "iPhone",
      "deviceModel": "iPhone 14",
      "deviceOS": "iOS",
      "isOnline": true,
      "lastOnlineAt": "2024-01-15T10:30:45",
      "ipAddress": "192.168.1.100"
    }
  ]
}
```

### Dashboard Stats
```json
{
  "onlineDevices": 3,
  "todayQRScans": 25,
  "totalOnlineUsers": 5,
  "totalRegisteredUsers": 150,
  "qrActivity": {...}
}
```

---

## 🎯 Next Steps

1. ✅ Build project: `dotnet build`
2. ✅ Start server: `dotnet run`
3. ✅ Test endpoints (see Quick Test above)
4. ⬜ Add UI to dashboard (show device list)
5. ⬜ Implement frontend heartbeat
6. ⬜ Add auto-refresh to device list

---

## ❓ Troubleshooting

**Devices not appearing?**
- Check server logs for "Device registered"
- Verify endpoint returns data
- Make sure QR is being scanned

**Wrong device info?**
- User-Agent might be modified
- Android model extraction can fail
- Falls back to generic name

**Device stays online forever?**
- Implement automatic heartbeat
- Set timeout in frontend
- Call offline endpoint on leave

---

## 📞 Support Docs

- **DEVICE_TRACKING_COMPLETE_GUIDE.md** - Full technical guide
- **DEVICE_TRACKING_TESTING.md** - Complete test procedures
- **DEVICE_TRACKING_IMPLEMENTATION_COMPLETE.md** - Implementation summary

---

## ✨ Summary

```
✅ Device info captured on QR scan
✅ Multiple devices tracked
✅ Real-time online status
✅ Dashboard integration ready
✅ API endpoints available
✅ Heartbeat system ready
✅ Build successful (0 errors)
```

---

**Status: READY FOR TESTING** ✅

Run: `dotnet build && dotnet run`

Test: `Invoke-WebRequest "http://172.20.10.2:5000/api/qrscans/online-devices"`
