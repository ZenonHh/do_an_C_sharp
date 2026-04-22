# 🔧 QR Code IP Address Fix - Implementation Guide

## ✅ CHANGES APPLIED

### 1. **Configuration Files Updated**
   - `appsettings.Development.json` - Added `ServerSettings:PublicUrl = http://192.168.1.100:5000`
   - `appsettings.json` - Added template with `ServerSettings:PublicUrl = http://localhost:5000`

### 2. **Backend Changes (POIsController.cs)**
   - ✅ Added `IConfiguration` dependency injection
   - ✅ Updated `GenerateQRCode()` to read from configuration: `PublicUrl`
   - ✅ Added new endpoint: `GET /api/pois/config/server` to return server URL

### 3. **Frontend Changes (index.html)**
   - ✅ Updated `generateQRCode()` function to:
     - Fetch server URL from `/api/pois/config/server`
     - Use server URL instead of window.location.host
     - Fallback to window.location if API call fails

## 🚀 HOW IT WORKS NOW

**Before:**
```
QR Code contains: http://localhost:5000/qr/POI_ABC123
❌ Phone cannot connect (localhost not resolvable)
```

**After:**
```
QR Code contains: http://192.168.1.100:5000/qr/POI_ABC123
✅ Phone can connect and scan (using actual IP)
```

## 🔑 CONFIGURATION

### For Development (Local Network Testing)
Edit `appsettings.Development.json`:
```json
{
  "ServerSettings": {
    "PublicUrl": "http://192.168.1.100:5000"
  }
}
```
Replace `192.168.1.100` with your actual server IP address.

### For Production
Edit `appsettings.json`:
```json
{
  "ServerSettings": {
    "PublicUrl": "http://yourdomain.com"
  }
}
```

## 📋 TESTING CHECKLIST

- [ ] Start the server (dotnet run)
- [ ] Open admin dashboard in browser
- [ ] Create a new POI (or click "Tạo" button to generate QR)
- [ ] Verify QR code contains your IP (not localhost)
  - Check by viewing QR code modal: "📱 Mã QR Code"
  - The text should show: `http://192.168.1.100:5000/qr/POI_XXXXX`
- [ ] Scan QR with phone on same network
- [ ] Verify Safari/Chrome opens without "couldn't connect" error
- [ ] Verify poi-public.html loads and shows POI info

## 🎯 WHAT TO CHANGE

**Find your server IP address:**
```powershell
# Windows PowerShell
ipconfig

# Look for "IPv4 Address" (usually 192.168.x.x or 10.x.x.x)
# Example: 192.168.1.100
```

**Update the IP in appsettings.Development.json:**
```json
"PublicUrl": "http://YOUR_IP_HERE:5000"
```

## ⚠️ COMMON ISSUES

**Problem:** "Page can't load" when scanning QR
- **Cause:** IP address in appsettings.json is wrong
- **Fix:** Update to correct IP address and restart server

**Problem:** QR shows localhost instead of IP
- **Cause:** Configuration not updated or server not restarted
- **Fix:** Check appsettings.Development.json and restart the server

**Problem:** Works on development but not production
- **Cause:** Using development config in production
- **Fix:** Update appsettings.json for production environment

## 📱 TESTING REMOTELY

To test from a phone on the same WiFi network:

1. Get server IP (usually starts with 192.168.x.x or 10.x.x.x)
2. Update `appsettings.Development.json` with this IP
3. Restart server with `dotnet run`
4. Create new POI to generate QR with IP
5. Scan QR from phone camera
6. Should see: Restaurant information page (poi-public.html)

## 🔗 API ENDPOINTS

- `GET /api/pois/config/server` - Returns current server URL
  ```json
  { "serverUrl": "http://192.168.1.100:5000" }
  ```

## ✅ BUILD STATUS
✅ Build successful (0 errors, 0 warnings)

All changes are backward compatible and won't affect existing functionality.
