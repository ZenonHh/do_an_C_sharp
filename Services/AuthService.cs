#nullable disable
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace DoAnCSharp.Services;

public interface IAuthService
{
    Task SetLoggedInAsync(bool status);
    Task<bool> IsLoggedInAsync();
    void Logout();
    Task<bool> RegisterAsync(string email, string password, string fullName, string avatar = "dotnet_bot.png");
}

public class AuthService : IAuthService
{
    private const string LoginKey = "isLoggedIn";
    private readonly DatabaseService _dbService;

    public AuthService(DatabaseService dbService)
    {
        _dbService = dbService;
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

    // Delegate hoàn toàn sang DatabaseService để dùng chung 1 DB và 1 logic hash
    public async Task<bool> RegisterAsync(string email, string password, string fullName, string avatar = "dotnet_bot.png")
    {
        return await _dbService.RegisterUserAsync(fullName, email, password);
    }
}
