using EzSCIM.IntegrationTests.Data.Entities;

namespace EzSCIM.IntegrationTests.Data;

/// <summary>
/// Provides seed data for integration tests.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Gets 5 predefined test users.
    /// </summary>
    public static List<UserEntity> GetSeedUsers()
    {
        return new List<UserEntity>
        {
            new UserEntity
            {
                Id = "user-001",
                UserName = "john.doe@example.com",
                DisplayName = "John Doe",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Active = true,
                Title = "Software Developer",
                ExternalId = "ext-john-001",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                ModifiedAt = DateTime.UtcNow.AddDays(-5)
            },
            new UserEntity
            {
                Id = "user-002",
                UserName = "jane.smith@example.com",
                DisplayName = "Jane Smith",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Active = true,
                Title = "System Administrator",
                ExternalId = "ext-jane-002",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                ModifiedAt = DateTime.UtcNow.AddDays(-2)
            },
            new UserEntity
            {
                Id = "user-003",
                UserName = "bob.wilson@example.com",
                DisplayName = "Bob Wilson",
                FirstName = "Bob",
                LastName = "Wilson",
                Email = "bob.wilson@example.com",
                Active = false,
                Title = "Former Employee",
                ExternalId = "ext-bob-003",
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                ModifiedAt = DateTime.UtcNow.AddDays(-10)
            },
            new UserEntity
            {
                Id = "user-004",
                UserName = "alice.johnson@example.com",
                DisplayName = "Alice Johnson",
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                Active = true,
                Title = "Project Manager",
                ExternalId = "ext-alice-004",
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                ModifiedAt = DateTime.UtcNow.AddDays(-1)
            },
            new UserEntity
            {
                Id = "user-005",
                UserName = "charlie.brown@example.com",
                DisplayName = "Charlie Brown",
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie.brown@example.com",
                Active = true,
                Title = "Senior Developer",
                ExternalId = "ext-charlie-005",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                ModifiedAt = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Gets 3 predefined test groups.
    /// </summary>
    public static List<GroupEntity> GetSeedGroups()
    {
        return new List<GroupEntity>
        {
            new GroupEntity
            {
                Id = "group-001",
                DisplayName = "Administrators",
                ExternalId = "ext-admin-group",
                MembersJson = "[{\"value\":\"user-002\",\"display\":\"Jane Smith\"},{\"value\":\"user-004\",\"display\":\"Alice Johnson\"}]",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                ModifiedAt = DateTime.UtcNow.AddDays(-20)
            },
            new GroupEntity
            {
                Id = "group-002",
                DisplayName = "Developers",
                ExternalId = "ext-dev-group",
                MembersJson = "[{\"value\":\"user-001\",\"display\":\"John Doe\"},{\"value\":\"user-005\",\"display\":\"Charlie Brown\"}]",
                CreatedAt = DateTime.UtcNow.AddDays(-80),
                ModifiedAt = DateTime.UtcNow.AddDays(-10)
            },
            new GroupEntity
            {
                Id = "group-003",
                DisplayName = "Users",
                ExternalId = "ext-users-group",
                MembersJson = "[{\"value\":\"user-001\",\"display\":\"John Doe\"},{\"value\":\"user-002\",\"display\":\"Jane Smith\"},{\"value\":\"user-004\",\"display\":\"Alice Johnson\"},{\"value\":\"user-005\",\"display\":\"Charlie Brown\"}]",
                CreatedAt = DateTime.UtcNow.AddDays(-120),
                ModifiedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
    }
}

