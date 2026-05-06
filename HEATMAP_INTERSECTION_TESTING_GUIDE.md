# 🎯 Heatmap Intersection Testing Guide
## Audio Prioritization Between Multiple Restaurants

---

## 📋 Overview

This guide covers testing the audio prioritization system when a user stands in the **intersection of two or more restaurant zones**. The system should prioritize audio from the **more popular restaurant** (higher heat weight) even when distances are equal.

**Key Change**: Three intersection test scenarios are now available to verify consistent behavior.

---

## 🧪 Three Test Intersection Scenarios

### **Test Set 1: Ốc Vũ (Priority 1) vs Quán Nướng Chilli (Priority 2)**
**Status**: ✅ Previously tested

| Restaurant | Coordinates | Radius | Priority | Heat Weight |
|---|---|---|---|---|
| **Ốc Vũ** | 10.7578, 106.7058 | 40m | 1 | Low |
| **Quán Nướng Chilli** | 10.7586, 106.7055 | 50m | 2 | High |
| **Intersection Point** | **10.7582, 106.7056** | N/A | N/A | **Chilli should win** |

**What to expect**:
- Standing at intersection point (10.7582, 106.7056)
- Both restaurants' zones overlap
- Audio from **Quán Nướng Chilli** plays (higher Priority = 2)
- Distance to both is roughly equal (~30-40m from each)

**Test using the test button**:
1. Click the **Test Cycle Button** (🏠 icon) repeatedly
2. Cycle through: 🦪 (OC VU) → 🔥 (CHILLI) → 🎯 (INTERSECTION 1)
3. Verify Chilli audio plays at intersection point

---

### **Test Set 2: Ốc Oanh (Priority 1) vs Khèn BBQ (Priority 2)** ✨ NEW
**Status**: 🆕 New scenario

| Restaurant | Coordinates | Radius | Priority | Heat Weight |
|---|---|---|---|---|
| **Ốc Oanh** | 10.7595, 106.7045 | 40m | 1 | Low |
| **Khèn BBQ - Nướng Ngói** | 10.7592, 106.7038 | 40m | 2 | High |
| **Intersection Point** | **10.7594, 106.7041** | N/A | N/A | **BBQ should win** |

**What to expect**:
- Standing at intersection point (10.7594, 106.7041)
- Both restaurants have same radius (40m) but different priorities
- Audio from **Khèn BBQ** plays (higher Priority = 2)
- System correctly prioritizes by heat weight over distance

**Test using the test button**:
1. Continue cycling: 🐚 (OC OANH) → 🍖 (KHEN BBQ) → 🎯 (INTERSECTION 2)
2. Verify BBQ audio plays at intersection point
3. Compare behavior with Test Set 1

---

### **Test Set 3: Ốc Thảo (Priority 1) vs Lẩu Dê Dũng Mập (Priority 2)** ✨ NEW
**Status**: 🆕 New scenario

| Restaurant | Coordinates | Radius | Priority | Heat Weight |
|---|---|---|---|---|
| **Ốc Thảo** | 10.7590, 106.7042 | 40m | 1 | Low |
| **Lẩu Dê Dũng Mập** | 10.7602, 106.7049 | 50m | 2 | High |
| **Intersection Point** | **10.7596, 106.7045** | N/A | N/A | **Lau De should win** |

**What to expect**:
- Standing at intersection point (10.7596, 106.7045)
- Lẩu Dê has larger radius (50m) AND higher priority (2)
- Audio from **Lẩu Dê Dũng Mập** plays (strongest heat weight)
- System handles asymmetric radius zones correctly

**Test using the test button**:
1. Continue cycling: 🐌 (OC THAO) → 🐐 (LAU DE) → 🎯 (INTERSECTION 3)
2. Verify Lau De audio plays at intersection point
3. Notice the strongest heat influence

---

## 🔬 How to Run the Tests

### **Method 1: Using Debug Test Cycle Button** (Recommended)
1. Run the app in **DEBUG mode** on your Android device
2. Open the Map screen
3. Look for the **Test Cycle Button** in the bottom-left corner (shows 🏠 initially)
4. **Tap repeatedly** to cycle through all test points:
   - 🏠 → Reset (no audio)
   - 🦪 (Test Set 1: Ốc Vũ)
   - 🔥 (Test Set 1: Chilli)
   - 🎯 (Test Set 1: Intersection)
   - 🐚 (Test Set 2: Ốc Oanh)
   - 🍖 (Test Set 2: BBQ)
   - 🎯 (Test Set 2: Intersection)
   - 🐌 (Test Set 3: Ốc Thảo)
   - 🐐 (Test Set 3: Lẩu Dê)
   - 🎯 (Test Set 3: Intersection)

5. **Monitor Debug Output** to see which restaurant is selected:
   ```
   [TEST] Cycle → 🎯 INTERSECTION 1 at (10.7582,106.7056)
   [RADAR] Overlap (heat-priority) winner: QUÁN NƯỚNG CHILLI (w=..., d=...m) | ỐC VŨ (w=..., d=...m)
   ```

