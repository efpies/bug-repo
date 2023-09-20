using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Options;
using QoodenTask.ServiceInterfaces;
using QoodenTask.Services;
using Radzen;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
                {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                    options.SlidingExpiration = true;
        
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            );

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSingleton<WeatherForecastService>();

        builder.Services.AddSingleton<ExchangeData>();
        builder.Services.AddScoped<ICurrencyService, CurrencyService>();
        builder.Services.AddScoped<IRateService, RateService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IBalanceService, BalanceService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<IDepositService, DepositService>();
        builder.Services.AddScoped<DialogService>();
        builder.Services.AddHostedService<ExchangeRateGenerator>();
        builder.Services.AddHostedService<MigrationService>();
        builder.Services.AddDbContextFactory<AppDbContext>(
            options =>
                options.UseNpgsql(builder.Configuration["DbConnection"]));
        builder.Services.AddDbContext<AppDbContext>(
            options =>
                options.UseNpgsql(builder.Configuration["DbConnection"]), ServiceLifetime.Transient);
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt => {
            opt.UseOneOfForPolymorphism();

            opt.SelectDiscriminatorNameUsing(_ => "$type");
            opt.SelectDiscriminatorValueUsing(subType => subType.BaseType!
                .GetCustomAttributes<JsonDerivedTypeAttribute>()
                .FirstOrDefault(x => x.DerivedType == subType)?
                .TypeDiscriminator!.ToString());
        });

        builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:63342")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }));
        builder.Services.Configure<RateOptions>(builder.Configuration.GetSection("RateOptions"));

        using (var scope = builder.Services.BuildServiceProvider().CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            try
            {
                var context = serviceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
        
            });
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseCors();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapBlazorHub();
        app.MapControllers();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}