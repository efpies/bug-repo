using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _dbContext;
    
    public TransactionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IList<Transaction>?> GetTxsByStatus(TransactionStatus status)
    {
        return await _dbContext.Transactions.Where(t => t.Status == status).ToListAsync();
    }

    public async Task<IList<Transaction>?> GetTxsByUser(int userId)
    {
        return await _dbContext.Transactions.Where(t => t.UserId == userId).ToListAsync();
    }
    
    public async Task<IList<Transaction>?> GetAllTxs(string? currencyId = null)
    {
        if (currencyId != null)
        {
            return await _dbContext.Transactions.Where(tx => tx.CurrencyId == currencyId).ToListAsync();
        }
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
    
    public async Task<Transaction?> CancelTx(Transaction tx)
    {
        return await ChangeStatusTx(tx, TransactionStatus.Canceled);
    }

    private async Task<Transaction?> ChangeStatusTx(Transaction tx, TransactionStatus newStatus)
    {
        tx.Status = newStatus;
        _dbContext.Transactions.Update(tx);
        await _dbContext.SaveChangesAsync();
        return tx;
    }
}