namespace QoodenTask.Models;

public class Currency
{
    public string Id { get; set; }
    public string Type { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Balance> Balances { get; set; }
    public List<Transaction> Transactions { get; set; }
}