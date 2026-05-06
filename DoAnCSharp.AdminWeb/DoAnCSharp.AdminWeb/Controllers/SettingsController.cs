using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly DatabaseService _db;

    public SettingsController(DatabaseService db) => _db = db;

    // Mobile app calls this at startup to get the current payment configuration.
    // Returns the payment model, free daily quota, and package details so the
    // admin can change them without requiring an app update.
    [HttpGet("app-config")]
    public async Task<ActionResult> GetAppConfig()
    {
        try
        {
            static int Parse(string? s, int fallback)
                => int.TryParse(s, out var n) ? n : fallback;

            string paymentModel       = await _db.GetSettingValueAsync("Payment.Model") ?? "listen";
            int dailyFreeListens      = Parse(await _db.GetSettingValueAsync("Payment.DailyFreeListens"), 5);
            int pkgBasicListens       = Parse(await _db.GetSettingValueAsync("Payment.PkgBasicListens"),   5);
            int pkgBasicPrice         = Parse(await _db.GetSettingValueAsync("Payment.PkgBasicPrice"),     5000);
            int pkgPremiumListens     = Parse(await _db.GetSettingValueAsync("Payment.PkgPremiumListens"), 20);
            int pkgPremiumPrice       = Parse(await _db.GetSettingValueAsync("Payment.PkgPremiumPrice"),   15000);
            int pkgVipListens         = Parse(await _db.GetSettingValueAsync("Payment.PkgVipListens"),     999);
            int pkgVipPrice           = Parse(await _db.GetSettingValueAsync("Payment.PkgVipPrice"),       50000);

            return Ok(new
            {
                paymentModel,
                dailyFreeListens,
                packages = new[]
                {
                    new { id = "basic",   listens = pkgBasicListens,   price = pkgBasicPrice   },
                    new { id = "premium", listens = pkgPremiumListens, price = pkgPremiumPrice },
                    new { id = "vip",     listens = pkgVipListens,     price = pkgVipPrice     }
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Admin web calls this to update a single setting value.
    [HttpPut("{key}")]
    public async Task<ActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest req)
    {
        try
        {
            await _db.UpsertSettingAsync(key, req.Value, updatedBy: "admin");
            return Ok(new { message = "Setting updated", key, value = req.Value });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Returns all settings — useful for an admin settings page.
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var settings = await _db.GetAllSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record UpdateSettingRequest(string Value);
