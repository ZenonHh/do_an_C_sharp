using SQLite;
using DoAnCSharp.Models;
using DoAnCSharp.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace DoAnCSharp.Services;

public class DatabaseService
{
    // Đã đổi tên file để hệ thống tạo DB mới chứa 15 quán ăn
    private const string DbFileName = "VinhKhanhTour_V6.db3";
    private SQLiteAsyncConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly AdminSyncService _sync;

    public DatabaseService(AdminSyncService sync)
    {
        _sync = sync;
    }

    // 1. HÀM KHỞI TẠO CƠ BẢN (Tạo bảng và nạp User)
    private async Task InitAsync()
    
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_connection != null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== DatabaseService.InitAsync Starting ===");
                
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
                System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
                
                _connection = new SQLiteAsyncConnection(dbPath);
                System.Diagnostics.Debug.WriteLine("✓ SQLite connection created");

                // Tạo các bảng
                await _connection.CreateTableAsync<PlayHistory>();
                System.Diagnostics.Debug.WriteLine("✓ PlayHistory table created");
                
                await _connection.CreateTableAsync<AudioPOI>();
                System.Diagnostics.Debug.WriteLine("✓ AudioPOI table created");
                
                await _connection.CreateTableAsync<User>();
                System.Diagnostics.Debug.WriteLine("✓ User table created");

                // Nạp dữ liệu User mặc định (nếu chưa có)
                var userCount = await _connection.Table<User>().CountAsync();
                System.Diagnostics.Debug.WriteLine($"User count: {userCount}");
                
                if (userCount == 0)
                {
                    var defaultUser = new User
                    {
                        FullName = "Gastronome Vĩnh Khánh",
                        Email = "admin@vinhkhanh.com",
                        Phone = "0909 123 456",
                        Password = PasswordHelper.Hash("123456"),
                        Avatar = "dotnet_bot.png"
                    };
                    await _connection.InsertAsync(defaultUser);
                    System.Diagnostics.Debug.WriteLine("✓ Default user created");
                }

                System.Diagnostics.Debug.WriteLine("=== DatabaseService.InitAsync Complete ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error in DatabaseService.InitAsync: {ex}");
                throw;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // 2. HÀM NẠP DỮ LIỆU 15 QUÁN ĂN (Public để App.xaml.cs có thể gọi)
    public async Task SeedDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== DatabaseService.SeedDataAsync Starting ===");
            
            await InitAsync(); // Đảm bảo đã khởi tạo kết nối

            if (_connection == null)
                throw new InvalidOperationException("Database connection not initialized");

            var count = await _connection.Table<AudioPOI>().CountAsync();
            System.Diagnostics.Debug.WriteLine($"Current POI count in database: {count}");
            
