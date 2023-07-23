namespace QoodenTask.Models;

public class CurrentRates
{
    public DateTime Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}