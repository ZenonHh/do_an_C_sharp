# 🔧 FIX: Quán Ăn Không Tìm Thấy - Chi Tiết Giải Pháp

## ✅ Những gì đã được fix:

### 1. **Added Missing `GetPOIByQRCodeAsync` Method**
- **File**: `DoAnCSharp.AdminWeb/Services/DatabaseService.cs`
- **Issue**: QRScansController gọi method không tồn tại
- **Fix**: Thêm method để search POI bằng QRCode
```csharp
public async Task<AudioPOI?> GetPOIByQRCodeAsync(string qrCode)
{
    await InitAsync();
    if (string.IsNullOrWhiteSpace(qrCode))
        return null;

    return await _connection!.Table<AudioPOI>()
        .Where(p => p.QRCode == qrCode)
        .FirstOrDefaultAsync();
}
```

### 2. **Fixed QRScansController QR Code Search Logic**
- **File**: `QRScansController.cs` lines 192-194
- **Issue**: Đã sửa để search với full code (không strip POI_ prefix)
- **Code**:
```csharp
// Search database with full code (database stores with POI_ prefix)
AudioPOI poi = null;
poi = await _db.GetPOIByQRCodeAsync(code);
```

### 3. **Created Test Endpoints**
- `/api/pois/debug/all` - Xem tất cả POI
- `/api/pois/debug/create-test` - Tạo test POI
- `/api/pois/qr/{qrCode}` - Search POI bằng QRCode

---

## 🚀 Các bước để test:

### **Bước 1: Tạo POI Test**

**Option A: Dùng Admin Dashboard (Dễ nhất)**
1. Truy cập: `http://172.20.10.2:5000`
2. Click vào tab: **🏪 Quán Ăn**
3. Click nút: **➕ Thêm Quán Ăn Mới**
4. Điền form:
   - **Tên Quán**: Cơm Tấm Bà Ghẻ
   - **Địa Chỉ**: 123 Nguyễn Hữu Cảnh, TP.HCM
   - **Mô Tả**: Quán cơm tấm nổi tiếng
   - **Vĩ Độ**: 10.7769
   - **Kinh Độ**: 106.7009
   - **Bán Kính**: 100
5. Click nút: **✅ Thêm Quán**
6. Sau khi thêm, sẽ thấy QR Code - nhấn để xem QR image

**Option B: Dùng PowerShell**
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/create-test" -Method POST
$data = $response.Content | ConvertFrom-Json
Write-Host "✅ POI created!"
Write-Host "QR Code: $($data.qrCode)"
Write-Host "Scan URL: $($data.scanUrl)"
```

**Option C: Dùng cURL**
```bash
curl -X POST http://172.20.10.2:5000/api/pois/debug/create-test
```

---

### **Bước 2: Kiểm tra POI vừa tạo**

```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/all" -Method GET
$data = $response.Content | ConvertFrom-Json
$data.pois | Format-Table -Property id, name, qrCode
```

---

### **Bước 3: Tạo QR Code để Scan**

Sau khi tạo POI, bạn có 2 cách lấy QR Code:

**Cách 1: Từ Admin Dashboard**
1. Tab **🏪 Quán Ăn** → Tìm POI vừa tạo
2. Click vào quán → Sẽ hiện QR Code
3. Download hoặc screenshot QR Code

**Cách 2: Từ URL trực tiếp**
Nếu QRCode là `POI_ABC123`:
- URL: `http://172.20.10.2:5000/qr/POI_ABC123`
- QR API: `https://api.qrserver.com/v1/create-qr-code/?size=400x400&data=http://172.20.10.2:5000/qr/POI_ABC123`

---

### **Bước 4: Scan QR Code**

1. **Trên máy ngoài**:
   - Dùng ứng dụng QR Scanner bình thường
   - Scan QR Code
   - Sẽ redirect tới: `http://172.20.10.2:5000/poi-public.html?poiId=1`

2. **Trên thiết bị cùng mạng**:
   - Safari/Chrome: Mở camera
   - Quét QR Code
   - Tự động mở trang

3. **Test trên Desktop**:
   - Vào URL: `http://172.20.10.2:5000/qr/POI_XXXXX`
   - Sẽ redirect tới info page

---

## 🔍 **Troubleshooting**

### ❌ Vẫn báo "Không tìm thấy quán"

**Nguyên nhân & Giải pháp:**

1. **Chưa tạo POI**
   - ✅ Xác nhận: `/api/pois/debug/all` trả về `totalCount > 0`
   - ✅ Giải pháp: Tạo POI theo bước 1

2. **POI không có QRCode**
   - ✅ Xác nhận: `/api/pois/debug/all` - check `qrCode` field
   - ✅ Giải pháp: Xóa POI, tạo lại (form sẽ auto-generate)

3. **QRCode không khớp**
   - ✅ Xác nhận: `/api/pois/qr/POI_ABC123` → Check có trả về data không
   - ✅ Giải pháp: Copy chính xác QRCode từ `/debug/all`

4. **Server không chạy**
   - ✅ Xác nhận: Mở `http://172.20.10.2:5000` có load dashboard không
   - ✅ Giải pháp: Khởi động server trong Visual Studio

5. **Database bị khóa**
   - ✅ Xác nhận: Check `C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\VinhKhanhTour_Full.db3`
   - ✅ Giải pháp: Restart server

---

## 📝 **Test Checklist**

```
□ Build thành công (0 errors)
□ Server chạy trên port 5000
□ Có thể truy cập http://172.20.10.2:5000
□ Tab "🏪 Quán Ăn" hiển thị
□ Tạo được 1 POI mới
□ POI có QRCode (không null/empty)
□ /api/pois/debug/all trả về 1+ POI
□ /api/pois/qr/POI_XXXXX trả về POI data
□ /qr/POI_XXXXX redirect tới info page
□ Info page load được POI name & address
□ Không còn lỗi "Không tìm thấy quán"
```

---

## 📞 **Nếu vẫn không hoạt động:**

1. **Clear build cache**:
   ```powershell
   cd C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb
   dotnet clean
   dotnet build
   ```

2. **Restart server**:
   - Ngừng server
   - Xóa database cũ: `C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\`
   - Khởi động lại server
   - Tạo POI mới

3. **Check logs**:
   - Xem Output window trong Visual Studio
   - Tìm lỗi từ DatabaseService hoặc QRScansController

---

**Status**: ✅ Tất cả fix đã áp dụng. Chỉ cần tạo POI để test!
