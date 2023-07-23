using Microsoft.EntityFrameworkCore;
using QoodenTask.Models;

namespace QoodenTask.Data;

public class AppDbContext : DbContext
{
    //private string _connStr { get; set; }
    public DbSet<User> Users => Set<User>();
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Migration> Migrations => Set<Migration>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var mb = modelBuilder.Entity<Balance>().ToTable("balances");
        mb.HasOne<User>().WithMany(u => u.Balances).HasForeignKey(b => b.UserId).HasPrincipalKey(u => u.Id);
        mb.HasOne<Currency>().WithMany(c => c.Balances).HasForeignKey(b => b.CurrencyId).HasPrincipalKey(c => c.Id);
        mb.Property(b => b.Id).HasColumnName("id").IsRequired();
        mb.Property(b => b.CurrencyId).HasColumnName("currency_id").IsRequired();
        mb.Property(b => b.UserId).HasColumnName("user_id").IsRequired();
        mb.Property(b => b.Amount).HasColumnName("amount");
        mb.Ignore(b => b.User);
        mb.Ignore(b => b.Currency);

        var mc = modelBuilder.Entity<Currency>().ToTable("currencies");
        mc.Property(c => c.Id).HasColumnName("id").IsRequired();
        mc.Property(c => c.Type).HasColumnName("type").IsRequired();
        mc.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();
        
        var mt = modelBuilder.Entity<Transaction>().ToTable("transactions");
        mt.HasOne<User>().WithMany(u => u.Transactions).HasForeignKey(b => b.UserId).HasPrincipalKey(u => u.Id);
        mt.HasOne<Currency>().WithMany(c => c.Transactions).HasForeignKey(b => b.CurrencyId).HasPrincipalKey(c => c.Id);
        mt.Property(t => t.Id).HasColumnName("id").IsRequired();
        mt.Property(t => t.CurrencyId).HasColumnName("currency_id").IsRequired();
        mt.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        mt.Property(t => t.Status).HasColumnName("status").IsRequired();
        mt.Ignore(t => t.User);
        mt.Ignore(t => t.Currency);

        var mu = modelBuilder.Entity<User>().ToTable("users");
        mu.Property(u => u.Id).HasColumnName("id").IsRequired();
        mu.Property(u => u.UserName).HasColumnName("username").IsRequired();
        mu.Property(u => u.Password).HasColumnName("password").IsRequired();
        mu.Property(u => u.Role).HasColumnName("role").IsRequired();
        mu.Property(u => u.IsActive).HasColumnName("is_active").IsRequired();

        var mm = modelBuilder.Entity<Migration>().ToTable("migrations");
        mm.Property(c => c.Id).HasColumnName("id").IsRequired();
        mm.Property(c => c.SourceName).HasColumnName("source_name").IsRequired();
        mm.Property(c => c.SourceType).HasColumnName("source_type").IsRequired();
        mm.Property(c => c.SourcePath).HasColumnName("source_path").IsRequired();
        mm.Property(c => c.Status).HasColumnName("status").IsRequired();
    }
}