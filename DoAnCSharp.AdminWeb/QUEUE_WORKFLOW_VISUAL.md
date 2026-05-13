# 🎯 QUEUE WORKFLOW - VISUAL GUIDE

## Workflow Khi Nhiều Người Quét Master QR Cùng Lúc

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    MASTER QR CODE ĐƯỢC QUÉT                             │
│              (Phố Ẩm Thực Vĩnh Khánh - Lối Vào)                        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │   QRScansController           │
                    │   /qr/FOODSTREET_VINHKHANH   │
                    └───────────────┬───────────────┘
                                    │
                    ┌───────────────▼───────────────┐
                    │  Check: Paid or Free User?    │
                    │  (MaxScans > 5 = Paid)        │
                    └───────────────┬───────────────┘
                                    │
                    ┌───────────────▼───────────────┐
                    │  QRQueueService.Enqueue()     │
                    │  Priority: Paid=10, Free=1    │
                    └───────────────┬───────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│ < 10 Active   │          │ 10-110 Total  │          │ > 110 Total   │
│ Requests      │          │ Requests      │          │ Requests      │
└───────┬───────┘          └───────┬───────┘          └───────┬───────┘
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│ PROCESS       │          │ QUEUE         │          │ REJECT        │
│ IMMEDIATELY   │          │ (Wait)        │          │ (Queue Full)  │
└───────┬───────┘          └───────┬───────┘          └───────┬───────┘
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐          ┌───────────────┐          ┌───────────────┐
│ User sees:    │          │ User sees:    │          │ User sees:    │
│ Restaurant    │          │ Waiting Page  │          │ "Overload"    │
│ List Page     │          │ with Timer    │          │ Page          │
│ (Instant)     │          │ & Position    │          │ [Try Again]   │
└───────────────┘          └───────┬───────┘          └───────────────┘
                                    │
                    ┌───────────────▼───────────────┐
                    │  Auto-refresh every 2s        │
                    │  Check queue position         │
                    └───────────────┬───────────────┘
                                    │
                    ┌───────────────▼───────────────┐
                    │  When slot available:         │
                    │  Auto-redirect to list page   │
                    └───────────────────────────────┘
```

## Admin Panel View - Real-time

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ADMIN PANEL - Tab: ⏳ Hàng Đợi QR                                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  📊 STATS (Auto-refresh every 2s)                                       │
│  ┌──────────┬──────────┬──────────┬──────────┐                        │
│  │ ⚡ Active │ ⏳ Queue  │ ✅ Done   │ ❌ Reject │                        │
│  │    10    │    25    │   156    │    3     │                        │
│  │  /10 max │  /100    │ 0.5 r/s  │  Full    │                        │
│  └──────────┴──────────┴──────────┴──────────┘                        │
│                                                                          │
│  ⚡ ĐANG XỬ LÝ (10)                    [████████████████████] 100%      │
│  ┌────────────────────────────────────────────────────────────────┐   │
│  │ ⚡ ...abc123  POI_XYZ  2.5s  14:30:15  ⭐ Paid                  │   │
│  │ ⚡ ...def456  POI_ABC  1.8s  14:30:17  🆓 Free                  │   │
│  │ ⚡ ...ghi789  POI_DEF  3.2s  14:30:14  ⭐ Paid                  │   │
│  │ ... (7 more)                                                    │   │
│  └────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ⏳ HÀNG ĐỢI (25)                      [█████░░░░░░░░░░░░░░░] 25%      │
│  ┌────────────────────────────────────────────────────────────────┐   │
│  │ #1  ...xyz123  POI_ABC  5.2s  ~15s  ⭐ Paid                     │   │
│  │ #2  ...uvw456  POI_DEF  4.8s  ~18s  ⭐ Paid                     │   │
│  │ #3  ...rst789  POI_GHI  3.5s  ~21s  🆓 Free                     │   │
│  │ ... (22 more)                                                   │   │
│  └────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  🧪 TEST: [➕ Free] [⭐ Paid] [🔥 Add 10]                               │
└─────────────────────────────────────────────────────────────────────────┘
```

## Timeline Example: 20 Users Scan Master QR

```
Time    Event                           Active  Queue   Admin Panel View
────────────────────────────────────────────────────────────────────────────
14:30:00  Users 1-10 scan QR            10      0       ⚡ 10 processing
                                                         ⏳ 0 waiting

14:30:05  Users 11-15 scan QR           10      5       ⚡ 10 processing
                                                         ⏳ 5 waiting (#1-#5)
                                                         Users 11-15 see:
                                                         "Position #1-#5"
                                                         "Wait ~15s"

14:30:10  Users 16-20 scan QR           10      10      ⚡ 10 processing
                                                         ⏳ 10 waiting (#1-#10)
                                                         Users 16-20 see:
                                                         "Position #6-#10"
                                                         "Wait ~30s"

14:30:30  Users 1-3 complete            7       10      ⚡ 7 processing
          Users 11-13 start                             ⏳ 7 waiting (#1-#7)
                                                         Users 11-13:
                                                         Auto-redirect!

14:30:45  Users 4-7 complete            6       7       ⚡ 6 processing
          Users 14-17 start                             ⏳ 3 waiting (#1-#3)
                                                         Users 14-17:
                                                         Auto-redirect!

14:31:00  All complete                  0       0       ⚡ 0 processing
                                                         ⏳ 0 waiting
                                                         ✅ 20 total processed
```

## Priority Queue Example: Paid vs Free

