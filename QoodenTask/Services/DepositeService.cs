using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class DepositeService : IDepositeService
{
    private readonly IUserService _userService;
    private readonly ICurrencyService _currencyService;
    private readonly AppDbContext _dbContext;
    
    public DepositeService(IUserService userService, ICurrencyService currencyService, AppDbContext dbContext)
    {
        _userService = userService;
        _currencyService = currencyService;
        _dbContext = dbContext;
    }
    
    public async Task<Transaction?> DepositFiat(int userId, DepositFiatModel depositFiatModel, string currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        if (currency.Type != "Fiat") return null; 
        var user = await _userService.GetById(userId);
        var tx = new Transaction
        {
            Amount = depositFiatModel.Amount,
            User = user,
            Currency = currency
        };
        _dbContext.Transactions.Add(tx);
        return tx;
    }

    public async Task<Transaction?> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        if (currency.Type != "Crypto") return null;
        var user = await _userService.GetById(userId);
        var tx = new Transaction
        {
            Amount = depositCryptoModel.Amount,
            User = user,
            Currency = currency
        };
        _dbContext.Transactions.Add(tx);
        return tx;
    }
}