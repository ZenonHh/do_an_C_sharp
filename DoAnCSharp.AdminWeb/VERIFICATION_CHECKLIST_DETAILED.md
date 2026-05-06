# ✅ VERIFICATION CHECKLIST - QR CODE FULL URL FIX

## 🔍 Code Changes Verification

### 1. POIsController.cs - Change 1
File: `DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb/Controllers/POIsController.cs`
Lines: 130-145

**Verify:**
- [ ] `GenerateQRCode()` method exists
- [ ] Returns `$"{publicUrl}/qr/{baseCode}"`
- [ ] Uses `_configuration["ServerSettings:PublicUrl"]`
- [ ] Has fallback to `Request.Scheme` + `Request.Host`

**Expected Code:**
```csharp
private string GenerateQRCode()
{
    string baseCode = "POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    string publicUrl = _configuration["ServerSettings:PublicUrl"] ?? 
                      $"{Request.Scheme}://{Request.Host}";
    return $"{publicUrl}/qr/{baseCode}";
}
```

---

### 2. POIsController.cs - Change 2
File: Same file
Lines: 149-165

**Verify:**
- [ ] `GetQRImageUrl()` method exists
- [ ] Checks `qrCode.StartsWith("http")`
- [ ] Uses full URL directly if starts with "http"
- [ ] Adds prefix if code-only

**Expected Code:**
```csharp
private string GetQRImageUrl(string qrCode)
{
    string fullUrl = qrCode.StartsWith("http") 
        ? qrCode 
        : $"{Request.Scheme}://{Request.Host}/qr/{qrCode}";
    var encodedUrl = Uri.EscapeDataString(fullUrl);
    return $"https://api.qrserver.com/v1/create-qr-code/?size=400x400&data={encodedUrl}";
}
```

---

### 3. DatabaseService.cs - Change 1
File: `DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb/Services/DatabaseService.cs`
Lines: 740-757

**Verify:**
- [ ] `SeedSampleDataAsync()` method contains POI seeding
- [ ] Gets `publicUrl` from environment or uses default
- [ ] QRCode field set with `$"{publicUrl}/qr/POI_..."`
- [ ] All 5 POIs use this format

**Expected Code in POI creation:**
```csharp
string publicUrl = Environment.GetEnvironmentVariable("VINHKHANH_PUBLIC_URL") ?? "http://172.20.10.2:5000";
QRCode = $"{publicUrl}/qr/POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
```

---

### 4. DatabaseService.cs - Change 2
File: Same file
Lines: 78-97

**Verify:**
- [ ] `GetPOIByQRCodeAsync()` method exists
- [ ] Tries exact match first
- [ ] Falls back to substring search
- [ ] Checks `!qrCode.StartsWith("http")` before substring

**Expected Code:**
```csharp
public async Task<AudioPOI?> GetPOIByQRCodeAsync(string qrCode)
{
    await InitAsync();
    if (string.IsNullOrWhiteSpace(qrCode))
        return null;

    var poi = await _connection!.Table<AudioPOI>()
        .Where(p => p.QRCode == qrCode)
        .FirstOrDefaultAsync();

    if (poi == null && !qrCode.StartsWith("http"))
    {
        poi = await _connection!.Table<AudioPOI>()
            .Where(p => p.QRCode.Contains(qrCode))
            .FirstOrDefaultAsync();
    }

    return poi;
}
```

---

## 🏗️ Build Verification

**Check:**
- [ ] `dotnet build` succeeds
- [ ] 0 errors
- [ ] 0 warnings
- [ ] Takes ~10-30 seconds

**Run:**
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet build
```

**Expected Output:**
```
Build succeeded. 0 Warning(s)
```

---

## 🗄️ Database Verification

**After Server Start:**

1. Check database file exists:
```powershell
Test-Path "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
# Should return: True
```

2. Check POIs seeded:
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/all"
$data = $response.Content | ConvertFrom-Json
$data.totalCount
# Should return: 5
```

3. Check QRCode format:
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/all"
$data = $response.Content | ConvertFrom-Json
$data.pois[0].qrCode
# Should start with: http://172.20.10.2:5000/qr/POI_
```

---

## 🌐 API Endpoint Verification

### Endpoint 1: Debug All POIs
**URL:** `http://172.20.10.2:5000/api/pois/debug/all`

**Verify:**
- [ ] Returns HTTP 200
- [ ] Returns 5 POIs
- [ ] Each POI has full URL in qrCode field
- [ ] URLs follow pattern: `http://172.20.10.2:5000/qr/POI_XXXXX`

**Expected Response:**
```json
{
  "totalCount": 5,
  "pois": [
    {
      "id": 1,
      "name": "Ốc Oanh",
      "qrCode": "http://172.20.10.2:5000/qr/POI_ABC123",
      "address": "534 Vĩnh Khánh, Q.4",
      "hasAudio": false
    },
    // ... more POIs
  ]
}
```

---

### Endpoint 2: QR Lookup
**URL:** `http://172.20.10.2:5000/api/pois/qr/POI_ABC123`

**Verify:**
- [ ] Replace POI_ABC123 with actual code
- [ ] Returns HTTP 200
- [ ] Returns POI object with matching code

**Expected Response:**
```json
{
  "id": 1,
  "name": "Ốc Oanh",
  "qrCode": "http://172.20.10.2:5000/qr/POI_ABC123",
  "address": "534 Vĩnh Khánh, Q.4",
  // ... more fields
}
```

---

