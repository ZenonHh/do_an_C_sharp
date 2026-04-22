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
    public async Task<ActionResult<List<User>>> GetAll()
    {
        try
        {
            var users = await _db.GetAllUsersAsync();
            return Ok(users);
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

            user.Id = id;
            await _db.UpdateUserAsync(user);
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
            var onlineUsers = await _db.GetOnlineUsersAsync();
            return Ok(onlineUsers);
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
            var activity = await _db.GetQRActivityTodayAsync();
            return Ok(new 
            { 
                totalScans = activity.Item1,
                uniqueUsers = activity.Item2,
                topPOIs = activity.Item3
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
