using Microsoft.AspNetCore.Mvc;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    [HttpGet("app-apk")]
    public IActionResult DownloadAppAPK()
    {
        // In production, serve real APK from wwwroot/apk/app.apk
        // For demo, return placeholder
        var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "apk", "VinhKhanhTour.apk");
        if (System.IO.File.Exists(apkPath))
        {
            var apkBytes = System.IO.File.ReadAllBytes(apkPath);
            return File(apkBytes, "application/vnd.android.package-archive", "VinhKhanhTour.apk");
        }

        // Placeholder text file for demo
        var content = "Demo APK Download - In production, place APK file at wwwroot/apk/VinhKhanhTour.apk";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return File(bytes, "application/vnd.android.package-archive", "VinhKhanhTour-Demo.apk");
    }
}

