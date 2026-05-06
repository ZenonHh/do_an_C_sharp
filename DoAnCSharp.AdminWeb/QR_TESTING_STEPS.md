# 🎯 Quick Start - QR Code Mobile Scanning Fix

## Step 1: Find Your Server IP Address

### On Windows:
```powershell
# Open PowerShell and run:
ipconfig

# Look for "IPv4 Address" under your network adapter
# You should see something like: 192.168.1.100 or 10.0.0.5
```

### On Mac/Linux:
```bash
# Find local IP
ifconfig

# Look for "inet" address (usually 192.168.x.x or 10.x.x.x)
```

**Example output:**
```
Ethernet adapter Ethernet:
   IPv4 Address. . . . . . . . . . : 192.168.1.100
```

## Step 2: Update Configuration

1. Open file: `DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb/appsettings.Development.json`

2. Replace the IP address (replace `192.168.1.100` with YOUR IP):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ServerSettings": {
    "PublicUrl": "http://192.168.1.100:5000"
  }
}
```

## Step 3: Restart the Server

```powershell
# Stop any running server (Ctrl+C if running)
# Then start fresh:
cd DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb
dotnet run
```

Wait for the message:
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

## Step 4: Test in Browser

1. Open browser on your **computer**
2. Visit: `http://YOUR_IP:5000` (replace YOUR_IP with your IP)
   - Example: `http://192.168.1.100:5000`
3. Go to "🏪 Quán Ăn" (Restaurants) tab
4. Click "➕ Thêm Quán Ăn Mới" (Add New Restaurant)
5. Fill in restaurant details (Name, Address, Coordinates required)
6. Click "🔄 Tạo" button to generate QR code
7. Click "👁️ Xem QR" to view the QR code
8. **CHECK:** The QR code text should show your IP, NOT "localhost"
   ```
   Expected: http://192.168.1.100:5000/qr/POI_XXXXXXXX
   Wrong:    http://localhost:5000/qr/POI_XXXXXXXX
   ```

## Step 5: Test on Phone

### Requirements:
- Phone must be on **SAME WiFi NETWORK** as your computer
- Your phone's browser (Safari, Chrome)
- Camera app to scan QR code

### Scan QR Code:
1. Open phone camera or QR scanner app
2. Point at QR code on your screen
3. Tap the notification or link that appears
4. **Should see:** Restaurant information page with gallery, audio, downloads
5. **Should NOT see:** "Safari can't open the page" error

## Troubleshooting

### ❌ "Safari can't open the page" Error
**Problem:** QR still contains `localhost` instead of IP

**Solutions:**
1. Verify IP in `appsettings.Development.json` is correct
2. Restart the server (Ctrl+C, then `dotnet run`)
3. Create a NEW POI to generate QR with new configuration
4. Check QR text shows IP not localhost

### ❌ "Connection timeout" on Phone
**Problem:** IP address is incorrect or phone not on same network

**Solutions:**
1. Verify IP with `ipconfig` command again
2. Check phone is connected to same WiFi as computer
3. Try pinging server from phone: open Safari and visit `http://192.168.1.100:5000`
4. Should see admin dashboard

### ❌ QR Code not generating
**Problem:** Server configuration not loaded

**Solutions:**
1. Check console for error messages
2. Verify `appsettings.Development.json` syntax is valid (proper JSON)
3. Restart server
4. Clear browser cache and refresh page

### ⚠️ Works on Computer but Not on Phone
**Problem:** Phone not on same network

**Solutions:**
1. Ensure phone WiFi connected to same router as computer
2. Some networks block device-to-device communication (guest WiFi)
3. Try connecting to main WiFi, not guest network
4. Check firewall isn't blocking port 5000

## Verification Commands

### Check if server is accessible from phone:
On computer, run:
```powershell
# This shows your IP
ipconfig | findstr "IPv4"

# Test from another machine on network:
# From phone's Safari: http://192.168.1.100:5000
# Should load the admin dashboard
```

### Check QR code content:
In browser console (F12):
```javascript
// This should show your server URL
console.log(API_BASE);

// Or visit this endpoint to see config:
// http://192.168.1.100:5000/api/pois/config/server
// Should show: {"serverUrl":"http://192.168.1.100:5000"}
```

## Success Criteria ✅

- [ ] Server IP found and configured in appsettings.json
- [ ] Server started with `dotnet run`
- [ ] QR code text shows IP address (not localhost)
- [ ] Phone can access `http://YOUR_IP:5000` from Safari
- [ ] Scanning QR code opens restaurant info page
- [ ] No "couldn't connect" errors

## Next Steps After Success

1. Once verified working, proceed to test other features
2. Fix any remaining header mobile rendering issues
3. Document any network requirements for deployment

---

**Questions?** Check the detailed guide: `QR_IP_FIX_GUIDE.md`
