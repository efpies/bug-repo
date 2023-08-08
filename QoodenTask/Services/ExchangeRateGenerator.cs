using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.Options;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class ExchangeRateGenerator: BackgroundService
{
    private readonly ExchangeData _exchangeData;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly Random _random = new Random();
    private readonly int _rateDelayMiliseconds;


    public ExchangeRateGenerator(IOptionsMonitor<RateOptions> rateOption, ExchangeData exchangeData, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _exchangeData = exchangeData;
        _dbContextFactory = dbContextFactory;
        _rateDelayMiliseconds =
            rateOption.CurrentValue.RateDelayMiliseconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _exchangeData.RateHistory = new List<CurrencyRate>();
        var currencies = new List<Currency>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var currencyService = new CurrencyService(_dbContextFactory.CreateDbContext()))
                {
                    currencies = await currencyService.GetCurrencies();
                }
                GenRates(currencies);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
 
            await Task.Delay(_rateDelayMiliseconds);
        }
    }

    protected virtual void GenRates(List<Currency> currencies)
    {
        foreach (var currency in currencies)
        {
            if (_exchangeData.RateHistory.FindLast(x => x.Currency.Id == currency.Id) is {} currencyRate)
            {
                _exchangeData.RateHistory.Add(new CurrencyRate()
                {
                    Currency = currency,
                    Date = DateTime.UtcNow,
                    Rate = currencyRate.Rate * (decimal)(_random.NextDouble() > 0.5d ? _random.NextDouble() + 1 : _random.NextDouble())
                });
            }
            else
            {
                _exchangeData.RateHistory.Add(new CurrencyRate()
                {
                    Currency = currency,
                    Date = DateTime.UtcNow,
                    Rate = (decimal)(_random.Next(1000) + _random.NextDouble())
                });
            }
        }
    }
}