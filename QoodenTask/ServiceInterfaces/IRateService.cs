using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface IRateService
{
    public Task<CurrentRates?> GetCurrentRates();
    public decimal GetCurrentRate(string currencyId);
}