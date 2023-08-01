using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;

namespace QoodenTask.ServiceInterfaces;

public interface IBalanceService
{
    public Task<Dictionary<string, UserBalance>?> GetBalance(int userId);
}