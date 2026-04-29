using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly ILogger<SyncController> _logger;

    public SyncController(DatabaseService db, ILogger<SyncController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("history")]
    public async Task<IActionResult> SyncHistory([FromBody] SyncHistoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PoiName))
            return BadRequest(new { error = "PoiName is required" });

        try
        {
            // Try to resolve POI ID by name
            var allPois = await _db.GetAllPOIsAsync();
            var poi = allPois.FirstOrDefault(p =>
                string.Equals(p.Name, request.PoiName, StringComparison.OrdinalIgnoreCase));

            var history = new PlayHistory
            {
                UserId = int.TryParse(request.UserId, out var uid) ? uid : 0,
                POIId = poi?.Id ?? 0,
                POIName = request.PoiName,
                PlayedAt = DateTime.TryParse(request.PlayedAt, out var dt) ? dt : DateTime.Now,
                Source = "app"
            };

            await _db.InsertPlayHistoryAsync(history);
            _logger.LogInformation("Synced app play history: {PoiName}", request.PoiName);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing history");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class SyncHistoryRequest
{
    public string UserId { get; set; } = "";
    public string PoiName { get; set; } = "";
    public string PlayedAt { get; set; } = "";
    public string? DeviceId { get; set; }
}
