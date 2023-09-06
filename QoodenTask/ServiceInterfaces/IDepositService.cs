using QoodenTask.Models;
using QoodenTask.Models.Deposit;

namespace QoodenTask.ServiceInterfaces;

public interface IDepositService
{
    public Task<Transaction?> DepositFiat(int userId, DepositFiatModel depositFiatModel, string? currencyId);
    public Task<Transaction?> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string? currencyId);
}