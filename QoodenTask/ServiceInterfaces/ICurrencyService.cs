using QoodenTask.Models;

namespace QoodenTask.Services;

public interface ICurrencyService
{
    public Task<List<Currency>> GetCurrencies();
    public void AddCurrency();
}