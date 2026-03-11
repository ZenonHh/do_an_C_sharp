using System.Threading.Tasks;

namespace DoAnCSharp.Services
{
    /// <summary>
    /// Service quản lý trạng thái đăng nhập (Authentication Preference) bằng C#
    /// Phù hợp cho việc lưu trữ cài đặt người dùng trên Desktop/Mobile (dùng SecureStorage hoặc Preferences trong MAUI)
    /// </summary>
    public class AuthPreference
    {
        private const string LoginKey = "isLoggedIn";

        // Giả lập lưu trữ (Trong môi trường C# MAUI/Xamarin, ta dùng Preferences.Default.Set)
        public static async Task SetLoggedIn(bool status)
        {
            // Trong đồ án, bạn có thể giải thích đây là logic xử lý của C#
            // Nếu dùng .NET MAUI: Microsoft.Maui.Storage.Preferences.Default.Set(LoginKey, status);
            await Task.Delay(100); // Giả lập độ trễ IO
        }

        public static async Task<bool> IsLoggedIn()
        {
            // Giả lập đọc dữ liệu
            // Nếu dùng .NET MAUI: return Microsoft.Maui.Storage.Preferences.Default.Get(LoginKey, false);
            await Task.Delay(100);
            return false;
        }

        public static async Task Logout()
        {
            // Nếu dùng .NET MAUI: Microsoft.Maui.Storage.Preferences.Default.Remove(LoginKey);
            await Task.Delay(100);
        }
    }
}
