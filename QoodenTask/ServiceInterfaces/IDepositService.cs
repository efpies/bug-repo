using QoodenTask.Models;
using QoodenTask.Models.Deposit;

namespace QoodenTask.ServiceInterfaces;

public interface IDepositService
{
    public Task<Transaction?> Deposit(int userId, BaseDepositModel depositModel, string currencyId);
}