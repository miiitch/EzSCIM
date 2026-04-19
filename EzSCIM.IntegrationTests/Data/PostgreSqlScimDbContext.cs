using EzSCIM.Demo.Data;
using EzSCIM.Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.IntegrationTests.Data;

/// <summary>
/// PostgreSQL DbContext for integration tests.
/// Inherits provider-agnostic configuration from <see cref="ScimDbContextBase"/>
/// and adds PostgreSQL-specific column types (jsonb for JSON columns).
/// </summary>
public class PostgreSqlScimDbContext : ScimDbContextBase
{
    public PostgreSqlScimDbContext(DbContextOptions<PostgreSqlScimDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PostgreSQL-specific: JSON columns stored as jsonb
        modelBuilder.Entity<DemoUserEntity>(entity =>
        {
            entity.Property(e => e.EmailsJson).HasColumnType("jsonb");
            entity.Property(e => e.PhoneNumbersJson).HasColumnType("jsonb");
            entity.Property(e => e.AddressesJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<DemoGroupEntity>(entity =>
        {
            entity.Property(e => e.MembersJson).HasColumnType("jsonb");
        });
    }
}