### Endpoint 3: QR Image URL
**URL:** `http://172.20.10.2:5000/api/pois/1/qr-image`

**Verify:**
- [ ] Returns valid QR image URL
- [ ] URL points to api.qrserver.com
- [ ] Contains full URL as data parameter

**Expected Response:**
```
https://api.qrserver.com/v1/create-qr-code/?size=400x400&data=http%3A%2F%2F172.20.10.2%3A5000%2Fqr%2FPOI_ABC123
```

---

### Endpoint 4: QR Redirect
**URL:** `http://172.20.10.2:5000/qr/POI_ABC123`

**Verify:**
- [ ] Returns HTTP 302 redirect
- [ ] Redirects to /poi-public.html
- [ ] Includes poiId query parameter

**Expected Response:**
```
HTTP/1.1 302 Found
Location: /poi-public.html?poiId=1&deviceId=device_...&code=POI_ABC123
```

---

## 📊 UI Verification

### Admin Dashboard
**URL:** `http://172.20.10.2:5000`

**Verify:**
- [ ] Page loads without errors
- [ ] "POIs" tab visible
- [ ] POI table shows 5 restaurants
- [ ] Each row has action buttons

---

### POI List
**In Admin Dashboard → POIs Tab**

**Verify:**
- [ ] All 5 restaurants display:
  - Ốc Oanh
  - Ốc Vũ
  - Ốc Nho
  - Quán Nướng Chilli
  - Lẩu Bò Khu Nhà Cháy
- [ ] Each has "View QR" button
- [ ] Click opens modal

---

### QR Code Modal
**Click "View QR" for any POI**

**Verify:**
- [ ] Modal appears
- [ ] QR code image displays clearly
- [ ] Image is scannable
- [ ] URL text below shows: `http://172.20.10.2:5000/qr/POI_XXXXX`
- [ ] URL is clickable/copyable

---

## 📱 Phone Testing

### Test 1: QR Scan
**Requirements:**
- [ ] Phone on same WiFi as PC
- [ ] Phone camera can scan QR codes

**Steps:**
1. Open admin dashboard on PC
2. Click "View QR" for any restaurant
3. Point phone camera at QR code
4. Wait for recognition
5. Tap notification
6. Browser should open

**Verify:**
- [ ] Browser opens automatically
- [ ] URL in address bar: `http://172.20.10.2:5000/qr/POI_...`
- [ ] Redirects to: `http://172.20.10.2:5000/poi-public.html?poiId=X`
- [ ] No "Restaurant not found" error

---

### Test 2: Restaurant Info Page
**After QR scan redirects**

**Verify:**
- [ ] Page loads without errors
- [ ] Restaurant name displays
- [ ] Address displays
- [ ] Description shows
- [ ] Image displays (if available)
- [ ] Audio button present (if audio available)

---

### Test 3: URL Direct Access
**Copy URL from QR modal**

```
http://172.20.10.2:5000/qr/POI_ABC123
```

**On phone browser:**
1. Paste URL in address bar
2. Press Enter

**Verify:**
- [ ] Redirects to poi-public.html
- [ ] Restaurant info displays
- [ ] Works without scanning

---

## 🐛 Error Checks

### Check: No Build Errors
```powershell
dotnet clean
dotnet build
# Should output: "Build succeeded. 0 Warning(s)"
```
- [ ] Passes

---

### Check: Server Starts
```powershell
dotnet run
# Should output: "Now listening on: http://0.0.0.0:5000"
```
- [ ] Passes

---

### Check: Database Seeds
```powershell
Invoke-WebRequest "http://172.20.10.2:5000/api/pois/debug/all" | ForEach-Object { ($_.Content | ConvertFrom-Json).totalCount }
# Should output: 5
```
- [ ] Passes

---

### Check: QR URLs Format
```powershell
$response = Invoke-WebRequest "http://172.20.10.2:5000/api/pois/debug/all"
($response.Content | ConvertFrom-Json).pois[0].qrCode
# Should start with: http://172.20.10.2:5000/qr/POI_
```
- [ ] Passes

---

### Check: API Responses
```powershell
$response = Invoke-WebRequest "http://172.20.10.2:5000/api/pois/qr/POI_ABC123" -ErrorAction SilentlyContinue
$response.StatusCode
# Should be: 200
```
- [ ] Passes

---

## ✅ Final Checklist

**Code Changes:**
- [ ] 4 changes applied
- [ ] Build succeeds (0 errors)

**Database:**
- [ ] Database file exists
- [ ] 5 POIs seeded
- [ ] QRCode field has full URLs

**APIs:**
- [ ] `/api/pois/debug/all` returns 5 POIs
- [ ] `/api/pois/qr/{code}` returns POI
- [ ] `/qr/{code}` redirects correctly
- [ ] All endpoints return correct data

**UI:**
- [ ] Admin dashboard loads
- [ ] POI table shows 5 restaurants
- [ ] QR modal displays QR image
- [ ] QR URL shows full path

**Phone Testing:**
- [ ] QR scan opens page automatically
- [ ] Restaurant info displays correctly
- [ ] URL direct access works
- [ ] No "Restaurant not found" errors

---

## 📋 Summary

**Total Items:** 50+
**Pass Required:** 100%

**If all checked:** ✅ **FIX COMPLETE & VERIFIED**

---

**Date Checked:** ___________
**Verified By:** ___________
**Status:** ☐ PASS ☐ FAIL

---

*Use this checklist after running the fix to ensure everything is working correctly.*
