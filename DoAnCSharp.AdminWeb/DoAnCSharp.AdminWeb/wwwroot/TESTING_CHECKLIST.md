# 🧪 TESTING GUIDE - POI Public & POI Detail Features

## Trang POI Public (`/poi-public.html`)

### ✅ Test 1: Back Button Removed
**Expected**: Header không có nút quay lại (←)
**How to test**:
1. Mở trang POI Public
2. Kiểm tra header - chỉ hiển thị: 🍲 Danh Sách Quán Ăn
3. Không có nút quay lại ở phía trái

### ✅ Test 2: Restaurant Images Display
**Expected**: Tất cả hình ảnh quán ăn hiển thị đúng
**How to test**:
1. Trang POI Public tải xong
2. Kiểm tra mỗi card quán ăn có hình ảnh
3. Khi hover lên card, hình ảnh zoom tắt/on mượt mà
4. Nếu API không trả hình, hiển thị placeholder

### ✅ Test 3: Scan Count Display
**Expected**: Header hiển thị "📊 Lượt xem: X/5" (X là số hiện tại)
**How to test**:
1. Mở POI Public lần đầu → Hiển thị "0/5"
2. Click "👁️ Xem Chi Tiết" lần 1 → Quay lại, hiển thị "1/5"
3. Lặp lại 4 lần nữa
4. Lần 6: Nút vô hiệu hóa, hiển thị "❌ Hết lượt"
5. Kiểm tra localStorage hoặc cookie lưu deviceId

### ✅ Test 4: Maximum 5 Views Enforced
**Expected**: Sau 5 lần xem, tất cả nút "👁️ Xem Chi Tiết" bị vô hiệu hóa
**How to test**:
1. Click "👁️ Xem Chi Tiết" trên 5 quán khác nhau
2. Mỗi lần quay lại POI Public, count tăng 1
3. Sau lần 5, tất cả button chuyển sang "❌ Hết lượt"
4. Không thể click tiếp

---

## Trang POI Detail (`/poi-detail.html?poiId=X`)

### ✅ Test 1: Hero Image Displays
**Expected**: Hình ảnh quán hiển thị ở đầu trang
**How to test**:
1. Click "👁️ Xem Chi Tiết" từ POI Public
2. Chuyển đến trang detail
3. Kiểm tra hình ảnh hiển thị ngay tại hero section (đầu trang)
4. Hình không bị bị trắng/mất

### ✅ Test 2: Language Switching Works
**Expected**: Bấm nút ngôn ngữ, nội dung thay đổi ngôn ngữ
**How to test** (kiểm tra từng ngôn ngữ):

#### Tiếng Việt (VI)
1. Click 🇻🇳 Tiếng Việt
2. Kiểm tra:
   - Nút active (background orange)
   - Mô tả hiển thị tiếng Việt
   - Notification: "✅ Đã chuyển sang Tiếng Việt"

#### English (EN)
1. Click 🇬🇧 English
2. Kiểm tra:
   - Nút active (background orange)
   - Mô tả hiển thị tiếng Anh
   - Notification: "✅ Switched to English"

#### 日本語 (JA)
1. Click 🇯🇵 日本語
2. Kiểm tra:
   - Nút active (background orange)
   - Mô tả hiển thị tiếng Nhật
   - Notification: "✅ 日本語に切り替えました"

#### Русский (RU)
1. Click 🇷🇺 Русский
2. Kiểm tra:
   - Nút active (background orange)
   - Mô tả hiển thị tiếng Nga
   - Notification: "✅ Переключено на русский"

#### 中文 (ZH)
1. Click 🇨🇳 中文
2. Kiểm tra:
   - Nút active (background orange)
   - Mô tả hiển thị tiếng Trung
   - Notification: "✅ 已切换到中文"

### ✅ Test 3: Hero Image Persists on Language Switch
**Expected**: Hình ảnh không mất khi chuyển ngôn ngữ
**How to test**:
1. Trang detail đã load, hình ảnh hiển thị
2. Click các nút ngôn ngữ liên tục (VI → EN → JA → RU → ZH)
3. Kiểm tra hình ảnh vẫn hiển thị trong suốt quá trình

