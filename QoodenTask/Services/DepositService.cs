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
    
    public async Task<Transaction?> Deposit(int userId, BaseDepositModel depositModel, string? currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        var user = await _userService.GetById(userId);
        if (user == null || currency == null) return null;
        var tx = new Transaction
        {
            Amount = depositModel.Amount,
            CurrencyId = currency.Id,
            UserId = user.Id,
            User = user,
            Currency = currency
        };
        if (depositModel is DepositFiatModel depositFiatModel)
        {
            if (currency.Type != CurrencyType.Fiat) throw new Exception("Incorrect currency type");
            tx.CardNumber = depositFiatModel.CardNumber;
            tx.CardHolder = depositFiatModel.CardHolder;
        }
        else if (depositModel is DepositCryptoModel depositCryptoModel)
        {
            if (currency.Type != CurrencyType.Crypto) throw new Exception("Incorrect currency type");
            tx.Address = depositCryptoModel.Address;
        }
        _dbContext.Transactions.Add(tx);
        await _dbContext.SaveChangesAsync();
        return tx;
    }
}