            if (count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Seeding 15 restaurants into database...");
                
                var initialData = new List<AudioPOI>
{
    // ---- THẾ GIỚI ỐC ----
    new AudioPOI { Name = "Ốc Oanh", Address = "534 Vĩnh Khánh, Q.4", Description = "Quán ốc 'huyền thoại' đông nhất Vĩnh Khánh. Nổi tiếng với ốc hương rang muối ớt và càng ghẹ nướng.", Lat = 10.7595, Lng = 106.7045, Radius = 40, Priority = 1, ImageAsset = "oc_oanh.jpg" },
    new AudioPOI { Name = "Ốc Vũ", Address = "37 Vĩnh Khánh, Q.4", Description = "Không gian siêu rộng, menu đa dạng và giá cả bình dân. Món khuyên dùng: Ốc tỏi nướng mỡ hành.", Lat = 10.7578, Lng = 106.7058, Radius = 40, Priority = 1, ImageAsset = "oc_vu.jpg" },
    new AudioPOI { Name = "Ốc Nho", Address = "178 Vĩnh Khánh, Q.4", Description = "Chân ái của giới trẻ với các món ốc sốt phô mai kéo sợi, sốt trứng muối béo ngậy cực đỉnh.", Lat = 10.7582, Lng = 106.7052, Radius = 40, Priority = 1, ImageAsset = "oc_nho.jpg" },
    new AudioPOI { Name = "Ốc Thảo", Address = "383 Vĩnh Khánh, Q.4", Description = "Quán lâu năm, giữ nguyên hương vị ốc truyền thống Sài Gòn. Nước mắm gừng pha cực ngon.", Lat = 10.7590, Lng = 106.7042, Radius = 40, Priority = 1, ImageAsset = "oc_thao.jpg" },
    new AudioPOI { Name = "Ốc Sóc", Address = "D58 Vĩnh Khánh, Q.4", Description = "Nổi bật với món nghêu hấp sả ớt cay nồng và ốc móng tay xào rau muống.", Lat = 10.7587, Lng = 106.7048, Radius = 40, Priority = 1, ImageAsset = "oc_soc.jpg" },
    new AudioPOI { Name = "Ốc Tuyết", Address = "430 Vĩnh Khánh, Q.4", Description = "Quán bình dân nhưng chất lượng tuyệt vời, phục vụ nhanh nhẹn, các món xào me rất đậm đà.", Lat = 10.7585, Lng = 106.7032, Radius = 40, Priority = 1, ImageAsset = "oc_tuyet.jpg" },
    new AudioPOI { Name = "Ốc Đào 2", Address = "Vĩnh Khánh, P.4, Q.4", Description = "Thương hiệu ốc lâu đời, nêm nếm theo khẩu vị đậm đà đặc trưng, ốc xào sa tế cay xé lưỡi.", Lat = 10.7581, Lng = 106.7061, Radius = 40, Priority = 1, ImageAsset = "oc_dao.jpg" },

    // ---- LẨU & NƯỚNG ----
    new AudioPOI { Name = "Quán Nướng Chilli", Address = "232 Vĩnh Khánh, Q.4", Description = "Thiên đường hàu nướng với hơn 20 loại sốt khác nhau, hải sản nướng ngói thơm lừng.", Lat = 10.7586, Lng = 106.7055, Radius = 50, Priority = 2, ImageAsset = "nuong_chilli.jpg" },
    new AudioPOI { Name = "Lẩu Bò Khu Nhà Cháy", Address = "Chung cư Đoàn Văn Bơ (Gần Vĩnh Khánh)", Description = "Lẩu bò gia truyền nước dùng ngọt thanh từ xương, bò viên tự làm dai giòn sừn sựt.", Lat = 10.7590, Lng = 106.7025, Radius = 50, Priority = 2, ImageAsset = "lau_bo.jpg" },
    new AudioPOI { Name = "Sườn Nướng Muối Ớt", Address = "Dọc đường Vĩnh Khánh, Q.4", Description = "Sườn heo nướng tẩm ớt cay nồng, ăn kèm đồ chua giải ngấy cực kỳ bắt bia.", Lat = 10.7588, Lng = 106.7040, Radius = 40, Priority = 2, ImageAsset = "suon_nuong.jpg" },
    new AudioPOI { Name = "Khèn BBQ - Nướng Ngói", Address = "165 Vĩnh Khánh, Q.4", Description = "Thịt được nướng trên ngói đỏ giúp giữ độ ngọt, không bị ám khói than, tẩm ướp chuẩn vị Tây Bắc.", Lat = 10.7592, Lng = 106.7038, Radius = 40, Priority = 2, ImageAsset = "khen_bbq.jpg" },
    new AudioPOI { Name = "Lẩu Dê Dũng Mập", Address = "Đầu đường Vĩnh Khánh", Description = "Lẩu dê nấu chao thơm phức, thịt dê núi mềm ngọt, không bị hôi, ăn kèm rau rừng.", Lat = 10.7602, Lng = 106.7049, Radius = 50, Priority = 2, ImageAsset = "lau_de.jpg" },

    // ---- ĂN VẶT & MÓN KHÁC ----
    new AudioPOI { Name = "Phá Lấu Cô Oanh", Address = "Đoạn giao Tôn Đản - Vĩnh Khánh", Description = "Phá lấu bò nấu nước cốt dừa béo ngậy, ăn kèm bánh mì nóng giòn chấm mắm me chua ngọt.", Lat = 10.7570, Lng = 106.7065, Radius = 30, Priority = 3, ImageAsset = "pha_lau.jpg" },
    new AudioPOI { Name = "Sushi Viên Vĩnh Khánh", Address = "Dọc vỉa hè Vĩnh Khánh", Description = "Sushi lề đường giá học sinh sinh viên nhưng cá hồi, trứng cuộn rất tươi và sạch sẽ.", Lat = 10.7598, Lng = 106.7042, Radius = 30, Priority = 3, ImageAsset = "sushi_vien.jpg" },
    new AudioPOI { Name = "Trái Cây Tô & Chè", Address = "Giữa phố Vĩnh Khánh", Description = "Tráng miệng mát lạnh giải nhiệt sau khi ăn đồ nướng cay nóng, trái cây xô ngập tràn sữa chua.", Lat = 10.7584, Lng = 106.7050, Radius = 30, Priority = 3, ImageAsset = "trai_cay_to.jpg" }
};
                
                int inserted = await _connection.InsertAllAsync(initialData);
                System.Diagnostics.Debug.WriteLine($"✓ Successfully inserted {inserted} POIs into database");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ℹ Database already has {count} POIs, skipping seed");
            }
            
