# 🚀 Vĩnh Khánh Food Tour - Server Running Guide

## ✅ Server Status: RUNNING

**Server đã được khởi động thành công!**

- 🌐 Server URL: `http://localhost:5000`
- 📅 Started: 2026-05-13
- 🔧 Environment: Development
- 🎯 .NET Version: 8.0.419

---

## 🌍 Truy Cập Ứng Dụng

### 1. Admin Dashboard (Homepage)
```
http://localhost:5000/
```
- Quản lý POIs (điểm ăn uống)
- Xem thống kê người dùng
- Quản lý thiết bị

### 2. QR Queue Monitor Dashboard
```
http://localhost:5000/qr-queue-monitor.html
```
- Theo dõi hàng đợi QR scan real-time
- Xem số requests đang xử lý
- Test queue system
- Thống kê performance

### 3. API Documentation (Swagger)
```
http://localhost:5000/swagger
```
- Xem tất cả API endpoints
- Test API trực tiếp từ browser

---

## 🔌 API Endpoints Chính

### Queue Management
```bash
# Lấy thống kê queue
GET http://localhost:5000/api/qrqueue/stats

# Kiểm tra trạng thái device
GET http://localhost:5000/api/qrqueue/status/{deviceId}

# Test thêm request
POST http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false
```

### QR Scanning
```bash
# Quét QR code
GET http://localhost:5000/qr/{code}

# Verify QR scan
GET http://localhost:5000/api/qrscans/verify?qrCode=POI_XXX&deviceId=device123
```

### POI Management
```bash
# Lấy tất cả POIs
GET http://localhost:5000/api/pois

# Lấy POI theo ID
GET http://localhost:5000/api/pois/{id}

# Tạo POI mới
POST http://localhost:5000/api/pois
```

### Devices
```bash
# Lấy tất cả devices
GET http://localhost:5000/api/devices

# Lấy devices của user
GET http://localhost:5000/api/devices/user/{userId}
```

---

## 🛠️ Quản Lý Server

### Dừng Server
Nhấn `Ctrl+C` trong terminal đang chạy server

### Khởi động lại Server
```bash
cd D:\C_Sharp\do_an_C_sharp-main\doancsharp.adminweb\doancsharp.adminweb
dotnet run --urls "http://localhost:5000"
```

### Build lại Project
```bash
dotnet build
```

### Xem Logs
Server logs được hiển thị trực tiếp trong terminal

---

## 🧪 Test Queue System

### Cách 1: Sử dụng Dashboard UI
1. Mở `http://localhost:5000/qr-queue-monitor.html`
2. Click "➕ Thêm Request (Free)" để test free user
3. Click "⭐ Thêm Request (Paid)" để test paid user
4. Click "🔥 Thêm 10 Requests" để test load cao

### Cách 2: Sử dụng PowerShell
```powershell
# Thêm 1 request
Invoke-WebRequest -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false" -Method POST

# Thêm 10 requests
1..10 | ForEach-Object {
    Invoke-WebRequest -Uri "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false" -Method POST
    Start-Sleep -Milliseconds 100
}

# Kiểm tra stats
Invoke-WebRequest -Uri "http://localhost:5000/api/qrqueue/stats" | Select-Object -ExpandProperty Content
```

---

## 📊 Queue System Configuration

**Cấu hình hiện tại:**
- Max Concurrent Requests: **10**
- Max Queue Size: **100**
- Request Timeout: **30 seconds**
- Priority: Paid users > Free users

**Để thay đổi cấu hình:**
Chỉnh sửa file `Services/QRQueueService.cs`:
```csharp
private const int MAX_CONCURRENT_REQUESTS = 10;
private const int MAX_QUEUE_SIZE = 100;
private const int REQUEST_TIMEOUT_SECONDS = 30;
```

---

## 🔍 Troubleshooting

### Port 5000 đã được sử dụng
```powershell
# Tìm process đang dùng port 5000
Get-NetTCPConnection -LocalPort 5000 | Select-Object OwningProcess

# Kill process
Stop-Process -Id <ProcessId> -Force
```

### Database không tồn tại
Server sẽ tự động tạo database SQLite khi khởi động lần đầu.
File database: `vinh_khanh_tour.db`

### Lỗi CORS
CORS đã được cấu hình AllowAll, không cần thay đổi.

### Không truy cập được từ máy khác
Server đang bind `0.0.0.0:5000` nên có thể truy cập từ:
- Localhost: `http://localhost:5000`
- Local IP: `http://192.168.x.x:5000`
- Ngrok/Railway: Theo URL được cung cấp

---

## 📱 Test với Mobile App

### Deep Link Format
```
vinhkhanhtour://poi/{poiId}
vinhkhanhtour://foodstreet
```

### QR Code Format
```
http://localhost:5000/qr/POI_XXXXXXXX
http://localhost:5000/qr/FOODSTREET_001
```

---

## 🎯 Next Steps

1. ✅ Server đang chạy
2. 🔍 Test các API endpoints
3. 📱 Test QR scanning flow
4. 🎨 Customize UI nếu cần
5. 🚀 Deploy lên production server

---

## 📞 Support

Nếu gặp vấn đề:
1. Kiểm tra logs trong terminal
2. Kiểm tra file `SERVER_RUNNING_GUIDE.md` này
3. Xem `QR_QUEUE_GUIDE.md` cho chi tiết về queue system

---

**🎉 Chúc bạn phát triển thành công!**
