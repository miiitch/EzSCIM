using EzSCIM.Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.Demo.Data;

/// <summary>
/// Provider-agnostic base DbContext for SCIM user and group entities.
/// Configures keys, indexes, required fields, and max lengths — but NOT column types.
/// Inherit and override <see cref="OnModelCreating"/> to add provider-specific column types
/// (e.g. nvarchar(max) for SQL Server, jsonb for PostgreSQL).
/// </summary>
public class ScimDbContextBase : DbContext
{
    /// <summary>
    /// Accepts non-generic <see cref="DbContextOptions"/> to support subclass constructors
    /// that pass <see cref="DbContextOptions{T}"/>.
    /// </summary>
    public ScimDbContextBase(DbContextOptions options) : base(options) { }

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
        });

        modelBuilder.Entity<DemoGroupEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DisplayName).IsUnique();
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
        });
    }
}

