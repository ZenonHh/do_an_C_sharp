using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserStatusController : ControllerBase
{
    private readonly DatabaseService _db;

    public UserStatusController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<UserStatus>> GetUserStatus(int userId)
    {
        try
        {
            var status = await _db.GetUserStatusAsync(userId);
            if (status == null)
                return NotFound(new { error = "User status not found" });
            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<UserStatus>>> GetAllUserStatus()
    {
        try
        {
            var statuses = await _db.GetAllUserStatusAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("set-online/{userId}")]
    public async Task<ActionResult> SetUserOnline(int userId, [FromBody] SetStatusRequest request)
    {
        try
        {
            await _db.SetUserOnlineAsync(userId, request.IsOnline, request.DeviceInfo, request.IpAddress);
            return Ok(new { message = "User status updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class SetStatusRequest
    {
        public bool IsOnline { get; set; }
        public string DeviceInfo { get; set; } = "";
        public string IpAddress { get; set; } = "";
    }
}
