# 🧪 QR CODE SCANNING - COMPLETE TESTING GUIDE

## 📋 Pre-Test Checklist

- [ ] Visual Studio closed or solution saved
- [ ] Previous dotnet.exe processes killed
- [ ] Old database deleted
- [ ] Network accessible (WiFi connected)
- [ ] Phone on same network as PC

---

## 🚀 Step 1: Start Server

### Option A: Using PowerShell Script (RECOMMENDED)
```powershell
cd C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb
.\restart-and-test-qr.ps1
```

**What script does:**
- ✅ Stops old dotnet processes
- ✅ Deletes old database
- ✅ Builds project
- ✅ Starts server
- ✅ Seeds 5 sample POIs
- ✅ Displays QR code info

### Option B: Manual Steps
```powershell
# Stop server
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue

# Delete old DB
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force

# Build & Run
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet clean
dotnet build
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## ✅ Step 2: Verify Server Running

### Test 1: Check API
```powershell
# In new PowerShell window
Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/all" | ConvertFrom-Json | ForEach-Object { $_.pois | ForEach-Object { Write-Host "$($_.name): $($_.qrCode)" } }
```

**Expected Output:**
```
Ốc Oanh: http://172.20.10.2:5000/qr/POI_ABC123
Ốc Vũ: http://172.20.10.2:5000/qr/POI_DEF456
Ốc Nho: http://172.20.10.2:5000/qr/POI_GHI789
Quán Nướng Chilli: http://172.20.10.2:5000/qr/POI_JKL012
Lẩu Bò Khu Nhà Cháy: http://172.20.10.2:5000/qr/POI_MNO345
```

### Test 2: Check Admin Dashboard
```
Open browser: http://172.20.10.2:5000
Expected: Admin dashboard with POI management tab
```

---

## 🖼️ Step 3: View QR Codes

### On Admin Dashboard
1. Click **"POIs"** tab
2. You should see table with 5 restaurants:
   - Ốc Oanh
   - Ốc Vũ
   - Ốc Nho
   - Quán Nướng Chilli
   - Lẩu Bò Khu Nhà Cháy

3. For each restaurant, click **"View QR"** or **"Generate QR"**
4. Modal should display:
   - QR Code image (clear & scannable)
   - QR Code URL: `http://172.20.10.2:5000/qr/POI_XXXXX`

---

## 📱 Step 4: Test QR Scanning (Method 1 - Phone Camera)

### Requirements
- iPhone or Android phone
- On same WiFi network as PC
- Phone camera can scan QR codes

### Steps
1. **On PC**: Open admin dashboard → click "View QR" for any restaurant
2. **On Phone**: 
   - Open Camera app
   - Point at QR code on PC screen
   - When QR is recognized → notification appears
   - Tap notification to open link

### Expected Result
- ✅ Browser opens on phone
- ✅ Redirects to: `http://172.20.10.2:5000/poi-public.html?poiId=1`
- ✅ Restaurant info page displays:
  - 📍 Restaurant name
  - 🏠 Address
  - 📝 Description
  - 🖼️ Image (if available)
  - 🔊 Audio guide button (if available)

### If Fails
**Error: "Quán ăn không tìm thấy" (Restaurant not found)**
- ❌ Database not seeded properly
- Solution: Check `/api/pois/debug/all` returns data
- Restart server: `Stop-Process -Name dotnet; dotnet run`

**Error: "Cannot reach 172.20.10.2:5000"**
- ❌ Phone not on same network
- ❌ PC firewall blocking port 5000
- Solution:
  - Check phone & PC on same WiFi
  - Allow port 5000 through firewall:
    ```powershell
    New-NetFirewallRule -DisplayName "Allow 5000" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5000
    ```

---

## 📱 Step 5: Test QR Scanning (Method 2 - QR Scanner App)

### Using Online QR Scanner
1. Open browser on phone: https://www.qr-code-generator.com/qr-code-scanner/
2. Click "Upload Image" or "Scan with camera"
3. Take photo of QR code on PC screen
4. Scanner reads the URL
5. Click URL to open in browser

### Using Mobile App
1. Download QR Scanner app (iOS/Android)
2. Scan QR code from PC screen
3. App reads full URL
4. Click to open in browser

---

## 🔗 Step 6: Test URL Direct Access

