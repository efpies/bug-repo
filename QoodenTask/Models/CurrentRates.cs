namespace QoodenTask.Models;

public class CurrentRates
{
    public DateTime Date { get; set; }
    public List<Currency> rates { get; set; }
}