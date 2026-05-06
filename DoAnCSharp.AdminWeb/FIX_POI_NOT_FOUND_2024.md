# 🔧 Fix "Quán Không Tìm Thấy" Error - Complete Guide

## 🎯 Problem
After updating device tracking features, QR scans show "Quán không tìm thấy" (Restaurant not found) error.

## 🔍 Root Causes
1. ✅ POIs not seeded in database
2. ✅ QR code format mismatch
3. ✅ Database not initialized properly
4. ✅ Old database file has stale data

## ✅ Solution - Step by Step

### Step 1: Delete Old Database (Fresh Start)

```powershell
# Close all PowerShell windows and Visual Studio first!

# Delete old database
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Old database deleted: $dbPath"
} else {
    Write-Host "ℹ️ No old database found"
}

# Also delete from project bin/obj folders
$projPath = "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
if (Test-Path "$projPath\bin") {
    Remove-Item "$projPath\bin" -Recurse -Force
    Write-Host "✅ Cleaned bin folder"
}
if (Test-Path "$projPath\obj") {
    Remove-Item "$projPath\obj" -Recurse -Force
    Write-Host "✅ Cleaned obj folder"
}

Write-Host ""
Write-Host "✅ All old data cleaned. Ready for fresh build."
```

### Step 2: Clean Build

```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"

# Kill any running processes
Get-Process "DoAnCSharp.AdminWeb" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# Clean and build
dotnet clean
dotnet build
```

**Expected Output:**
```
Build succeeded. 0 Warning(s)
```

### Step 3: Start Server & Verify POIs Are Seeded

```powershell
# Start the server
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5000
```

### Step 4: Verify POIs in Database (NEW PowerShell window)

```powershell
# Get all POIs
Write-Host "Checking POIs in database..."
$r = Invoke-WebRequest "http://172.20.10.2:5000/api/pois" -ErrorAction SilentlyContinue
$pois = $r.Content | ConvertFrom-Json

if ($pois -and $pois.Count -gt 0) {
    Write-Host "✅ Found $($pois.Count) POIs:"
    $pois | ForEach-Object {
        Write-Host "  📍 Name: $($_.name)"
        Write-Host "     QRCode: $($_.qrCode)"
        Write-Host "     ID: $($_.id)"
        Write-Host ""
    }
} else {
    Write-Host "❌ No POIs found! Seeding may have failed."
    Write-Host ""
    Write-Host "Try these fixes:"
    Write-Host "1. Restart the server"
    Write-Host "2. Delete database and rebuild"
    Write-Host "3. Check if SeedSampleDataAsync is being called in Program.cs"
}
```

### Step 5: Test QR Scan

```powershell
# Get the first POI's QR code
$r = Invoke-WebRequest "http://172.20.10.2:5000/api/pois" -ErrorAction SilentlyContinue
$poi = ($r.Content | ConvertFrom-Json)[0]

if ($poi) {
    $qrCode = $poi.qrCode
    Write-Host "Testing QR scan with code: $qrCode"
    Write-Host ""
    
    # Extract just the code part if it's a full URL
    if ($qrCode -match 'POI_[A-Z0-9]+') {
        $codeMatch = $matches[0]
        Write-Host "Extracted code: $codeMatch"
        
        # Test endpoint
        $testUrl = "http://172.20.10.2:5000/qr/$codeMatch"
        Write-Host "Testing URL: $testUrl"
        Write-Host ""
        
        try {
            $response = Invoke-WebRequest $testUrl -MaximumRedirection 0 -ErrorAction SilentlyContinue
            Write-Host "Response Status: $($response.StatusCode)"
            Write-Host "Redirect Location: $($response.Headers.Location)"
        } catch {
            Write-Host "Error: $($_.Exception.Message)"
        }
    }
}
```

---

## 🐛 If Still Getting "Quán Không Tìm Thấy"

### Check 1: Verify Database Initialization

Add this to check logs:

```powershell
# Check if server logs show seeding
# Look in the server console output for:
# "Seeding..." messages or error logs

# In server window, you should see:
# - "Creating tables..."
# - "Seeding sample data..."
# - POI records being inserted
```

### Check 2: Verify GetPOIByQRCodeAsync Logic

The method in DatabaseService.cs does:
1. Try exact match: `p.QRCode == qrCode`
2. If not found, try substring match: `p.QRCode.Contains(qrCode)`

This should work for both:
- Full URL: `http://172.20.10.2:5000/qr/POI_6ECD45E665`
- Code only: `POI_6ECD45E665`

### Check 3: Verify QRCode Format in Database

```powershell
# Query the exact QRCode values
$r = Invoke-WebRequest "http://172.20.10.2:5000/api/pois" -ErrorAction SilentlyContinue
$pois = $r.Content | ConvertFrom-Json

Write-Host "QR Code Formats in Database:"
$pois | ForEach-Object {
    Write-Host "  $($_.qrCode)"
}

# All should start with:
# - http://172.20.10.2:5000/qr/POI_ (if full URL)
# - OR POI_ (if code only)
```

---

## 🎯 Expected Result After Fix

### ✅ When POIs are properly seeded:

```powershell
✅ Found 5 POIs:
  📍 Name: Ốc Oanh
     QRCode: http://172.20.10.2:5000/qr/POI_XXXXX
     ID: 1

  📍 Name: Ốc Vũ
     QRCode: http://172.20.10.2:5000/qr/POI_XXXXX
     ID: 2

  ... (3 more POIs)
```

### ✅ When QR scan works:

```
1. Scan QR code from phone
2. Browser opens: http://172.20.10.2:5000/qr/POI_XXXXX
3. Server logs: "Quét QR thành công: POI_XXXXX → POI 1"
4. Page shows: Restaurant information page
5. ❌ NOT: "Quán không tìm thấy" error
```

---

## 📋 Troubleshooting Checklist

- [ ] Deleted old database file
- [ ] Cleaned bin/obj folders
- [ ] Ran `dotnet clean`
- [ ] Ran `dotnet build` (succeeded)
- [ ] Started server with `dotnet run`
- [ ] Checked /api/pois endpoint shows 5 POIs
- [ ] All POIs have QR codes starting with "POI_"
- [ ] QR code lookup test succeeds
- [ ] Phone QR scan now shows restaurant info (not error)

---

## 🔍 Debug: Check Server Logs

When you run `dotnet run`, look for these messages:

✅ **Good Signs:**
```
Initializing database...
Creating tables...
Seeding sample data...
Seed complete. 5 POIs added.
Now listening on: http://0.0.0.0:5000
```

❌ **Bad Signs:**
```
Exception in SeedSampleDataAsync
Error creating tables
Connection failed
```

---

## 🚀 Quick Complete Fix (One Command)

Copy and run this (after closing Visual Studio and killing processes):

```powershell
# Complete fix in one go
$dbPath = "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) { Remove-Item $dbPath -Force }

$projPath = "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
if (Test-Path "$projPath\bin") { Remove-Item "$projPath\bin" -Recurse -Force }
if (Test-Path "$projPath\obj") { Remove-Item "$projPath\obj" -Recurse -Force }

cd $projPath
dotnet clean; dotnet build; dotnet run
```

---

## 📞 Still Having Issues?

If the issue persists, provide:
1. Screenshot of server console output
2. Output of `/api/pois` endpoint
3. Exact QR code you're trying to scan
4. Full error message from poi-public.html

Then I can provide more targeted fixes!

---

**Status: Ready to implement ✅**
