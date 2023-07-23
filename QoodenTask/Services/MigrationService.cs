using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Enums;
using QoodenTask.Models;
using Type = System.Type;

namespace QoodenTask.Services;

public class MigrationService: BackgroundService
{
    private IDbContextFactory<AppDbContext> _dbContextFactory { get; set; }
    
    public MigrationService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            if (await dbContext.Migrations.CountAsync() < 1)
            {
                await AddFirstMigrations(dbContext);
            }
            
            var migrations = await dbContext.Migrations.Where(m => m.Status == MigrationStatusEnum.IsWaiting).ToListAsync();

            foreach (var migration in migrations)
            {
                if (migration.SourceType == MigrationSourceTypeEnum.Json)
                {
                    var jsonService = new JsonService();
                    if (migration.SourceName == typeof(User).Name)
                    {
                        var dataList = await jsonService.GetDataFromJson<User>(migration.SourcePath);
                        await dbContext.Users.AddRangeAsync(dataList);
                    }
                    else if (migration.SourceName == typeof(Currency).Name)
                    {
                        var dataList = await jsonService.GetDataFromJson<Currency>(migration.SourcePath);
                        await dbContext.Currencies.AddRangeAsync(dataList);
                    }

                    migration.Status = MigrationStatusEnum.Done;
                    dbContext.Migrations.Update(migration);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }

    private async Task<int> AddFirstMigrations(AppDbContext appDbContext)
    {
        appDbContext.Migrations.Add(new Migration
        {
            SourceName = typeof(User).Name,
            SourcePath = @"..\QoodenTask\Users.json",
            SourceType = MigrationSourceTypeEnum.Json,
            Status = MigrationStatusEnum.IsWaiting
        });
        appDbContext.Migrations.Add(new Migration
        {
            SourceName = typeof(Currency).Name,
            SourcePath = @"..\QoodenTask\Curencies.json",
            SourceType = MigrationSourceTypeEnum.Json,
            Status = MigrationStatusEnum.IsWaiting
        });
        return await appDbContext.SaveChangesAsync();
    }
}