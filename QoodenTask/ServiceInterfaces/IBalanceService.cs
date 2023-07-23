using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;

namespace QoodenTask.ServiceInterfaces;

public interface IBalanceService
{
    public Task<Dictionary<string, UserBalance>> GetBalance(int userId);
    public Task<Transaction> DepositFiat(int userId, DepositFiatModel depositFiatModel, string currencyId);
    public Task<Transaction> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string currencyId);
    public Task<List<Transaction>> GetTxsByStatus(TransactionStatusEnum status);
    public Task<List<Transaction>> GetTxsByUser(int userId);
    public Task<List<Transaction>> GetTxs();
    public Task<Transaction> GetTxById(int txId);
    public Task<Transaction> ChangeStatusTx(int txId, TransactionStatusEnum newStatus);
    public Task<CurrentRates> GetCurrentRates();
    public Task<decimal> GetCurrentRate(string currencyId);
}