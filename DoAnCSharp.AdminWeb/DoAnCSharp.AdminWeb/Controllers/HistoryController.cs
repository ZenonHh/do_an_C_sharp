using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly DatabaseService _db;

    public HistoryController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var history = await _db.GetAllHistoryAsync();
            var users = await _db.GetAllUsersAsync();

            var result = history
                .OrderByDescending(h => h.PlayedAt)
                .Select(h => {
                    var user = users.FirstOrDefault(u => u.Id == h.UserId);
                    return new {
                        id = h.Id,
                        userId = h.UserId,
                        userName = user?.FullName ?? (h.UserId > 0 ? $"User #{h.UserId}" : "Khách"),
                        userEmail = user?.Email ?? "",
                        poiId = h.POIId,
                        poiName = h.POIName,
                        playedAt = h.PlayedAt,
                        source = h.Source ?? "web"
                    };
                }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _db.DeleteHistoryAsync(id);
            if (result == 0)
                return NotFound(new { error = "History not found" });

            return Ok(new { message = "History deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
