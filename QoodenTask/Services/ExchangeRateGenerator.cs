using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class ExchangeRateGenerator: BackgroundService
{
    //public List<CurrencyRate> RateHistory { get; private set; }
    private ExchangeData _exchangeData { get; set; }
    private IConfiguration _configuration { get; set; }
    private IDbContextFactory<AppDbContext> _dbContextFactory { get; set; }

    private Random _random { get; set; }

    private int _rateDelay { get; set; }


    public ExchangeRateGenerator(/*ICurrencyService currencyService,*/ 
        IConfiguration configuration, ExchangeData exchangeData, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _exchangeData = exchangeData;
        _configuration = configuration;
        //_currencyService = currencyService;
        _dbContextFactory = dbContextFactory;
        _random = new Random();
        _rateDelay = Convert.ToInt32(configuration["RateDelay"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _exchangeData.RateHistory = new List<CurrencyRate>();
        var currencies = new List<Currency>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var currencyService = new CurrencyService(_dbContextFactory.CreateDbContext()))//*new AppDbContext(_configuration["DbConnection"])))
                {
                    currencies = await currencyService.GetCurrencies();
                }
                GenRates(currencies);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
 
            await Task.Delay(_rateDelay);
        }
    }

    private void GenRates(List<Currency> currencies)
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