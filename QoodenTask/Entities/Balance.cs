namespace QoodenTask.Models;

public class Balance
{
    public int Id { get; set; }
    public string CurrencyId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public User User { get; set; }

}