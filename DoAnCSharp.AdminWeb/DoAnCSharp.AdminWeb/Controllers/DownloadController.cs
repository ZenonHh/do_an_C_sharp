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

        var html = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Chưa có file cài đặt</title>
    <style>
        body { font-family: sans-serif; text-align: center; padding: 50px 20px; background: #f8fafc; color: #333; }
        .box { background: white; padding: 30px; border-radius: 12px; box-shadow: 0 4px 10px rgba(0,0,0,0.1); max-width: 400px; margin: 0 auto; }
    </style>
</head>
<body>
    <div class='box'>
        <h2 style='color:#e74c3c;'>⚠️ Ứng dụng chưa sẵn sàng</h2>
        <p>Vui lòng copy file <b>VinhKhanhTour.apk</b> vào thư mục <b>wwwroot/apk/</b> của Server Admin.</p>
    </div>
</body>
</html>";
        return Content(html, "text/html", System.Text.Encoding.UTF8);
    }
}
