# Hướng Dẫn Sửa Địa Chỉ IP và Test QR Code

## ✅ Đã Sửa

File `appsettings.Development.json` đã được cập nhật:
```json
"ServerSettings": {
  "PublicUrl": "http://192.168.1.43:5000"
}
```

## 🚀 Cách Test QR Code

### Bước 1: Khởi động server

```bash
cd DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb
dotnet run
```

Server sẽ chạy tại: `http://192.168.1.43:5000`

### Bước 2: Lấy QR Code để test

**Cách 1: Qua Dashboard Admin**
1. Mở trình duyệt: `http://192.168.1.43:5000/index.html`
2. Vào mục "Quản lý POI"
3. Click vào một POI bất kỳ
4. Click "Xem QR Code"
5. Quét QR code bằng điện thoại

**Cách 2: Lấy QR Code Master (Phố Ẩm Thực)**
1. Mở: `http://192.168.1.43:5000/master-qr.html`
2. Sẽ hiển thị QR code lối vào phố ẩm thực
3. Quét QR này sẽ hiển thị danh sách tất cả quán

**Cách 3: Test trực tiếp bằng API**
```bash
# Lấy thông tin QR code của POI ID 1
curl http://192.168.1.43:5000/api/pois/1/qr-info
```

### Bước 3: Test Queue Monitor

1. Mở dashboard: `http://192.168.1.43:5000/qr-queue-monitor.html`
2. Click "🔥 Thêm 10 Requests" để test hàng đợi
3. Quan sát:
   - Số requests đang xử lý (max 10)
   - Số requests đang chờ trong queue
   - Thời gian xử lý

### Bước 4: Test QR Scan với điện thoại

**Điều kiện:**
- Điện thoại và máy tính phải cùng mạng WiFi
- IP máy tính: `192.168.1.43`

**Các URL QR code sẽ có dạng:**
```
http://192.168.1.43:5000/qr/POI_XXXXXXXXXX
http://192.168.1.43:5000/qr/FOODSTREET_VINHKHANH
```

**Khi quét QR:**
1. Nếu hệ thống rảnh (< 10 requests) → Xử lý ngay
2. Nếu hệ thống bận → Hiển thị trang chờ với vị trí trong queue
3. Nếu queue đầy (≥ 100) → Hiển thị "Hệ thống quá tải"

## 🔍 Kiểm Tra Địa Chỉ IP Hiện Tại

Nếu IP thay đổi, chạy lệnh này để xem IP mới:

**Windows:**
```bash
ipconfig | findstr "IPv4"
```

**Kết quả hiện tại:**
```
IPv4 Address: 192.168.1.43  ← Đây là IP cần dùng
```

## 📝 Các File Cần Cấu Hình IP

### 1. appsettings.Development.json (✅ Đã sửa)
```json
{
  "ServerSettings": {
    "PublicUrl": "http://192.168.1.43:5000"
  }
}
```

### 2. appsettings.json (Production)
```json
{
  "ServerSettings": {
    "PublicUrl": "http://localhost:5000"
  }
}
```

## 🧪 Test Scenarios

### Test 1: QR Scan Đơn Giản
1. Quét 1 QR code
2. Kiểm tra xem có redirect đến trang POI không
3. Kiểm tra dashboard có hiển thị request không

### Test 2: Test Queue (Nhiều người quét cùng lúc)
1. Mở dashboard: `http://192.168.1.43:5000/qr-queue-monitor.html`
2. Click "🔥 Thêm 10 Requests"
3. Quan sát:
   - 10 requests đầu → Xử lý ngay (Active)
   - Requests còn lại → Vào queue (Queued)
   - Sau 30s → Requests timeout tự động bị xóa

### Test 3: Test Paid vs Free User
1. Click "⭐ Thêm Request (Paid)" → Màu xanh, priority cao
2. Click "➕ Thêm Request (Free)" → Màu xám, priority thấp
3. Paid users sẽ được xử lý trước

### Test 4: Test Limit Exceeded
1. Quét QR 5 lần (free user limit)
2. Lần thứ 6 sẽ hiển thị "Đã hết lượt miễn phí"

## 🐛 Troubleshooting

