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

    private async Task<Transaction?> FillTxForFiat(DepositFiatModel depositFiatModel, Transaction tx)
    {
        if (tx.Currency.Type != CurrencyType.Fiat) throw new Exception("Incorrect currency type"); 

        tx.CardHolder = depositFiatModel.CardHolder;
        tx.CardNumber = depositFiatModel.CardNumber;

        return tx;
    }

    private async Task<Transaction?> FillTxForCrypto(DepositCryptoModel depositCryptoModel, Transaction tx)
    {
        if (tx.Currency.Type != CurrencyType.Crypto) throw new Exception("Incorrect currency type");

        tx.Address = depositCryptoModel.Address;

        return tx;
    }
    
    public async Task<Transaction?> Deposit(int userId, BaseDepositModel depositModel, string currencyId)
    {
        var tx = await CreateBaseTransactionWithoutSave(userId, currencyId, depositModel);

        switch (depositModel)
        {
            case DepositFiatModel depositFiatModel:
                tx = await FillTxForFiat(depositFiatModel, tx);
                break;
            case DepositCryptoModel depositCryptoModel:
                tx = await FillTxForCrypto(depositCryptoModel, tx);
                break;
        }

        if (tx == null) return tx;
        
        _dbContext.Transactions.Add(tx);
        await _dbContext.SaveChangesAsync();

        return tx;
    }
}