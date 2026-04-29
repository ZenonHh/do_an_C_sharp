# 📋 Hướng Dẫn Chi Tiết - Cải Tiến Giao Diện 3 Trang

## 🎯 Mục Đích

Cải thiện giao diện người dùng 3 trang HTML công khai (`master-qr.html`, `poi-public.html`, `poi-detail.html`) với thiết kế hiện đại, responsive tốt, và giữ lại 100% chức năng hiện thời.

---

## 📱 1. Master QR Page (`master-qr.html`)

### Trước & Sau
```
TRƯỚC:
- Header đơn giản, logo text
- QR section cơ bản
- Buttons thẳng hàng
- Features list grid cơ bản

SAU:
- Header với gradient, logo emoji có animation
- QR section dạng card với shadow
- Buttons full-width với gradient colors
- Features grid 2x2 với hover effects
```

### Thay Đổi Chi Tiết

#### 1. Header Section
```html
<!-- TRƯỚC -->
<div class="header">
    <div class="logo">🍴</div>
    <h1>Master QR Code</h1>
    <p class="subtitle">Mã quét chính - In ra để khách quét xem danh sách quán ăn</p>
</div>

<!-- SAU -->
<div class="header">
    <div class="logo">🍴</div>  <!-- Có animation float -->
    <h1>Master QR Code</h1>
    <p>Mã quét chính cho khách tham quan</p>  <!-- Text ngắn hơn -->
</div>
```

**CSS Mới:**
```css
.header {
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    padding: 40px 20px;
    text-align: center;
    box-shadow: 0 8px 32px rgba(232, 93, 4, 0.2);
}

.header .logo {
    font-size: 48px;
    animation: float 3s ease-in-out infinite;
}

@keyframes float {
    0%, 100% { transform: translateY(0px); }
    50% { transform: translateY(-10px); }
}
```

#### 2. QR Section - Card Layout
```html
<!-- SAU -->
<div class="qr-section">
    <h3>📱 Mã QR của bạn</h3>
    <div id="qrDisplay"><!-- QR displays here --></div>
    <div id="qrInfo"><!-- Info displays here --></div>
</div>
```

**CSS Mới:**
```css
.qr-section {
    background: white;
    border-radius: 16px;
    padding: 30px 20px;
    margin-bottom: 25px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.08);
}

.qr-container {
    border: 3px solid #E85D04;
    border-radius: 12px;
    padding: 15px;
    background: white;
    display: inline-block;
    box-shadow: 0 4px 15px rgba(232, 93, 4, 0.2);
}
```

#### 3. Action Buttons - Full Width
```css
.action-buttons {
    display: flex;
    gap: 12px;
    flex-direction: column;
}

.action-buttons .btn {
    width: 100%;
    padding: 14px 24px;
    border-radius: 12px;
    font-weight: 600;
    transition: all 0.3s ease;
}

.action-buttons .btn-primary {
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    color: white;
    box-shadow: 0 4px 15px rgba(232, 93, 4, 0.3);
}

.action-buttons .btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(232, 93, 4, 0.4);
}
```

#### 4. Features - Grid with Animation
```css
.feature-list {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 20px;
}

.feature-item {
    text-align: center;
    padding: 15px;
    border-radius: 12px;
    background: #f8f9fa;
    transition: all 0.3s ease;
}

.feature-item:hover {
    background: #fff3e0;
    transform: translateY(-5px);
}
```

---

## 🍲 2. POI Public Page (`poi-public.html`)

### Trước & Sau
```
TRƯỚC:
- Header cơ bản
- Back button inline với title
- Cards cơ bản
- Buttons 2 cột

SAU:
- Header nâng cao với scan info badge
- Back button riêng
- Cards đẹp hơn với rating badge
- Buttons 2 cột, responsive
- Better hover effects
```

### Thay Đổi Chi Tiết

