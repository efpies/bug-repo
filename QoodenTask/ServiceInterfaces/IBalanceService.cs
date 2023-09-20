using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;

namespace QoodenTask.ServiceInterfaces;

public interface IBalanceService
{
    public Task<IDictionary<string, UserBalance>?> GetBalance(int userId);
    public Task<decimal> GetUsdBalance(int userId);
}