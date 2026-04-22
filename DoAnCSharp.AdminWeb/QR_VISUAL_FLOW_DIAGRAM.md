# 🎯 QR SCANNING FLOW - VISUAL GUIDE

## 📊 BEFORE FIX (❌ BROKEN)

```
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: Create POI in Admin Dashboard                       │
└─────────────────────┬───────────────────────────────────────┘
                      │ User fills form & clicks "Thêm Quán"
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 2: Frontend generateQRCode()                           │
│ Generates: fullURL = "http://192.168.0.125:5000/qr/POI_A.." │
└─────────────────────┬───────────────────────────────────────┘
                      │ Sends to backend
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: Backend GenerateQRCode() [OLD - WRONG]             │
│ Returns: "http://192.168.0.125:5000/qr/POI_ABC123"         │
│ ❌ Stores FULL URL in database                             │
└─────────────────────┬───────────────────────────────────────┘
                      │ Saves to DB
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 4: Database                                            │
│ POI {                                                        │
│   Id: 1,                                                     │
│   Name: "Ốc Oanh",                                          │
│   QRCode: "http://192.168.0.125:5000/qr/POI_ABC123"  ❌   │
│ }                                                            │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 5: User scans QR on phone                              │
│ QR image encoded URL: http://192.168.0.125:5000/qr/POI_A.. │
└─────────────────────┬───────────────────────────────────────┘
                      │ Phone opens Safari
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 6: Endpoint /qr/{code}                                 │
│ Receives: code = "POI_ABC123"                               │
│ Searches: WHERE QRCode == "POI_ABC123"                      │
│                                                             │
│ Database has: "http://192.168.0.125:5000/qr/POI_ABC123"    │
│ ❌ NO MATCH!                                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 7: Result                                              │
│ ❌ POI not found                                            │
│ ❌ Redirect: /poi-public.html?error=poi_not_found          │
│ ❌ Blank page or error message                             │
└─────────────────────────────────────────────────────────────┘
```

---

## ✅ AFTER FIX (WORKING)

```
┌─────────────────────────────────────────────────────────────┐
│ STEP 1: Create POI in Admin Dashboard                       │
└─────────────────────┬───────────────────────────────────────┘
                      │ User fills form & clicks "Thêm Quán"
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 2: Frontend generateQRCode() [NEW - FIXED]             │
│ 1. Fetch /api/pois/config/server → serverUrl               │
│ 2. Generate code: "POI_ABC123"                              │
│ 3. Build fullURL: serverUrl + "/qr/" + code                │
│    = "http://192.168.0.125:5000/qr/POI_ABC123"             │
│ 4. Send fullURL to backend                                  │
└─────────────────────┬───────────────────────────────────────┘
                      │ Sends to backend
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 3: Backend GenerateQRCode() [NEW - FIXED]             │
│ Returns: "POI_ABC123"                                       │
│ ✅ Stores ONLY CODE in database                            │
└─────────────────────┬───────────────────────────────────────┘
                      │ Saves to DB
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 4: Database                                            │
│ POI {                                                        │
│   Id: 1,                                                     │
│   Name: "Ốc Oanh",                                          │
│   QRCode: "POI_ABC123"  ✅                                 │
│ }                                                            │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 5: User scans QR on phone                              │
│ QR image encoded URL: http://192.168.0.125:5000/qr/POI_A.. │
└─────────────────────┬───────────────────────────────────────┘
                      │ Phone opens Safari
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 6: Endpoint /qr/{code} [NEW - FIXED]                  │
│ Receives: code = "POI_ABC123"                               │
│ Searches: WHERE QRCode == "POI_ABC123"                      │
│                                                             │
│ Database has: "POI_ABC123"                                  │
│ ✅ MATCH!                                                  │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ STEP 7: Result                                              │
│ ✅ POI found (Id = 1)                                       │
│ ✅ Redirect: /poi-public.html?poiId=1&code=POI_ABC123      │
│ ✅ Page loads with restaurant info                         │
│ ✅ Images, audio, downloads visible                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 DATA FLOW COMPARISON

### **OLD (❌ BROKEN)**
```
Frontend      Database        Endpoint Search   Result
───────────   ────────────    ────────────────  ──────
fullURL   →   fullURL     →   code          →  ❌ NO MATCH
http://...    http://...      POI_ABC123       ERROR
```

### **NEW (✅ FIXED)**
```
Frontend      Database        Endpoint Search   Result
───────────   ────────────    ────────────────  ──────
fullURL   →   code        →   code          →  ✅ MATCH
http://...    POI_ABC123      POI_ABC123       SUCCESS
```

---

## 📱 MOBILE QR SCANNING

### **What User Sees (BEFORE FIX)**

```
Phone Screen (BROKEN):
┌──────────────────────────────────────┐
│ Safari                        < ↻  ⋮  │
├──────────────────────────────────────┤
│ 🌐 192.168.0.125:5000              │
│                                      │
│ ⚠️  Safari can't open the page       │
│     because it couldn't connect      │
│     to the server.                   │
│                                      │
│ [More...]                            │
└──────────────────────────────────────┘
```

### **What User Sees (AFTER FIX)**

```
Phone Screen (WORKING):
┌──────────────────────────────────────┐
│ Safari                        < ↻  ⋮  │
├──────────────────────────────────────┤
│ 🌐 192.168.0.125:5000              │
│                                      │
│ 🍴 ỐC OANH                          │
│ Quán ốc nổi tiếng                   │
│                                      │
│ 📷 [Gallery with images]             │
│                                      │
│ 🎧 [Audio player]                    │
│ Vietnamese: Play Audio               │
│ English: Play Audio                  │
│                                      │
│ ⬇️ [Download buttons]                │
│ [Get iOS App] [Get Android App]      │
└──────────────────────────────────────┘
```

---

## 🔧 CONFIGURATION DIAGRAM

```
┌────────────────────────────────────────────────┐
│ appsettings.Development.json                   │
├────────────────────────────────────────────────┤
│ "ServerSettings": {                            │
│   "PublicUrl": "http://192.168.0.125:5000"    │
│ }                                              │
└────────┬─────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────────────────────┐
│ Frontend (index.html)                          │
│ fetch('/api/pois/config/server')              │
│   → receives: "http://192.168.0.125:5000"    │
│   → builds fullURL for QR encoding            │
└────────┬─────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────────────────────┐
│ QR Code Image (what user scans)                │
│ Contains: http://192.168.0.125:5000/qr/POI_.. │
└────────┬─────────────────────────────────────┘
         │ User scans with phone
         ▼
