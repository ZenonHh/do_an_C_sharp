# 🌐 Hướng Dẫn Sửa Chữa Chuyển Đổi Ngôn Ngữ

## Vấn đề Hiện Tại
Khi người dùng nhấn vào nút tiếng khác (English, 日本語, Русский, 中文), giao diện hiển thị thông báo thành công nhưng mô tả quán vẫn hiển thị tiếng Việt.

## Nguyên Nhân Gốc Rễ
1. **Dữ liệu Seed cũ**: Database chứa dữ liệu POI cũ mà chỉ có mô tả tiếng Việt (`Description` field)
2. **Thiếu các trường dịch**: Dữ liệu cũ không có `DescriptionEn`, `DescriptionJa`, `DescriptionRu`, `DescriptionZh`
3. **Fallback logic**: Khi không tìm thấy bản dịch, code fallback về Vietnamese

## Giải Pháp Triển Khai
Chúng tôi đã cập nhật:

### 1. Model AudioPOI ✅
- Thêm 4 trường mô tả đa ngôn ngữ:
  - `DescriptionEn` - Tiếng Anh
  - `DescriptionJa` - Tiếng Nhật
  - `DescriptionRu` - Tiếng Nga
  - `DescriptionZh` - Tiếng Trung

### 2. Dữ Liệu Seed ✅
- Cập nhật `DatabaseService.SeedSampleDataAsync()` với dữ liệu 15 quán gồm:
  - **"Ốc Oanh"** - Đầy đủ 5 ngôn ngữ
  - **"Ốc Vũ"** - Đầy đủ 5 ngôn ngữ
  - **"Quán Nướng Chilli"** - Đầy đủ 5 ngôn ngữ
  - 12 quán còn lại - Tiếng Việt (fallback)

### 3. JavaScript Frontend ✅
- Sửa `loadPOIDetails()` để normalize field names đúng
- Cải thiện `getDescriptionByLanguage()` với console logging
- `switchLanguage()` - gọi `renderPOIDetails()` để re-render

### 4. Image Display ✅
- Sửa `poi-detail.html` - hiển thị hình ảnh đúng
- Sửa `poi-public.html` - hiển thị hình ảnh trong danh sách

## ⚠️ Bước Xóa Dữ Liệu Cũ (QUAN TRỌNG)

Database cũ vẫn chứa dữ liệu seed old. **Bạn phải xóa database cũ** để seed data mới chạy.

### Cách Xóa Database

**Tuỳ theo hệ thống:**

1. **Nếu database ở AppData:**
```powershell
Remove-Item "C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force
```

2. **Nếu database ở project directory:**
```powershell
Remove-Item "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb\bin\Debug\net8.0\data\VinhKhanhTour_Full.db3" -Force
```

3. **Hoặc - Xóa cả folder data:**
```powershell
Remove-Item "C:\Users\LENOVO\source\repos\do_an_C_sharp\DoAnCSharp.AdminWeb\DoAnCSharp.AdminWeb\bin\Debug\net8.0\data" -Recurse -Force
```

## 📋 Các Bước Triển Khai

### 1. **Dừng ứng dụng** (nếu đang chạy)
   - Đóng Visual Studio hoặc dừng ứng dụng trong IIS Express

### 2. **Xóa Database Cũ**
   ```powershell
   # Chọn một trong các lệnh trên
   Remove-Item "C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\VinhKhanhTour_Full.db3" -Force
   ```

### 3. **Build Solution**
   ```
   Visual Studio: Build > Clean Solution
   Visual Studio: Build > Build Solution
   ```

### 4. **Chạy Ứng Dụng**
   - Ứng dụng sẽ:
     - Tạo database mới
     - Chạy `SeedSampleDataAsync()`
     - Populate 15 quán với dữ liệu đa ngôn ngữ

### 5. **Test Chuyển Đổi Ngôn Ngữ**
   - Mở http://localhost:xxxx/poi-public.html
   - Nhấn vào quán "Ốc Oanh" hoặc "Ốc Vũ"
   - Nhấn các nút ngôn ngữ
   - ✅ Mô tả nên thay đổi sang tiếng tương ứng

## 🔍 Debug Mode

Mở **Browser Developer Console** (F12) để xem logs:

```javascript
// Logs tương tự sẽ xuất hiện:
getDescriptionByLanguage(en) = "The most legendary and busy snail restaurant..."
getDescriptionByLanguage(ja) = "ビンカン地区で最も有名で..."
getDescriptionByLanguage(ru) = "Самый легендарный и оживленный..."
```

Nếu thấy "Fallback to Vietnamese" - có nghĩa bản dịch đó chưa được seeding.

## 🎯 Kết Quả Dự Kiến

✅ Khi chuyển sang tiếng khác:
- Nút ngôn ngữ được highlight
- Mô tả thay đổi sang ngôn ngữ tương ứng
- Thông báo thành công hiện ra
- Audio narration dùng ngôn ngữ đúng (nếu có Web Speech API)

✅ Hình ảnh hiển thị trên cả hai trang:
- poi-detail.html - Chi tiết quán
- poi-public.html - Danh sách quán

## 📝 Ghi Chú

- 3 quán đầu ("Ốc Oanh", "Ốc Vũ", "Quán Nướng Chilli") có đầy đủ 5 ngôn ngữ
- 12 quán còn lại vẫn dùng Vietnamese (fallback)
- Bạn có thể thêm bản dịch cho các quán còn lại vào `DatabaseService.cs`
- Khi thêm bản dịch, cần xóa database cũ và rebuild

## 🚀 Lệnh Quick Start

```powershell
# 1. Dừng app
Stop-Process -Name "DoAnCSharp.AdminWeb" -ErrorAction SilentlyContinue

# 2. Xóa database
$dbPath = "C:\Users\LENOVO\AppData\Roaming\VinhKhanhTour\VinhKhanhTour_Full.db3"
if (Test-Path $dbPath) { Remove-Item $dbPath -Force }

# 3. Build lại (trong Visual Studio)
# 4. Chạy lại ứng dụng
```

---

**Cập Nhật Lần Cuối**: 2024 (Sau khi thêm dữ liệu đa ngôn ngữ vào seed)
