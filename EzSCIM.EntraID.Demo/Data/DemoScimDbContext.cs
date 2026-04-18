using EzSCIM.EntraID.Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.EntraID.Demo.Data;

/// <summary>
/// EF Core DbContext for the demo SCIM API backed by SQL Server (or Azure SQL).
/// </summary>
public class DemoScimDbContext : DbContext
{
    public DemoScimDbContext(DbContextOptions<DemoScimDbContext> options) : base(options) { }

    public DbSet<DemoUserEntity> Users { get; set; } = null!;
    public DbSet<DemoGroupEntity> Groups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DemoUserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Active).HasDefaultValue(true);
            // JSON columns: nvarchar(max) — no length limit needed
            entity.Property(e => e.EmailsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.PhoneNumbersJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AddressesJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<DemoGroupEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DisplayName).IsUnique();
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.MembersJson).HasColumnType("nvarchar(max)");
        });
    }
}

