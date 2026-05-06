# 🎯 Fix "Quán Không Tìm Thấy" - Quick Action Guide

## ⚡ The Problem
After updating device tracking:
- ❌ QR scans show "Quán không tìm thấy" (Restaurant not found)
- ❌ Device tracking works ✅
- ❌ Online count updates ✅
- ❌ But POI lookup fails ❌

## 🎯 Why This Happens
1. **Old database** has stale data
2. **POI seeding** didn't complete
3. **QR code format** mismatch in lookup

## ✅ The Fix (Quick Version)

### Option 1: Auto-Fix Script (Recommended - 2 minutes)

```powershell
# Run this one command:
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb"
.\fix-poi-not-found.ps1
```

**This does:**
1. Kills old processes
2. Deletes old database
3. Cleans build files
4. Rebuilds project
5. Starts server with fresh POI seeding

---

### Option 2: Manual Fix (5 minutes)

**Step 1:** Close all PowerShell windows and Visual Studio

**Step 2:** Delete database
```powershell
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force -ErrorAction SilentlyContinue
```

**Step 3:** Clean and rebuild
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet clean; dotnet build; dotnet run
```

---

## 🧪 Test After Fix

**In a NEW PowerShell window (while server is running):**

```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb"
.\test-poi-seeding.ps1
```

**This will:**
1. ✅ Check if server is running
2. ✅ Show all POIs in database
3. ✅ Test QR scan with first POI
4. ✅ Verify redirect works

---

## ✅ Expected Result

```
✅ Found 5 POIs:

  📍 Name: Ốc Oanh
     ID: 1
     QRCode: http://172.20.10.2:5000/qr/POI_XXXXX

  📍 Name: Ốc Vũ
     ID: 2
     ...

✅ QR scan successful!
   Redirected to: /poi-public.html?poiId=1
✅ POI found and matched correctly!
```

---

## 📱 Real Phone Test

After fix is complete:

1. **Open on your phone:**
   ```
   http://172.20.10.2:5000
   ```

2. **Click on any POI card**

3. **Click "View QR Code"**

4. **Scan the QR code from another phone**

5. **You should see:**
   ✅ Restaurant info page with photos
   ✅ NO "Quán không tìm thấy" error

---

## 🔍 If Still Getting Error

### Check 1: Server Console Output

When server starts, you should see:
```
Initializing database...
Creating tables...
Seeding sample data...
Seed complete.
```

If you see **errors**, report them.

### Check 2: POIs Endpoint

```powershell
Invoke-WebRequest "http://172.20.10.2:5000/api/pois" | ConvertFrom-Json | ConvertTo-Json
```

Should show 5 POIs with QRCode fields.

### Check 3: QR Code Format

All QR codes should match this pattern:
```
http://172.20.10.2:5000/qr/POI_XXXXXXXXX
```

---

## 🎯 Device Tracking Still Works!

✅ Devices still auto-register on QR scan
✅ Online count still updates
✅ IP addresses still captured
✅ Heartbeat still works

This fix only restores POI lookup!

---

## 📋 Files Created

| File | Purpose |
|------|---------|
| `fix-poi-not-found.ps1` | Auto-fix script (recommended) |
| `test-poi-seeding.ps1` | Test if POIs are seeded |
| `FIX_POI_NOT_FOUND_2024.md` | Detailed troubleshooting guide |

---

## ⚡ Quick Start Now!

```powershell
# DO THIS NOW:
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb"
.\fix-poi-not-found.ps1
```

**That's it!** The server will start and POI seeding will complete.

---

## 🆘 Need Help?

If the script doesn't work:

1. **Check server console** for error messages
2. **Run test script** to see what's wrong
3. **Manually check** `/api/pois` endpoint
4. **Report** the exact error you see

**Most likely:** The database just needed to be recreated with fresh seeding. This fix does exactly that! 🚀

---

**Status: Ready to run! ✅**
