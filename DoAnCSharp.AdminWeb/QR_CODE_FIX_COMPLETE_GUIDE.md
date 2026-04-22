# 🎯 QR CODE SCANNING FIX - HƯỚNG DẪN HOÀN CHỈNH

## ✅ Những gì đã được sửa

### 1. **GenerateQRCode() - Nhúng Full URL**
   - **Trước**: Chỉ trả về code (POI_ABC123)
   - **Sau**: Trả về full URL (http://172.20.10.2:5000/qr/POI_ABC123)
   - **Tác dụng**: Điện thoại quét được URL đầy đủ → mở ngay được trang

### 2. **GetQRImageUrl() - Hỗ trợ cả 2 format**
   - Nếu QRCode là full URL → dùng trực tiếp
   - Nếu QRCode là code only → thêm prefix http://...
   - **Tác dụng**: QR image generator hoạt động với cả 2 format

### 3. **DatabaseService.SeedSampleDataAsync() - Seed với Full URL**
   - POIs được tạo với full URL trong QRCode field
   - **Tác dụng**: Database đã có data sẵn khi server khởi động

### 4. **GetPOIByQRCodeAsync() - Tìm kiếm linh hoạt**
   - Tìm exact match trước (full URL)
   - Nếu không tìm thấy → tìm substring (code portion)
   - **Tác dụng**: Tương thích với cả format cũ và mới

---

## 🚀 Hướng dẫn Khởi Động Server

### Cách 1️⃣: Dùng PowerShell Script (ĐƠNGIẢN NHẤT)
```powershell
cd C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb
.\restart-and-test-qr.ps1
```

**Script sẽ tự động:**
- ✅ Dừng server cũ
- ✅ Xóa database cũ
- ✅ Build project
- ✅ Khởi động server
- ✅ Kiểm tra POIs đã được seed
- ✅ Hiển thị danh sách quán ăn với QR codes

---

### Cách 2️⃣: Chạy thủ công (Nếu script bị lỗi)

Mở PowerShell ở folder project:
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"

# 1. Dừng server cũ
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue

# 2. Xóa database cũ
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force -ErrorAction SilentlyContinue

# 3. Clean build
dotnet clean
dotnet build

# 4. Khởi động server
dotnet run
```

---

## 📱 Hướng dẫn Test QR Code Scanning

### Step 1: Mở Admin Dashboard
```
Trên máy tính: http://172.20.10.2:5000
Hoặc localhost: http://localhost:5000
```

### Step 2: Xem Mã QR
- Click vào tab **"POIs"**
- Bạn sẽ thấy danh sách 5 quán ăn mẫu:
  - Ốc Oanh
  - Ốc Vũ
  - Ốc Nho
  - Quán Nướng Chilli
  - Lẩu Bò Khu Nhà Cháy

### Step 3: Xem/Quét QR Code
- Click nút **"Generate QR"** hoặc **"View QR"** cho mỗi quán
- Modal sẽ hiển thị:
  - 📱 Hình ảnh QR Code
  - 🔗 Đường dẫn: http://172.20.10.2:5000/qr/POI_XXXXX

### Step 4: Test Quét QR
**Cách A - Bằng điện thoại thật:**
- Mở camera ứng dụng trên điện thoại
- Quét QR code
- Nó sẽ tự động mở trình duyệt → hiển thị thông tin quán ăn

**Cách B - Bằng QR Scanner online:**
- Truy cập: https://www.qr-code-generator.com/qr-code-scanner/
- Upload hình QR hoặc quét từ màn hình
- Nó sẽ đọc được URL

**Cách C - Test URL trực tiếp:**
- Copy đường dẫn từ modal
- Paste vào browser: http://172.20.10.2:5000/qr/POI_XXXXX
- Nó sẽ redirect tới trang thông tin quán

---

## 🎯 Kết quả Kỳ Vọng

### ✅ Nếu mọi thứ hoạt động tốt:
1. **Admin Dashboard**: Hiển thị 5 POIs
2. **Mã QR**: Có hình ảnh rõ ràng
3. **Quét/Paste URL**: Mở trang thông tin quán ăn
4. **Trang POI Public**: Hiển thị:
   - 📍 Tên quán ăn
   - 📮 Địa chỉ
   - 📝 Mô tả
   - 🎵 Nút phát audio (nếu có)
   - 🖼️ Hình ảnh

### ❌ Nếu có lỗi:

**Lỗi: "Quán ăn không tìm thấy"**
- ✅ Kiểm tra database có POIs không: `http://172.20.10.2:5000/api/pois/debug/all`
- ✅ Kiểm tra QR code format: phải có "POI_" prefix
- ✅ Restart server: `Stop-Process -Name dotnet` rồi `dotnet run`

**Lỗi: Server không khởi động**
- ✅ Kiểm tra port 5000 có bị chiếm không: `netstat -ano | findstr :5000`
- ✅ Kill process chiếm port: `taskkill /PID <PID> /F`
- ✅ Chạy: `dotnet run`

**Lỗi: QR code không quét được**
- ✅ Kiểm tra QR code có chứa full URL không
- ✅ Debug: Xem network tab DevTools để kiểm tra request
- ✅ Database field QRCode phải là format: `http://172.20.10.2:5000/qr/POI_XXXXX`

---

## 📊 Cấu trúc Database

```
AudioPOI Table:
├── Id (int)
├── Name (string) - Tên quán
├── Address (string) - Địa chỉ
├── Description (string) - Mô tả
├── QRCode (string) - FULL URL: http://172.20.10.2:5000/qr/POI_XXXXX  ← NEW FORMAT!
├── ImageAsset (string)
└── ...
```

---

## 🔄 Luồng Quét QR (Full Flow)

```
1. Admin tạo/xem POI
   └─> Bấm "Generate QR"
   └─> POIsController.GenerateQRCode()
       └─> Trả về: "http://172.20.10.2:5000/qr/POI_ABC123"
       └─> Lưu vào database

2. Hiển thị QR Image
   └─> POIsController.GetQRImageUrl()
   └─> Gọi QR API: https://api.qrserver.com/v1/create-qr-code/?data=http://172.20.10.2:5000/qr/POI_ABC123
   └─> Hiển thị hình ảnh QR

3. Điện thoại quét QR
   └─> Camera đọc URL: http://172.20.10.2:5000/qr/POI_ABC123
   └─> Mở trình duyệt → tới URL này

4. Server nhận request
   └─> Route: GET /qr/POI_ABC123
   └─> QRScansController.QuickScanQR(code="POI_ABC123")
   └─> Gọi: GetPOIByQRCodeAsync("POI_ABC123")
       └─> Tìm trong database (match substring)
       └─> Trả về POI object

5. Redirect tới trang Public
   └─> Redirect: /poi-public.html?poiId=1&deviceId=...
   └─> poi-public.html load dữ liệu
   └─> Hiển thị thông tin quán ăn ✅
```

---

## 🛠️ Files Đã Sửa

| File | Sửa gì | Dòng |
|------|--------|------|
| **POIsController.cs** | GenerateQRCode() - Embed full URL | 130-145 |
| **POIsController.cs** | GetQRImageUrl() - Hỗ trợ cả 2 format | 149-165 |
| **DatabaseService.cs** | SeedSampleDataAsync() - Seed full URLs | 740-757 |
| **DatabaseService.cs** | GetPOIByQRCodeAsync() - Tìm kiếm linh hoạt | 78-97 |

---

## 💡 Tips

- **Public URL**: Chỉnh ở appsettings.Development.json → `ServerSettings:PublicUrl`
- **Port**: Server chạy trên port 5000, bind đến 0.0.0.0 để các thiết bị khác truy cập được
- **Network**: Đảm bảo điện thoại cùng WiFi với máy tính (hoặc cùng network)
- **Testing**: Dùng điện thoại hoặc máy tính khác để test URL: http://172.20.10.2:5000

---

## ✨ Kết quả Cuối Cùng

Khi mọi thứ hoạt động:
```
📱 Điện thoại quét QR
   ↓
🌐 Mở trang: http://172.20.10.2:5000/qr/POI_ABC123
   ↓
🏪 Hiển thị thông tin quán ăn:
   • Tên quán
   • Địa chỉ
   • Mô tả
   • Hình ảnh
   • Audio guide (nếu có)
   ✅ SUCCESS!
```

**Enjoy QR Code Scanning! 🚀**