```
Scenario: 10 Free users in queue, 3 Paid users scan QR

BEFORE:
┌─────────────────────────────────────────┐
│ Queue (10 users)                        │
├─────────────────────────────────────────┤
│ #1  Free User A                         │
│ #2  Free User B                         │
│ #3  Free User C                         │
│ #4  Free User D                         │
│ #5  Free User E                         │
│ #6  Free User F                         │
│ #7  Free User G                         │
│ #8  Free User H                         │
│ #9  Free User I                         │
│ #10 Free User J                         │
└─────────────────────────────────────────┘

3 Paid Users Scan QR ⭐⭐⭐

AFTER (Priority Re-ordering):
┌─────────────────────────────────────────┐
│ Queue (13 users)                        │
├─────────────────────────────────────────┤
│ #1  ⭐ Paid User 1  (Priority 10)       │
│ #2  ⭐ Paid User 2  (Priority 10)       │
│ #3  ⭐ Paid User 3  (Priority 10)       │
│ #4  🆓 Free User A  (Priority 1)        │
│ #5  🆓 Free User B  (Priority 1)        │
│ #6  🆓 Free User C  (Priority 1)        │
│ #7  🆓 Free User D  (Priority 1)        │
│ #8  🆓 Free User E  (Priority 1)        │
│ #9  🆓 Free User F  (Priority 1)        │
│ #10 🆓 Free User G  (Priority 1)        │
│ #11 🆓 Free User H  (Priority 1)        │
│ #12 🆓 Free User I  (Priority 1)        │
│ #13 🆓 Free User J  (Priority 1)        │
└─────────────────────────────────────────┘

Admin Panel Shows:
- Paid users (green background)
- Free users (gray background)
- Paid users at top of queue
```

## User Experience Flow

```
┌─────────────────────────────────────────────────────────────────┐
│  USER SCANS MASTER QR CODE                                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
         ┌───────────────┴───────────────┐
         │                               │
         ▼                               ▼
┌─────────────────┐            ┌─────────────────┐
│ Scenario A:     │            │ Scenario B:     │
│ System Not Busy │            │ System Busy     │
│ (< 10 active)   │            │ (≥ 10 active)   │
└────────┬────────┘            └────────┬────────┘
         │                               │
         ▼                               ▼
┌─────────────────┐            ┌─────────────────┐
│ ✅ INSTANT      │            │ ⏳ WAITING PAGE │
│                 │            │                 │
│ [Restaurant     │            │ ⏳ Đang Chờ     │
│  List Page]     │            │                 │
│                 │            │    #5           │
│ • Quán A        │            │ Vị trí của bạn  │
│ • Quán B        │            │                 │
│ • Quán C        │            │ Thời gian chờ:  │
│ • ...           │            │    ~15s         │
│                 │            │                 │
│ [🔊 Play Audio] │            │ [Spinner...]    │
└─────────────────┘            └────────┬────────┘
                                        │
                                        │ Auto-refresh
                                        │ every 2s
                                        │
                                        ▼
                               ┌─────────────────┐
                               │ When your turn: │
                               │ Auto-redirect → │
                               └────────┬────────┘
                                        │
                                        ▼
                               ┌─────────────────┐
                               │ [Restaurant     │
                               │  List Page]     │
                               └─────────────────┘
```

## System Capacity Visualization

```
CAPACITY LEVELS:

Level 1: NORMAL (0-10 requests)
┌──────────────────────────────────────┐
│ ████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25% Active
│ All users get instant access         │
│ No queue needed                      │
└──────────────────────────────────────┘

Level 2: BUSY (10-50 requests)
┌──────────────────────────────────────┐
│ ████████████████████░░░░░░░░░░░░░░░░ │ 100% Active
│ ████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 40% Queue
│ Queue active, wait time 10-30s       │
│ Good user experience                 │
└──────────────────────────────────────┘

Level 3: HEAVY (50-100 requests)
┌──────────────────────────────────────┐
│ ████████████████████████████████████ │ 100% Active
│ ████████████████████████████████████ │ 100% Queue
│ Long wait times (1-3 minutes)        │
│ Consider scaling up                  │
└──────────────────────────────────────┘

Level 4: OVERLOAD (> 100 requests)
┌──────────────────────────────────────┐
│ ████████████████████████████████████ │ 100% Active
│ ████████████████████████████████████ │ 100% Queue
│ ❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌ │ Rejecting
│ Users see "System Overload" message  │
│ URGENT: Scale up or increase limits  │
└──────────────────────────────────────┘
```

## Quick Reference

### For Admin:
- **Tab Location:** Admin Panel → "⏳ Hàng Đợi QR" (2nd tab)
- **Auto-refresh:** Every 2 seconds
- **Test Buttons:** Add Free/Paid/10 requests
- **Visual Indicators:** 
  - Green = Paid users
  - Gray = Free users
  - Blue progress = Active
  - Orange progress = Queue

### For Users:
- **< 10 active:** Instant access
- **10-110 total:** Wait in queue (see position & timer)
- **> 110 total:** "System overload" message
- **Paid users:** Jump to front of queue

### Key Metrics:
- **Max Concurrent:** 10 requests
- **Max Queue:** 100 requests
- **Timeout:** 30 seconds
- **Avg Processing:** ~3 seconds per request
- **Priority:** Paid (10) > Free (1)

---

**Hệ thống đã sẵn sàng! Mở admin panel và test ngay!** 🚀
