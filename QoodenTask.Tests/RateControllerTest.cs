using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Services;

namespace QoodenTask.Tests;

public class RateControllerTest
{
    private HttpClient _client;
    private WebApplicationFactory<Program> _webApplicationFactory;
    private IConfiguration _configuration;
    private AppDbContext _dbContext;
    private IDbContextFactory<AppDbContext> _dbContextFactory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(
            b =>
            {
                b.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                    conf.AddEnvironmentVariables();

                    _configuration = conf.Build();
                });
                b.ConfigureServices(c =>
                    {
                        
                        var descriptor = c.Single(s => s.ImplementationType == typeof(MigrationService));
                        c.Remove(descriptor);
                        
                        c.AddDbContextFactory<AppDbContext>(
                            options =>
                                options.UseNpgsql(_configuration["DbConnection_temp"]));
                                
                        c.AddDbContext<AppDbContext>(
                            options =>
                                options.UseNpgsql(_configuration["DbConnection_temp"]));
                    });
            });
        
        _dbContextFactory = _webApplicationFactory.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        }
    
    [SetUp]
    public async Task Setup()
    {
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
        
        _dbContext.Currencies.Add(new Currency()
        {
            Id = "trxTest",
            IsActive = true,
            Type = CurrencyType.Crypto
        });
        _dbContext.Currencies.Add(new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = CurrencyType.Crypto
        });
        await _dbContext.SaveChangesAsync();
    }
    
    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _dbContext.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dbContextFactory.CreateDbContext().Database.EnsureDeleted();
        _webApplicationFactory.Dispose();
    }

    [Test]
    public async Task GetRates_Success()
    {
        //Wait ExchangeRateGenerator
        await Task.Delay(5000);
        
        var response = await _client.GetAsync("/rates");
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        var result = response.Content.ReadAsStringAsync().Result;
        var currentRates = JsonConvert.DeserializeObject<CurrentRates>(result);
        var currencies = await _dbContext.Currencies.Where(c => c.IsActive).ToListAsync();
        if (currentRates != null)
        {
            currentRates.Rates.Count().Should().Be(currencies.Count);

            foreach (var currency in currencies)
            {
                currentRates.Rates[currency.Id].Should().BePositive();
            }
        }
    }
}