using Microsoft.EntityFrameworkCore;
using ScimAPI.IntegrationTests.Data.Entities;

namespace ScimAPI.IntegrationTests.Data;

/// <summary>
/// DbContext for integration tests with PostgreSQL.
/// </summary>
public class ScimDbContext : DbContext
{
    public ScimDbContext(DbContextOptions<ScimDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<GroupEntity> Groups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserEntity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.UserName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
        });

        // Configure GroupEntity
        modelBuilder.Entity<GroupEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DisplayName).IsUnique();
            entity.Property(e => e.DisplayName).IsRequired();
        });
    }
}