### **Method 2: Manual GPS Simulation**
1. Use Android emulator with GPS spoofing
2. Set location to intersection coordinates manually
3. Observe audio behavior in real-time

---

## 🔍 What to Verify

### **Test Set 1 (OC Vu vs Chilli)**
- [ ] At 10.7582, 106.7056 → **Chilli audio plays** (Priority 2 > Priority 1)
- [ ] Distance is roughly equal for both restaurants
- [ ] Debug log shows Chilli wins based on HeatWeight

### **Test Set 2 (Oanh vs BBQ)**
- [ ] At 10.7594, 106.7041 → **BBQ audio plays** (Priority 2 > Priority 1)
- [ ] Equal radii (40m) don't affect prioritization
- [ ] Heat weight comparison still works with symmetric zones

### **Test Set 3 (Thao vs Lau De)**
- [ ] At 10.7596, 106.7045 → **Lau De audio plays** (Priority 2 > Priority 1)
- [ ] Asymmetric radii (40m vs 50m) don't override priority
- [ ] Strongest heat influence dominates

---

## 📊 Priority Weight Calculation

The system calculates heat weight as:
```
BaseWeight = Priority switch {
    1 => 6,      // Snail restaurants
    2 => 3,      // Grilled/Hot pot
    3 => 1       // Street food
}

TotalWeight = BaseWeight + PlayCount
```

**Example**:
- Ốc Vũ (Priority 1) + 0 plays = 6 + 0 = **6 weight**
- Quán Nướng Chilli (Priority 2) + 2 plays = 3 + 2 = **5 weight** → Still loses to 6!

But with the heatmap enabled and popularity reflected in display:
- The heatmap shows visual intensity based on weight
- Audio selection picks **highest weight among overlapping zones**

---

## 🗺️ How Audio Prioritization Works (Code)

**File**: [Views/MapPage.xaml.cs](Views/MapPage.xaml.cs#L307-L329)

```csharp
if (inRangePois.Count > 1)
{
    // Vùng giao nhau: ưu tiên theo độ phổ biến (HeatWeight), khoảng cách chỉ là tie-breaker
    var ranked = inRangePois
        .OrderByDescending(x => x.Poi.HeatWeight)      // ← Rank by heat (popularity)
        .ThenBy(x => x.DistanceM)                      // ← Then by distance if equal heat
        .ToList();
    poi = ranked[0].Poi;  // ← Select highest priority restaurant
}
```

**Selection Order**:
1. **HeatWeight (Primary)** → Higher weight = Higher priority
2. **Distance (Secondary)** → If weights tied, closer restaurant wins

---

## 📱 Expected Debug Output

When standing at an intersection point, you should see:

```
[RADAR] Overlap (heat-priority) winner: QUÁN NƯỚNG CHILLI (w=5, d=35.2m) | ỐC VŨ (w=6, d=32.1m)
```

**Interpretation**:
- Two restaurants detected in overlapping zones
- Chilli weight=5, distance=35.2m
- OC Vu weight=6, distance=32.1m
- Since weights are different: **weight wins** → Check which is higher
- If `w=5 < w=6` → OC Vu audio plays (wins on heat)
- If `w=5 > w=6` → Chilli audio plays (wins on heat)

---

## 🎵 Audio Quality Checks

For each test set, verify:

1. **No duplicate audio** - Only one restaurant plays at a time
2. **Smooth transitions** - No stuttering when moving between zones
3. **Correct language** - Audio translates properly (based on app language)
4. **Description accuracy** - Restaurant details appear correctly

---

## 🚀 Next Steps

1. **Run all three test sets** to verify consistent behavior
2. **Document results** in test report
3. **If any test fails**:
   - Check `HeatWeight` calculation in `GetPOIPlayCountsAsync()`
   - Verify `CheckGeofenceAndPlayAudio()` ranking logic
   - Review `PlayAudioAlert()` to ensure only one POI plays
4. **Expand coverage** - Add more test cases with different priority combinations

---

## 📝 Test Report Template

```markdown
### Test Execution: [DATE]
Environment: [Emulator/Device Model]
App Version: [Version]

**Test Set 1 (OC Vu vs Chilli)**: PASS / FAIL
- Details: ...

**Test Set 2 (Oanh vs BBQ)**: PASS / FAIL  
- Details: ...

**Test Set 3 (Thao vs Lau De)**: PASS / FAIL
- Details: ...

**Overall Result**: PASS / FAIL
**Notes**: ...
```

---

## 🔗 Related Files

- [MapPage.xaml.cs](Views/MapPage.xaml.cs) - Audio logic & test infrastructure
- [map.html](Resources/Raw/map.html) - Heatmap visualization
- [DatabaseService.cs](Services/DatabaseService.cs) - Weight calculation
- [AudioPOI.cs](Models/AudioPOI.cs) - Heat weight property

