using Microsoft.EntityFrameworkCore;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;
using QoodenTask.Data;

namespace QoodenTask.Services;

public class CurrencyService : ICurrencyService, IDisposable
{
    private readonly AppDbContext _dbContext;

    public CurrencyService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Currency>?> GetCurrencies()
    {
        return await _dbContext.Currencies.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<Currency?> GetCurrency(string? id)
    {
        return await _dbContext.Currencies.SingleOrDefaultAsync(c => c.Id == id && c.IsActive);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}