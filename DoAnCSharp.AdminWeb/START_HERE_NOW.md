# ⚡ QUICK START - RUN THESE COMMANDS NOW

## 🚀 Option 1: Automated Script (EASIEST)

Copy and paste this in PowerShell:

```powershell
cd C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb
.\restart-and-test-qr.ps1
```

**What happens:**
- ✅ Stops old server
- ✅ Deletes old database  
- ✅ Builds project
- ✅ Starts server
- ✅ Seeds 5 restaurants
- ✅ Shows POI list with QR codes

---

## 🚀 Option 2: Manual Commands

Open **PowerShell** as Administrator:

```powershell
# Step 1: Navigate to project
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"

# Step 2: Stop old processes
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue

# Step 3: Delete old database
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force -ErrorAction SilentlyContinue

# Step 4: Clean build
dotnet clean
dotnet build

# Step 5: Start server
dotnet run
```

**You should see:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5000
```

---

## 📱 Test #1: Verify Server Running

Open **new PowerShell** window and run:

```powershell
# Check if POIs are seeded
Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/all" | ForEach-Object { $_.Content | ConvertFrom-Json }
```

**Expected output:**
```
totalCount: 5
pois: [
  { id: 1, name: "Ốc Oanh", qrCode: "http://172.20.10.2:5000/qr/POI_..." },
  { id: 2, name: "Ốc Vũ", qrCode: "http://172.20.10.2:5000/qr/POI_..." },
  ...
]
```

---

## 📱 Test #2: View Admin Dashboard

Open browser and visit:

```
http://172.20.10.2:5000
```

**You should see:**
- ✅ Admin dashboard loads
- ✅ "POIs" tab available
- ✅ Table shows 5 restaurants
- ✅ Each has "View QR" button

---

## 📱 Test #3: View QR Codes

1. Click **"View QR"** button for any restaurant
2. Modal should appear with:
   - QR Code image (clear & scannable)
   - QR URL below image

**QR URL should look like:**
```
http://172.20.10.2:5000/qr/POI_ABC123DEF
```

✅ **If you see full URL → FIX IS WORKING!**

---

## 📱 Test #4: Test on Phone (Real Test)

### Using Phone Camera:
1. Open phone camera
2. Point at QR code on PC screen
3. Wait for recognition
4. Tap the notification that appears
5. Browser should open → restaurant info page

### If it works:
```
✅ Restaurant name appears
✅ Address displays
✅ Image shows
✅ Audio button available (maybe)
```

### If it doesn't work:
```
❌ "Restaurant not found" error
└─ Check: /api/pois/debug/all returns data
└─ Solution: Restart server & delete old database

❌ Can't reach server
└─ Check: Phone on same WiFi as PC
└─ Solution: Connect to same network
```

---

## 🔗 Alternative Test Methods

### Test with URL directly (No phone needed):

```
Copy this from QR modal:
http://172.20.10.2:5000/qr/POI_ABC123

Paste in browser address bar:
✅ Should redirect to restaurant info page
```

### Test with API:

```powershell
# Replace POI_ABC123 with actual code from debug/all
curl "http://172.20.10.2:5000/api/pois/qr/POI_ABC123"

# Expected: Restaurant data in JSON
```

---

## ✅ Checklist - Everything Working?

- [ ] Server running (port 5000)
- [ ] Admin dashboard opens
- [ ] 5 POIs show in table
- [ ] QR codes display in modal
- [ ] QR code URLs show full path
- [ ] /api/pois/debug/all returns 5 POIs
- [ ] URL http://172.20.10.2:5000/qr/POI_... works
- [ ] Phone scan opens restaurant page

**All checked?** ✅ **SUCCESS!**

---

## 🐛 If Something Fails

### Error: Server won't start
```powershell
# Port in use?
netstat -ano | findstr :5000

# Kill and retry
taskkill /PID <number> /F
dotnet run
```

### Error: 0 POIs
```powershell
# Force reseed
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force
# Restart server
```

### Error: "Restaurant not found"
```
Check database has data:
http://172.20.10.2:5000/api/pois/debug/all

If 0 POIs → delete database & restart
If shows POIs → check QRCode format (should have http://)
```

### Error: Phone can't reach server
```
- Are you on same WiFi? (Both PC and phone)
- Is firewall blocking? 
  netsh advfirewall firewall add rule name="Allow 5000" dir=in action=allow protocol=tcp localport=5000
```

---

## 📞 Need Help?

See detailed guides:
- `QR_CODE_FIX_COMPLETE_GUIDE.md` - Full setup guide
- `QR_TESTING_GUIDE_COMPLETE.md` - Detailed testing
- `QR_CODE_CHANGES_DETAILED.md` - Technical details

---

## 🎯 Final Goal

```
When you scan QR code on phone:
1. Camera recognizes it
2. Opens browser automatically  
3. Shows restaurant info
4. ✅ DONE!

No manual copy/paste needed! 🎉
```

---

**Ready? Run the automated script:**

```powershell
cd C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb
.\restart-and-test-qr.ps1
```

**Let's go! 🚀**
