using EzSCIM.Demo.Data;
using EzSCIM.Demo.Data.Entities;
using EzSCIM.Demo.Data.Repositories;
using EzSCIM.IntegrationTests.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Repositories;
using Testcontainers.PostgreSql;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Web Application Factory for integration tests with Testcontainers PostgreSQL.
/// Creates a PostgreSQL container, seeds data, and configures the shared Demo data layer
/// with <see cref="PostgreSqlScimDbContext"/> instead of the Demo's SQL Server context.
/// </summary>
public class ScimWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private string _connectionString = string.Empty;

    /// <summary>
    /// Initializes the PostgreSQL container and seeds data.
    /// </summary>
    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("scimdb")
            .WithUsername("scimuser")
            .WithPassword("scimpass")
            .Build();

        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PostgreSQL container started");

        // Wait for PostgreSQL to be fully ready
        using var testConn = new Npgsql.NpgsqlConnection(_connectionString);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                await testConn.OpenAsync();
                await testConn.CloseAsync();
                break;
            }
            catch
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Waiting for PostgreSQL (attempt {attempt + 1})...");
                await Task.Delay(500);
            }
        }

        await CreateDatabaseAndSeedAsync();
    }

    /// <summary>
    /// Creates the database schema and loads seed data using PostgreSqlScimDbContext.
    /// </summary>
    private async Task CreateDatabaseAndSeedAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlScimDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);

        using var context = new PostgreSqlScimDbContext(optionsBuilder.Options);

        await context.Database.EnsureCreatedAsync();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Database schema created");

        var users = SeedData.GetSeedUsers();
        await context.Users.AddRangeAsync(users);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seeded {users.Count} users");

        var groups = SeedData.GetSeedGroups();
        await context.Groups.AddRangeAsync(groups);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seeded {groups.Count} groups");

        await context.SaveChangesAsync();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seed data saved successfully");
    }

    /// <summary>
    /// Configures the web host: replaces SQL Server with PostgreSQL and disables authentication.
    /// The Demo's DemoScimRepository, DemoUserGroupRepository, and filter translators are reused as-is.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Remove ALL DemoScimDbContext registrations (Aspire SQL Server pools, options, health checks)
            var toRemove = services
                .Where(d =>
                    (d.ServiceType.FullName?.Contains("DemoScimDbContext") == true) ||
                    (d.ImplementationType?.FullName?.Contains("DemoScimDbContext") == true) ||
                    (d.ServiceType.FullName?.Contains("DbContextPool") == true &&
                     d.ServiceType.FullName?.Contains("DemoScimDbContext") == true))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            services.RemoveAll<DbContextOptions<EzSCIM.EntraID.Demo.Data.DemoScimDbContext>>();

            // Re-register DemoScimDbContext with PostgreSQL (required by Program.cs DI)
            services.AddDbContext<EzSCIM.EntraID.Demo.Data.DemoScimDbContext>(options =>
                options.UseNpgsql(_connectionString)
                    .ConfigureWarnings(w => w.Ignore(
                        Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

            // Register PostgreSqlScimDbContext for tests
            services.AddDbContext<PostgreSqlScimDbContext>(options =>
                options.UseNpgsql(_connectionString));

            // Register ScimDbContextBase → PostgreSqlScimDbContext (used by DemoUserGroupRepository)
            services.RemoveAll<ScimDbContextBase>();
            services.AddScoped<ScimDbContextBase>(sp => sp.GetRequiredService<PostgreSqlScimDbContext>());

            // Re-register data repository using the shared lib (if not already registered by Program.cs)
            services.RemoveAll<IUserGroupDataRepository<DemoUserEntity, DemoGroupEntity>>();
            services.AddScoped<IUserGroupDataRepository<DemoUserEntity, DemoGroupEntity>, DemoUserGroupRepository>();

            // Disable authentication for tests
            services.PostConfigure<AuthorizationOptions>(options =>
            {
                options.FallbackPolicy = null;
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            });

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Services configured for integration tests (PostgreSQL)");
        });
    }

    /// <summary>
    /// Disposes the PostgreSQL container.
    /// </summary>
    public new async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PostgreSQL container stopped and disposed");
        }
        await base.DisposeAsync();
    }
}