### Vấn đề: Quét QR không lên gì

**Nguyên nhân có thể:**
1. ❌ Server chưa chạy
2. ❌ IP sai hoặc đã thay đổi
3. ❌ Điện thoại không cùng mạng WiFi
4. ❌ Firewall chặn port 5000

**Cách fix:**

**1. Kiểm tra server đang chạy:**
```bash
# Mở trình duyệt máy tính
http://192.168.1.43:5000/index.html

# Nếu không mở được → Server chưa chạy
cd DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb
dotnet run
```

**2. Kiểm tra IP:**
```bash
ipconfig | findstr "IPv4"
# Nếu IP khác 192.168.1.43 → Cập nhật appsettings.Development.json
```

**3. Kiểm tra Firewall:**
```powershell
# Mở PowerShell as Administrator
New-NetFirewallRule -DisplayName "Allow Port 5000" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

**4. Test từ điện thoại:**
- Mở trình duyệt điện thoại
- Truy cập: `http://192.168.1.43:5000/index.html`
- Nếu không mở được → Kiểm tra WiFi

### Vấn đề: QR Code hiển thị sai URL

**Kiểm tra:**
```bash
# Gọi API để xem QR code
curl http://192.168.1.43:5000/api/pois/foodstreet-qr
```

**Kết quả mong đợi:**
```json
{
  "webUrl": "http://192.168.1.43:5000/qr/FOODSTREET_VINHKHANH",
  "qrImageUrl": "https://api.qrserver.com/v1/create-qr-code/?size=400x400&data=...",
  "description": "Mã QR lối vào Phố Ẩm Thực Vĩnh Khánh"
}
```

### Vấn đề: Queue không hoạt động

**Kiểm tra logs:**
```bash
# Khi chạy dotnet run, xem console có log này không:
🚀 Queue processor started
🧹 Cleanup worker started
```

**Test queue:**
```bash
# Test thêm request
curl -X POST "http://192.168.1.43:5000/api/qrqueue/test/enqueue?isPaid=false"

# Kiểm tra stats
curl "http://192.168.1.43:5000/api/qrqueue/stats"
```

## 📱 Test Trên Điện Thoại

### Cách 1: Quét QR Code
1. Mở app Camera trên điện thoại
2. Quét QR code từ màn hình máy tính
3. Click vào link hiện ra
4. Sẽ mở trang web với thông tin POI

### Cách 2: Nhập URL trực tiếp
1. Mở trình duyệt điện thoại
2. Nhập: `http://192.168.1.43:5000/qr/FOODSTREET_VINHKHANH`
3. Sẽ hiển thị danh sách quán ăn

### Cách 3: Test Deep Link (Nếu đã cài app)
1. Quét QR code
2. App sẽ tự động mở (nếu đã cài)
3. Nếu chưa cài → Mở web

## 🎯 Checklist Trước Khi Test

- [ ] Server đang chạy (`dotnet run`)
- [ ] IP đúng trong `appsettings.Development.json`
- [ ] Firewall cho phép port 5000
- [ ] Điện thoại cùng mạng WiFi với máy tính
- [ ] Có thể mở `http://192.168.1.43:5000/index.html` từ máy tính
- [ ] Có thể mở `http://192.168.1.43:5000/index.html` từ điện thoại

## 🔗 Các URL Quan Trọng

```
Admin Dashboard:
http://192.168.1.43:5000/index.html

Queue Monitor:
http://192.168.1.43:5000/qr-queue-monitor.html

Master QR (Phố Ẩm Thực):
http://192.168.1.43:5000/master-qr.html

POI List:
http://192.168.1.43:5000/pois-list.html

API Swagger:
http://192.168.1.43:5000/swagger
```

## 💡 Tips

1. **Để xem QR code nhanh:** Mở `master-qr.html` và in ra giấy
2. **Test nhiều người:** Dùng dashboard để thêm 10 requests cùng lúc
3. **Monitor real-time:** Mở `qr-queue-monitor.html` trên màn hình thứ 2
4. **Debug:** Xem console logs khi chạy `dotnet run`

Bây giờ bạn có thể test QR code rồi! 🎉
