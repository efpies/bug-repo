using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QoodenTask.Common;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using QoodenTask.Services;

namespace QoodenTask.Tests;

public class AdminWalletControllerTest
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
            Role = Roles.Admin
        });
        
        _dbContext.Users.Add(new User
        {
            UserName = "UsrTest",
            Password = "usrTest",
            Role = Roles.User
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
    public async Task GetTxs_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = CurrencyType.Crypto
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            user.Transactions = new List<Transaction>();
            user.Transactions.Add(new Transaction()
            {
                Amount = 55,
                Currency = ethTest,
                CurrencyId = ethTest.Id,
                User = user,
                UserId = user.Id,
                Status = TransactionStatus.Waiting
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
                var balancesResponse = await _client.GetAsync($"admin/wallets/tx");
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
    public async Task ApproveTx_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = CurrencyType.Crypto
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            user.Transactions = new List<Transaction>();
            user.Transactions.Add(new Transaction()
            {
                Amount = 55,
                Currency = ethTest,
                CurrencyId = ethTest.Id,
                User = user,
                UserId = user.Id,
                Status = TransactionStatus.Waiting
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
                
                var response = await _client.PatchAsync($"admin/wallets/approve/1", null);
                response.Should().HaveStatusCode(HttpStatusCode.OK);
                
                await _dbContext.DisposeAsync();
                
                _dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                var tx = await _dbContext.Transactions.FirstOrDefaultAsync(t =>
                    t.Id == 1);
                
                if (tx != null) tx.Status.Should().Be(TransactionStatus.Approved);
                else tx.Should().NotBeNull();
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
    public async Task ApproveTx_FailOnWrongTxId()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
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
                
                var response = await _client.PatchAsync($"admin/wallets/approve/-1", null);
                response.Should().HaveStatusCode(HttpStatusCode.NotFound);
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
    public async Task DeclineTx_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        var ethTest = new Currency()
        {
            Id = "ethTest",
            IsActive = true,
            Type = CurrencyType.Crypto
        };

        _dbContext.Currencies.Add(ethTest);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
            user.Transactions = new List<Transaction>();
            user.Transactions.Add(new Transaction()
            {
                Amount = 55,
                Currency = ethTest,
                CurrencyId = ethTest.Id,
                User = user,
                UserId = user.Id,
                Status = TransactionStatus.Waiting
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
                
                var response = await _client.PatchAsync($"admin/wallets/decline/1", null);
                response.Should().HaveStatusCode(HttpStatusCode.OK);
                
                await _dbContext.DisposeAsync();
                
                _dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                var tx = await _dbContext.Transactions.FirstOrDefaultAsync(t =>
                    t.Id == 1);
                
                if (tx != null) tx.Status.Should().Be(TransactionStatus.Declined);
                else tx.Should().NotBeNull();
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
    public async Task DeclineTx_FailOnWrongTxId()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");
        if (user != null)
        {
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
                
                var response = await _client.PatchAsync($"admin/wallets/decline/-1", null);
                response.Should().HaveStatusCode(HttpStatusCode.NotFound);
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
}