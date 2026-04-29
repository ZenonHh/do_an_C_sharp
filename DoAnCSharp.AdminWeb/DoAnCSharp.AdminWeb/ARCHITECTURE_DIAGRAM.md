# 🏗️ Architecture Diagram - Final Implementation

## **Frontend User Flow**

```
┌─────────────────────────────────────────────────────────────────────┐
│                         POI Detail Page                              │
│                      (poi-detail.html)                              │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                    ┌────────────┼────────────┐
                    │            │            │
           ┌────────▼────┐  ┌────▼────┐  ┌───▼─────┐
           │   Language  │  │  Audio  │  │ Back    │
           │  Switching  │  │  Play   │  │ Button  │
           │  (5 btns)   │  │         │  │         │
           └────────┬────┘  └────┬────┘  └─────────┘
                    │            │
        ┌───────────▼────────┐   │
        │ switchLanguage()   │   │
        │                    │   │
        │ currentLanguage    │   │
        │ = selected lang    │   │
        └───────────┬────────┘   │
                    │            │
        ┌───────────▼────────────▼───────────┐
        │     renderPOIDetails()              │
        │  (Re-render with new language)      │
        └──────────────┬──────────────────────┘
                       │
        ┌──────────────▼──────────────┐
        │ getDescriptionByLanguage()  │
        │                             │
        │ currentPOI.description      │ ← Vietnamese
        │ currentPOI.descriptionEn    │ ← English
        │ currentPOI.descriptionJa    │ ← Japanese (NEW)
        │ currentPOI.descriptionRu    │ ← Russian (NEW)
        │ currentPOI.descriptionZh    │ ← Chinese (NEW)
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │    Update DOM with text      │
        │    (Description card)        │
        └─────────────────────────────┘
```

---

## **Audio Playback & Tracking Flow**

```
┌─────────────────────────────────────────────────────────────────────┐
│                    User clicks "▶ Play Audio"                        │
└─────────────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │  playAudio()      │
                    │                   │
                    │ Get description   │
                    │ by current lang   │
                    └─────────┬─────────┘
                              │
                    ┌─────────▼──────────────┐
                    │ Create SpeechUtterance│
                    │                       │
                    │ utterance.lang =      │
                    │   vi-VN               │
                    │   en-US               │
                    │   ja-JP               │
                    │   ru-RU               │
                    │   zh-CN               │
                    └─────────┬──────────────┘
                              │
                    ┌─────────▼──────────────┐
                    │ utterance.onstart()   │
                    │                       │
                    │ trackListeningHistory()
                    └─────────┬──────────────┘
                              │
                    ┌─────────▼──────────────┐
                    │ POST to Backend:       │
                    │ /api/qrscans/          │
                    │ track-listen           │
                    │                        │
                    │ {deviceId, poiId}      │
                    └─────────┬──────────────┘
                              │
                    ┌─────────▼──────────────┐
                    │ QRScansController      │
                    │                        │
                    │ - Increment scanCount  │
                    │ - Save to database     │
                    │ - Record play history  │
                    └─────────┬──────────────┘
                              │
                    ┌─────────▼──────────────┐
                    │ Admin Panel Updates:   │
                    │ "Top Quán Được Nghe   │
                    │ Nhiều Nhất" ranking    │
                    └────────────────────────┘
```

---

## **Database Schema**

```
┌─────────────────────────────────────┐
│         AudioPOI Table              │
├─────────────────────────────────────┤
│ Id (Primary Key)                    │
│ Name (String)                       │
│ Address (String)                    │
│ Description (String) ← Vietnamese   │
│ DescriptionEn (String) ← English    │ NEW ✅
│ DescriptionJa (String) ← Japanese   │ NEW ✅
│ DescriptionRu (String) ← Russian    │ NEW ✅
│ DescriptionZh (String) ← Chinese    │ NEW ✅
│ Lat (Double)                        │
│ Lng (Double)                        │
│ Radius (Int)                        │
│ Priority (Int)                      │
│ ImageAsset (String)                 │
│ QRCode (String)                     │
│ CreatedAt (DateTime)                │
│ UpdatedAt (DateTime)                │
└─────────────────────────────────────┘
```

---

## **API Response Structure**

