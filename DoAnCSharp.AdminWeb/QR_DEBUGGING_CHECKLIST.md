# 🔍 DEBUGGING CHECKLIST - QR Scanning Issues

## 🚀 KIỂM TRA TỪNG BƯỚC

### **STEP 1: Xác nhận Server Chạy**
```powershell
# Terminal check
netstat -ano | findstr ":5000"

# Nếu có output: ✅ Server running
# Nếu không: ❌ Start server
```

```
Browser test:
http://192.168.0.125:5000/test-qr

Expected: Redirect to poi-public.html?poiId=1&test=true
(test endpoint để verify endpoint hoạt động)
```

---

### **STEP 2: Check Database Existence**
```powershell
# Verify database file
Test-Path "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"

# Output:
# True = ✅ Database exists
# False = ❌ Database doesn't exist (will be created on first request)
```

---

### **STEP 3: Check POI Exists**
```
Browser:
http://192.168.0.125:5000/api/pois

Expected JSON response:
[
  {
    "id": 1,
    "name": "Test Restaurant",
    "qrCode": "POI_ABC123DEF",  ← Should be CODE, not full URL
    ...
  }
]

Check: QR Code format:
✅ Correct: "POI_ABC123DEF"
❌ Wrong: "http://192.168.0.125:5000/qr/POI_ABC123DEF"
```

---

### **STEP 4: Test QR Endpoint Directly**
```
Browser (desktop):
http://192.168.0.125:5000/qr/POI_ABC123DEF

Expected:
- Redirect to: /poi-public.html?poiId=1&code=POI_ABC123DEF
- Page should load with restaurant info

Server log should show:
[QR] Received code: POI_ABC123DEF
[QR] Looking up POI with code: POI_ABC123DEF
[QR] POI found: 1 - Test Restaurant
[QR] Redirecting to: /poi-public.html?poiId=1&code=POI_ABC123DEF
```

---

### **STEP 5: Check poi-public.html**
```
Browser:
http://192.168.0.125:5000/poi-public.html?poiId=1

Expected:
- Page loads
- Shows loading spinner briefly
- Displays restaurant info
- No errors in console

If blank page:
1. Check console (F12 → Console tab)
2. Look for red error messages
3. Check Network tab for failed requests
```

---

### **STEP 6: Verify API Response**
```
Browser:
http://192.168.0.125:5000/api/pois/1

Expected JSON:
{
  "id": 1,
  "name": "Test Restaurant",
  "address": "...",
  "description": "...",
  "lat": 10.7595,
  "lng": 106.7045,
  ...
}

If 404:
❌ POI with ID=1 doesn't exist
→ Create new POI first
```

---

### **STEP 7: Test QR Scan on Phone**

#### **Setup:**
```
1. Phone on same WiFi as server
2. Server running
3. POI exists in database
```

#### **Expected Flow:**
```
Phone camera 📱
  ↓ Scan QR code
  ↓ Safari opens link
  ↓ GET /qr/POI_ABC123
  ↓ Server log: [QR] POI found
  ↓ Redirect to /poi-public.html?poiId=1
  ↓ Page loads, renders restaurant info ✅
```

#### **Debug on iPhone:**
```
1. Open Safari
2. Go to: http://192.168.0.125:5000
3. Tap in address bar
4. Should show autocomplete options
5. If not: ❌ Server not reachable from phone

Check WiFi:
- Phone WiFi should match computer WiFi
- Both on same network
- No "Guest Network" restriction
```

---

## 🛠️ COMMON ISSUES & FIXES

### **Issue: "Safari can't open the page"**
```
Causes:
1. ❌ Wrong IP address
   Fix: Update appsettings.Development.json with correct IP

2. ❌ Server not running
   Fix: Start server with dotnet run

3. ❌ Firewall blocking
   Fix: Allow port 5000 in Windows Firewall

4. ❌ Phone not on same network
   Fix: Check WiFi connection matches

Verify:
- Ping from phone: http://192.168.0.125:5000
- Should see admin dashboard
```

### **Issue: "Page not found" (404)**
```
Causes:
1. ❌ poi-public.html missing or not being served
   Fix: Verify file exists at wwwroot/poi-public.html

2. ❌ Wrong URL format
   Fix: Check console log for actual redirect URL

Debug:
- Browser: http://192.168.0.125:5000/poi-public.html
- Should load page (even if blank)
```

### **Issue: Blank page when scanning**
```
Causes:
1. ❌ POI not found in database
   Check: /api/pois/1 returns data?
   Fix: Create new POI

2. ❌ JavaScript error in poi-public.html
   Check: Browser console (F12 → Console)
   Fix: Report error message

3. ❌ API endpoint error
   Check: Network tab (F12 → Network)
   Look for failed requests to /api/pois/{id}
   Fix: Check server log for errors
```

### **Issue: Wrong QR code format in database**
```
OLD (Wrong):
QRCode = "http://192.168.0.125:5000/qr/POI_ABC123"
↓
Database search fails (looking for just code)

NEW (Correct):
QRCode = "POI_ABC123"
↓
Database search succeeds

Fix:
1. Delete database
2. Restart server
3. Create new POI
4. Verify: /api/pois should show QRCode as code only
```

---

## 📝 TROUBLESHOOTING TEMPLATE

When reporting issue, include:

```
❌ Error:
   [Describe what you see on screen]

📱 Device:
   [iPhone/Android, Safari/Chrome]

🌐 URL when error occurs:
   [What's in address bar?]

📊 Server Log:
   [Copy last 10 lines from server terminal]

🖥️ Browser Console Error:
   [F12 → Console → Copy error message]

🔍 Expected:
   [What should happen]

⏱️ When started happening:
   [After which step]
```

---

## ✅ VERIFICATION CHECKLIST

Before declaring "fixed", verify:

- [ ] Server running (`netstat -ano | findstr ":5000"`)
- [ ] Database exists (`$env:APPDATA\VinhKhanhTour`)
- [ ] POI in database (`/api/pois` returns data)
- [ ] QR code format correct (code only, not URL)
- [ ] `/api/pois/1` endpoint works
- [ ] `/qr/POI_ABC123` endpoint redirects
- [ ] `/poi-public.html?poiId=1` loads without error
- [ ] Phone can ping server (`http://192.168.0.125:5000`)
- [ ] Safari on phone opens page without "couldn't connect"
- [ ] Restaurant info displays on phone after QR scan

---

## 🎯 SUCCESS INDICATORS

When everything is working:

1. **Server terminal shows:**
   ```
   [QR] Received code: POI_XXXXX
   [QR] POI found: 1 - Test Restaurant
   [QR] Redirecting to: /poi-public.html?poiId=1
   ```

2. **Phone shows:**
   ```
   ✅ Page loads (not blank)
   ✅ Restaurant name visible
   ✅ Images displaying
   ✅ Audio buttons present
   ✅ Download buttons visible
   ```

3. **No errors:**
   ```
   ✅ Safari: No "couldn't connect" error
   ✅ Console: No red error messages
   ✅ Network: All requests successful (200/302)
   ```

---

## 📞 STILL BROKEN?

Share:
1. Server log (last 20 lines)
2. Browser console error (if any)
3. Current URL in address bar
4. What you see on screen
5. What you expected to see

I'll debug from there! 👍
