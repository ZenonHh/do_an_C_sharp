using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

public class AppPaymentSyncRequest
{
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PackageName { get; set; } = "";
    public decimal Amount { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly DatabaseService _db;

    public PaymentsController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<UserPayment>> GetUserPayment(int userId)
    {
        try
        {
            var payment = await _db.GetUserPaymentByUserIdAsync(userId);
            if (payment == null)
                return NotFound(new { error = "Payment record not found" });
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> AddPayment([FromBody] UserPayment payment)
    {
        try
        {
            if (payment.UserId <= 0)
                return BadRequest(new { error = "UserId is required" });

            var existing = await _db.GetUserPaymentByUserIdAsync(payment.UserId);
            if (existing != null)
                return BadRequest(new { error = "User already has a payment record" });

            await _db.InsertUserPaymentAsync(payment);
            return CreatedAtAction(nameof(GetUserPayment), new { userId = payment.UserId }, payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("user/{userId}")]
    public async Task<ActionResult> UpdatePayment(int userId, [FromBody] UserPayment payment)
    {
        try
        {
            var existing = await _db.GetUserPaymentByUserIdAsync(userId);
            if (existing == null)
                return NotFound(new { error = "Payment record not found" });

            payment.UserId = userId;
            payment.Id = existing.Id;
            await _db.UpdateUserPaymentAsync(payment);
            return Ok(new { message = "Payment updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("sync-from-app")]
    public async Task<ActionResult> SyncFromApp([FromBody] AppPaymentSyncRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { error = "Email is required" });

            // 1. Tìm hoặc tạo User trên Server dựa trên Email từ App
            var users = await _db.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));
            
            if (user == null)
            {
                user = new User 
                {
                    Email = request.Email,
                    FullName = string.IsNullOrEmpty(request.FullName) ? "App User" : request.FullName,
                    Password = "AppUserAuto", // Mật khẩu ảo cho user đồng bộ từ App
                    Phone = "",
                    Avatar = "dotnet_bot.png"
                };
                await _db.InsertUserAsync(user);
                
                // Lấy lại danh sách để có Id mới
                users = await _db.GetAllUsersAsync();
                user = users.FirstOrDefault(u => string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                // Ép Server cập nhật lại Tên thật của người dùng nếu App gửi lên tên mới
                if (!string.IsNullOrEmpty(request.FullName) && request.FullName != "App User" && user.FullName != request.FullName)
                {
                    user.FullName = request.FullName;
                    await _db.UpdateUserAsync(user);
                }
            }

            if (user == null) return BadRequest(new { error = "Failed to sync user" });

            // 2. Luôn tạo bản ghi mới cho mỗi lần mua — giữ toàn bộ lịch sử giao dịch
            var now = DateTime.Now;
            var payment = new UserPayment
            {
                UserId = user.Id,
                PackageName = request.PackageName,
                Amount = request.Amount,
                PaymentDate = now,
                IsPaid = true
            };
            await _db.InsertUserPaymentAsync(payment);

            // 3. Cập nhật trạng thái IsPaid + PaidAt trong bảng User để Users tab hiển thị đúng
            user.IsPaid = true;
            user.PaidAt = now;
            await _db.UpdateUserAsync(user);

            return Ok(new { message = "Payment synced successfully to Admin Web" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Get all payments for admin dashboard
    [HttpGet]
    public async Task<ActionResult> GetAllPayments()
    {
        try
        {
            var payments = await _db.GetAllPaymentsAsync();
            var users = await _db.GetAllUsersAsync();

            // Ghép hóa đơn với tên và Email người mua
            var result = payments.Select(p => {
                var user = users.FirstOrDefault(u => u.Id == p.UserId);
                return new {
                    id = p.Id,
                    userName = user?.FullName ?? "Khách vãng lai",
                    userEmail = user?.Email ?? "Không có Email",
                    packageName = p.PackageName,
                    amount = p.Amount,
                    paymentDate = p.PaymentDate
                };
            }).OrderByDescending(x => x.paymentDate).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
