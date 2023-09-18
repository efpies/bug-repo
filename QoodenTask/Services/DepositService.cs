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
    
    private async Task<Transaction> CreateBaseTransactionWithoutSave(int userId, string currencyId, BaseDepositModel baseDepositModel)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        var user = await _userService.GetById(userId);
        
        if (user == null) throw new Exception("User not found");
        if (currency == null) throw new Exception("Currency not found");
        
        return new Transaction
        {
            Amount = baseDepositModel.Amount,
            CurrencyId = currency.Id,
            UserId = user.Id,
            User = user,
            Currency = currency
        };
    }

    public async Task<Transaction?> DepositFiat(int userId, DepositFiatModel depositFiatModel, string currencyId)
    {
        var tx = await CreateBaseTransactionWithoutSave(userId, currencyId, depositFiatModel);
        if (tx.Currency.Type != CurrencyType.Fiat) throw new Exception("Incorrect currency type"); 

        tx.CardHolder = depositFiatModel.CardHolder;
        tx.CardNumber = depositFiatModel.CardNumber;
        
        _dbContext.Transactions.Add(tx);
        await _dbContext.SaveChangesAsync();
        return tx;
    }

    public async Task<Transaction?> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string currencyId)
    {
        var tx = await CreateBaseTransactionWithoutSave(userId, currencyId, depositCryptoModel);
        if (tx.Currency.Type != CurrencyType.Fiat) throw new Exception("Incorrect currency type");

        tx.Address = depositCryptoModel.Address;

        _dbContext.Transactions.Add(tx);
        await _dbContext.SaveChangesAsync();
        return tx;
    }
}