### ✅ Test 4: Audio with Language Switch
**Expected**: Audio phát được và tự động chuyển đổi ngôn ngữ
**How to test**:
1. Bấm "▶ Play Audio"
2. Audio bắt đầu phát (notification: "⏸ Pause")
3. Bấm nút ngôn ngữ khác (ví dụ English)
4. Audio tự động dừng và phát lại bằng ngôn ngữ mới
5. Để audio phát xong, sau đó kiểm tra:
   - Listening history được lưu
   - Notification: "✅ Switched to [Language]"

### ✅ Test 5: Scan Limit Enforcement on Detail Page
**Expected**: Sau 5 lần nghe, nút audio bị vô hiệu hóa
**How to test**:
1. Lần 1-5: Bấm "▶ Play Audio", audio phát
2. Lần 6: Bấm nút, hiển thị: "⚠️ Hết lượt nghe. Tải app để nghe không giới hạn!"
3. Nút "▶ Play Audio" không hoạt động

---

## 🔄 Cross-Browser Testing

Kiểm tra trên các trình duyệt sau:
- ✅ Chrome (Windows/Mac/Linux)
- ✅ Firefox (Windows/Mac/Linux)
- ✅ Safari (Mac/iOS)
- ✅ Edge (Windows)
- ✅ Mobile Browser (iOS Safari, Chrome Mobile)

### Mobile Testing Checklist
- ✅ POI Public: Cards xếp 1 hàng trên mobile
- ✅ POI Detail: Hero image responsive
- ✅ Language buttons hiển thị đúng trên mobile
- ✅ Audio button không bị che khuất

---

## 📱 Responsive Design Testing

### Desktop (>1024px)
- ✅ Grid 3 cột quán ăn
- ✅ Header đầy đủ không bị cắt
- ✅ Language buttons nằm cạnh nhau

### Tablet (768px - 1024px)
- ✅ Grid 2 cột quán ăn
- ✅ Language buttons vẫn nằm hàng

### Mobile (<768px)
- ✅ Grid 1 cột quán ăn
- ✅ Language buttons stack hoặc wrap
- ✅ Header responsive
- ✅ Nút không bị overflow

---

## 🎯 Bug Verification Checklist

| Bug | Fix | Verify | Status |
|-----|-----|--------|--------|
| Back button visible | Removed | Header clean | ✅ |
| Images not showing | Added CSS styling | All images display | ✅ |
| Scan count format wrong | Changed logic | Shows "X" not "X/5" | ✅ |
| Language stuck on English | Fixed button logic | All 5 languages work | ✅ |
| Hero image missing | Set before render | Image loads immediately | ✅ |

---

## 🐛 Console Error Checking

Mở DevTools (F12) và kiểm tra Console:
- ✅ Không có lỗi JavaScript đỏ
- ✅ Không có lỗi Network (404, 500, etc)
- ✅ Warnings có thể có nhưng không critical

### Expected Warnings (OK):
```
CORS warning nếu API khác domain
```

### NOT OK (phải sửa):
```
Uncaught ReferenceError
Failed to fetch
TypeError: Cannot set property
```

---

## 🧩 API Integration Testing

### POI Public API
**Endpoint**: `GET /api/pois`
**Expected Response**:
```json
[
  {
    "id": 1,
    "name": "Quán A",
    "imageUrl": "https://...",
    "imageAsset": "...",
    "address": "...",
    "rating": 4.5,
    "priority": 1
  }
]
```

### POI Detail API
**Endpoint**: `GET /api/pois/{id}`
**Expected Response**:
```json
{
  "id": 1,
  "name": "Quán A",
  "imageUrl": "https://...",
  "description": "...",
  "descriptionEn": "...",
  "descriptionJa": "...",
  "descriptionRu": "...",
  "descriptionZh": "..."
}
```

### Scan Limit API
**Endpoint**: `GET /api/qrscans/scan-limit?deviceId=X`
**Expected Response**:
```json
{
  "deviceId": "device_web_xxx",
  "scanCount": 2,
  "maxScans": 5
}
```

---

## ✅ Final Sign-Off

When ALL tests pass:
- [ ] POI Public back button removed
- [ ] All restaurant images display
- [ ] Scan count shows correctly
- [ ] Language switching works for all 5 languages
- [ ] Hero image displays and persists
- [ ] Audio changes language properly
- [ ] Max 5 scans enforced on both pages
- [ ] No console errors
- [ ] Responsive on mobile/tablet/desktop
- [ ] Cross-browser compatible

**Ready for Deployment**: 🚀 YES / NO
