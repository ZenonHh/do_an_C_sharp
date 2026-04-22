using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class UserPayment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidDate { get; set; } = null;

    public string PaymentMethod { get; set; } = "Credit Card";

    public decimal Amount { get; set; } = 0m;
}
