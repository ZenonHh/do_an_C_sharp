# 🧪 Testing Guide - Language Switching & Listening Tracking

## ✅ What Should Work Now

### 1. **All 5 Languages Displaying Correctly**
The model now has all required fields. The frontend `getDescriptionByLanguage()` function will:
- Return Vietnamese (`description`)
- Return English (`descriptionEn`)
- Return Japanese (`descriptionJa`)
- Return Russian (`descriptionRu`)
- Return Chinese (`descriptionZh`)

### 2. **Language Switching UI**
- All 5 language buttons display and highlight correctly
- Clicking buttons updates the `currentLanguage` variable
- Page re-renders with new language description (if database has data)

### 3. **Audio Playing**
- Audio narration plays in selected language
- Uses Web Speech API with correct language codes
- Still tracks listening history on backend

### 4. **Listening Tracking (Hidden from UI)**
- Still calls `/api/qrscans/track-listen` when audio plays
- Updates play count on backend
- Admin panel "Top Quán Được Nghe Nhiều Nhất" still updates
- NO listening history list displayed on page

---

## 🔍 Testing Steps

### Step 1: Verify Model Has New Fields ✅
```csharp
// Models/AudioPOI.cs should have:
public string Description { get; set; } = string.Empty;
public string DescriptionEn { get; set; } = string.Empty;
public string DescriptionJa { get; set; } = string.Empty;
public string DescriptionRu { get; set; } = string.Empty;
public string DescriptionZh { get; set; } = string.Empty;
```
**Status:** ✅ Already updated in `DoAnCSharp.AdminWeb/Models/AudioPOI.cs`

---

### Step 2: Populate Test Data

You need to add translations to your database. Update restaurant records to include:

```sql
UPDATE AudioPOI SET
    DescriptionEn = 'Description in English...',
    DescriptionJa = '日本語での説明...',
    DescriptionRu = 'Описание на русском языке...',
    DescriptionZh = '中文描述...'
WHERE Id = 1;  -- Replace with actual restaurant ID
```

Or via database UI:
1. Open database viewer
2. Find restaurant record (e.g., "Óc Oanh")
3. Add text for each Description field

**IMPORTANT:** Without database data, all languages will fall back to Vietnamese

---

### Step 3: Test Language Switching

1. Open poi-detail.html in browser with restaurant ID
   ```
   Example: http://localhost:5000/poi-detail.html?poiId=1
   ```

2. Verify page loads with Vietnamese description

3. Click English button (🇬🇧 English)
   - ✅ Button should highlight
   - ✅ Description should change to English (if DescriptionEn populated)
   - ✅ No listening history section visible

4. Click Japanese button (🇯🇵 日本語)
   - ✅ Button should highlight
   - ✅ Description should change to Japanese (if DescriptionJa populated)

5. Click Russian button (🇷🇺 Русский)
   - ✅ Button should highlight
   - ✅ Description should change to Russian (if DescriptionRu populated)

6. Click Chinese button (🇨🇳 中文)
   - ✅ Button should highlight
   - ✅ Description should change to Chinese (if DescriptionZh populated)

7. Click Vietnamese button (🇻🇳 Tiếng Việt)
   - ✅ Button should highlight
   - ✅ Description should change back to Vietnamese

**Expected Result:** All buttons work, and descriptions change (if translations exist in DB)

---

### Step 4: Test Audio Playback in Different Languages

1. Select Vietnamese language
2. Click "▶ Play Audio" button
   - ✅ Audio should play in Vietnamese
   - ✅ Button text changes to "⏸ Pause"
   - ✅ Check browser console for "Audio started, tracking listening history"
   - ✅ Listen in admin panel to confirm listening count increased

3. Switch to English
   - ✅ Audio stops (if already playing)
   - ✅ Click Play again - audio plays in English
   - ✅ Listening count increments again

4. Repeat for Japanese, Russian, Chinese
   - ✅ All languages should support audio narration
   - ✅ Each play should increment backend counter

---

### Step 5: Verify Listening History NOT Displayed

1. Open poi-detail.html in browser
2. Scroll down entire page
   - ❌ Should NOT see "🎵 LISTENING HISTORY" section
   - ❌ Should NOT see history items listed
   - ✅ Should see "Footer" with "FoodTour Vĩnh Khánh" at bottom

3. Click Play multiple times
   - ❌ Still no history section appears
   - ✅ Backend tracking still happens (check admin panel)

---

### Step 6: Verify Backend Tracking Still Works

1. Open Admin Panel
2. Go to top restaurants or analytics section
3. Click Play audio multiple times on poi-detail page
4. Return to Admin Panel
   - ✅ Restaurant play count should increase
   - ✅ "Top Quán Được Nghe Nhiều Nhất" ranking updates

---

## 🐛 Troubleshooting

### **Issue: All languages show Vietnamese text**
**Cause:** Database doesn't have translations populated
**Fix:** Add English, Japanese, Russian, Chinese descriptions to database records

### **Issue: Language button doesn't highlight**
**Cause:** CSS issue or JavaScript error
**Fix:** Check browser console for JavaScript errors

### **Issue: Audio doesn't play**
**Cause:** Web Speech API not supported in browser
**Fix:** Use Chrome, Edge, or Firefox (Safari limited support)

### **Issue: Backend tracking not working**
**Cause:** API endpoint error or device limit exceeded
**Fix:** Check browser console Network tab, look at `/api/qrscans/track-listen` response

### **Issue: Listening history still shows**
**Cause:** Old HTML cached, or wrong file edited
**Fix:** Hard refresh (Ctrl+Shift+R), check poi-detail.html contains no "LISTENING HISTORY" section

---

## 📋 Build & Deployment Checklist

- ✅ Code builds without errors
- ✅ Model has 5 language fields
- ✅ Frontend language switching code ready
- ✅ Listening history UI removed
- ✅ Listening tracking calls still work
- ⏳ Database populated with translations (YOUR ACTION NEEDED)
- ⏳ All 5 languages tested (YOUR ACTION NEEDED)
- ⏳ Backend tracking verified (YOUR ACTION NEEDED)

---

## 🎯 Success Criteria

When all tests pass:
- ✅ Vietnamese, English, Japanese, Russian, Chinese all display correct text
- ✅ Audio plays in all 5 languages
- ✅ No listening history displayed on page
- ✅ Backend play count increments (visible in admin panel)
- ✅ No console errors
- ✅ Ready for production deployment