            // Verify the final count
            var finalCount = await _connection.Table<AudioPOI>().CountAsync();
            System.Diagnostics.Debug.WriteLine($"✓ Final POI count: {finalCount}");
            
            System.Diagnostics.Debug.WriteLine("=== DatabaseService.SeedDataAsync Complete ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ ERROR in SeedDataAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"✗ Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    // 3. HÀM LẤY DANH SÁCH QUÁN ĂN (Cho Trang chủ và Bản đồ)
    public async Task<List<AudioPOI>> GetPOIsAsync()
    {
        try
        {
            await InitAsync();
            
            if (_connection == null)
                throw new InvalidOperationException("Database connection not initialized");
            
            var pois = await _connection.Table<AudioPOI>().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"✓ GetPOIsAsync returned {pois.Count} POIs");
            
            return pois;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ ERROR in GetPOIsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"✗ Stack Trace: {ex.StackTrace}");
            return new List<AudioPOI>();
        }
    }

    // 4. HÀM LẤY THÔNG TIN NGƯỜI DÙNG (Cho trang Cá nhân)
    public async Task<User> GetOrCreateUserAsync(string email)
    {
        await InitAsync();
        var user = await _connection!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        
        // Nếu user lần đầu đăng nhập, tạo luôn cho họ 1 hồ sơ
        if (user == null)
        {
            user = new User 
            { 
                Email = email, 
                FullName = "Người Dùng Mới", // Sẽ cho phép họ đổi tên sau
                Phone = "Đang cập nhật",
                Avatar = "dotnet_bot.png"
            };
            await _connection.InsertAsync(user);
        }
        return user;
    }

    // THÊM HÀM NÀY: Chỉ tìm kiếm User, KHÔNG tạo mới nếu không thấy
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        var allUsers = await _connection!.Table<User>().ToListAsync();
        return allUsers.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    // 5. Lấy User đang đăng nhập
    public async Task<User?> GetCurrentUserAsync()
    {
        await InitAsync();
        
        // Lấy Email mà người dùng vừa gõ lúc nãy ở trang Login
        string currentEmail = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "admin@vinhkhanh.com");
        
        return await _connection!.Table<User>().Where(u => u.Email == currentEmail).FirstOrDefaultAsync(); 
    }

    // 6. HÀM ĐĂNG KÝ (Tạo tài khoản mới)
    public async Task<bool> RegisterUserAsync(string fullName, string email, string password)
    {
        await InitAsync();
        
        // Kiểm tra xem Email đã có ai đăng ký chưa
        var existingUser = await _connection!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        if (existingUser != null)
            return false; // Trùng email -> Thất bại

        // Chưa có thì tạo mới
        var newUser = new User
        {
            FullName = fullName,
            Email = email,
            Password = PasswordHelper.Hash(password),
            Avatar = "dotnet_bot.png",
            Phone = "Đang cập nhật"
        };
        
        var result = await _connection.InsertAsync(newUser);
        return result > 0; // Trả về true nếu thêm thành công
    }

    // 7. HÀM ĐĂNG NHẬP (Kiểm tra đúng Email và Mật khẩu)
    public async Task<User?> LoginUserAsync(string email, string password)
    {
        await InitAsync();
        // Tìm user có cả Email VÀ Password trùng khớp
        string hashed = PasswordHelper.Hash(password);
        return await _connection!.Table<User>()
                             .Where(u => u.Email == email && u.Password == hashed)
                             .FirstOrDefaultAsync();
    }
    public async Task SavePlayHistoryAsync(AudioPOI poi)
    {
        await InitAsync();
        string userId = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "guest");
        var playedAt = DateTime.Now;
        var history = new PlayHistory
        {
            PoiName = poi.Name,
            ImageAsset = poi.ImageAsset,
            PlayedAt = playedAt,
            UserId = userId,
            IsSynced = false
        };
        await _connection!.InsertAsync(history);

        // Sync lên admin server (fire and forget — không ảnh hưởng nếu server tắt)
        _ = _sync.SyncPlayHistoryAsync(userId, poi.Name, playedAt);
    }

    public async Task<List<PlayHistory>> GetRecentPlayHistoryAsync()
    {
        await InitAsync();
        return await _connection!.Table<PlayHistory>().OrderByDescending(x => x.PlayedAt).Take(10).ToListAsync();
    }

    // Lấy TOÀN BỘ lịch sử để hiển thị trên trang Lịch sử riêng
    public async Task<List<PlayHistory>> GetAllPlayHistoryAsync()
    {
        await InitAsync();
        return await _connection!.Table<PlayHistory>().OrderByDescending(x => x.PlayedAt).ToListAsync();
    }
