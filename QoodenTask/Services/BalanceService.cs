using Microsoft.EntityFrameworkCore;
using QoodenTask.Common;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class BalanceService : IBalanceService
{
    private readonly IUserService _userService;
    private readonly ICurrencyService _currencyService;
    private readonly IRateService _rateService;

    private readonly AppDbContext _dbContext;
    
    public BalanceService(IUserService userService, ICurrencyService currencyService, IRateService rateService, AppDbContext dbContext)
    {
        _userService = userService;
        _currencyService = currencyService;
        _rateService = rateService;
        _dbContext = dbContext;
    }

    public async Task<decimal> GetUsdBalance(int userId)
    {
        var balances = await GetBalance(userId);
        if (balances != null)
        {
            return balances.Select(b => b.Value.UsdAmount).Sum();
        }

        return 0;
    }

    public async Task<IDictionary<string, UserBalance>?> GetBalance(int userId)
    {
        var user = await _userService.GetById(userId);
        
        if (user != null)
        {
            if (user is { Role: Roles.Admin }) return null;
            user.Balances = await GetUserBalances(user) as List<Balance>;
        }
        else
        {
            throw new Exception("Incorrect user id");
        }

        var balances = new Dictionary<string, UserBalance>();
        
        if (user is { Balances: null })
        {
            return balances;
        }
        
        var currencies = await _currencyService.GetCurrencies();

        balances = currencies?.ToDictionary(c => c.Id , _ => new UserBalance());

        var currentRates = await _rateService.GetCurrentRates();
        
        user.Balances.ForEach( balance =>
        {
            balances![balance.CurrencyId].Balance = balance.Amount;
            balances[balance.CurrencyId].UsdAmount = 
                ( balance.Amount * currentRates?.Rates[balance.CurrencyId] ?? 0 );
        });

        return balances;
    }

    private async Task<IList<Balance>?> GetUserBalances(User user)
    {
        return await _dbContext.Balances.Where(b => b.UserId == user.Id).ToListAsync();
    }
}