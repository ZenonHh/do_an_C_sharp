using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;
using System.Text;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class POIsController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly ILogger<POIsController> _logger;
    private readonly IConfiguration _configuration;

    public POIsController(DatabaseService db, ILogger<POIsController> logger, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<List<AudioPOI>>> GetAll()
    {
        try
        {
            var pois = await _db.GetAllPOIsAsync();
            return Ok(pois);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("main-images")]
    public async Task<ActionResult> GetMainImages()
    {
        try
        {
            var pois = await _db.GetAllPOIsAsync();
            var uploaded = await _db.GetAllMainImagePathsAsync();
            var result = pois.ToDictionary(
                p => p.Id,
                p => uploaded.TryGetValue(p.Id, out var url) && !string.IsNullOrEmpty(url)
                    ? url
                    : (string.IsNullOrWhiteSpace(p.ImageAsset) ? null : $"/images/restaurants/{p.ImageAsset}")
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AudioPOI>> GetById(int id)
    {
        try
        {
            var poi = await _db.GetPOIByIdAsync(id);
            if (poi == null)
                return NotFound(new { error = "POI not found" });
            return Ok(poi);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("qr/{qrCode}")]
    public async Task<ActionResult<AudioPOI>> GetByQRCode(string qrCode)
    {
        try
        {
            var poi = await _db.GetPOIByQRCodeAsync(qrCode);
            if (poi == null)
                return NotFound(new { error = "POI not found with this QR code" });
            return Ok(poi);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AudioPOI poi)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poi.Name))
                return BadRequest(new { error = "Name is required" });

            if (string.IsNullOrWhiteSpace(poi.QRCode))
                poi.QRCode = GenerateQRCode();

            poi.CreatedAt = DateTime.Now;
            poi.UpdatedAt = DateTime.Now;

            await _db.InsertPOIAsync(poi);
            return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] AudioPOI poi)
    {
        try
        {
            var existing = await _db.GetPOIByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "POI not found" });

            poi.Id = id;

            if (string.IsNullOrWhiteSpace(poi.QRCode))
                poi.QRCode = existing.QRCode ?? GenerateQRCode();

            poi.UpdatedAt = DateTime.Now;
            // Preserve created date
            poi.CreatedAt = existing.CreatedAt;

            await _db.UpdatePOIAsync(poi);
            return Ok(new { message = "POI updated successfully", poi });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates a QR code string containing the full public URL for scanning.
    /// Example: http://192.168.31.102:5000/qr/POI_ABC123
    /// </summary>
    private string GenerateQRCode()
    {
        string publicUrl = (_configuration["ServerSettings:PublicUrl"] ??
                           $"{Request.Scheme}://{Request.Host}").TrimEnd('/');
        string baseCode = "POI_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        return $"{publicUrl}/qr/{baseCode}";
    }

    /// <summary>
    /// Build QR image URL for web display (used in modal)
    /// Handles both full URLs and code-only formats
    /// </summary>
    private string GetQRImageUrl(string qrCode)
    {
        // Deep links and http URLs are used as-is; bare codes get a web URL built around them
        string fullUrl = (qrCode.StartsWith("http") || qrCode.StartsWith("vinhkhanhtour://"))
            ? qrCode
            : $"{Request.Scheme}://{Request.Host}/qr/{qrCode}";

        // Escape URL for QR API
        var encodedUrl = Uri.EscapeDataString(fullUrl);

        // Return QR API URL to generate image
        return $"https://api.qrserver.com/v1/create-qr-code/?size=400x400&data={encodedUrl}";
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var existing = await _db.GetPOIByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "POI not found" });

            await _db.DeletePOIAsync(id);
            return Ok(new { message = "POI deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Trả về URL và ảnh QR của mã phố ẩm thực — dùng để in và đặt tại lối vào phố Vĩnh Khánh.
    /// Người dùng chỉ cần quét một lần, app hiển thị toàn bộ danh sách quán để chọn.
    /// </summary>
    [HttpGet("foodstreet-qr")]
    public ActionResult<object> GetFoodStreetQRInfo()
    {
        string publicUrl = (_configuration["ServerSettings:PublicUrl"] ??
                           $"{Request.Scheme}://{Request.Host}").TrimEnd('/');
        string webUrl = $"{publicUrl}/qr/FOODSTREET_VINHKHANH";
        string qrImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=400x400&data={Uri.EscapeDataString(webUrl)}";
        return Ok(new { webUrl, qrImageUrl, description = "Mã QR lối vào Phố Ẩm Thực Vĩnh Khánh" });
    }

    [HttpGet("config/server")]
    public ActionResult<object> GetServerConfig()
    {
        string publicUrl = _configuration["ServerSettings:PublicUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        return Ok(new { serverUrl = publicUrl });
    }

    [HttpGet("debug/all")]
    public async Task<ActionResult<object>> DebugAllPOIs()
    {
        try
        {
            var allPOIs = await _db.GetAllPOIsAsync();
            return Ok(new
            {
                totalCount = allPOIs.Count,
                pois = allPOIs.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    qrCode = p.QRCode,
                    address = p.Address,
                    hasAudio = !string.IsNullOrEmpty(p.AudioUrl)
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("debug/create-test")]
    public async Task<ActionResult> CreateTestPOI()
    {
        try
        {
            var testPOI = new AudioPOI
            {
                Name = "Quán Cơm Tấm Bà Ghẻ",
                Address = "123 Nguyễn Hữu Cảnh, Tp. HCM",
                Description = "Quán cơm tấm nổi tiếng với cơm tấm nóng, ruốc, trứng ốp, cá kho tộ. Đây là quán cơm tấm truyền thống Sài Gòn được nhiều khách hàng yêu thích.",
                Lat = 10.7769,
                Lng = 106.7009,
                Radius = 100,
                Priority = 1,
                ImageAsset = "dotnet_bot.png",
                QRCode = GenerateQRCode(),
                AudioUrl = null,
                OwnerId = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var result = await _db.InsertPOIAsync(testPOI);
            return Ok(new
            {
                message = "Test POI created successfully",
                poiId = testPOI.Id,
                qrCode = testPOI.QRCode,
                scanUrl = $"{_configuration["ServerSettings:PublicUrl"]}/qr/{testPOI.QRCode}"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ===== QR CODE ENDPOINTS =====

    /// <summary>
    /// Get QR code info with full URL for camera scanning
    /// Returns QR image URL, web URL, and deep link
    /// </summary>
    [HttpGet("{id}/qr-info")]
    public async Task<ActionResult> GetQRInfo(int id)
    {
        try
        {
            var poi = await _db.GetPOIByIdAsync(id);
            if (poi == null)
                return NotFound(new { error = "POI not found" });

            var qrCode = poi.QRCode ?? GenerateQRCode();

            // Build full URLs
            var host = $"{Request.Scheme}://{Request.Host}";
            var webUrl = $"{host}/qr/{qrCode}";
            var deepLink = $"vinhkhanhtour://poi/{poi.Id}";
            var qrImageUrl = GetQRImageUrl(qrCode);

            return Ok(new
            {
                poiId = poi.Id,
                poiName = poi.Name,
                qrCode = qrCode,
                webUrl = webUrl,
                deepLink = deepLink,
                qrImageUrl = qrImageUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get QR code image (redirect to QR API)
    /// </summary>
    [HttpGet("{id}/qr-image")]
    public async Task<IActionResult> GetQRImage(int id)
    {
        try
        {
            var poi = await _db.GetPOIByIdAsync(id);
            if (poi == null)
                return NotFound(new { error = "POI not found" });

            var qrImageUrl = GetQRImageUrl(poi.QRCode ?? GenerateQRCode());
            return Redirect(qrImageUrl);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Regenerate QR code for a POI
    /// </summary>
    [HttpPost("{id}/regenerate-qr")]
    public async Task<ActionResult> RegenerateQRCode(int id)
    {
        try
        {
            var poi = await _db.GetPOIByIdAsync(id);
            if (poi == null)
                return NotFound(new { error = "POI not found" });

            poi.QRCode = GenerateQRCode();
            poi.UpdatedAt = DateTime.Now;

            await _db.UpdatePOIAsync(poi);

            // Return new QR info
            var host = $"{Request.Scheme}://{Request.Host}";
            var webUrl = $"{host}/qr/{poi.QRCode}";
            var qrImageUrl = GetQRImageUrl(poi.QRCode);

            return Ok(new
            {
                poiId = poi.Id,
                poiName = poi.Name,
                qrCode = poi.QRCode,
                webUrl = webUrl,
                qrImageUrl = qrImageUrl,
                message = "QR code regenerated successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ===== IMAGE MANAGEMENT ENDPOINTS =====

    [HttpGet("{id}/images")]
    public async Task<ActionResult<List<RestaurantImage>>> GetImages(int id)
    {
        try
        {
            var images = await _db.GetRestaurantImagesAsync(id);
            return Ok(images);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/images/upload")]
    public async Task<ActionResult> UploadImages(int id, [FromForm] IFormFileCollection files, [FromForm] bool setMainImage = false)
    {
        try
        {
            var poi = await _db.GetPOIByIdAsync(id);
            if (poi == null)
                return NotFound(new { error = "POI not found" });

            if (files == null || files.Count == 0)
                return BadRequest(new { error = "No files uploaded" });

            // Create uploads directory if not exists
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "restaurants");
            Directory.CreateDirectory(uploadsPath);

            var uploadedImages = new List<RestaurantImage>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    continue;

                // Generate unique filename
                var fileName = $"{id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create database record
                var image = new RestaurantImage
                {
                    RestaurantId = id,
                    ImagePath = $"/uploads/restaurants/{fileName}",
                    ImageName = file.FileName,
                    FileSizeBytes = file.Length,
                    MimeType = file.ContentType,
                    IsMainImage = setMainImage && i == 0, // First image as main if requested
                    DisplayOrder = i,
                    UploadedAt = DateTime.Now,
                    UploadedBy = "Admin" // TODO: Get from auth context
                };

                await _db.InsertRestaurantImageAsync(image);
                uploadedImages.Add(image);
            }

            return Ok(new { 
                message = $"{uploadedImages.Count} images uploaded successfully", 
                images = uploadedImages 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("images/{imageId}/set-main")]
    public async Task<ActionResult> SetMainImage(int imageId)
    {
        try
        {
            var image = await _db.GetRestaurantImageByIdAsync(imageId);
            if (image == null)
                return NotFound(new { error = "Image not found" });

            // Unset all main images for this restaurant
            var allImages = await _db.GetRestaurantImagesAsync(image.RestaurantId);
            foreach (var img in allImages)
            {
                img.IsMainImage = false;
                await _db.UpdateRestaurantImageAsync(img);
            }

            // Set this image as main
            image.IsMainImage = true;
            await _db.UpdateRestaurantImageAsync(image);

            return Ok(new { message = "Main image updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("images/{imageId}")]
    public async Task<ActionResult> DeleteImage(int imageId)
    {
        try
        {
            var image = await _db.GetRestaurantImageByIdAsync(imageId);
            if (image == null)
                return NotFound(new { error = "Image not found" });

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete database record
            await _db.DeleteRestaurantImageAsync(imageId);

            return Ok(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("stats/scanned")]
    public async Task<ActionResult> GetScannedStats()
    {
        try
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            // Get all play history
            var allHistory = await _db.GetAllPlayHistoryAsync();

            // Count unique POIs scanned today
            var poisToday = allHistory
                .Where(h => h.PlayedAt.Date == today)
                .Select(h => h.POIId)
                .Distinct()
                .Count();

            // Count unique POIs scanned this week
            var poisWeek = allHistory
                .Where(h => h.PlayedAt >= weekStart)
                .Select(h => h.POIId)
                .Distinct()
                .Count();

            // Top 10 most listened POIs — group by name to merge web and app records
            var topScannedPOIs = allHistory
                .Where(h => !string.IsNullOrEmpty(h.POIName))
                .GroupBy(h => h.POIName)
                .Select(g => new
                {
                    poiName = g.Key,
                    scanCount = g.Count(),
                    lastScanned = g.Max(h => h.PlayedAt),
                    webCount = g.Count(h => h.Source == "web"),
                    appCount = g.Count(h => h.Source == "app" || h.Source == null)
                })
                .OrderByDescending(x => x.scanCount)
                .Take(10)
                .ToList();

            return Ok(new
            {
                poisScannedToday = poisToday,
                poisScannedWeek = poisWeek,
                topScannedPOIs
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
