using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QoodenTask.Common;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.Services;

namespace QoodenTask.Tests;

public class AdminUserControllerTest
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
        _dbContextFactory.CreateDbContext().Database.EnsureDeleted();
        _webApplicationFactory.Dispose();
    }

    [Test]
    public async Task GetUsers_Success()
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
                var response = await _client.GetAsync($"admin/users");
                response.Should().HaveStatusCode(HttpStatusCode.OK);
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
    public async Task BlockUser_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");

        if (user != null)
        {
            user.IsActive = true;

            _dbContext.Update(user);
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
                
                var response = await _client.PatchAsync($"admin/users/block/{user.Id}", null);
                response.Should().HaveStatusCode(HttpStatusCode.OK);
                
                await _dbContext.DisposeAsync();
                
                _dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                var blockedUser = await _dbContext.Users.FirstOrDefaultAsync(t =>
                    t.Id == user.Id);
                
                if (blockedUser != null) blockedUser.IsActive.Should().Be(false);
                else blockedUser.Should().NotBeNull();
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
    public async Task BlockUser_FailOnWrongUserId()
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
                
                var response = await _client.PatchAsync($"admin/users/block/-1", null);
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
    public async Task UnblockUser_Success()
    {
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "UsrTest" && u.Password == "usrTest");

        if (user != null)
        {
            user.IsActive = false;

            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            
            var admin = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");

            var usrs = await _dbContext.Users.ToListAsync();
            
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
                
                var response = await _client.PatchAsync($"admin/users/unblock/{user.Id}", null);
                response.Should().HaveStatusCode(HttpStatusCode.OK);
                
                await _dbContext.DisposeAsync();
                
                _dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                var unblockedUser = await _dbContext.Users.FirstOrDefaultAsync(t =>
                    t.Id == user.Id);
                
                if (unblockedUser != null) unblockedUser.IsActive.Should().Be(true);
                else unblockedUser.Should().NotBeNull();
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
    public async Task UnblockUser_FailOnWrongUserId()
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
                
                var response = await _client.PatchAsync($"admin/users/unblock/-1", null);
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