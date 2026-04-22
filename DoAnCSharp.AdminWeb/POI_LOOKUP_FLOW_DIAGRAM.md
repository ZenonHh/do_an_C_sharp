# 🔄 Flow Diagram: POI Lookup & QR Scan

## Current State (BEFORE Fix)
```
User scans QR code
       ↓
Server receives: /qr/POI_6ECD45E665
       ↓
TrackDeviceInfoAsync() ✅ Works!
       ↓
GetPOIByQRCodeAsync() ❌ FAILS - No POIs in database!
       ↓
Returns: "Quán không tìm thấy"
```

---

## Fixed State (AFTER Fix)
```
1️⃣ Delete old database
       ↓
2️⃣ Rebuild project
       ↓
3️⃣ Start server
       ↓
4️⃣ SeedSampleDataAsync() runs
       ↓
   Creates 5 POIs:
   - Ốc Oanh
   - Ốc Vũ
   - Ốc Nho
   - Quán Nướng Chilli
   - Lẩu Bò Khu Nhà Cháy
       ↓
5️⃣ User scans QR code
       ↓
6️⃣ Server receives: /qr/POI_6ECD45E665
       ↓
7️⃣ TrackDeviceInfoAsync() ✅ Capture device
       ↓
8️⃣ GetPOIByQRCodeAsync() ✅ Found POI!
       ↓
9️⃣ Redirect to: /poi-public.html?poiId=1
       ↓
🔟 Display restaurant info ✅
```

---

## What The Fix Script Does

```powershell
Step 1: Kill processes
   Get-Process "DoAnCSharp.AdminWeb" | Stop-Process

Step 2: Delete old database
   Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"

Step 3: Clean build
   dotnet clean
   Remove-Item bin -Recurse
   Remove-Item obj -Recurse

Step 4: Rebuild
   dotnet build

Step 5: Start with fresh seeding
   dotnet run
      ↓
      Creates new database
      ↓
      Runs SeedSampleDataAsync()
      ↓
      Inserts 5 POIs
```

---

## POI Seeding Process (In Program.cs)

```csharp
// In Program.cs during startup:
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    
    // 1. Initialize database (create tables)
    await dbService.InitAsync();
    
    // 2. Seed sample data (insert POIs)
    await dbService.SeedSampleDataAsync();
}

// This happens BEFORE server starts listening!
```

---

## POI Lookup Logic (In DatabaseService.cs)

```csharp
public async Task<AudioPOI?> GetPOIByQRCodeAsync(string qrCode)
{
    // 1. Try exact match
    var poi = await _connection.Table<AudioPOI>()
        .Where(p => p.QRCode == qrCode)
        .FirstOrDefaultAsync();
    
    // 2. If not found and it's not a URL, try substring match
    if (poi == null && !qrCode.StartsWith("http"))
    {
        poi = await _connection.Table<AudioPOI>()
            .Where(p => p.QRCode.Contains(qrCode))
            .FirstOrDefaultAsync();
    }
    
    return poi;
}
```

**Handles both formats:**
- Full URL: `http://172.20.10.2:5000/qr/POI_6ECD45E665`
- Code only: `POI_6ECD45E665`

---

## Device Tracking + POI Lookup (Together)

```
QR Scan Flow:
   ↓
QuickScanQR(code) in QRScansController
   ↓
┌─────────────────────────────────────┐
│ Step 1: Track Device                │
│ await TrackDeviceInfoAsync()        │
│ - Get IP address                    │
│ - Get User-Agent                    │
│ - Detect device type (iOS/Android)  │
│ - Save to UserDevice table ✅       │
└─────────────────────────────────────┘
   ↓
┌─────────────────────────────────────┐
│ Step 2: Look Up POI                 │
│ await GetPOIByQRCodeAsync(code)     │
│ - Search AudioPOI table             │
│ - Match by QR code                  │
│ - Return POI info ✅                │
└─────────────────────────────────────┘
   ↓
┌─────────────────────────────────────┐
│ Step 3: Redirect                    │
│ /poi-public.html?poiId={poi.Id}    │
│ - Show restaurant info              │
│ - Display device in online list ✅  │
└─────────────────────────────────────┘
```

---

## Expected Database State After Fix

```
VinhKhanhTour_Full.db3 (Fresh)
│
├─ AudioPOI Table
│  ├─ ID: 1, Name: "Ốc Oanh", QRCode: "http://...POI_XXXXX"
│  ├─ ID: 2, Name: "Ốc Vũ", QRCode: "http://...POI_XXXXX"
│  ├─ ID: 3, Name: "Ốc Nho", QRCode: "http://...POI_XXXXX"
│  ├─ ID: 4, Name: "Quán Nướng Chilli", QRCode: "http://...POI_XXXXX"
│  └─ ID: 5, Name: "Lẩu Bò Khu Nhà Cháy", QRCode: "http://...POI_XXXXX"
│
├─ UserDevice Table
│  ├─ [Device from iPhone scan]
│  ├─ [Device from Android scan]
│  └─ [More devices as they scan...]
│
└─ Other tables (Users, Payments, etc.)
```

---

## Why This Error Happened

```
Likely Scenario:

1. Device tracking code was added
   (TrackDeviceInfoAsync, etc.)

2. Server was restarted

3. Database existed from before
   (empty or missing POIs)

4. SeedSampleDataAsync() didn't run
   OR ran but failed silently

5. QR scan tries to find POI
   But none exist in database!

6. Error: "Quán không tìm thấy"
```

---

## Why The Fix Works

```
1. Delete old database
   → Force fresh creation

2. Clean build files
   → Remove any cached state

3. Rebuild project
   → Ensure latest code

4. Start server
   → InitAsync() creates tables
   → SeedSampleDataAsync() inserts POIs
   → Database now has 5 POIs!

5. QR scan now works
   → POIs exist in database
   → Lookup succeeds
   → Shows restaurant info ✅
```

---

## Summary

| Aspect | Before | After |
|--------|--------|-------|
| Database | ❌ Empty or stale | ✅ Fresh with 5 POIs |
| Device Tracking | ✅ Works | ✅ Still works |
| QR Lookup | ❌ Fails | ✅ Success |
| Online Count | ✅ Updates | ✅ Still updates |
| Result | ❌ "Quán không tìm thấy" | ✅ Shows restaurant |

---

**Ready to run the fix? 🚀**
