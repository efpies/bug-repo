using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class BalanceService : IBalanceService
{
    private IUserService _userService { get; set; }
    private ICurrencyService _currencyService { get; set; }
    private ExchangeData _exchangeData { get; set; }
    
    private AppDbContext _dbContext { get; set; }
    
    public BalanceService(IUserService userService, ICurrencyService currencyService, ExchangeData exchangeData, AppDbContext dbContext)
    {
        _userService = userService;
        _currencyService = currencyService;
        _exchangeData = exchangeData;
        _dbContext = dbContext;
    }
    /*public async Task<Dictionary<string, UserBalance>> GetBalance(int userId)
    {
        var user = await _userService.GetById(userId);
        
        if (user.Role == "Admin") return null;
        
        var balances = new Dictionary<string, UserBalance>();
        foreach (var currency in await _currencyService.GetCurrencies())
        {
            balances.Add(currency.Id, new UserBalance());
        }
        if (user.Balances is not { })
        {
            return balances;
        }

        foreach (var balance in user.Balances)
        {
            balances[balance.Currency.Id].Balance = balance.Amount;
            balances[balance.Currency.Id].UsdAmount = balance.Amount * await GetCurrentRate(balance.Currency.Id);;
        }

        return balances;
    }

    public async Task<Currency> DepositFiat(int userId, DepositFiatModel depositFiatModel, string currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        if (currency.Type != "Fiat") return null; 
        var user = await _userService.GetById(userId);
        user.Balances.Find(x => x.Currency.Id == currencyId)!.Amount += depositFiatModel.Amount;
        _userService.Update(user);
        return currency;
    }

    public async Task<Currency> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string currencyId)
    {
        var currency = await _currencyService.GetCurrency(currencyId);
        if (currency.Type != "Crypto") return null;
        var user = await _userService.GetById(userId);
        user.Balances.Find(x => x.Currency.Id == currencyId)!.Amount += depositCryptoModel.Amount;
        _userService.Update(user);
        return currency;
    }

    public async Task<CurrentRates> GetCurrentRates()
    {
        var currencies = await _currencyService.GetCurrencies();
        var currentRates = new CurrentRates()
        {
            Date = DateTime.Now,
            Rates = new Dictionary<string, decimal>()
        };
        foreach (var currency in currencies)
        {
            var lastCurrencyRate = await GetCurrentRate(currency.Id);
            currentRates.Rates.Add(currency.Id,lastCurrencyRate);
        }

        if (currentRates.Rates is null)
            return null;
        
        return currentRates;
    }*/

    public async Task<Dictionary<string, UserBalance>> GetBalance(int userId)
    {
        var user = await _userService.GetById(userId);
        
        if (user.Role == "Admin") return null;
        
        var balances = new Dictionary<string, UserBalance>();
        foreach (var currency in await _currencyService.GetCurrencies())
        {
            balances.Add(currency.Id, new UserBalance());
        }
        if (user.Balances is not { })
        {
            return balances;
        }

        foreach (var balance in user.Balances)
        {
            balances[balance.Currency.Id].Balance = balance.Amount;
            balances[balance.Currency.Id].UsdAmount = balance.Amount * await GetCurrentRate(balance.Currency.Id);;
        }

        return balances;
    }

    public async Task<Transaction> DepositFiat(int userId, DepositFiatModel depositFiatModel, string currencyId)
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

    public async Task<Transaction> DepositCrypto(int userId, DepositCryptoModel depositCryptoModel, string currencyId)
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

    public async Task<List<Transaction>> GetTxsByStatus(TransactionStatusEnum status)
    {
        return await _dbContext.Transactions.Where(t => t.Status == status).ToListAsync();
    }

    public async Task<List<Transaction>> GetTxsByUser(int userId)
    {
        return await _dbContext.Transactions.Where(t => t.User.Id == userId).ToListAsync();
    }
    
    public async Task<List<Transaction>> GetTxs()
    {
        return await _dbContext.Transactions.ToListAsync();
    }
    
    public async Task<Transaction> GetTxById(int txId)
    {
        return await _dbContext.Transactions.FindAsync(txId);
    }

    public async Task<Transaction> ChangeStatusTx(int txId, TransactionStatusEnum newStatus)
    {
        var tx = await GetTxById(txId);
        if (tx != null)
        {
            tx.Status = newStatus;
            _dbContext.Transactions.Update(tx);
        }
        return tx;
    }

    public async Task<CurrentRates> GetCurrentRates()
    {
        var currencies = await _currencyService.GetCurrencies();
        var currentRates = new CurrentRates()
        {
            Date = DateTime.Now,
            Rates = new Dictionary<string, decimal>()
        };
        foreach (var currency in currencies)
        {
            var lastCurrencyRate = await GetCurrentRate(currency.Id);
            currentRates.Rates.Add(currency.Id, lastCurrencyRate);
        }

        if (currentRates.Rates is null)
            return null;

        return currentRates;
    }

    public async Task<decimal> GetCurrentRate(string currencyId)
    {
        return _exchangeData.RateHistory.FindLast(r => r.Currency.Id == currencyId).Rate;
    }
}