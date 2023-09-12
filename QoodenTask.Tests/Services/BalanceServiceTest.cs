using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QoodenTask.Common;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;
using QoodenTask.Services;

namespace QoodenTask.Tests.Services;

public class BalanceServiceTest
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

        _dbContext.Users.Add(new User()
        {
            UserName = "testUser",
            Password = "testUser",
            Role = Roles.User
        });

        await _dbContext.SaveChangesAsync();

        _dbContext.Balances.Add(new Balance()
        {
            UserId = 1,
            Amount = 2,
            CurrencyId = "trxTest"
        });

        _dbContext.Balances.Add(new Balance()
        {
            UserId = 1,
            Amount = 1,
            CurrencyId = "ethTest"
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
        _webApplicationFactory.Dispose();
    }

    [Test]
    public async Task GetBalance_Success()
    {
        var serviceScopeFactory = _webApplicationFactory.Services.GetService<IServiceScopeFactory>();
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var balanceService = scope.ServiceProvider.GetRequiredService<IBalanceService>();
            var balances = await balanceService.GetBalance(1);
            balances!.Count.Should().Be(2);
        }
    }

    [Test]
    public async Task GetUsdBalance_Success()
    {
        var serviceScopeFactory = _webApplicationFactory.Services.GetService<IServiceScopeFactory>();
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var balanceService = scope.ServiceProvider.GetRequiredService<IBalanceService>();
            var balances = await balanceService.GetUsdBalance(1);
            balances!.Should().BePositive();
        }
    }
}