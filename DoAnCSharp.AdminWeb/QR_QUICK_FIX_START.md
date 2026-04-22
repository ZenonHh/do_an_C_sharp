# ⚡ QUICK START - QR Code Fix (5 phút)

## ✅ Code changes: DONE ✓

Build successful, all changes applied.

---

## 📋 CÁC BƯỚC TIẾP THEO

### **1️⃣ STOP SERVER**
```powershell
# Bấm Ctrl+C nếu đang chạy trong Visual Studio Terminal
# Hoặc từ PowerShell:
taskkill /IM dotnet.exe /F
```

### **2️⃣ DELETE OLD DATABASE**
```powershell
# Open PowerShell as Admin
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force -ErrorAction SilentlyContinue

# Verify it's deleted
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Write-Host "❌ Database still exists"
} else {
    Write-Host "✅ Database deleted"
}
```

### **3️⃣ START SERVER AGAIN**
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet run

# Wait for: "Now listening on: http://localhost:5000"
```

### **4️⃣ CREATE NEW POI**
```
Browser: http://192.168.0.125:5000
Tab: 🏪 Quán Ăn
Click: ➕ Thêm Quán Ăn Mới

Fill in:
- Tên: "Test Restaurant"
- Địa Chỉ: "123 Test Street"
- Vĩ độ: 10.7595
- Kinh độ: 106.7045

Click: 🔄 Tạo (to generate QR)
Click: ✅ Thêm Quán (to add)
```

### **5️⃣ SCAN QR CODE**
```
Phone:
1. Open camera or QR scanner
2. Scan QR code from browser
3. Should see: Restaurant info page ✅
```

---

## 🐛 IF STILL ERROR

### **Check console log:**
```
Safari on iPhone:
1. Swipe from bottom or from corner
2. Find Developer Tools
3. Tab: Console
4. Look for red errors
```

### **Check server log:**
```powershell
# In the terminal where server is running
# Look for lines starting with [QR]

[QR] Received code: POI_XXXXX
[QR] Looking up POI with code: POI_XXXXX
[QR] POI found: 1 - Test Restaurant  ✅
[QR] Redirecting to: /poi-public.html?poiId=1
```

### **Verify endpoint works:**
```
Phone browser:
http://192.168.0.125:5000/api/pois

Should see: JSON list of POIs
```

---

## 📊 Summary

| Step | Action | Status |
|------|--------|--------|
| 1 | Code changes | ✅ Done |
| 2 | Build | ✅ Done |
| 3 | Stop server | ⏳ TODO |
| 4 | Delete database | ⏳ TODO |
| 5 | Restart server | ⏳ TODO |
| 6 | Create POI | ⏳ TODO |
| 7 | Test QR scan | ⏳ TODO |

---

## 🎯 Expected Result

**After completing all steps:**
- ✅ Quét QR → Safari opens
- ✅ Page loads (not blank)
- ✅ Restaurant info displays
- ✅ Images, audio, downloads visible
- ✅ No "couldn't connect" error

---

## 📞 Problem?

If still getting blank page or error:
1. Check server console for `[QR]` logs
2. Check Safari console (DevTools) for JS errors  
3. Verify database was deleted
4. Verify POI was created with correct data
5. Try creating fresh POI (don't use old QR codes)

Let me know the exact error! 👍
