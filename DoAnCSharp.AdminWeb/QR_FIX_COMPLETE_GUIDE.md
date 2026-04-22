# 🔧 QR Code Fix - Complete Instructions

## ❌ Vấn đề phát hiện

Database đang lưu **full URL** (ví dụ: `http://192.168.0.125:5000/qr/POI_ABC123`) vào trường QRCode.

Nhưng endpoint `/qr/{code}` chỉ nhận **code part** (ví dụ: `POI_ABC123`).

**Kết quả:** Không thể tìm thấy POI trong database → Redirect lỗi.

---

## ✅ Cách khắc phục

### **Thay đổi được thực hiện:**

1. ✅ **Backend (POIsController.cs)**
   - `GenerateQRCode()` bây giờ chỉ tạo **code** (POI_ABC123)
   - Không còn lưu full URL vào database

2. ✅ **Frontend (index.html)**
   - `generateQRCode()` vẫn tạo full URL cho QR image
   - Nhưng database sẽ chỉ lưu code part

3. ✅ **Endpoint (Program.cs)**
   - `/qr/{code}` bây giờ tìm kiếm chính xác theo code
   - Đơn giản hơn, hiệu quả hơn

4. ✅ **Build:** Thành công (0 errors)

---

## 📋 Các bước cần làm

### **Bước 1: Stop server hiện tại**
```powershell
# Bấm Ctrl+C trong terminal nơi server đang chạy
# Hoặc từ PowerShell khác:
Stop-Process -Name dotnet -Force
```

### **Bước 2: Xóa database cũ**
```powershell
# Xóa database để reset (sẽ tạo mới khi server start)
Remove-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force -ErrorAction SilentlyContinue

# Hoặc backup nó trước:
Rename-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" "VinhKhanhTour_Full.db3.bak" -Force
```

### **Bước 3: Start server lại**
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb"
dotnet run
```

**Hoặc sử dụng script automation:**
```powershell
cd "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb"
.\cleanup-and-restart.ps1
```

### **Bước 4: Tạo POI mới**
1. Mở browser: `http://192.168.0.125:5000`
2. Tab "🏪 Quán Ăn"
3. Click "➕ Thêm Quán Ăn Mới"
4. Điền thông tin (tên, địa chỉ, tọa độ - bắt buộc)
5. Click "🔄 Tạo" để tạo mã QR
6. Thêm quán ăn

**Quan trọng:** POI cũ không hoạt động (vì database reset). Phải tạo mới.

### **Bước 5: Quét QR trên điện thoại**
1. Mở camera hoặc QR scanner
2. Quét mã QR vừa tạo
3. **Kỳ vọng:** Trang tải, hiển thị thông tin quán
4. **Nếu lỗi:** Kiểm tra console log (F12 trên Safari DevTools)

---

## 🧪 Kiểm tra Debug

Nếu vẫn lỗi, hãy kiểm tra:

### **1. Check console log trên Safari (iPhone)**
- Swipe từ bên dưới lên (hoặc từ góc)
- Tìm Developer Tools
- Tab Console
- Tìm dòng nào có màu đỏ (errors)

### **2. Check server log**
Mở terminal nơi server chạy, tìm dòng `[QR]`:
```
[QR] Received code: POI_ABC123DEF
[QR] Looking up POI with code: POI_ABC123DEF
[QR] POI found: 1 - Ốc Oanh
[QR] Redirecting to: /poi-public.html?poiId=1&code=POI_ABC123DEF
```

### **3. Verify database**
```powershell
# Check xem database được tạo không
Get-Item "$env:APPDATA\VinhKhanhTour\VinhKhanhTour_Full.db3" -ErrorAction SilentlyContinue
```

### **4. Test endpoint trực tiếp**
Trên điện thoại, mở Safari:
```
http://192.168.0.125:5000/api/pois
```
Nên thấy danh sách POI dưới dạng JSON.

---

## ⚠️ Thông tin quan trọng

### **Cấu hình IP**
```json
{
  "ServerSettings": {
    "PublicUrl": "http://192.168.0.125:5000"
  }
}
```
Đảm bảo IP này khớp với IP thực tế của server!

### **Thay đổi mạng**
```
1. Cập nhật IPv4 mới trong appsettings.Development.json
2. Restart server
3. Tạo QR code mới (QR cũ có thể không hoạt động)
```

---

## 🎯 Tóm tắt

| Bước | Hành động | Kết quả |
|------|-----------|--------|
| 1 | Stop server | Server dừng |
| 2 | Xóa database | Database reset |
| 3 | Restart server | Server chạy, database tạo mới |
| 4 | Tạo POI mới | QR code tạo với code format mới |
| 5 | Quét QR | Redirect → poi-public.html → Hiển thị |

---

## 📞 Nếu vẫn lỗi

Báo lỗi cụ thể:
- ❌ Lỗi là gì?
- 📱 Hiển thị trên điện thoại gì?
- 🖥️ Server log hiển thị gì?
- 🌐 URL trong address bar là gì?

Tôi sẽ giúp debug!
