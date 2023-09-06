using QoodenTask.Enums;

namespace QoodenTask.Models;

public class Transaction
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CurrencyId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public string? Address { get; set; }
    public string? CardNumber { get; set; }
    public string? CardHolder { get; set; }
    public User User { get; set; }
    public Currency Currency { get; set; }
}