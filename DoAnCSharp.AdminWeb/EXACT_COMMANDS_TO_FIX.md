# ⚡ EXACT COMMANDS TO RUN - Copy & Paste

## 🚀 READY TO FIX? FOLLOW THESE COMMANDS

### **Command 1: Stop Server**
```powershell
# If running in terminal, press Ctrl+C
# Or from PowerShell:
taskkill /IM dotnet.exe /F
```

**Output:**
```
SUCCESS: The process "dotnet.exe" with PID XXX has been terminated.
```

---

### **Command 2: Delete Old Database**
```powershell
# Delete the database file
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force -ErrorAction SilentlyContinue

# Verify it's gone
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Write-Host "❌ Database still exists - try again"
} else {
    Write-Host "✅ Database deleted - ready for fresh start"
}
```

**Expected Output:**
```
✅ Database deleted - ready for fresh start
```

---

### **Command 3: Navigate to Server Directory**
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"

# Verify you're in right directory
Get-ChildItem | grep -i "program.cs"
```

**Expected Output:**
```
Mode                 LastWriteTime         Length Name
----                 ---------         ------  ----
-a----         4/21/2026   1:19 PM         XXXX Program.cs
```

---

### **Command 4: Start Server**
```powershell
dotnet run
```

**Expected Output:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

⏳ **Wait for these messages** before proceeding!

---

### **Command 5: Browser - Open Admin Dashboard**

Open in browser (on computer, NOT phone):
```
http://192.168.0.125:5000
```

**Expected:**
- Admin dashboard loads
- 7 tabs visible
- Animations smooth

---

### **Command 6: Create New POI**

Follow these steps in admin dashboard:

1. **Click Tab:** 🏪 Quán Ăn

2. **Click Button:** ➕ Thêm Quán Ăn Mới

3. **Fill Form:**
```
Tên Quán:           Test Restaurant #1
Địa Chỉ:           123 Test Street, Q.1
Vĩ Độ:             10.7595
Kinh Độ:           106.7045
```

4. **Click:** 🔄 Tạo
   - Wait 2 seconds
   - Input should auto-fill with QR code

5. **Verify QR Code Format:**
   - Check input field
   - Should show: `http://192.168.0.125:5000/qr/POI_XXXXX`
   - ✅ Correct: Should have full URL
   - ❌ Wrong: Empty or just code

6. **Click:** ✅ Thêm Quán
   - Message: "✅ Thêm quán ăn thành công"
   - POI appears in list

7. **Verify in Database:**
   - Click: 👁️ Xem QR
   - See QR code image
   - See QR text below

---

### **Command 7: Verify QR Code in Database**

Open in browser:
```
http://192.168.0.125:5000/api/pois
```

**Expected JSON:**
```json
[
  {
    "id": 1,
    "name": "Test Restaurant #1",
    "address": "123 Test Street, Q.1",
    "qrCode": "POI_ABC123DEF45",
    "lat": 10.7595,
    "lng": 106.7045,
    ...
  }
]
```

**Check:**
- ✅ qrCode should be: `POI_ABC123DEF45` (code only)
- ❌ qrCode should NOT be: `http://192.168.0.125:5000/qr/...` (full URL)

---

### **Command 8: Test Endpoint on Desktop**

Open in browser:
```
http://192.168.0.125:5000/qr/POI_ABC123DEF45
```
(Replace POI_ABC123DEF45 with actual QR code from database)

**Expected:**
- Redirects to: `poi-public.html?poiId=1&code=POI_ABC123DEF45`
- Page loads
- See restaurant info (or loading spinner)

**Server Log Should Show:**
```
[QR] Received code: POI_ABC123DEF45
[QR] Looking up POI with code: POI_ABC123DEF45
[QR] POI found: 1 - Test Restaurant #1
[QR] Redirecting to: /poi-public.html?poiId=1&code=POI_ABC123DEF45
```

---

### **Command 9: Test on Phone**

**Setup (iPhone):**
```
1. WiFi: Connect to same WiFi as computer
2. Camera: Open Camera app
3. Point: At QR code on screen
4. Tap: Link that appears
```

**Expected:**
- Safari opens
- Page loads (not blank)
- See: "Test Restaurant #1"
- See: Images (if added)
- See: Audio player
- See: Download buttons

---

### **Command 10: Verify Success**

Check these 3 things:

**1. Server Log:**
```powershell
# In terminal where server runs, should see:
[QR] Received code: POI_ABC123DEF45
[QR] POI found: 1 - Test Restaurant #1
[QR] Redirecting to: /poi-public.html...
```

**2. Browser (Desktop):**
```
/api/pois shows QRCode as: POI_ABC123 (not URL)
/qr/POI_ABC123 redirects successfully
/poi-public.html?poiId=1 loads with info
```

**3. Phone Safari:**
```
Scan QR → Page loads (not blank)
See restaurant name ✅
No errors ✅
```

---

## 🧹 BONUS: Automated Cleanup Script

Instead of running individual commands, use this:

```powershell
# Copy entire block and paste in PowerShell

# 1. Kill dotnet
taskkill /IM dotnet.exe /F 2>$null

# 2. Delete database
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Database deleted"
}

# 3. Navigate and start
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
Write-Host "✅ Starting server..."
dotnet run
```

---

## ⚠️ COMMON MISTAKES

### ❌ Wrong: Keeping old database
```powershell
# Old database has wrong QR format
# MUST delete before testing
```

### ❌ Wrong: Not restarting server
```powershell
# Changes in files won't load
# MUST restart (dotnet run)
```

### ❌ Wrong: Testing old POIs
```powershell
# Old POIs have wrong QR code format
# MUST create NEW POI
```

### ❌ Wrong: Wrong IP address
```powershell
# Verify: appsettings.Development.json
# PublicUrl: "http://192.168.0.125:5000"
# Check actual IP with: ipconfig
```

---

## 📞 IF SOMETHING FAILS

**Copy this info:**

```powershell
# Get current date/time
Get-Date

# Get IP address
ipconfig | findstr "IPv4"

# Check database exists
Test-Path "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"

# Check server is running
netstat -ano | findstr ":5000"

# Get server process
Get-Process dotnet -ErrorAction SilentlyContinue
```

**Then tell me:**
1. Which command failed?
2. What was the error?
3. What did you expect?
4. What did you see instead?

---

## ✅ SUCCESS INDICATORS

| Check | Success | Failure |
|-------|---------|---------|
| Server starts | `Now listening on: http://localhost:5000` | Error or hangs |
| Database created | File exists in AppData/VinhKhanhTour | File not found |
| POI created | Form shows success message | Error message |
| QR format | Code only (POI_ABC123) | Full URL or empty |
| Desktop test | Page loads with info | Blank or error |
| Phone scan | Restaurant info displays | "Can't open page" |

---

**Ready? Start with Command 1! 🚀**
