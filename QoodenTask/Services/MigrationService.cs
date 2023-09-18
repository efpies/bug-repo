using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;

namespace QoodenTask.Services;

public class MigrationService: BackgroundService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    
    public MigrationService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(stoppingToken);
        if (!await dbContext.Migrations.AnyAsync(cancellationToken: stoppingToken))
        {
            await AddFirstMigrations(dbContext);
        }
            
        var migrations = await dbContext.Migrations.Where(m => m.Status == MigrationStatus.Waiting).ToListAsync(cancellationToken: stoppingToken);

        foreach (var migration in migrations)
        {
            if (migration.SourceType == MigrationSourceType.Json)
            {
                try
                {
                    if (migration.SourceName == nameof(User))
                    {
                        var dataList = await JsonService.GetDataFromJson<User>(migration.SourcePath);
                        if (dataList != null) await dbContext.Users.AddRangeAsync(dataList, stoppingToken);
                    }
                    else if (migration.SourceName == nameof(Currency))
                    {
                        var dataList = await JsonService.GetDataFromJson<Currency>(migration.SourcePath);
                        if (dataList != null) await dbContext.Currencies.AddRangeAsync(dataList, stoppingToken);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid SourceName: {migration.SourceName}");
                    }

                    migration.Status = MigrationStatus.Done;
                    dbContext.Migrations.Update(migration);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch(Exception exception)
                {
                    Console.WriteLine($"Error: {exception.Message}");
                }
            }
            else
            {
                Console.WriteLine($"A suitable handler was not found. Migration sourceType: {migration.SourceType}");
            }
        }
    }

    private async Task AddFirstMigrations(AppDbContext appDbContext)
    {
        appDbContext.Migrations.Add(new Migration
        {
            SourceName = nameof(User),
            SourcePath = @"..\QoodenTask\Users.json",
            SourceType = MigrationSourceType.Json,
            Status = MigrationStatus.Waiting
        });
        appDbContext.Migrations.Add(new Migration
        {
            SourceName = nameof(Currency),
            SourcePath = @"..\QoodenTask\Curencies.json",
            SourceType = MigrationSourceType.Json,
            Status = MigrationStatus.Waiting
        });
        await appDbContext.SaveChangesAsync();
    }
}