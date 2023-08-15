﻿using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
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

    public async Task<Dictionary<string, UserBalance>?> GetBalance(int userId)
    {
        var user = await _userService.GetById(userId);
        
        if (user.Role == "Admin") return null;

        var userBalances = await GetUserBalances(user);
        
        var balances = new Dictionary<string, UserBalance>();
        foreach (var currency in await _currencyService.GetCurrencies())
        {
            balances.Add(currency.Id, new UserBalance());
        }
        if (user.Balances is not { })
        {
            return balances;
        }

        user.Balances.ForEach( balance =>
        {
            balances[balance.CurrencyId].Balance = balance.Amount;
            balances[balance.CurrencyId].UsdAmount =
                76; //(decimal)(balance.Amount * _rateService.GetCurrentRate(balance.Currency.Id));
        });

        return balances;
    }
    public async Task<List<Balance>?> GetUserBalances(User user)
    {
        var balances = await _dbContext.Balances.Where(b => b.UserId == user.Id).ToListAsync();
        return balances;
    }
}