```json
{
  "id": 1,
  "name": "Óc Oanh",
  "address": "123 Nguyễn Huệ, Vĩnh Khánh",
  "description": "Quán ốc huyền thoại đông khách nhất Vĩnh Khánh, nổi tiếng với nước dùng đậm đà và thịt ốc ngon tuyệt vời.",
  "descriptionEn": "The legendary and busiest escargot restaurant in Vinh Khanh, famous for its rich broth and delicious snail meat.",
  "descriptionJa": "ビンカインで最も混雑している伝説のカタツムリレストラン、濃厚なスープと美味しいカタツムリの肉で有名です。",
  "descriptionRu": "Легендарный и самый многолюдный ресторан улиток в Винь Кане, известный своим насыщенным бульоном и вкусным мясом улиток.",
  "descriptionZh": "永汉最繁忙的传奇蜗牛餐厅，以浓郁的汤和美味的蜗牛肉而闻名。",
  "rating": 5.0,
  "imageUrl": "/images/restaurants/snail.jpg",
  "qrCode": "POI_1",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

## **Frontend Language State Machine**

```
                        ┌──────────────┐
                        │ Page Loaded  │
                        │ currentLang  │
                        │ = 'vi' (VI)  │
                        └──────┬───────┘
                               │
           ┌───────────────────┼───────────────────┐
           │                   │                   │
      User clicks           User clicks        User clicks
      English btn           Russian btn        Chinese btn
           │                   │                   │
      ┌────▼─────┐        ┌────▼─────┐        ┌────▼─────┐
      │ currentLang = 'en'│ currentLang = 'ru'│ currentLang = 'zh'
      │ getDesc() → EN   │ getDesc() → RU   │ getDesc() → ZH
      │ Update UI        │ Update UI        │ Update UI
      └────┬─────┘        └────┬─────┘        └────┬─────┘
           │                   │                   │
           └───────────────────┼───────────────────┘
                               │
                        ┌──────▼───────┐
                        │ Cycle Complete
                        │ Click other lang
                        │ Repeat...
                        └──────────────┘
```

---

## **UI Layout - After Changes**

```
┌────────────────────────────────────────────────┐
│  ← Back Button    [Hero Image]                  │
├────────────────────────────────────────────────┤
│  ⭐ Restaurant Name              Rating: 5.0    │
│  📍 Address Information                         │
├────────────────────────────────────────────────┤
│  🌐 LANGUAGE                                    │
│  [VI]  [EN]  [JA]  [RU]  [ZH]                  │
│  (All buttons functional)                      │
├────────────────────────────────────────────────┤
│  📚 ABOUT                                       │
│  [Description in selected language]             │
│  Fallback to Vietnamese if translation missing │
├────────────────────────────────────────────────┤
│  🎧 AUDIO GUIDE              ✓ Free Trial      │
│  [▶ Play Audio]                                │
│  [📥 Download App]                             │
├────────────────────────────────────────────────┤
│  FoodTour Vĩnh Khánh — Powered by FoodTour     │
│  (Footer)                                      │
└────────────────────────────────────────────────┘

REMOVED: ❌ Listening History Section
KEPT:    ✅ Audio Play Button
         ✅ Language Buttons  
         ✅ Backend Tracking (hidden from UI)
```

---

## **What's New vs What Changed**

```
BEFORE                          AFTER
──────────────────────────────────────────────────

Model Fields:                   Model Fields:
- Description (VI)             - Description (VI) ✅
                               - DescriptionEn (EN) ✅ NEW
                               - DescriptionJa (JA) ✅ NEW
                               - DescriptionRu (RU) ✅ NEW
                               - DescriptionZh (ZH) ✅ NEW

UI Display:                    UI Display:
- Listening history shown     - Listening history hidden ✅
- VI & EN languages work      - All 5 languages work ✅
- JA, RU, ZH show VI text ❌  - JA, RU, ZH show correct ✅
                              (if database has data)

Backend:                       Backend:
- Tracking working            - Tracking still working ✅
- Analytics updating          - Analytics still updating ✅

Code Quality:                  Code Quality:
- 2 languages working         - All infrastructure ready ✅
- History cluttering UI       - Clean, minimal UI ✅
- Hard to add new languages   - Easy to add new languages ✅
```

---

## **Implementation Summary Table**

| Component | Status | Location | Details |
|-----------|--------|----------|---------|
| Model Support | ✅ NEW | `Models/AudioPOI.cs` | 5 language fields added |
| Frontend UI | ✅ CLEANED | `wwwroot/poi-detail.html` | History section removed |
| Backend Tracking | ✅ PRESERVED | `Controllers/QRScansController.cs` | Still functional |
| Language Switching | ✅ READY | `getDescriptionByLanguage()` | Works for all 5 |
| Audio Playback | ✅ READY | `playAudio()` | All 5 languages supported |
| Analytics | ✅ READY | `trackListeningHistory()` | Still updates ranking |
| Build Status | ✅ SUCCESS | All files | Compiles without errors |
| Database Ready | ⏳ PENDING | `AudioPOI` table | Needs translations |

---

## **Next Steps Flowchart**

```
┌─────────────────────────────┐
│ Code Complete ✅            │
│ Build Successful ✅         │
└────────────┬────────────────┘
             │
             ▼
    ┌─────────────────┐
    │ Add Translations│
    │ to Database     │
    │                 │
    │ EN, JA, RU, ZH  │
    │ for all POIs    │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ Test Each       │
    │ Language:       │
    │ - VI ✓          │
    │ - EN            │
    │ - JA            │
    │ - RU            │
    │ - ZH            │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ Verify Backend  │
    │ Tracking:       │
    │ Admin Panel     │
    │ Updates ✓       │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ Deploy to Prod  │
    │ 🚀 Ready!      │
    └─────────────────┘
```

