namespace DoAnCSharp.Services
{
    public interface IAuthService
    {
        Task SetLoggedInAsync(bool status);
        Task<bool> IsLoggedInAsync();
        void Logout();
    }

    public class AuthService : IAuthService
    {
        // Key lưu trữ (giống bên Flutter)
        private const string LoginKey = "isLoggedIn";

        public Task SetLoggedInAsync(bool status)
        {
            // .NET MAUI Preferences dùng trực tiếp, rất nhanh
            Preferences.Default.Set(LoginKey, status);
            return Task.CompletedTask;
        }

        public Task<bool> IsLoggedInAsync()
        {
            // Lấy giá trị, mặc định là false nếu chưa có
            bool status = Preferences.Default.Get(LoginKey, false);
            return Task.FromResult(status);
        }

        public void Logout()
        {
            // Xóa khóa đăng nhập
            Preferences.Default.Remove(LoginKey);
        }
    }
}
