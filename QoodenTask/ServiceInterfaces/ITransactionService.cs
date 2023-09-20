using QoodenTask.Enums;
using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface ITransactionService
{
    public Task<IList<Transaction>?> GetTxsByStatus(TransactionStatus status);
    public Task<IList<Transaction>?> GetTxsByUser(int userId);
    public Task<IList<Transaction>?> GetAllTxs(string? currencyId = null);
    public Task<Transaction?> GetTxById(int txId);
    public Task<Transaction?> ApproveTx(Transaction tx);
    public Task<Transaction?> DeclineTx(Transaction tx);
    public Task<Transaction?> CancelTx(Transaction tx);
}