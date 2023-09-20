using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface ICurrencyService
{
    public Task<IList<Currency>?> GetCurrencies();
    public Task<Currency?> GetCurrency(string? id);
}