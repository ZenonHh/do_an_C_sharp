# QR Queue Management System - Hướng Dẫn Sử Dụng

## Tổng Quan

Hệ thống quản lý hàng đợi QR scan với khả năng:
- ✅ Giới hạn số request xử lý đồng thời (10 requests)
- ✅ Xếp hàng tự động khi quá tải (tối đa 100 requests trong queue)
- ✅ Ưu tiên user trả phí (Paid users được xử lý trước)
- ✅ Real-time monitoring dashboard
- ✅ Auto-cleanup requests bị timeout (30s)
- ✅ Thống kê chi tiết về throughput và performance

## Kiến Trúc

```
┌─────────────────────────────────────────────────────────────┐
│                    QR Scan Request                          │
│                          ↓                                   │
│              ┌──────────────────────┐                       │
│              │  QRScansController   │                       │
│              └──────────┬───────────┘                       │
│                         ↓                                    │
│              ┌──────────────────────┐                       │
│              │   QRQueueService     │                       │
│              │  (Singleton Service) │                       │
│              └──────────┬───────────┘                       │
│                         ↓                                    │
│         ┌───────────────┴────────────────┐                 │
│         ↓                                 ↓                 │
│  ┌─────────────┐                  ┌─────────────┐         │
│  │   Active    │                  │   Pending   │         │
│  │  Requests   │                  │    Queue    │         │
│  │  (Max: 10)  │                  │  (Max: 100) │         │
│  └─────────────┘                  └─────────────┘         │
│         ↓                                 ↓                 │
│    Processing                      Waiting                 │
│         ↓                                 ↓                 │
│    Complete ←─────────────────────── Auto Process         │
└─────────────────────────────────────────────────────────────┘
```

## Các File Đã Tạo

### 1. **QRQueueService.cs**
- Service quản lý hàng đợi (Singleton)
- Xử lý concurrent requests với ConcurrentQueue và ConcurrentDictionary
- Background workers:
  - ProcessQueueAsync: Xử lý queue liên tục
  - CleanupStaleRequestsAsync: Dọn dẹp requests timeout

**Cấu hình:**
```csharp
MAX_CONCURRENT_REQUESTS = 10  // Số request xử lý đồng thời
MAX_QUEUE_SIZE = 100          // Kích thước queue tối đa
REQUEST_TIMEOUT_SECONDS = 30  // Timeout cho mỗi request
```

### 2. **QRQueueController.cs**
- API endpoints để monitor và test queue
- Endpoints:
  - `GET /api/qrqueue/stats` - Thống kê tổng quan
  - `GET /api/qrqueue/status/{deviceId}` - Trạng thái request của device
  - `GET /api/qrqueue/active` - Danh sách requests đang xử lý
  - `GET /api/qrqueue/queued` - Danh sách requests đang chờ
  - `GET /api/qrqueue/metrics` - Metrics chi tiết
  - `POST /api/qrqueue/test/enqueue` - Test thêm request
  - `POST /api/qrqueue/test/complete` - Test hoàn thành request

### 3. **qr-queue-monitor.html**
- Dashboard real-time monitoring
- Tự động refresh mỗi 2 giây
- Hiển thị:
  - Số requests đang xử lý / đang chờ
  - Tổng số đã xử lý / bị từ chối
  - Danh sách chi tiết từng request
  - Progress bars
  - Test controls để simulate load

### 4. **QRScansController.cs (Updated)**
- Tích hợp QRQueueService vào flow xử lý QR scan
- Tự động enqueue requests khi có người quét QR
- Hiển thị trang chờ với countdown timer
- Auto-redirect khi đến lượt

## Cách Sử Dụng

### 1. Khởi động server

```bash
cd DoAnCSharp.AdminWeb/DoAnCSharp.AdminWeb
dotnet run
```

Server sẽ chạy tại: `http://localhost:5000`

### 2. Truy cập Queue Monitor Dashboard

Mở trình duyệt và truy cập:
```
http://localhost:5000/qr-queue-monitor.html
```

### 3. Test hệ thống

**Cách 1: Sử dụng Dashboard UI**
- Click "➕ Thêm Request (Free)" để thêm free user request
- Click "⭐ Thêm Request (Paid)" để thêm paid user request
- Click "🔥 Thêm 10 Requests" để simulate load cao

**Cách 2: Sử dụng API**

Thêm request vào queue:
```bash
curl -X POST "http://localhost:5000/api/qrqueue/test/enqueue?isPaid=false"
```

Kiểm tra stats:
```bash
curl "http://localhost:5000/api/qrqueue/stats"
```

### 4. Test với QR scan thực tế

Khi user quét QR code, hệ thống sẽ tự động:
1. Kiểm tra xem user có phải paid user không
2. Thêm request vào queue với priority phù hợp
3. Hiển thị trang chờ nếu queue đầy
4. Tự động xử lý khi đến lượt
5. Redirect đến trang POI khi hoàn thành

## Flow Xử Lý Request

### Khi User Quét QR Code:

```
1. User quét QR → /qr/{code}
   ↓
2. QRScansController.QuickScanQR()
   ↓
3. Kiểm tra paid/free user
   ↓
4. QRQueueService.EnqueueRequestAsync()
   ↓
5. Kiểm tra trạng thái:
   
   a) Có slot trống (< 10 active)
      → Xử lý ngay lập tức
      → Trả về POI data
   
   b) Queue chưa đầy (< 100)
      → Thêm vào queue
      → Hiển thị trang chờ với countdown
      → Auto-refresh mỗi 2s
      → Redirect khi đến lượt
   
   c) Queue đầy (≥ 100)
      → Trả về trang "Hệ thống quá tải"
      → Đề xuất thử lại sau

6. Background worker tự động xử lý queue
   → Ưu tiên paid users
   → FIFO cho cùng priority level

7. Sau khi xử lý xong
   → CompleteRequest()
   → Giải phóng slot
   → Queue tự động tiếp tục
```

