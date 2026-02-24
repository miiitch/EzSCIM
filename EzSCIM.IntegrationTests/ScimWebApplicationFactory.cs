using EzSCIM.IntegrationTests.Data;
using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.IntegrationTests.Data.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using EzSCIM.DataRepositories;
using EzSCIM.Filtering;
using EzSCIM.Models;
using EzSCIM.Repositories;
using Testcontainers.PostgreSql;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Web Application Factory for integration tests with Testcontainers PostgreSQL.
/// Creates a PostgreSQL container, seeds data, and configures EF Core repositories.
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
        // Create and start PostgreSQL container
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("scimdb")
            .WithUsername("scimuser")
            .WithPassword("scimpass")
            .Build();

        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PostgreSQL container started");
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Connection string: {_connectionString}");

        // Create database schema and seed data
        await CreateDatabaseAndSeedAsync();
    }

    /// <summary>
    /// Creates the database schema and loads seed data.
    /// </summary>
    private async Task CreateDatabaseAndSeedAsync()
    {
        // Create a separate DbContext instance for seeding (not using DI)
        var optionsBuilder = new DbContextOptionsBuilder<ScimDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);

        using var context = new ScimDbContext(optionsBuilder.Options);

        // Create database schema
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Database schema created");

        // Seed users
        var users = SeedData.GetSeedUsers();
        await context.Users.AddRangeAsync(users);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seeded {users.Count} users");

        // Seed groups
        var groups = SeedData.GetSeedGroups();
        await context.Groups.AddRangeAsync(groups);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seeded {groups.Count} groups");

        await context.SaveChangesAsync();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Seed data saved successfully");
    }

    /// <summary>
    /// Configures the web host to use EF Core repositories and disable authentication.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing IScimRepository registration
            services.RemoveAll<IScimRepository>();

            // Add DbContext with PostgreSQL
            services.AddDbContext<ScimDbContext>(options =>
                options.UseNpgsql(_connectionString));

            // Register EF Core data repository (combined user+group)
            services.AddScoped<IUserGroupDataRepository<UserEntity, GroupEntity>, EfUserGroupDataRepository>();

            // Also register IUserDataRepository pointing to the same instance
            // (for JsonUserRepositoryAdapter which only needs user data)
            services.AddScoped<IUserDataRepository<UserEntity>>(sp =>
                sp.GetRequiredService<IUserGroupDataRepository<UserEntity, GroupEntity>>());

            // Register filter translators
            services.AddScoped<IScimFilterTranslator<UserEntity>, GenericScimFilterTranslator<UserEntity>>();
            services.AddScoped<IScimFilterTranslator<GroupEntity>, GenericScimFilterTranslator<GroupEntity>>();

            // Register SCIM repository adapters
            services.AddScoped<IScimUserOnlyRepository<ScimUser>>(sp =>
            {
                var dataRepo = sp.GetRequiredService<IUserDataRepository<UserEntity>>();
                var translator = sp.GetRequiredService<IScimFilterTranslator<UserEntity>>();
                // Use JsonUserRepositoryAdapter that handles multi-valued JSON attributes
                return new Data.Repositories.JsonUserRepositoryAdapter(dataRepo, translator);
            });

            services.AddScoped<IScimUserGroupRepository<ScimUser, ScimGroup>>(sp =>
            {
                var dataRepo = sp.GetRequiredService<IUserGroupDataRepository<UserEntity, GroupEntity>>();
                var userTranslator = sp.GetRequiredService<IScimFilterTranslator<UserEntity>>();
                var groupTranslator = sp.GetRequiredService<IScimFilterTranslator<GroupEntity>>();
                return new ScimUserGroupRepositoryAdapter<UserEntity, GroupEntity>(dataRepo, userTranslator, groupTranslator);
            });

            // Register composite IScimRepository
            services.AddScoped<IScimRepository>(sp =>
            {
                var userRepo = sp.GetRequiredService<IScimUserOnlyRepository<ScimUser>>();
                var groupRepo = sp.GetRequiredService<IScimUserGroupRepository<ScimUser, ScimGroup>>();
                var dbContext = sp.GetRequiredService<ScimDbContext>();
                
                // Create a composite repository that delegates to both and implements PATCH
                return new CompositeScimRepository(userRepo, groupRepo, dbContext);
            });

            // Disable authentication for tests
            services.PostConfigure<AuthorizationOptions>(options =>
            {
                options.FallbackPolicy = null;
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            });

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Services configured for integration tests");
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

    /// <summary>
    /// Composite repository that delegates to user and group repositories.
    /// Implements PATCH operations directly using the DbContext.
    /// </summary>
    private class CompositeScimRepository : IScimRepository
    {
        private readonly IScimUserOnlyRepository<ScimUser> _userRepo;
        private readonly IScimUserGroupRepository<ScimUser, ScimGroup> _groupRepo;
        private readonly ScimDbContext _dbContext;

        public CompositeScimRepository(
            IScimUserOnlyRepository<ScimUser> userRepo,
            IScimUserGroupRepository<ScimUser, ScimGroup> groupRepo,
            ScimDbContext dbContext)
        {
            _userRepo = userRepo;
            _groupRepo = groupRepo;
            _dbContext = dbContext;
        }

        // User operations
        public Task<ScimUser?> GetUserAsync(string id) => _userRepo.GetUserAsync(id);
        public Task<ScimUser?> GetUserByUserNameAsync(string userName) => _userRepo.GetUserByUserNameAsync(userName);
        public Task<ScimListResponse<ScimUser>> GetUsersAsync(EzSCIM.Filtering.AST.FilterExpression? filter = null, int startIndex = 1, int count = 100)
            => _userRepo.GetUsersAsync(filter, startIndex, count);
        public Task<ScimUser> CreateUserAsync(ScimUser user) => _userRepo.CreateUserAsync(user);
        public Task<ScimUser?> UpdateUserAsync(string id, ScimUser user) => _userRepo.UpdateUserAsync(id, user);
        
        /// <summary>
        /// Implements PATCH for users using UserEntityPatchApplier for JSON multi-valued attributes.
        /// EF Core automatically detects changes on tracked entities.
        /// </summary>
        public async Task<ScimUser?> PatchUserAsync(string id, ScimPatchRequest patchRequest)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return null;

            // Use specialized UserEntityPatchApplier for JSON multi-valued attributes
            var modified = Data.UserEntityPatchApplier.ApplyPatch(user, patchRequest.Operations);
            
            if (!modified)
            {
                Console.WriteLine($"[PatchUserAsync] WARNING: No properties were modified for user {id}");
            }
            else
            {
                Console.WriteLine($"[PatchUserAsync] Successfully modified user {id}");
            }

            user.ModifiedAt = DateTime.UtcNow;
            
            // EF Core change tracking automatically detects modifications to properties
            // (including JSON columns) since the entity was obtained via FindAsync
            await _dbContext.SaveChangesAsync();

            // Return updated user as ScimUser using extension method
            return user.ToScimUser();
        }

        public Task<bool> DeleteUserAsync(string id) => _userRepo.DeleteUserAsync(id);

        // Group operations
        public Task<ScimGroup?> GetGroupAsync(string id) => _groupRepo.GetGroupAsync(id);
        public Task<ScimGroup?> GetGroupByDisplayNameAsync(string displayName) => _groupRepo.GetGroupByDisplayNameAsync(displayName);
        public Task<ScimListResponse<ScimGroup>> GetGroupsAsync(EzSCIM.Filtering.AST.FilterExpression? filter = null, int startIndex = 1, int count = 100)
            => _groupRepo.GetGroupsAsync(filter, startIndex, count);
        public Task<ScimGroup> CreateGroupAsync(ScimGroup group) => _groupRepo.CreateGroupAsync(group);
        public Task<ScimGroup?> UpdateGroupAsync(string id, ScimGroup group) => _groupRepo.UpdateGroupAsync(id, group);
        
        /// <summary>
        /// Implements PATCH for groups using ScimPatchApplier with ScimProperty attributes.
        /// Members array is handled separately.
        /// </summary>
        public async Task<ScimGroup?> PatchGroupAsync(string id, ScimPatchRequest patchRequest)
        {
            var group = await _dbContext.Groups.FindAsync(id);
            if (group == null)
                return null;

            // Separate members operations from scalar operations
            // Handle both "members" and "members[filter]" paths
            var membersOps = patchRequest.Operations
                .Where(op => op.Path != null && 
                    (string.Equals(op.Path, "members", StringComparison.OrdinalIgnoreCase) ||
                     op.Path.StartsWith("members[", StringComparison.OrdinalIgnoreCase)))
                .ToList();
            var scalarOps = patchRequest.Operations
                .Where(op => op.Path == null || 
                    (!string.Equals(op.Path, "members", StringComparison.OrdinalIgnoreCase) &&
                     !op.Path.StartsWith("members[", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Apply scalar operations using generic ScimPatchApplier
            ScimPatchApplier.ApplyPatch(group, scalarOps);

            // Handle members operations separately (array type)
            foreach (var op in membersOps)
            {
                ApplyMembersPatchOperation(group, op);
            }

            group.ModifiedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Return updated group as ScimGroup
            return await GetGroupAsync(id);
        }

        private void ApplyMembersPatchOperation(Data.Entities.GroupEntity group, ScimPatchOperation op)
        {
            var operation = op.Op?.ToLowerInvariant() ?? "replace";
            var currentMembers = ParseMembersJson(group.MembersJson);
            
            // Check if path contains a filter (e.g., members[value eq "userId"])
            string? filterValue = null;
            if (op.Path != null && op.Path.Contains("[") && op.Path.Contains("]"))
            {
                // Extract filter value from path like: members[value eq "userId"]
                var match = System.Text.RegularExpressions.Regex.Match(
                    op.Path, 
                    @"members\[value\s+eq\s+""([^""]+)""\]",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    filterValue = match.Groups[1].Value;
                }
            }
            
            if (operation == "add" && op.Value != null)
            {
                var newMembers = ParseMembersFromValue(op.Value);
                foreach (var member in newMembers)
                {
                    if (!currentMembers.Any(m => m.Value == member.Value))
                        currentMembers.Add(member);
                }
                group.MembersJson = System.Text.Json.JsonSerializer.Serialize(currentMembers);
            }
            else if (operation == "remove")
            {
                if (filterValue != null)
                {
                    // Remove specific member matching the filter
                    currentMembers.RemoveAll(m => m.Value == filterValue);
                }
                else if (op.Value != null)
                {
                    var membersToRemove = ParseMembersFromValue(op.Value);
                    currentMembers.RemoveAll(m => membersToRemove.Any(r => r.Value == m.Value));
                }
                else
                {
                    currentMembers.Clear();
                }
                group.MembersJson = System.Text.Json.JsonSerializer.Serialize(currentMembers);
            }
            else if (operation == "replace" && op.Value != null)
            {
                // Replace entire members list
                var newMembers = ParseMembersFromValue(op.Value);
                group.MembersJson = System.Text.Json.JsonSerializer.Serialize(newMembers);
            }
        }

        private List<MemberInfo> ParseMembersJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<MemberInfo>();
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<MemberInfo>>(json) ?? new List<MemberInfo>();
            }
            catch
            {
                return new List<MemberInfo>();
            }
        }

        private List<MemberInfo> ParseMembersFromValue(object value)
        {
            var result = new List<MemberInfo>();
            
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("value", out var valueProperty))
                        {
                            var display = item.TryGetProperty("display", out var displayProperty) 
                                ? displayProperty.GetString() ?? "" 
                                : "";
                            result.Add(new MemberInfo { Value = valueProperty.GetString() ?? "", Display = display });
                        }
                    }
                }
            }
            else if (value is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is Dictionary<string, string> dict && dict.TryGetValue("value", out var val))
                    {
                        dict.TryGetValue("display", out var display);
                        result.Add(new MemberInfo { Value = val, Display = display ?? "" });
                    }
                }
            }

            return result;
        }

        private class MemberInfo
        {
            public string Value { get; set; } = "";
            public string Display { get; set; } = "";
        }

        public Task<bool> DeleteGroupAsync(string id) => _groupRepo.DeleteGroupAsync(id);

        // Schema operations (not implemented for tests)
        public Task<List<ScimSchema>> GetCustomSchemasAsync() => Task.FromResult(new List<ScimSchema>());
        public Task<ScimSchema> AddOrUpdateCustomSchemaAsync(ScimSchema schema) => Task.FromResult(schema);
    }
}

