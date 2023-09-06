using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class DepositService : IDepositService
{
    private readonly IUserService _userService;
    private readonly ICurrencyService _currencyService;
    private readonly AppDbContext _dbContext;
    
    public DepositService(IUserService userService, ICurrencyService currencyService, AppDbContext dbContext)
    {
        _userService = userService;
        _currencyService = currencyService;
        _dbContext = dbContext;
    }
    
    public async Task<Transaction?> DepositFiat(int userId, DepositFiatModel depositFiatModel, string? currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        if (currency?.Type != CurrencyType.Fiat) throw new Exception("Incorrect currency type"); 
        var user = await _userService.GetById(userId);
        var tx = new Transaction
        {
            Amount = depositFiatModel.Amount,
            CurrencyId = currency.Id,
            UserId = user!.Id,
            CardNumber = depositFiatModel.CardNumber,
            CardHolder = depositFiatModel.CardHolder,
            User = user,
            Currency = currency
        };
        _dbContext.Transactions.Add(tx);
        await _dbContext.SaveChangesAsync();
        return tx;
    }

    public async Task<Transaction?> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string? currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        if (currency?.Type != CurrencyType.Crypto) throw new Exception("Incorrect currency type");
        var user = await _userService.GetById(userId);
        if (user != null)
        {
            var tx = new Transaction
            {
                Amount = depositCryptoModel.Amount,
                CurrencyId = currency.Id,
                UserId = user.Id,
                Address = depositCryptoModel.Address,
                User = user,
                Currency = currency
            };
            _dbContext.Transactions.Add(tx);
            await _dbContext.SaveChangesAsync();
            return tx;
        }
        return null;
    }
}