### Manual URL Test
Instead of scanning, copy URL directly:
```
http://172.20.10.2:5000/qr/POI_ABC123
```

Paste into browser on phone or another PC:
- ✅ Should redirect to `/poi-public.html?poiId=1`
- ✅ Should display restaurant info

---

## 🧪 Step 7: API Endpoint Tests

### Test QR Lookup
```powershell
$code = "POI_ABC123"  # Get actual code from debug/all
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/qr/$code"
$response.Content | ConvertFrom-Json
```

**Expected:**
```json
{
  "id": 1,
  "name": "Ốc Oanh",
  "address": "534 Vĩnh Khánh, Q.4",
  "qrCode": "http://172.20.10.2:5000/qr/POI_ABC123",
  ...
}
```

### Test QR Redirect
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/qr/POI_ABC123" -MaximumRedirection 0 -ErrorAction SilentlyContinue
$response.Headers["Location"]  # Should show redirect path
```

**Expected:**
```
/poi-public.html?poiId=1&deviceId=device_192.168.x.x&code=POI_ABC123
```

---

## 📊 Complete Test Matrix

| Test | Method | Expected | Status |
|------|--------|----------|--------|
| **Server Running** | Browser: http://172.20.10.2:5000 | Dashboard displays | ☐ |
| **POI API** | GET /api/pois/debug/all | Returns 5 POIs with URLs | ☐ |
| **QR Code View** | Click "View QR" on POI | Shows QR image & URL | ☐ |
| **QR Code Format** | Check QRCode field | Contains full URL | ☐ |
| **QR Scan** | Phone camera scans QR | Opens restaurant page | ☐ |
| **Info Page** | Page displays after scan | Shows name/address/images | ☐ |
| **URL Redirect** | Access /qr/{code} directly | Redirects to poi-public | ☐ |
| **QR Lookup** | API /pois/qr/{code} | Returns POI data | ☐ |

---

## 🎯 Success Criteria

✅ **All tests pass if:**
1. Server starts without errors
2. 5 POIs display with QR codes
3. QR codes contain full URLs (not just codes)
4. QR image renders clearly
5. Phone can scan QR code
6. Restaurant info page displays after scan
7. No "Restaurant not found" errors

---

## 🐛 Troubleshooting

### Issue: Server won't start
```powershell
# Check if port is in use
netstat -ano | findstr :5000

# Kill process using port
taskkill /PID <PID> /F

# Try again
dotnet run
```

### Issue: Database empty (0 POIs)
```powershell
# Delete old database
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force

# Restart server
Stop-Process -Name dotnet -Force
dotnet run
```

### Issue: QR codes showing as code-only
```
This means changes weren't applied!
- Check: database seeding is using full URLs
- Check: GenerateQRCode() returns full URL
- Verify: POIsController.cs has changes
- Rebuild: dotnet clean; dotnet build
```

### Issue: Phone can't reach server
```
- Both on same WiFi? 
- Firewall blocking port 5000?
  netsh advfirewall firewall add rule name="Allow 5000" dir=in action=allow protocol=tcp localport=5000
- Try with IP instead of hostname
```

### Issue: "Restaurant not found" error
```
- Check API returns POI data
- Verify QRCode format in database
- Check GetPOIByQRCodeAsync() logic
- Look at server logs for errors
```

---

## 📝 Test Log Template

```
Date: ___________
Tester: ___________

Server Start:      [ ] Pass [ ] Fail
API /debug/all:    [ ] Pass [ ] Fail  
QR Code Display:   [ ] Pass [ ] Fail
QR Code Format:    [ ] Pass [ ] Fail
Phone Scan:        [ ] Pass [ ] Fail
Info Page:         [ ] Pass [ ] Fail
URL Redirect:      [ ] Pass [ ] Fail

Issues Found:
_________________________
_________________________

Notes:
_________________________
_________________________
```

---

## 🎉 When Everything Works

```
✅ Admin dashboard: Shows 5 restaurants
✅ Each POI: Has QR code (clear image)
✅ QR code: Contains full URL
✅ Phone scan: Opens browser automatically
✅ Browser opens: Restaurant info page
✅ Info page: Displays all details
✅ No errors: Everything smooth

👉 SUCCESS! QR scanning is fully functional! 🚀
```

---

**Good luck with testing! 📱✨**
