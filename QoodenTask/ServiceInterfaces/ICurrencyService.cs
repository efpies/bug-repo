using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface ICurrencyService
{
    public Task<List<Currency>?> GetCurrencies();
    public Task<Currency?> GetCurrency(string? id);
}