┌────────────────────────────────────────────────┐
│ Safari on Phone                                │
│ Opens: http://192.168.0.125:5000/qr/POI_ABC123│
└────────┬─────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────────────────────┐
│ Server Endpoint /qr/{code}                    │
│ Receives: POI_ABC123                          │
│ Searches: Database where QRCode == POI_ABC123 │
│ Result: ✅ POI found → Redirect to poi-public │
└────────┬─────────────────────────────────────┘
         │
         ▼
┌────────────────────────────────────────────────┐
│ poi-public.html loaded                        │
│ fetch('/api/pois/1')                          │
│ Displays restaurant info                      │
└────────────────────────────────────────────────┘
```

---

## 📝 CODE FLOW DIAGRAM

```
USER ACTION: Click "🔄 Tạo" in Add POI Form
│
├─→ Frontend: generateQRCode()
│   │
│   ├─→ fetch('/api/pois/config/server')
│   │   └─→ Server returns: { serverUrl: "http://192.168.0.125:5000" }
│   │
│   ├─→ Generate code: "POI_ABC123"
│   │
│   ├─→ Build URL: "http://192.168.0.125:5000/qr/POI_ABC123"
│   │
│   └─→ Set input: document.getElementById("poiQRCode").value = fullURL
│
├─→ User fills other form fields (Name, Address, Coords)
│
├─→ USER ACTION: Click "✅ Thêm Quán" (Submit Form)
│
├─→ Frontend: POST /api/pois with data:
│   {
│     name: "Ốc Oanh",
│     address: "534 Vĩnh Khánh",
│     qrCode: "http://192.168.0.125:5000/qr/POI_ABC123"
│   }
│
├─→ Backend POIsController.Create():
│   │
│   ├─→ If qrCode is empty: GenerateQRCode()
│   │   └─→ Returns: "POI_NEW456" ✅ (only code)
│   │
│   ├─→ Save to database:
│   │   POI { qrCode: "POI_NEW456", ... }
│   │
│   └─→ Response: { id: 1, qrCode: "POI_NEW456", ... }
│
├─→ USER ACTION: Click "👁️ Xem QR" Button
│
├─→ Frontend: Display QR Code Modal
│   │
│   ├─→ QRCode.js encodes: "POI_NEW456" (from DB)
│   │   ✅ But QR image encodes FULL URL (already in input field)
│   │
│   └─→ Show QR image & text
│
└─→ USER ACTION: Open phone camera & scan QR
    │
    └─→ [Continue to mobile scanning flow above]
```

---

## ✅ VERIFICATION CHECKLIST

After applying fix, verify:

```
□ Code changes applied (3 files modified)
□ Build successful (0 errors)
□ Database deleted
□ Server restarted
□ New POI created
□ /api/pois shows QRCode in correct format (code, not URL)
□ QR code visible in browser (visual check)
□ QR endpoint works: /qr/POI_ABC123 redirects
□ poi-public.html loads: /poi-public.html?poiId=1
□ Phone can access server: http://192.168.0.125:5000
□ QR scan works: Safari opens, page loads, info displays
```

---

**All diagrams show the fixed (✅ WORKING) implementation.**
