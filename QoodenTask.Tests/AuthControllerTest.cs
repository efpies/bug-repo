using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QoodenTask.Common;
using QoodenTask.Controllers;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.Services;

namespace QoodenTask.Tests;

public class AuthControllerTest
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
    public async Task Login_Success()
    {
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = user.Password,
                UserId = user.Id
            });
        
            var response = await _client.PostAsync("auth/login", content);
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Headers.Should().NotBeEmpty();
        }

        /*CookieContainer cookies = new CookieContainer();
         var uri = _client.DefaultRequestHeaders.Host;
        // получаем из запроса все элементы с заголовком Set-Cookie
        foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
            // добавляем заголовки кук в CookieContainer
            cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);
        
        foreach (Cookie cookie in cookies.GetCookies(new Uri("https://localhost:44390")))
            Console.WriteLine($"{cookie.Name}: {cookie.Value}");
        
        _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));

        var resp2 = await _client.GetAsync("admin/users");
        resp2.Should().HaveStatusCode(HttpStatusCode.OK);*/
    }
    
    [Test]
    public async Task Login_FailOnWrongUserId()
    {
        _client = _webApplicationFactory.CreateClient();

        var content = JsonContent.Create(new LoginDto
        {
            Password = "Pass",
            UserId = -1
        });
        var uri = _client.DefaultRequestHeaders.Host;
        var response = await _client.PostAsync("auth/login", content);
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
        response.Headers.Should().BeEmpty();
    }
    
    [Test]
    public async Task Login_FailOnWrongPassword()
    {
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = "wrong",
                UserId = user.Id
            });
            var uri = _client.DefaultRequestHeaders.Host;
            var response = await _client.PostAsync("auth/login", content);
            response.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            response.Headers.Should().BeEmpty();
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task Logout_Success()
    {
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = "admTest",
                UserId = user.Id
            });
            var uri = _client.DefaultRequestHeaders.Host;
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);
            
            foreach (Cookie cookie in cookies.GetCookies(new Uri("https://localhost:44390")))
                Console.WriteLine($"{cookie.Name}: {cookie.Value}");
            
            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));
            
            var logoutResponse = await _client.PostAsync("auth/logou",null);
            logoutResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            /*_client = _webApplicationFactory.CreateClient();
            var testResponse = await _client.PatchAsync($"auth/change-password?testpass",null);
            testResponse.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
            var user2 = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest");*/
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task SignUp_Success()
    {
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var content = JsonContent.Create(new UserDto
            {
                Password = "Password",
                UserName = "SignUp"
            });
        
        var response = await _client.PostAsync("auth/sign-up", content);
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        response.Headers.Should().NotBeEmpty();
            
        var newUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "SignUp" && u.Password == "Password");
        newUser.Should().NotBeNull();
    }
    
    [Test]
    public async Task ChangePassword_Success()
    {
        
        _client = _webApplicationFactory.CreateClient();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "admTest");
        if (user != null)
        {
            var content = JsonContent.Create(new LoginDto
            {
                Password = "admTest",
                UserId = user.Id
            });
            var uri = _client.DefaultRequestHeaders.Host;
            var loginResponse = await _client.PostAsync("auth/login", content);

            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in loginResponse.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(new Uri("https://localhost:44390"), cookieHeader);
            
            foreach (Cookie cookie in cookies.GetCookies(new Uri("https://localhost:44390")))
                Console.WriteLine($"{cookie.Name}: {cookie.Value}");
            
            _client.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri("https://localhost:44390")));
            
            var response = await _client.PatchAsync($"auth/change-password?newPass=newpass",null);
            response.Should().HaveStatusCode(HttpStatusCode.OK);

            user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "AdmTest" && u.Password == "newpass");
            user.Should().NotBeNull();
        }
        else
        {
            user.Should().NotBeNull();
        }
    }
    
    [Test]
    public async Task ChangePassword_FailOnUnauthorized()
    {
        _client = _webApplicationFactory.CreateClient();
        var testResponse = await _client.PatchAsync($"auth/change-password?newPass=newpass",null);
        testResponse.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
    }
}