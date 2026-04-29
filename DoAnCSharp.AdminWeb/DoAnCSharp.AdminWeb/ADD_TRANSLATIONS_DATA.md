# 🔧 SQL - Thêm Dữ Liệu Tiếng Nga, Nhật, Trung Cho Quán Ốc Oanh

## ⚠️ QUAN TRỌNG: Chạy SQL này để thêm translations test

```sql
-- Thêm các mô tả bằng tiếng Nga, Nhật, Trung cho quán ốc
UPDATE AudioPOI SET
    DescriptionEn = 'Óc Oanh is the busiest escargot restaurant in Vinh Khanh, famous for its fresh snails, rich broth, and delicious taste.',
    DescriptionJa = 'オックオアンはビンカインで最も混雑しているカタツムリレストランです。新鮮なカタツムリ、濃厚なスープ、そして美味しい味で有名です。',
    DescriptionRu = 'Óc Oanh - самый многолюдный ресторан улиток в Винь Кане. Известен свежими улитками, насыщенным бульоном и восхитительным вкусом.',
    DescriptionZh = 'Óc Oanh是永汉最繁忙的蜗牛餐厅。以新鲜的蜗牛、浓郁的汤和美味的味道而闻名。'
WHERE Name = 'Óc Oanh' OR Name LIKE '%Óc Oanh%';

-- Kiểm tra kết quả
SELECT Id, Name, Description, DescriptionEn, DescriptionRu, DescriptionJa, DescriptionZh 
FROM AudioPOI 
WHERE Name LIKE '%Óc Oanh%';
```

---

## 📋 SQL Mẫu Cho Tất Cả Quán

```sql
-- Ốc Vũ
UPDATE AudioPOI SET
    DescriptionEn = 'Óc Vũ offers fresh escargots with unique seasoning and cozy atmosphere.',
    DescriptionJa = 'オックヴーは、ユニークな調味料と居心地の良い雰囲気で新鮮なカタツムリを提供しています。',
    DescriptionRu = 'Óc Vũ предлагает свежих улиток с уникальным приправой и уютной атмосферой.',
    DescriptionZh = 'Óc Vũ提供新鲜的蜗牛，独特的调味料和舒适的氛围。'
WHERE Name = 'Óc Vũ' OR Name LIKE '%Óc Vũ%';

-- Quán Nướng Chilli
UPDATE AudioPOI SET
    DescriptionEn = 'Quán Nướng Chilli specializes in grilled meats with spicy chili sauce.',
    DescriptionJa = 'Quán Nướng Chilliは、辛い唐辛子ソースで焼き肉を専門としています。',
    DescriptionRu = 'Quán Nướng Chilli специализируется на жареном мясе с острым соусом из чили.',
    DescriptionZh = 'Quán Nướng Chilli专门提供用辛辣辣椒酱烧烤的肉类。'
WHERE Name LIKE '%Chilli%';

-- Lẩu Bò Khu Nhà Cháy
UPDATE AudioPOI SET
    DescriptionEn = 'Lẩu Bò Khu Nhà Cháy is famous for beef hotpot with traditional herbs and spices.',
    DescriptionJa = 'ラウボーク火区域火は、伝統的なハーブとスパイスを使った牛鍋で有名です。',
    DescriptionRu = 'Лау Бо Кху Нха Чай известен говяжьим бульоном с традиционными травами и специями.',
    DescriptionZh = 'Lẩu Bò Khu Nhà Cháy以传统草药和香料的牛肉火锅而闻名。'
WHERE Name LIKE '%Lẩu Bò%';
```

---

## ✅ Cách Thực Hiện:

### **Option 1: SQL Server Management Studio**
1. Mở SSMS (SQL Server Management Studio)
2. Kết nối đến database
3. Paste SQL ở trên
4. Chạy (Execute) - Ctrl+E
5. Refresh browser để xem kết quả

### **Option 2: Visual Studio**
1. Mở Visual Studio
2. Đi đến **View → SQL Server Object Explorer**
3. Mở database connection
4. Tìm bảng `AudioPOI`
5. Edit data hoặc paste SQL vào **New Query**

### **Option 3: SQLite Browser (Nếu dùng SQLite)**
1. Tải SQLite Browser: https://sqlitebrowser.org/
2. Mở file database
3. Vào tab **Execute SQL**
4. Paste SQL
5. Click **Execute**

---

## 🧪 Sau Khi Thêm Dữ Liệu:

1. **Refresh browser** - F5 hoặc Ctrl+Shift+R
2. **Mở DevTools** - F12
3. **Console → Gõ**: `console.log(currentPOI)`
4. **Kiểm tra**: Các trường `descriptionRu`, `descriptionJa`, `descriptionZh` có giá trị không?
5. **Test**: Click nút Русский → ABOUT text phải thay đổi thành tiếng Nga

---

## 📊 Kết Quả Mong Đợi:

```json
{
  "id": 1,
  "name": "Óc Oanh",
  "description": "Quán ốc huyền thoại...",
  "descriptionEn": "Óc Oanh is the busiest...",
  "descriptionJa": "オックオアンはビンカインで...",
  "descriptionRu": "Óc Oanh - самый многолюдный...",
  "descriptionZh": "Óc Oanh是永汉最繁忙..."
}
```

Khi này:
- Click 🇻🇳 → Hiển thị `description` (Tiếng Việt)
- Click 🇬🇧 → Hiển thị `descriptionEn` (Tiếng Anh)
- Click 🇷🇺 → Hiển thị `descriptionRu` (Tiếng Nga) ✅
- Click 🇯🇵 → Hiển thị `descriptionJa` (Tiếng Nhật) ✅
- Click 🇨🇳 → Hiển thị `descriptionZh` (Tiếng Trung) ✅

---

## 💡 Ghi Chú:

- Nếu bạn muốn translations **tự động**, hãy dùng Azure Translator hoặc Google Translate API
- Nếu muốn **manual**, hãy hire translator cho từng ngôn ngữ
- Nếu muốn **nhanh**, copy-paste dữ liệu từ file này

