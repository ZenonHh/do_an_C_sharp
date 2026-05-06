using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DatabaseService _db;

    public UsersController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var users = await _db.GetAllUsersAsync();
            var allPayments = await _db.GetAllPaymentsAsync();

            // Lấy giao dịch mới nhất của mỗi user để hiển thị gói và ngày thanh toán
            var latestByUser = allPayments
                .GroupBy(p => p.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.PaymentDate).First());

            var result = users.Select(u =>
            {
                latestByUser.TryGetValue(u.Id, out var pay);
                return new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    phone = u.Phone,
                    language = u.Language,
                    isPaid = u.IsPaid || pay != null,           // paid if either flag or any payment record
                    paidAt = pay?.PaymentDate ?? u.PaidAt,      // prefer payment record date
                    packageName = pay?.PackageName ?? ""         // latest package bought
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        try
        {
            var user = await _db.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "User not found" });
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new { error = "Email is required" });

            await _db.InsertUserAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] User user)
    {
        try
        {
            var existing = await _db.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "User not found" });

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            // Phone is managed by the mobile app — don't overwrite from admin web
            existing.Language = user.Language;

            if (user.IsPaid && !existing.IsPaid)
                existing.PaidAt = DateTime.Now;
            else if (!user.IsPaid)
                existing.PaidAt = null;

            existing.IsPaid = user.IsPaid;

            await _db.UpdateUserAsync(existing);
            return Ok(new { message = "User updated successfully" });
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
            var existing = await _db.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "User not found" });

            await _db.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Dashboard endpoints
    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<OnlineUserSummary>> GetDashboardSummary()
    {
        try
        {
            var summary = await _db.GetDashboardSummaryAsync();

            // Thiết bị online: heartbeat mỗi 30s, coi offline nếu miss (35s)
            var allDevices = await _db.GetAllUserDevicesAsync();
            var onlineDeviceCount = allDevices.Count(d =>
                (DateTime.Now - d.LastOnlineAt).TotalSeconds <= 60 &&
                d.DeviceOS != "Windows" && d.DeviceOS != "macOS" && d.DeviceOS != "Linux");

            summary.TotalOnlineUsers = onlineDeviceCount;
            summary.OnlineDevices = onlineDeviceCount;
            summary.TodayQRScans = 0; // Ẩn số lượt nghe hôm nay

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("dashboard/online-users")]
    public async Task<ActionResult<List<UserDevice>>> GetOnlineUsers()
    {
        try
        {
            // Thiết bị hoạt động: gửi heartbeat trong 35s qua
            var allDevices = await _db.GetAllUserDevicesAsync();
            var realOnlineDevices = allDevices
                .Where(d => (DateTime.Now - d.LastOnlineAt).TotalSeconds <= 60 &&
                            d.DeviceOS != "Windows" && d.DeviceOS != "macOS" && d.DeviceOS != "Linux")
                .ToList();

            return Ok(realOnlineDevices);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("dashboard/qr-activity")]
    public async Task<ActionResult> GetQRActivity()
    {
        try
        {
            return Ok(new 
            { 
                totalScans = 0, // Trả về 0 để ẩn lượt quét trên Dashboard
                uniqueUsers = 0,
                topPOIs = new List<object>() // Trả về mảng rỗng để ẩn danh sách
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