## Ưu Tiên Xử Lý

```
Priority Level:
┌─────────────────────────────────┐
│  Paid Users (Priority: 10)      │  ← Xử lý trước
├─────────────────────────────────┤
│  Free Users (Priority: 1)       │  ← Xử lý sau
└─────────────────────────────────┘

Trong cùng priority level: FIFO (First In First Out)
```

## Monitoring & Metrics

Dashboard hiển thị:

### Real-time Stats
- **Đang xử lý**: Số requests đang được xử lý (max 10)
- **Đang chờ**: Số requests trong queue (max 100)
- **Đã xử lý**: Tổng số requests đã hoàn thành
- **Bị từ chối**: Số requests bị từ chối do queue đầy

### Active Requests List
- Device ID (masked)
- QR Code
- Thời gian đã chờ
- Thời gian bắt đầu xử lý
- Paid/Free status

### Queued Requests List
- Vị trí trong queue (#1, #2, ...)
- Device ID (masked)
- QR Code
- Thời gian đã chờ
- Thời gian chờ ước tính
- Paid/Free status (Paid users hiển thị màu xanh)

### Performance Metrics
- Average processing rate (requests/second)
- Service uptime
- Utilization percentage
- Available slots

## Trang Chờ Cho User

Khi user phải chờ trong queue, họ sẽ thấy:

```
┌─────────────────────────────────┐
│         ⏳ Đang Chờ Xử Lý       │
│                                 │
│            #5                   │
│    Vị trí của bạn trong hàng đợi│
│                                 │
│         [Spinner]               │
│                                 │
│  Thời gian chờ ước tính: 15s   │
│                                 │
│ Trang sẽ tự động chuyển khi     │
│      đến lượt bạn...            │
└─────────────────────────────────┘
```

Trang này:
- Auto-refresh mỗi 2 giây
- Cập nhật vị trí trong queue real-time
- Countdown timer
- Tự động redirect khi đến lượt

## API Endpoints

### Queue Management

```
GET  /api/qrqueue/stats
     → Lấy thống kê tổng quan

GET  /api/qrqueue/status/{deviceId}
     → Kiểm tra trạng thái request của device

GET  /api/qrqueue/active
     → Danh sách requests đang xử lý

GET  /api/qrqueue/queued
     → Danh sách requests đang chờ

GET  /api/qrqueue/metrics
     → Metrics chi tiết (utilization, throughput, etc.)

POST /api/qrqueue/test/enqueue?isPaid=true
     → Test thêm request vào queue

POST /api/qrqueue/test/complete?deviceId=xxx
     → Test đánh dấu request hoàn thành
```

## Cấu Hình & Tuning

### Điều chỉnh capacity

Trong `QRQueueService.cs`:

```csharp
// Tăng số request xử lý đồng thời
private const int MAX_CONCURRENT_REQUESTS = 20; // Mặc định: 10

// Tăng kích thước queue
private const int MAX_QUEUE_SIZE = 200; // Mặc định: 100

// Điều chỉnh timeout
private const int REQUEST_TIMEOUT_SECONDS = 60; // Mặc định: 30
```

### Điều chỉnh refresh rate

Trong `qr-queue-monitor.html`:

```javascript
// Thay đổi tần suất refresh dashboard
refreshInterval = setInterval(fetchQueueStats, 1000); // Mặc định: 2000ms
```

Trong trang chờ user (BuildQueueWaitingHtml):

```javascript
// Thay đổi tần suất check status
checkInterval = setInterval(checkStatus, 1000); // Mặc định: 2000ms
```

## Troubleshooting

### Queue bị đầy liên tục
- Tăng `MAX_CONCURRENT_REQUESTS`
- Tối ưu code xử lý POI để giảm thời gian xử lý
- Scale horizontal (thêm server)

### Requests bị timeout
- Tăng `REQUEST_TIMEOUT_SECONDS`
- Kiểm tra database performance
- Kiểm tra network latency

### Dashboard không cập nhật
- Kiểm tra console browser có lỗi không
- Verify API `/api/qrqueue/stats` hoạt động
- Kiểm tra CORS settings

## Lợi Ích

✅ **Tránh quá tải server**: Giới hạn số request xử lý đồng thời
✅ **Trải nghiệm user tốt hơn**: Hiển thị vị trí chờ thay vì lỗi
✅ **Ưu tiên paid users**: Tăng giá trị cho khách hàng trả phí
✅ **Monitoring real-time**: Dễ dàng theo dõi và debug
✅ **Auto-scaling ready**: Có thể mở rộng dễ dàng
✅ **Graceful degradation**: Xử lý tốt khi hệ thống quá tải

## Kết Luận

Hệ thống Queue Management đã được tích hợp hoàn chỉnh vào dự án Vĩnh Khánh Food Tour. Bạn có thể:

1. ✅ Xem hàng đợi real-time qua dashboard
2. ✅ Quản lý concurrent requests hiệu quả
3. ✅ Ưu tiên paid users tự động
4. ✅ Monitor performance và throughput
5. ✅ Test và simulate load dễ dàng

Truy cập `http://localhost:5000/qr-queue-monitor.html` để bắt đầu!
