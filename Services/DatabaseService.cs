using SQLite;
using DoAnCSharp.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace DoAnCSharp.Services;

public class DatabaseService
{
    private const string DbFileName = "VinhKhanhTour.db3";
    private SQLiteAsyncConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    // 1. HÀM KHỞI TẠO CƠ BẢN (Tạo bảng và nạp User)
    private async Task InitAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_connection != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
            _connection = new SQLiteAsyncConnection(dbPath);

            // Tạo các bảng
            await _connection.CreateTableAsync<AudioPOI>();
            await _connection.CreateTableAsync<User>();

            // Nạp dữ liệu User mặc định (nếu chưa có)
            var userCount = await _connection.Table<User>().CountAsync();
            if (userCount == 0)
            {
                var defaultUser = new User 
                { 
                    FullName = "Gastronome Vĩnh Khánh", 
                    Email = "admin@vinhkhanh.com",
                    Phone = "0909 123 456"
                };
                await _connection.InsertAsync(defaultUser);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // 2. HÀM NẠP DỮ LIỆU QUÁN ĂN (Public để App.xaml.cs có thể gọi)
    public async Task SeedDataAsync()
    {
        await InitAsync(); // Đảm bảo đã khởi tạo kết nối

        var count = await _connection!.Table<AudioPOI>().CountAsync();
        if (count == 0)
        {
            var initialData = new List<AudioPOI>
            {
                new AudioPOI { Name = "Ốc Oanh", Description = "Chào mừng đến Ốc Oanh. Món chính là ốc hương xào bắp bơ.", Lat = 10.7600, Lng = 106.7000, Radius = 50, Priority = 1, ImageAsset = "oc_oanh.jpg" },
                new AudioPOI { Name = "Ốc Đào 2", Description = "Ốc Đào nổi tiếng với các món ốc xào me đậm đà.", Lat = 10.7581, Lng = 106.7061, Radius = 50, Priority = 1, ImageAsset = "oc_dao2.webp" },
                new AudioPOI { Name = "Ốc Vũ", Description = "Ốc Vũ là điểm đến lý tưởng cho món ốc mỡ xào tỏi.", Lat = 10.7578, Lng = 106.7058, Radius = 50, Priority = 2, ImageAsset = "dotnet_bot.png" }
            };
            await _connection.InsertAllAsync(initialData);
        }
    }

    // 3. HÀM LẤY DANH SÁCH QUÁN ĂN (Cho Trang chủ và Bản đồ)
    public async Task<List<AudioPOI>> GetPOIsAsync()
    {
        await InitAsync();
        return await _connection!.Table<AudioPOI>().ToListAsync();
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

    // SỬA HÀM CŨ: Không lấy ông đầu tiên nữa, mà lấy đúng ông đang đăng nhập
    public async Task<User?> GetCurrentUserAsync()
    {
        await InitAsync();
        
        // Lấy Email mà người dùng vừa gõ lúc nãy ở trang Login
        string currentEmail = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "admin@vinhkhanh.com");
        
        return await _connection!.Table<User>().Where(u => u.Email == currentEmail).FirstOrDefaultAsync(); 
    }
    // 1. HÀM ĐĂNG KÝ (Tạo tài khoản mới)
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
            Password = password, // Lưu mật khẩu
            Avatar = "dotnet_bot.png",
            Phone = "Đang cập nhật"
        };
        
        var result = await _connection.InsertAsync(newUser);
        return result > 0; // Trả về true nếu thêm thành công
    }

    // 2. HÀM ĐĂNG NHẬP (Kiểm tra đúng Email và Mật khẩu)
    public async Task<User?> LoginUserAsync(string email, string password)
    {
        await InitAsync();
        // Tìm user có cả Email VÀ Password trùng khớp
        return await _connection!.Table<User>()
                             .Where(u => u.Email == email && u.Password == password)
                             .FirstOrDefaultAsync();
    }
}