#nullable disable
using SQLite;
using DoAnCSharp.Models;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading.Tasks;

namespace DoAnCSharp.Services;

// ĐÂY LÀ PHẦN INTERFACE ĐÃ ĐƯỢC KHAI BÁO REGISTER_ASYNC
public interface IAuthService
{
    Task SetLoggedInAsync(bool status);
    Task<bool> IsLoggedInAsync();
    void Logout();

    // Khai báo hàm đăng ký
    Task<bool> RegisterAsync(string email, string password, string fullName, string avatar = "dotnet_bot.png");
}

public class AuthService : IAuthService
{
    private const string LoginKey = "isLoggedIn";
    private SQLiteAsyncConnection _connection;

    private async Task InitAsync()
    {
        if (_connection != null)
            return;

        // Dùng chung một Database để tránh lỗi đăng nhập
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "VinhKhanhTour_Full.db3");

        _connection = new SQLiteAsyncConnection(dbPath);
        await _connection.CreateTableAsync<User>();
    }

    public Task SetLoggedInAsync(bool status)
    {
        Preferences.Default.Set(LoginKey, status);
        return Task.CompletedTask;
    }

    public Task<bool> IsLoggedInAsync()
    {
        bool status = Preferences.Default.Get(LoginKey, false);
        return Task.FromResult(status);
    }

    public void Logout()
    {
        Preferences.Default.Remove(LoginKey);
        Preferences.Default.Remove("CurrentUserEmail");
    }

    // THỰC THI HÀM ĐĂNG KÝ Ở ĐÂY
    public async Task<bool> RegisterAsync(string email, string password, string fullName, string avatar = "dotnet_bot.png")
    {
        await InitAsync();

        var existingUser = await _connection.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return false;
        }

        var newUser = new User
        {
            Email = email,
            Password = password,
            FullName = fullName,
            Avatar = avatar
        };

        await _connection.InsertAsync(newUser);
        return true;
    }
}