#### 1. Header - Nâng Cao
```html
<!-- TRƯỚC -->
<div class="header">
    <h1>
        <button class="back-button" onclick="goBack()">←</button>
        🍲 Danh Sách Quán Ăn
    </h1>
    <p>Chọn quán ăn để xem thông tin chi tiết</p>
    <div class="scan-info">...</div>
</div>

<!-- SAU -->
<div class="header">
    <div class="header-top">
        <button class="back-button" onclick="goBack()">←</button>
        <h1>🍲 Danh Sách Quán Ăn</h1>
    </div>
    <p>Chọn quán ăn để xem thông tin chi tiết</p>
    <div class="scan-info">📊 Lượt xem: <span id="scanCount">0</span>/5</div>
</div>
```

**CSS:**
```css
.header {
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    color: white;
    padding: 25px 20px;
    box-shadow: 0 8px 32px rgba(232, 93, 4, 0.2);
}

.header-top {
    display: flex;
    align-items: center;
    gap: 15px;
    margin-bottom: 12px;
}

.back-button {
    background: rgba(255, 255, 255, 0.3);
    border-radius: 8px;
    padding: 8px 12px;
    transition: all 0.3s ease;
}

.back-button:hover {
    background: rgba(255, 255, 255, 0.5);
}

.scan-info {
    background: rgba(255, 255, 255, 0.2);
    padding: 10px 16px;
    border-radius: 12px;
    border: 2px solid rgba(255, 255, 255, 0.3);
    display: inline-block;
    margin-top: 12px;
}
```

#### 2. Restaurant Cards - Enhanced
```html
<!-- SAU -->
<div class="restaurant-card">
    <div class="poi-image">
        <img src="${imageUrl}" alt="${restaurant.name}">
        <div class="card-category-badge">${category}</div>
    </div>
    <div class="poi-content">
        <div class="poi-name">
            ${restaurant.name}
            <div class="poi-rating">⭐ ${rating}</div>
        </div>
        <div class="poi-address">
            📍
            <a href="${mapsUrl}">${address}</a>
        </div>
        <div class="poi-actions">
            <button class="btn-view-details">👁️ Xem Chi Tiết</button>
            <a href="${mapsUrl}" class="btn-maps">📍 Maps</a>
        </div>
    </div>
</div>
```

**CSS:**
```css
.restaurant-card {
    background: white;
    border-radius: 16px;
    overflow: hidden;
    box-shadow: 0 4px 15px rgba(0,0,0,0.1);
    transition: all 0.3s ease;
}

.restaurant-card:hover {
    transform: translateY(-8px);
    box-shadow: 0 12px 32px rgba(0,0,0,0.15);
}

.poi-image {
    position: relative;
    width: 100%;
    height: 200px;
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
}

.card-category-badge {
    position: absolute;
    top: 12px;
    right: 12px;
    background: rgba(232, 93, 4, 0.95);
    color: white;
    padding: 6px 14px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
}

.poi-content {
    padding: 18px;
}

.poi-name {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 18px;
    font-weight: 700;
    color: #2c3e50;
    margin-bottom: 10px;
}

.poi-rating {
    background: #fff3e0;
    color: #E85D04;
    padding: 4px 8px;
    border-radius: 6px;
    font-size: 13px;
}

.poi-actions {
    display: flex;
    gap: 10px;
    margin-top: 12px;
}

.btn-view-details {
    flex: 1;
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    color: white;
    padding: 10px 14px;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
}

.btn-maps {
    background: #fff3e0;
    color: #E85D04;
    padding: 10px 14px;
    border-radius: 8px;
}
```

#### 3. Grid Layout - Responsive
```css
.pois-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
    gap: 20px;
}

@media (max-width: 768px) {
    .pois-grid {
        grid-template-columns: 1fr;
    }
}
```

#### 4. Loading & Error States
```css
.loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 60px 20px;
}

.loading-spinner {
    width: 50px;
    height: 50px;
    border: 4px solid rgba(0,0,0,0.1);
    border-top-color: #E85D04;
    border-radius: 50%;
    animation: spin 1s linear infinite;
}

.error-container {
    background: white;
    border-left: 4px solid #ef4444;
    padding: 30px;
    border-radius: 12px;
    text-align: center;
    margin: 30px 20px;
}

.error-container h2 {
    color: #ef4444;
    font-size: 22px;
}
```

