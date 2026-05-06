# ⚡ RESTART SERVER NOW - 3 SIMPLE STEPS

## What I Fixed:
1. ✅ Enabled database seeding (was commented out)
2. ✅ Removed duplicate method
3. ✅ Build successful

**Result**: Database will have 5 test restaurants with QR codes

---

## 🚀 DO THIS NOW:

### **Step 1: Kill Old Server (30 seconds)**
```powershell
Get-Process | Where-Object {$_.ProcessName -eq "dotnet"} | Stop-Process -Force
```

OR use Task Manager:
- Find `dotnet.exe` 
- Right-click → End Task

---

### **Step 2: Open Terminal**
```
Windows Key → Type: powershell
Go to: C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb
```

OR click on folder, right-click → Open in Terminal

---

### **Step 3: Run Server**
```powershell
dotnet build
dotnet run
```

**Wait 3 seconds** ⏳ (seeding happens)

You'll see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5000
```

---

## ✅ Test It:

### **Test 1: Check Data in Browser**
```
http://172.20.10.2:5000/api/pois/debug/all
```
Should see 5 POIs like:
```json
{
  "totalCount": 5,
  "pois": [
    {"id": 1, "name": "Ốc Oanh", "qrCode": "POI_...", "address": "534 Vĩnh Khánh"}
    // ... 4 more
  ]
}
```

### **Test 2: Scan QR on Phone**
1. Go to: `http://172.20.10.2:5000`
2. Tab "🏪 Quán Ăn"
3. Click on a restaurant
4. Click QR Code button to download/view
5. Scan it with phone camera
6. Should show restaurant info page ✅

### **Test 3: Quick Browser Test**
```
http://172.20.10.2:5000/qr/POI_ABC123
```
(Replace ABC123 with actual QR code)

Should redirect to restaurant info page (NOT error)

---

## What You'll See:

### **Before Fix** ❌:
- Scan QR → "❌ Quán ăn không tìm thấy"
- `/api/pois/debug/all` → Empty list (totalCount: 0)

### **After Fix** ✅:
- Scan QR → Beautiful restaurant page with:
  - 🏪 Restaurant name
  - 📍 Address
  - 📝 Description
  - 🖼️ Images
  - 🎧 Audio player (Vietnamese, English, Japanese)
  - 📲 Download app buttons
- `/api/pois/debug/all` → 5 POIs

---

## If It Still Doesn't Work:

**Option A: Clear Database & Restart**
```powershell
# Delete old database
Remove-Item "C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force

# Restart server (will create fresh database with seeding)
dotnet run
```

**Option B: Check Build**
```powershell
dotnet clean
dotnet build

# Look for error messages, if any
```

**Option C: Check Server Running**
```powershell
# In new terminal:
Invoke-WebRequest http://172.20.10.2:5000/api/pois/debug/all
```

Should return JSON, not error

---

## 📋 Checklist:

```
☐ Server process killed
☐ Opened terminal in correct folder
☐ Ran: dotnet build (should succeed)
☐ Ran: dotnet run (should show "listening on...")
☐ Waited 3 seconds for seeding
☐ Checked http://172.20.10.2:5000/api/pois/debug/all in browser
☐ Saw 5 POIs returned
☐ Scanned QR code on phone
☐ Saw restaurant info (NOT error)
```

---

## That's It! 🎉

All fixes are in place. Just restart server and test.

If you have issues, check:
1. Server actually running? (Try visiting http://172.20.10.2:5000)
2. Database path? (C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\)
3. Network? (Phone on same network as PC?)

Good luck! 🚀
