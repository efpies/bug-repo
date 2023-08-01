using QoodenTask.Enums;
using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface ITransactionService
{
    public Task<List<Transaction>?> GetTxsByStatus(TransactionStatus status);
    public Task<List<Transaction>?> GetTxsByUser(int userId);
    public Task<List<Transaction>?> GetTxs();
    public Task<Transaction?> GetTxById(int txId);
    public Task<Transaction?> ChangeStatusTx(Transaction tx, TransactionStatus newStatus);
    public Task<Transaction?> ApproveTx(Transaction tx);
    public Task<Transaction?> DeclineTx(Transaction tx);
}