---

## 🍽️ 3. POI Detail Page (`poi-detail.html`)

### Trước & Sau
```
TRƯỚC:
- Hero image full width
- Content dạng section
- Language buttons inline
- Audio controls cơ bản

SAU:
- Hero section với overlay back button
- Content dạng card tập trung
- Language buttons group, active state rõ
- Audio section special styling
- Card-based organization
```

### Thay Đổi Chi Tiết

#### 1. Hero Section - Overlay Design
```html
<!-- SAU -->
<div class="detail-hero-section">
    <img id="heroImage" class="detail-hero-image" src="/images/placeholder.jpg">
    <div class="hero-overlay">
        <button class="back-button" onclick="goBack()">←</button>
    </div>
</div>
```

**CSS:**
```css
.detail-hero-section {
    position: relative;
    width: 100%;
    height: 300px;
    overflow: hidden;
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
}

.detail-hero-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.hero-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 60px;
    background: linear-gradient(to bottom, rgba(0,0,0,0.4), transparent);
    display: flex;
    align-items: center;
    padding: 0 15px;
    z-index: 10;
}

.back-button {
    background: rgba(255, 255, 255, 0.3);
    border: none;
    color: white;
    padding: 8px 12px;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s ease;
}

.back-button:hover {
    background: rgba(255, 255, 255, 0.5);
}
```

#### 2. Card-Based Content
```html
<!-- SAU -->
<div class="card-section">
    <div class="restaurant-header">
        <div>
            <h1 class="restaurant-title">${currentPOI.name}</h1>
        </div>
        <div class="rating-badge">⭐ ${rating}</div>
    </div>
    <div class="address-info">📍 ${address}</div>
</div>

<div class="card-section">
    <div class="section-title-card">🌐 LANGUAGE</div>
    <div class="language-buttons-group">
        <button class="lang-button ${currentLanguage === 'vi' ? 'active' : ''}" 
                onclick="switchLanguage('vi')">🇻🇳 Tiếng Việt</button>
        <!-- ... other languages ... -->
    </div>
</div>

<div class="card-section">
    <div class="section-title-card">📚 ABOUT</div>
    <p class="section-description">${getDescription()}</p>
</div>

<div class="card-section audio-section-card">
    <div class="audio-header">
        <span class="section-title-card">🎧 AUDIO GUIDE</span>
        <span class="free-trial-badge">Free Trial</span>
    </div>
    <button class="btn-play-audio" onclick="playAudio()">▶ Play Audio</button>
    <button class="btn-download-app" onclick="downloadApp()">📥 Download App</button>
</div>
```

