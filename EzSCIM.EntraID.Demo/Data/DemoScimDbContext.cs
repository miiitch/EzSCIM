using EzSCIM.Demo.Data;
using EzSCIM.Demo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzSCIM.EntraID.Demo.Data;

/// <summary>
/// SQL Server / Azure SQL DbContext for the demo SCIM API.
/// Inherits provider-agnostic configuration from <see cref="ScimDbContextBase"/>
/// and adds SQL Server-specific column types (nvarchar(max) for JSON columns).
/// </summary>
public class DemoScimDbContext : ScimDbContextBase
{
    public DemoScimDbContext(DbContextOptions<DemoScimDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQL Server-specific: JSON columns stored as nvarchar(max)
        modelBuilder.Entity<DemoUserEntity>(entity =>
        {
            entity.Property(e => e.EmailsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.PhoneNumbersJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AddressesJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<DemoGroupEntity>(entity =>
        {
            entity.Property(e => e.MembersJson).HasColumnType("nvarchar(max)");
        });
    }
}