// 8. HÀM LẤY WEIGHT HEATMAP = ĐỘ NỔI TIẾNG (Priority) + SỐ LƯỢT NGHE
    public async Task<List<(double Lat, double Lng, int Weight)>> GetPOIPlayCountsAsync()
    {
        await InitAsync();
        var pois = await _connection!.Table<AudioPOI>().ToListAsync();
        var histories = await _connection!.Table<PlayHistory>().ToListAsync();

        var counts = histories
            .GroupBy(h => h.PoiName)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<(double, double, int)>();
        foreach (var poi in pois)
        {
            // Priority 1→base 6, Priority 2→base 3, Priority 3→base 1
            int baseWeight = poi.Priority switch { 1 => 6, 2 => 3, _ => 1 };
            int playCount = counts.TryGetValue(poi.Name, out var c) ? c : 0;
            result.Add((poi.Lat, poi.Lng, baseWeight + playCount));
        }
        return result;
    }

    // 9. HÀM CẬP NHẬT THÔNG TIN NGƯỜI DÙNG
public async Task<bool> UpdateUserAsync(string email, string newFullName, string newPassword, string newAvatarPath)
{
    await InitAsync();
    // Tìm user đang đăng nhập
    var user = await _connection!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
    
    if (user != null)
    {
        // Cập nhật các trường nếu người dùng có nhập mới
        if (!string.IsNullOrWhiteSpace(newFullName)) user.FullName = newFullName;
        if (!string.IsNullOrWhiteSpace(newPassword)) user.Password = PasswordHelper.Hash(newPassword);
        if (!string.IsNullOrWhiteSpace(newAvatarPath)) user.Avatar = newAvatarPath;
        
        var result = await _connection.UpdateAsync(user);
        return result > 0; // Trả về true nếu update thành công
    }
    return false;
}

}