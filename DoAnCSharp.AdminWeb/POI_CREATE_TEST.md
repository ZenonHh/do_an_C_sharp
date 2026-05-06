# Hướng dẫn tạo test POI để test QR scanning

## Bước 1: Tạo test POI
Gửi request POST tới:
```
POST http://172.20.10.2:5000/api/pois/debug/create-test
```

Ví dụ với curl:
```bash
curl -X POST http://172.20.10.2:5000/api/pois/debug/create-test
```

Hoặc PowerShell:
```powershell
$response = Invoke-WebRequest -Uri "http://172.20.10.2:5000/api/pois/debug/create-test" -Method POST
$data = $response.Content | ConvertFrom-Json
Write-Host "QR Code: $($data.qrCode)"
Write-Host "Scan URL: $($data.scanUrl)"
```

## Bước 2: Check tất cả POI
```
GET http://172.20.10.2:5000/api/pois/debug/all
```

## Bước 3: Scan QR code
Dùng mã QR từ bước 1:
```
http://172.20.10.2:5000/qr/POI_XXXXXX
```

## Bước 4: Check POI bằng QR code
```
GET http://172.20.10.2:5000/api/pois/qr/POI_XXXXXX
```

---

**Vấn đề:** App báo "không tìm thấy quán"
**Nguyên nhân có thể:**
1. Chưa tạo POI nào trong database
2. POI không có QRCode
3. QRCode không khớp

**Giải pháp:** 
1. Chạy endpoint `/api/pois/debug/create-test` để tạo test POI
2. Thử scan QR code đó
