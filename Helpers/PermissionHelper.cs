using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace DoAnCSharp.Helpers;

public static class PermissionHelper
{
    // Hàm xin quyền Camera
    public static async Task<bool> RequestCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }
        return status == PermissionStatus.Granted;
    }

    // Hàm xin quyền Thư viện ảnh (Đối với Android 12 trở xuống và iOS)
    public static async Task<bool> RequestStoragePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
        }
        return status == PermissionStatus.Granted;
    }

    // Hàm xin quyền Thư viện Media (Dùng cho Android 13+)
    public static async Task<bool> RequestMediaPermissionAsync()
    {
        // Trên Android 13+, bạn cần xin quyền cụ thể hơn.
        // MAUI sẽ tự động xử lý tốt nhất dựa trên nền tảng.
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Photos>();
        }
        return status == PermissionStatus.Granted;
    }
}