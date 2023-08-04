using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class RateService: IRateService
{
    
    private ICurrencyService _currencyService { get; set; }
    private ExchangeData _exchangeData { get; set; }

    public RateService(ICurrencyService currencyService, ExchangeData exchangeData)
    {
        _currencyService = currencyService;
        _exchangeData = exchangeData;
    }
    
    public async Task<CurrentRates?> GetCurrentRates()
    {
        var currencies = await _currencyService.GetCurrencies();
        var currentRates = new CurrentRates()
        {
            Date = DateTime.Now,
            Rates = new Dictionary<string, decimal>()
        };
        foreach (var currency in currencies)
        {
            var lastCurrencyRate = GetCurrentRate(currency.Id);
            if (lastCurrencyRate != null)
            currentRates.Rates.Add(currency.Id, (decimal)lastCurrencyRate);
        }

        if (currentRates.Rates is null)
            return null;

        return currentRates;
    }

    public decimal? GetCurrentRate(string currencyId)
    {
        return _exchangeData.RateHistory.FindLast(r => r.Currency.Id == currencyId)?.Rate;
    }
}