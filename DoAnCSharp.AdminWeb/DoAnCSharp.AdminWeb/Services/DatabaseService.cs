using SQLite;
using DoAnCSharp.AdminWeb.Models;
using System.Collections.Generic;

namespace DoAnCSharp.AdminWeb.Services;

public class DatabaseService
{
    private const string DbFileName = "VinhKhanhTour_Full.db3";
    private SQLiteAsyncConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task InitAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_connection != null) return;

            string dbPath;
            
            // Check for custom database path from environment variable
            var customPath = Environment.GetEnvironmentVariable("VINHKHANH_DB_PATH");
            if (!string.IsNullOrEmpty(customPath) && Directory.Exists(customPath))
            {
                dbPath = Path.Combine(customPath, DbFileName);
            }
            else
            {
                // Default path - same as MAUI app
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VinhKhanhTour"
                );
                Directory.CreateDirectory(appDataPath);
                dbPath = Path.Combine(appDataPath, DbFileName);
            }

            _connection = new SQLiteAsyncConnection(dbPath);

            // Create tables
            await _connection.CreateTableAsync<AudioPOI>();
            await _connection.CreateTableAsync<User>();
            await _connection.CreateTableAsync<PlayHistory>();
            await _connection.CreateTableAsync<UserPayment>();
            await _connection.CreateTableAsync<QRScanLimit>();
            await _connection.CreateTableAsync<UserStatus>();

            // New tables for enhanced features
            await _connection.CreateTableAsync<UserDevice>();
            await _connection.CreateTableAsync<RestaurantImage>();
            await _connection.CreateTableAsync<AdminUser>();
            await _connection.CreateTableAsync<SystemSetting>();
            await _connection.CreateTableAsync<AuditLog>();
            await _connection.CreateTableAsync<QRCodeSession>();
            await _connection.CreateTableAsync<QRScanRequest>();
            await _connection.CreateTableAsync<DeviceScanLimit>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // POI Operations
    public async Task<List<AudioPOI>> GetAllPOIsAsync()
    {
        await InitAsync();
        return await _connection!.Table<AudioPOI>().ToListAsync();
    }

    public async Task<AudioPOI?> GetPOIByIdAsync(int id)
    {
        await InitAsync();
        return await _connection!.Table<AudioPOI>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<AudioPOI?> GetPOIByQRCodeAsync(string qrCode)
    {
        await InitAsync();
        if (string.IsNullOrWhiteSpace(qrCode))
            return null;

        // Try exact match first
        var poi = await _connection!.Table<AudioPOI>()
            .Where(p => p.QRCode == qrCode)
            .FirstOrDefaultAsync();

        // If not found, try to match by code portion (handle case where QRCode field contains full URL)
        if (poi == null && !qrCode.StartsWith("http"))
        {
            poi = await _connection!.Table<AudioPOI>()
                .Where(p => p.QRCode.Contains(qrCode))
                .FirstOrDefaultAsync();
        }

        return poi;
    }

    public async Task<int> InsertPOIAsync(AudioPOI poi)
    {
        await InitAsync();
        return await _connection!.InsertAsync(poi);
    }

    public async Task<int> UpdatePOIAsync(AudioPOI poi)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(poi);
    }

    public async Task<int> DeletePOIAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<AudioPOI>(id);
    }

    // User Operations
    public async Task<List<User>> GetAllUsersAsync()
    {
        await InitAsync();
        return await _connection!.Table<User>().ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _connection!.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserAsync(User user)
    {
        await InitAsync();
        return await _connection!.InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(user);
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<User>(id);
    }

    // Play History Operations
    public async Task<List<PlayHistory>> GetAllHistoryAsync()
    {
        await InitAsync();
        return await _connection!.Table<PlayHistory>().OrderByDescending(h => h.PlayedAt).ToListAsync();
    }

    public async Task<List<PlayHistory>> GetAllPlayHistoryAsync()
    {
        await InitAsync();
        return await _connection!.Table<PlayHistory>().OrderByDescending(h => h.PlayedAt).ToListAsync();
    }

    public async Task<int> DeleteHistoryAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<PlayHistory>(id);
    }

    // User Payment Operations
    public async Task<UserPayment?> GetUserPaymentByUserIdAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserPayment>().Where(p => p.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<int> UpdateUserPaymentAsync(UserPayment payment)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(payment);
    }

    public async Task<int> InsertUserPaymentAsync(UserPayment payment)
    {
        await InitAsync();
        return await _connection!.InsertAsync(payment);
    }

    // QR Scan Limit Operations
    public async Task<QRScanLimit?> GetQRScanLimitByUserIdAsync(int userId)
    {
        await InitAsync();
        var limit = await _connection!.Table<QRScanLimit>().Where(q => q.UserId == userId).FirstOrDefaultAsync();

        if (limit != null)
        {
            // Reset count if it's a new day
            if (limit.LastResetDate.Date < DateTime.Now.Date)
            {
                limit.ScanCount = 0;
                limit.LastResetDate = DateTime.Now;
                await _connection!.UpdateAsync(limit);
            }
        }
        return limit;
    }

    public async Task<bool> CanUserScanQRAsync(int userId, bool isPaidUser)
    {
        await InitAsync();
        var limit = await GetQRScanLimitByUserIdAsync(userId);

        if (limit == null)
        {
            limit = new QRScanLimit
            {
                UserId = userId,
                IsPaidUser = isPaidUser,
                MaxScans = isPaidUser ? int.MaxValue : 5,
                ScanCount = 0,
                LastResetDate = DateTime.Now
            };
            await InsertQRScanLimitAsync(limit);
            return true;
        }

        if (isPaidUser) return true; // Paid users: unlimited
        return limit.ScanCount < limit.MaxScans; // Free users: limited
    }

    public async Task IncrementQRScanCountAsync(int userId)
    {
        await InitAsync();
        var limit = await GetQRScanLimitByUserIdAsync(userId);
        if (limit != null)
        {
            limit.ScanCount++;
            await _connection!.UpdateAsync(limit);
        }
    }

    public async Task<int> InsertQRScanLimitAsync(QRScanLimit limit)
    {
        await InitAsync();
        return await _connection!.InsertAsync(limit);
    }

    public async Task<int> UpdateQRScanLimitAsync(QRScanLimit limit)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(limit);
    }

    // User Status Operations
    public async Task<UserStatus?> GetUserStatusAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserStatus>().Where(s => s.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<List<UserStatus>> GetAllUserStatusAsync()
    {
        await InitAsync();
        return await _connection!.Table<UserStatus>().ToListAsync();
    }

    public async Task<int> UpdateUserStatusAsync(UserStatus status)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(status);
    }

    public async Task<int> InsertUserStatusAsync(UserStatus status)
    {
        await InitAsync();
        return await _connection!.InsertAsync(status);
    }

    public async Task SetUserOnlineAsync(int userId, bool isOnline, string deviceInfo = "", string ipAddress = "")
    {
        await InitAsync();
        var status = await GetUserStatusAsync(userId);

        if (status == null)
        {
            status = new UserStatus
            {
                UserId = userId,
                IsOnline = isOnline,
                LastActiveAt = DateTime.Now,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress
            };
            await InsertUserStatusAsync(status);
        }
        else
        {
            status.IsOnline = isOnline;
            status.LastActiveAt = DateTime.Now;
            if (!string.IsNullOrEmpty(deviceInfo)) status.DeviceInfo = deviceInfo;
            if (!string.IsNullOrEmpty(ipAddress)) status.IpAddress = ipAddress;
            await UpdateUserStatusAsync(status);
        }
    }

    // Dashboard Operations
    public async Task<OnlineUserSummary> GetDashboardSummaryAsync()
    {
        await InitAsync();

        var allUsers = await GetAllUsersAsync();
        var onlineStatuses = await GetAllUserStatusAsync();
        var onlineUsers = onlineStatuses.Where(s => s.IsOnline).ToList();

        var paidUsers = await _connection!.Table<UserPayment>()
            .Where(p => p.IsPaid == true)
            .ToListAsync();

        // Get today's QR scans
        var today = DateTime.Today;
        var todayScans = await _connection!.Table<PlayHistory>()
            .Where(h => h.PlayedAt >= today)
            .ToListAsync();

        return new OnlineUserSummary
        {
            TotalOnlineUsers = onlineUsers.Count,
            OnlineDevices = onlineUsers.Count,
            ActiveListeningUsers = todayScans.Select(h => h.UserId).Distinct().Count(),
            TotalRegisteredUsers = allUsers.Count,
            TotalPaidUsers = paidUsers.Count,
            TodayQRScans = todayScans.Count,
            OnlineDeviceDetails = new List<UserDevice>()
        };
    }

    public async Task<List<UserDevice>> GetOnlineUsersAsync()
    {
        await InitAsync();

        var onlineStatuses = await GetAllUserStatusAsync();
        var onlineUserIds = onlineStatuses.Where(s => s.IsOnline).Select(s => s.UserId).ToList();

        var devices = new List<UserDevice>();

        foreach (var userId in onlineUserIds)
        {
            var userDevices = await GetUserDevicesAsync(userId);
            foreach (var device in userDevices)
            {
                if (device.IsOnline)
                {
                    devices.Add(device);
                }
            }
        }

        return devices;
    }

    public async Task<(int totalScans, int uniqueUsers, List<(string POIName, int count)> topPOIs)> GetQRActivityTodayAsync()
    {
        await InitAsync();

        var today = DateTime.Today;
        var todayScans = await _connection!.Table<PlayHistory>()
            .Where(h => h.PlayedAt >= today)
            .ToListAsync();

        var uniqueUsers = todayScans.Select(h => h.UserId).Distinct().Count();

        // Top POIs
        var topPOIs = todayScans
            .GroupBy(h => h.POIName)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => (g.Key, g.Count()))
            .ToList();

        return (todayScans.Count, uniqueUsers, topPOIs);
    }

    public async Task<UserPayment?> GetPaymentByUserIdAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserPayment>()
            .Where(p => p.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // ===== NEW FEATURE OPERATIONS =====

    // UserDevice Operations
    public async Task<List<UserDevice>> GetAllUserDevicesAsync()
    {
        await InitAsync();
        return await _connection!.Table<UserDevice>().ToListAsync();
    }

    public async Task<List<UserDevice>> GetUserDevicesAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserDevice>().Where(d => d.UserId == userId).ToListAsync();
    }

    public async Task<UserDevice?> GetUserDeviceByIdAsync(int deviceId)
    {
        await InitAsync();
        return await _connection!.Table<UserDevice>().Where(d => d.Id == deviceId).FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserDeviceAsync(UserDevice device)
    {
        await InitAsync();
        return await _connection!.InsertAsync(device);
    }

    public async Task<int> UpdateUserDeviceAsync(UserDevice device)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(device);
    }

    public async Task<int> DeleteUserDeviceAsync(int deviceId)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<UserDevice>(deviceId);
    }

    // RestaurantImage Operations
    public async Task<List<RestaurantImage>> GetPOIImagesAsync(int restaurantId)
    {
        await InitAsync();
        return await _connection!.Table<RestaurantImage>().Where(i => i.RestaurantId == restaurantId).ToListAsync();
    }

    public async Task<List<RestaurantImage>> GetRestaurantImagesAsync(int restaurantId)
    {
        return await GetPOIImagesAsync(restaurantId);
    }

    public async Task<RestaurantImage?> GetRestaurantImageByIdAsync(int imageId)
    {
        await InitAsync();
        return await _connection!.Table<RestaurantImage>().Where(i => i.Id == imageId).FirstOrDefaultAsync();
    }

    public async Task<int> InsertRestaurantImageAsync(RestaurantImage image)
    {
        await InitAsync();
        return await _connection!.InsertAsync(image);
    }

    public async Task<int> UpdateRestaurantImageAsync(RestaurantImage image)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(image);
    }

    public async Task<int> DeleteRestaurantImageAsync(int imageId)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<RestaurantImage>(imageId);
    }

    // AdminUser Operations
    public async Task<List<AdminUser>> GetAllAdminUsersAsync()
    {
        await InitAsync();
        return await _connection!.Table<AdminUser>().ToListAsync();
    }

    public async Task<AdminUser?> GetAdminUserByIdAsync(int id)
    {
        await InitAsync();
        return await _connection!.Table<AdminUser>().Where(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<AdminUser?> GetAdminUserByUsernameAsync(string username)
    {
        await InitAsync();
        return await _connection!.Table<AdminUser>().Where(a => a.Username == username).FirstOrDefaultAsync();
    }

    public async Task<int> InsertAdminUserAsync(AdminUser admin)
    {
        await InitAsync();
        return await _connection!.InsertAsync(admin);
    }

    public async Task<int> UpdateAdminUserAsync(AdminUser admin)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(admin);
    }

    public async Task<int> DeleteAdminUserAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<AdminUser>(id);
    }

    // SystemSetting Operations
    public async Task<string?> GetSettingValueAsync(string key)
    {
        await InitAsync();
        var setting = await _connection!.Table<SystemSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
        return setting?.Value;
    }

    public async Task<List<SystemSetting>> GetAllSettingsAsync()
    {
        await InitAsync();
        return await _connection!.Table<SystemSetting>().ToListAsync();
    }

    public async Task<int> UpsertSettingAsync(string key, string value, string description = "", string settingType = "string", string updatedBy = "system")
    {
        await InitAsync();
        var existing = await _connection!.Table<SystemSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;
            return await _connection!.UpdateAsync(existing);
        }
        else
        {
            var newSetting = new SystemSetting
            {
                Key = key,
                Value = value,
                Description = description,
                SettingType = settingType,
                UpdatedAt = DateTime.Now,
                UpdatedBy = updatedBy
            };
            return await _connection!.InsertAsync(newSetting);
        }
    }

    // AuditLog Operations
    public async Task<List<AuditLog>> GetAllAuditLogsAsync()
    {
        await InitAsync();
        return await _connection!.Table<AuditLog>().OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<int> InsertAuditLogAsync(AuditLog log)
    {
        await InitAsync();
        return await _connection!.InsertAsync(log);
    }

    public async Task LogAdminActionAsync(int adminUserId, string action, string entityType, int? entityId, string? oldValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null, bool isSuccess = true, string? errorMessage = null)
    {
        var log = new AuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            IPAddress = ipAddress ?? "",
            UserAgent = userAgent ?? "",
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage ?? "",
            CreatedAt = DateTime.Now
        };
        await InsertAuditLogAsync(log);
    }

    // ===== QR CODE SESSION OPERATIONS =====

    public async Task<QRCodeSession?> GetQRCodeSessionByTokenAsync(string token)
    {
        await InitAsync();
        return await _connection!.Table<QRCodeSession>()
            .Where(q => q.SessionToken == token && q.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<QRCodeSession?> GetCurrentQRCodeAsync(int restaurantId)
    {
        await InitAsync();
        var now = DateTime.Now;
        return await _connection!.Table<QRCodeSession>()
            .Where(q => q.RestaurantId == restaurantId && q.IsActive && q.ExpiresAt > now)
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateQRCodeSessionAsync(int restaurantId, int durationMinutes = 5)
    {
        await InitAsync();

        // Deactivate old sessions
        var oldSessions = await _connection!.Table<QRCodeSession>()
            .Where(q => q.RestaurantId == restaurantId && q.IsActive)
            .ToListAsync();

        foreach (var session in oldSessions)
        {
            session.IsActive = false;
            await _connection!.UpdateAsync(session);
        }

        // Create new session
        var newSession = new QRCodeSession
        {
            RestaurantId = restaurantId,
            QRCode = GenerateQRCode(),
            SessionToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(durationMinutes),
            ScanCount = 0,
            IsActive = true,
            LastScannedAt = null,
            LastScannedByUserId = null
        };

        return await _connection!.InsertAsync(newSession);
    }

    public async Task<int> UpdateQRCodeSessionAsync(QRCodeSession session)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(session);
    }

    // ===== QR SCAN REQUEST OPERATIONS =====

    public async Task<int> InsertQRScanRequestAsync(QRScanRequest request)
    {
        await InitAsync();
        return await _connection!.InsertAsync(request);
    }

    public async Task<List<QRScanRequest>> GetRestaurantScanRequestsAsync(int restaurantId)
    {
        await InitAsync();
        return await _connection!.Table<QRScanRequest>()
            .Where(r => r.RestaurantId == restaurantId)
            .OrderByDescending(r => r.ScanTime)
            .ToListAsync();
    }

    public async Task<List<QRScanRequest>> GetPendingScanRequestsAsync(int restaurantId)
    {
        await InitAsync();
        return await _connection!.Table<QRScanRequest>()
            .Where(r => r.RestaurantId == restaurantId && r.Status == "pending")
            .OrderByDescending(r => r.ScanTime)
            .ToListAsync();
    }

    public async Task<int> UpdateQRScanRequestAsync(QRScanRequest request)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(request);
    }

    public async Task<(int totalScansToday, int uniqueUsersToday, Dictionary<string, int> scansPerDay)> GetWeeklyScanStatisticsAsync(int restaurantId)
    {
        await InitAsync();

        var requests = await _connection!.Table<QRScanRequest>()
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();

        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        var totalScansToday = requests.Count(r => r.ScanTime.Date == today);
        var uniqueUsersToday = requests
            .Where(r => r.ScanTime.Date == today)
            .Select(r => r.UserId)
            .Distinct()
            .Count();

        var scansPerDay = new Dictionary<string, int>();
        var dayNames = new[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

        for (int i = 0; i < 7; i++)
        {
            var dayDate = weekStart.AddDays(i);
            var dayScans = requests.Count(r => r.ScanTime.Date == dayDate);
            scansPerDay[dayNames[i]] = dayScans;
        }

        return (totalScansToday, uniqueUsersToday, scansPerDay);
    }

    // DeviceScanLimit Operations (NEW for QR scan limits per device)
    public async Task<DeviceScanLimit?> GetDeviceScanLimitAsync(string deviceId)
    {
        await InitAsync();
        return await _connection!.Table<DeviceScanLimit>().Where(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
    }

    public async Task SaveDeviceScanLimitAsync(DeviceScanLimit limit)
    {
        await InitAsync();
        var existing = await _connection!.Table<DeviceScanLimit>().Where(d => d.DeviceId == limit.DeviceId).FirstOrDefaultAsync();
        if (existing != null)
        {
            // Update existing
            existing.ScanCount = limit.ScanCount;
            existing.MaxScans = limit.MaxScans;
            existing.LastResetDate = limit.LastResetDate;
            await _connection!.UpdateAsync(existing);
        }
        else
        {
            // Insert new
            limit.CreatedAt = DateTime.UtcNow;
            await _connection!.InsertAsync(limit);
        }
    }

    private string GenerateQRCode()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
    }

    // Seed sample data for development
    public async Task SeedSampleDataAsync()
    {
        await InitAsync();

        // Seed sample users if none exist
        var userCount = await _connection!.Table<User>().CountAsync();
        if (userCount == 0)
        {
            var sampleUsers = new List<User>
            {
                new User { Id = 1, FullName = "Nguyễn Văn A", Email = "user1@example.com", Password = "password", Phone = "0901234567", Avatar = "dotnet_bot.png", Language = "vi", IsPaid = true, PaidAt = DateTime.Now.AddMonths(-1) },
                new User { Id = 2, FullName = "Trần Thị B", Email = "user2@example.com", Password = "password", Phone = "0909876543", Avatar = "dotnet_bot.png", Language = "vi", IsPaid = false },
                new User { Id = 3, FullName = "Lê Văn C", Email = "user3@example.com", Password = "password", Phone = "0912345678", Avatar = "dotnet_bot.png", Language = "en", IsPaid = true, PaidAt = DateTime.Now.AddDays(-15) },
                new User { Id = 4, FullName = "Phạm Thị D", Email = "user4@example.com", Password = "password", Phone = "0913456789", Avatar = "dotnet_bot.png", Language = "vi", IsPaid = false },
                new User { Id = 5, FullName = "Hoàng Văn E", Email = "user5@example.com", Password = "password", Phone = "0914567890", Avatar = "dotnet_bot.png", Language = "ja", IsPaid = true, PaidAt = DateTime.Now }
            };

            foreach (var user in sampleUsers)
            {
                await _connection!.InsertAsync(user);
            }
        }

        // Seed sample POIs if none exist
        var poiCount = await _connection!.Table<AudioPOI>().CountAsync();
        if (poiCount == 0)
        {
            // Get public server URL from environment or use default
            string publicUrl = Environment.GetEnvironmentVariable("VINHKHANH_PUBLIC_URL") ?? "http://172.20.10.2:5000";

            var samplePOIs = new List<AudioPOI>
            {
                new AudioPOI { Name = "Ốc Oanh", Address = "534 Vĩnh Khánh, Q.4", Description = "Quán ốc nổi tiếng", Lat = 10.7595, Lng = 106.7045, Radius = 40, Priority = 1, ImageAsset = "dotnet_bot.png", QRCode = $"{publicUrl}/qr/POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Vũ", Address = "37 Vĩnh Khánh, Q.4", Description = "Ốc vũ tươi ngon", Lat = 10.7578, Lng = 106.7058, Radius = 40, Priority = 1, ImageAsset = "dotnet_bot.png", QRCode = $"{publicUrl}/qr/POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Nho", Address = "178 Vĩnh Khánh, Q.4", Description = "Ốc nho sốt phô mai", Lat = 10.7582, Lng = 106.7052, Radius = 40, Priority = 1, ImageAsset = "dotnet_bot.png", QRCode = $"{publicUrl}/qr/POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Quán Nướng Chilli", Address = "232 Vĩnh Khánh, Q.4", Description = "Nướng hải sản", Lat = 10.7586, Lng = 106.7055, Radius = 50, Priority = 2, ImageAsset = "dotnet_bot.png", QRCode = $"{publicUrl}/qr/POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Lẩu Bò Khu Nhà Cháy", Address = "Gần Vĩnh Khánh", Description = "Lẩu bò gia truyền", Lat = 10.7590, Lng = 106.7025, Radius = 50, Priority = 2, ImageAsset = "dotnet_bot.png", QRCode = $"{publicUrl}/qr/POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            foreach (var poi in samplePOIs)
            {
                await _connection!.InsertAsync(poi);
            }
        }

        // Seed sample payments if none exist
        var paymentCount = await _connection!.Table<UserPayment>().CountAsync();
        if (paymentCount == 0)
        {
            var samplePayments = new List<UserPayment>
            {
                new UserPayment { UserId = 1, IsPaid = true, PaymentMethod = "Credit Card", Amount = 99000, PaidDate = DateTime.Now.AddMonths(-1) },
                new UserPayment { UserId = 2, IsPaid = false, PaymentMethod = "", Amount = 0, PaidDate = null },
                new UserPayment { UserId = 3, IsPaid = true, PaymentMethod = "Bank Transfer", Amount = 99000, PaidDate = DateTime.Now.AddDays(-15) },
                new UserPayment { UserId = 4, IsPaid = false, PaymentMethod = "", Amount = 0, PaidDate = null },
                new UserPayment { UserId = 5, IsPaid = true, PaymentMethod = "Credit Card", Amount = 99000, PaidDate = DateTime.Now }
            };

            foreach (var payment in samplePayments)
            {
                await _connection!.InsertAsync(payment);
            }
        }

        // Seed sample play history
        var historyCount = await _connection!.Table<PlayHistory>().CountAsync();
        if (historyCount == 0)
        {
            var today = DateTime.Today;
            var sampleHistory = new List<PlayHistory>
            {
                new PlayHistory { UserId = 1, POIName = "Ốc Oanh", ImageAsset = "dotnet_bot.png", PlayedAt = today.AddHours(8) },
                new PlayHistory { UserId = 2, POIName = "Ốc Vũ", ImageAsset = "dotnet_bot.png", PlayedAt = today.AddHours(9) },
                new PlayHistory { UserId = 3, POIName = "Ốc Nho", ImageAsset = "dotnet_bot.png", PlayedAt = today.AddHours(10) },
                new PlayHistory { UserId = 1, POIName = "Quán Nướng Chilli", ImageAsset = "dotnet_bot.png", PlayedAt = today.AddHours(11) },
                new PlayHistory { UserId = 4, POIName = "Lẩu Bò Khu Nhà Cháy", ImageAsset = "dotnet_bot.png", PlayedAt = today.AddHours(12) },
                new PlayHistory { UserId = 5, POIName = "Ốc Oanh", ImageAsset = "dotnet_bot.png", PlayedAt = today.AddHours(13) }
            };

            foreach (var history in sampleHistory)
            {
                await _connection!.InsertAsync(history);
            }
        }

        // Seed sample AdminUsers if none exist
        var adminCount = await _connection!.Table<AdminUser>().CountAsync();
        if (adminCount == 0)
        {
            var sampleAdmins = new List<AdminUser>
            {
                new AdminUser { Username = "admin", Password = "Admin@123", FullName = "Quản Trị Viên", Email = "admin@vinhkhanhtour.com", Role = "admin", IsActive = true, CreatedAt = DateTime.Now, LoginCount = 0 },
                new AdminUser { Username = "manager", Password = "Manager@123", FullName = "Quản Lý Hệ Thống", Email = "manager@vinhkhanhtour.com", Role = "manager", IsActive = true, CreatedAt = DateTime.Now, LoginCount = 0 }
            };

            foreach (var admin in sampleAdmins)
            {
                await _connection!.InsertAsync(admin);
            }
        }

        // Seed sample UserDevices if none exist
        var deviceCount = await _connection!.Table<UserDevice>().CountAsync();
        if (deviceCount == 0)
        {
            var sampleDevices = new List<UserDevice>
            {
                new UserDevice { UserId = 1, DeviceId = Guid.NewGuid().ToString(), DeviceName = "iPhone 12", DeviceModel = "iPhone12,1", DeviceOS = "iOS", AppVersion = "1.0.0", IsOnline = true, LastOnlineAt = DateTime.Now, RegisteredAt = DateTime.Now.AddDays(-30), IpAddress = "192.168.1.100", LocationInfo = "Hồ Chí Minh", IsActive = true },
                new UserDevice { UserId = 2, DeviceId = Guid.NewGuid().ToString(), DeviceName = "Samsung Galaxy A12", DeviceModel = "SM-A125F", DeviceOS = "Android", AppVersion = "1.0.0", IsOnline = false, LastOnlineAt = DateTime.Now.AddHours(-2), RegisteredAt = DateTime.Now.AddDays(-15), IpAddress = "192.168.1.101", LocationInfo = "Bình Dương", IsActive = true },
                new UserDevice { UserId = 3, DeviceId = Guid.NewGuid().ToString(), DeviceName = "iPad Air 4", DeviceModel = "iPad Air (4th generation)", DeviceOS = "iOS", AppVersion = "1.0.0", IsOnline = true, LastOnlineAt = DateTime.Now, RegisteredAt = DateTime.Now.AddDays(-7), IpAddress = "192.168.1.102", LocationInfo = "Đồng Nai", IsActive = true }
            };

            foreach (var device in sampleDevices)
            {
                await _connection!.InsertAsync(device);
            }
        }

        // Seed SystemSettings if none exist
        var settingCount = await _connection!.Table<SystemSetting>().CountAsync();
        if (settingCount == 0)
        {
            var sampleSettings = new List<SystemSetting>
            {
                new SystemSetting { Key = "App.Name", Value = "Vĩnh Khánh Tour", Description = "Tên ứng dụng", SettingType = "string", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "App.Version", Value = "1.0.0", Description = "Phiên bản ứng dụng", SettingType = "string", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Payment.Price", Value = "99000", Description = "Giá premium (VND)", SettingType = "decimal", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Payment.Currency", Value = "VND", Description = "Đơn vị tiền tệ", SettingType = "string", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Feature.QRScanning", Value = "true", Description = "Bật/tắt quét QR", SettingType = "bool", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Feature.AudioGuide", Value = "true", Description = "Bật/tắt hướng dẫn âm thanh", SettingType = "bool", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Security.SessionTimeout", Value = "3600", Description = "Thời gian hết phiên (giây)", SettingType = "int", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Maintenance.Mode", Value = "false", Description = "Chế độ bảo trì", SettingType = "bool", UpdatedAt = DateTime.Now, UpdatedBy = "system" }
            };

            foreach (var setting in sampleSettings)
            {
                await _connection!.InsertAsync(setting);
            }
        }
    }

    // ===== ADDITIONAL METHODS FOR ADMIN DASHBOARD =====

    /// <summary>
    /// Get all payments for admin dashboard
    /// </summary>
    public async Task<List<UserPayment>> GetAllPaymentsAsync()
    {
        await InitAsync();
        return await _connection!.Table<UserPayment>().ToListAsync();
    }

    /// <summary>
    /// Get all device scan limits for admin dashboard
    /// </summary>
    public async Task<List<DeviceScanLimit>> GetAllDeviceScanLimitsAsync()
    {
        await InitAsync();
        var limits = await _connection!.Table<DeviceScanLimit>().ToListAsync();

        // Reset counts if new day
        foreach (var limit in limits)
        {
            if (limit.LastResetDate < DateTime.UtcNow.Date)
            {
                limit.ScanCount = 0;
                limit.LastResetDate = DateTime.UtcNow.Date;
                await _connection!.UpdateAsync(limit);
            }
        }

        return limits;
    }
}