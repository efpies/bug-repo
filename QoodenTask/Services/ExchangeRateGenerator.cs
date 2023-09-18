using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.Options;

namespace QoodenTask.Services;

public class ExchangeRateGenerator: BackgroundService
{
    private readonly ExchangeData _exchangeData;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly Random _random = new Random();
    private readonly int _rateDelayMilliseconds;


    public ExchangeRateGenerator(IOptionsMonitor<RateOptions> rateOption, ExchangeData exchangeData, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _exchangeData = exchangeData;
        _dbContextFactory = dbContextFactory;
        _rateDelayMilliseconds =
            rateOption.CurrentValue.RateDelayMiliseconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _exchangeData.RateHistory = new List<CurrencyRate>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IList<Currency>? currencies;
                using (var currencyService = new CurrencyService(await _dbContextFactory.CreateDbContextAsync(stoppingToken)))
                {
                    currencies = await currencyService.GetCurrencies();
                }

                if (currencies != null) GenRates(currencies);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
 
            await Task.Delay(_rateDelayMilliseconds, stoppingToken);
        }
    }

    protected virtual void GenRates(IList<Currency> currencies)
    {
        foreach (var currency in currencies)
        {
            var item = new CurrencyRate()
            {
                Currency = currency,
                Date = DateTime.UtcNow,
            };
            if (_exchangeData.RateHistory.FindLast(x => x.Currency.Id == currency.Id) is {} currencyRate)
            {
                item.Rate = currencyRate.Rate *
                            (decimal)(_random.NextDouble() > 0.5d ? _random.NextDouble() + 1 : _random.NextDouble());
            }
            else
            {
                item.Rate = (decimal)(_random.Next(1000) + _random.NextDouble());
            }
            _exchangeData.RateHistory.Add(item);
        }
    }
}