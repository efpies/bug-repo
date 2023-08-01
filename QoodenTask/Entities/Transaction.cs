using QoodenTask.Enums;

namespace QoodenTask.Models;

public class Transaction
{
    public int Id { get; set; }
    public string CurrencyId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public User User { get; set; }
    public Currency Currency { get; set; }
}