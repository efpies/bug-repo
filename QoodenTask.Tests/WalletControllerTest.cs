using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QoodenTask.Common;
using QoodenTask.Controllers;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.Models.Deposit;
using QoodenTask.Services;

namespace QoodenTask.Tests;

public class WalletControllerTest
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
        
        _dbContext.Users.Add(new User
        {
            UserName = "AdmTest",
            Password = "admTest",
            Role = Constants.Admin
        });
        
        _dbContext.Users.Add(new User
        {
            UserName = "UsrTest",
            Password = "usrTest",
            Role = Constants.User
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
    public async Task User_GetBalance_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = "Crypto"
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            user.Balances = new List<Balance>();
            user.Balances.Add(new Balance
            {
                Amount = 55,
                Currency = ethTest,
                CurrencyId = ethTest.Id,
                User = user,
                UserId = user.Id
            });

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            
            var content = JsonContent.Create(new LoginDto
            {
                Password = user.Password,
                UserId = user.Id
            });
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));
            
            var balancesResponse = await _client.GetAsync("wallet/balance");
            balancesResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task Admin_GetBalance_Success()
    {
        //Wait ExchangeRateGenerator
        await Task.Delay(5000);
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = "Crypto"
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            user.Balances = new List<Balance>();
            user.Balances.Add(new Balance
            {
                Amount = 55,
                Currency = ethTest,
                CurrencyId = ethTest.Id,
                User = user,
                UserId = user.Id
            });

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            
            var admin = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");

            if (admin != null)
            {
                var content = JsonContent.Create(new LoginDto
                {
                    Password = admin.Password,
                    UserId = admin.Id
                });
                var loginResponse = await _client.PostAsync("auth/login", content);

                CookieContainer cookies = new CookieContainer();
                foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                    cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

                _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));
                var balancesResponse = await _client.GetAsync($"wallet/balance/{user.Id}");
                balancesResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            }
            else
            {
                admin.Should().NotBeNull();
            }


        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task Admin_GetBalance_FailOnWrongUserId()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();

        var admin = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");

            if (admin != null)
            {
                var content = JsonContent.Create(new LoginDto
                {
                    Password = admin.Password,
                    UserId = admin.Id
                });
                var loginResponse = await _client.PostAsync("auth/login", content);

                CookieContainer cookies = new CookieContainer();
                foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                    cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

                _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));
            }
            else
            {
                admin.Should().NotBeNull();
            }

            var balancesResponse = await _client.GetAsync($"wallet/balance/-1");
            balancesResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DepositFiat_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "rubTest",
            IsActive = true,
            Type = "Fiat"
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = user.Password,
                UserId = user.Id
            });
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));

            BaseDepositModel depositModel = new DepositFiatModel()
            {
                Amount = 55,
                CardNumber = "2323244234244322",
                CardHolder = "Peter Test"
            };

            var depositeContent = JsonContent.Create(depositModel);

            var response = await _client.PostAsync("wallet/deposit/rubTest", depositeContent);
            response.Should().HaveStatusCode(HttpStatusCode.OK);
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task DepositFiat_FailOnWrongCurrency()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = "Crypto"
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = user.Password,
                UserId = user.Id
            });
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));

            BaseDepositModel depositModel = new DepositFiatModel()
            {
                Amount = 55,
                CardNumber = "2323244234244322",
                CardHolder = "Peter Test"
            };

            var depositeContent = JsonContent.Create(depositModel);

            var response = await _client.PostAsync("wallet/deposit/ethTest", depositeContent);
            response.Should().HaveStatusCode(HttpStatusCode.NotFound);
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task DepositCrypto_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = "Crypto"
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = user.Password,
                UserId = user.Id
            });
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));

            BaseDepositModel depositModel = new DepositCryptoModel()
            {
                Amount = 55,
                Address = "2323244234244322"
            };

            var depositeContent = JsonContent.Create(depositModel);

            var response = await _client.PostAsync("wallet/deposit/ethTest", depositeContent);
            response.Should().HaveStatusCode(HttpStatusCode.OK);
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task DepositCrypto_FailOnWrongCurrency()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "rubTest",
            IsActive = true,
            Type = "Fiat"
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = user.Password,
                UserId = user.Id
            });
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);

            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));

            BaseDepositModel depositModel = new DepositCryptoModel()
            {
                Amount = 55,
                Address = "2323244234244322"
            };

            var depositeContent = JsonContent.Create(depositModel);

            var response = await _client.PostAsync("wallet/deposit/rubTest", depositeContent);
            response.Should().HaveStatusCode(HttpStatusCode.NotFound);
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
}