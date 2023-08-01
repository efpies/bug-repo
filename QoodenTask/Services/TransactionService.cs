using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class TransactionService : ITransactionService
{
    private AppDbContext _dbContext { get; set; }
    
    public TransactionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Transaction>?> GetTxsByStatus(TransactionStatus status)
    {
        return await _dbContext.Transactions.Where(t => t.Status == status).ToListAsync();
    }

    public async Task<List<Transaction>?> GetTxsByUser(int userId)
    {
        return await _dbContext.Transactions.Where(t => t.User.Id == userId).ToListAsync();
    }
    
    public async Task<List<Transaction>?> GetTxs()
    {
        return await _dbContext.Transactions.ToListAsync();
    }
    
    public async Task<Transaction?> GetTxById(int txId)
    {
        return await _dbContext.Transactions.FindAsync(txId);
    }

    public async Task<Transaction?> ApproveTx(Transaction tx)
    {
        return await ChangeStatusTx(tx, TransactionStatus.Approved);
    }
    
    public async Task<Transaction?> DeclineTx(Transaction tx)
    {
        return await ChangeStatusTx(tx, TransactionStatus.Declined);
    }

    public async Task<Transaction?> ChangeStatusTx(Transaction tx, TransactionStatus newStatus)
    {
        tx.Status = newStatus;
        _dbContext.Transactions.Update(tx);
        return tx;
    }
}