using Microsoft.AspNetCore.Mvc;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    [HttpGet("app-apk")]
    public IActionResult DownloadAppAPK()
    {
        var apkPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "apk", "VinhKhanhTour.apk");
        if (System.IO.File.Exists(apkPath))
        {
            return PhysicalFile(apkPath, "application/vnd.android.package-archive", "VinhKhanhTour.apk");
        }

        return NotFound(new { message = "APK chưa sẵn sàng. Vui lòng liên hệ quản trị viên." });
    }
}