**CSS:**
```css
.detail-content {
    background: #f5f7fa;
    padding: 20px;
    max-width: 800px;
    margin: 0 auto;
}

.card-section {
    background: white;
    border-radius: 16px;
    padding: 20px;
    margin-bottom: 15px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.08);
}

.restaurant-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 15px;
    margin-bottom: 12px;
}

.restaurant-title {
    font-size: 24px;
    font-weight: 700;
    color: #2c3e50;
    margin: 0;
    line-height: 1.3;
}

.rating-badge {
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    color: white;
    padding: 8px 12px;
    border-radius: 8px;
    font-weight: 600;
    font-size: 14px;
    flex-shrink: 0;
}

.address-info {
    color: #7f8c8d;
    font-size: 14px;
    display: flex;
    align-items: center;
    gap: 8px;
}

.section-title-card {
    font-size: 16px;
    font-weight: 700;
    color: #2c3e50;
    margin-bottom: 16px;
    display: flex;
    align-items: center;
    gap: 8px;
}

.language-buttons-group {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
}

.lang-button {
    padding: 10px 14px;
    border: 2px solid #ecf0f1;
    background: white;
    color: #7f8c8d;
    border-radius: 8px;
    cursor: pointer;
    font-weight: 600;
    font-size: 13px;
    transition: all 0.3s ease;
}

.lang-button:hover {
    border-color: #E85D04;
    color: #E85D04;
}

.lang-button.active {
    border-color: #E85D04;
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    color: white;
    box-shadow: 0 4px 12px rgba(232, 93, 4, 0.3);
}

.section-description {
    color: #555;
    font-size: 15px;
    line-height: 1.6;
    white-space: pre-wrap;
    word-wrap: break-word;
    margin: 0;
}

.audio-section-card {
    background: linear-gradient(135deg, #fff3e0 0%, #ffe8cc 100%);
    border: 2px solid #E85D04;
}

.audio-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px;
}

.free-trial-badge {
    background: #E85D04;
    color: white;
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 700;
    white-space: nowrap;
}

.btn-play-audio {
    width: 100%;
    padding: 14px;
    background: linear-gradient(135deg, #E85D04 0%, #D84315 100%);
    color: white;
    border: none;
    border-radius: 10px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    margin-bottom: 12px;
    box-shadow: 0 4px 12px rgba(232, 93, 4, 0.2);
    transition: all 0.3s ease;
}

.btn-play-audio:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(232, 93, 4, 0.3);
}

.btn-download-app {
    width: 100%;
    padding: 14px;
    background: white;
    color: #E85D04;
    border: 2px solid #E85D04;
    border-radius: 10px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
}

.btn-download-app:hover {
    background: #ffe8cc;
    transform: scale(1.02);
}
```

#### 3. Responsive Design
```css
@media (max-width: 600px) {
    .detail-hero-section {
        height: 220px;
    }

    .detail-content {
        padding: 15px;
    }

    .restaurant-header {
        flex-direction: column;
        align-items: flex-start;
    }

    .restaurant-title {
        font-size: 20px;
    }

    .card-section {
        padding: 16px;
        margin-bottom: 12px;
    }

    .language-buttons-group {
        gap: 8px;
    }

    .lang-button {
        padding: 8px 12px;
        font-size: 12px;
    }
}
```

---

## 🎨 Hệ Thống Thiết Kế Chung

### Color Palette
```css
/* Primary Colors */
--primary-orange: #E85D04;
--primary-red: #D84315;

/* Text Colors */
--text-dark: #2c3e50;
--text-medium: #555;
--text-light: #7f8c8d;

/* Background Colors */
--bg-light: #f5f7fa;
--bg-soft: #f8f9fa;
--bg-white: white;

/* Border Colors */
--border-light: #ecf0f1;

/* Status Colors */
--error-red: #ef4444;
--success-green: #10b981;
--warning-orange: #f97316;
```

### Typography Scale
```css
h1: 24-32px, weight 700, line-height 1.3
h2: 20-22px, weight 700, line-height 1.3
h3: 16-18px, weight 600-700
body: 14-16px, weight 400-500
label: 13-14px, weight 600
small: 12-13px, weight 500
```

### Spacing System
```css
xs: 8px
sm: 12px
md: 15px
lg: 20px
xl: 25px
xxl: 30px
```

### Shadow System
```css
light: 0 4px 15px rgba(0,0,0,0.08);
medium: 0 4px 20px rgba(0,0,0,0.1);
dark: 0 12px 32px rgba(0,0,0,0.15);
```

### Border Radius
```css
button: 8px
input: 8px
card: 12-16px
badge: 20-25px
full: 50% (circular)
```

---

## ✅ Kiểm Tra Danh Sách

- [x] Master QR page: Header, QR section, buttons, features
- [x] POI Public page: Header, cards, grid, loading/error states
- [x] POI Detail page: Hero, cards, language buttons, audio section
- [x] Responsive design trên mobile
- [x] Hover effects và animations
- [x] All functionality preserved
- [x] Build successful ✓

---

## 🚀 Triển Khai

Tất cả 3 file đã được cập nhật và build thành công!

```
master-qr.html ✓
poi-public.html ✓
poi-detail.html ✓
```

Không có breaking changes, tất cả chức năng được bảo tồn.
