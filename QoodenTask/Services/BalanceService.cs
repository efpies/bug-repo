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
    private IRateService _rateService { get; set; }
    
    private AppDbContext _dbContext { get; set; }
    
    public BalanceService(IUserService userService, ICurrencyService currencyService, IRateService rateService, AppDbContext dbContext)
    {
        _userService = userService;
        _currencyService = currencyService;
        _rateService = rateService;
        _dbContext = dbContext;
    }

    public async Task<Dictionary<string, UserBalance>?> GetBalance(int userId)
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

        user.Balances.ForEach( balance =>      {
            balances[balance.Currency.Id].Balance = balance.Amount;
            balances[balance.Currency.Id].UsdAmount = (decimal)(balance.Amount * _rateService.GetCurrentRate(balance.Currency.Id));
        });

        return balances;
    }
}