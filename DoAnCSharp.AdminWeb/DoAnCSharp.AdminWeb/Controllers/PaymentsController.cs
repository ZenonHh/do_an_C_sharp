using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

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

    // Get all payments for admin dashboard
    [HttpGet]
    public async Task<ActionResult<List<UserPayment>>> GetAllPayments()
    {
        try
        {
            var payments = await _db.GetAllPaymentsAsync();
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
