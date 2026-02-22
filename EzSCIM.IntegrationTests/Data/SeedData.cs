using EzSCIM.IntegrationTests.Data.Entities;
using System.Text.Json;

namespace EzSCIM.IntegrationTests.Data;

/// <summary>
/// Provides seed data for integration tests.
/// </summary>
public static class SeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
                NameFormatted = "Mr. John Doe",
                FirstName = "John",
                LastName = "Doe",
                NameHonorificPrefix = "Mr.",
                EmailsJson = JsonSerializer.Serialize(new[]
                {
                    new EmailData { Value = "john.doe@example.com", Type = "work", Primary = true }
                }, JsonOptions),
                PhoneNumbersJson = JsonSerializer.Serialize(new[]
                {
                    new PhoneNumberData { Value = "555-0101", Type = "work", Primary = true }
                }, JsonOptions),
                AddressesJson = JsonSerializer.Serialize(new[]
                {
                    new AddressData
                    {
                        Formatted = "123 Main St, Springfield, IL 62701, USA",
                        StreetAddress = "123 Main St",
                        Locality = "Springfield",
                        Region = "IL",
                        PostalCode = "62701",
                        Country = "USA",
                        Type = "work",
                        Primary = true
                    }
                }, JsonOptions),
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
                EmailsJson = JsonSerializer.Serialize(new[]
                {
                    new EmailData { Value = "jane.smith@example.com", Type = "work", Primary = true }
                }, JsonOptions),
                PhoneNumbersJson = JsonSerializer.Serialize(new[]
                {
                    new PhoneNumberData { Value = "555-0102", Type = "work", Primary = true }
                }, JsonOptions),
                AddressesJson = JsonSerializer.Serialize(new[]
                {
                    new AddressData
                    {
                        Formatted = "456 Oak Ave, Portland, OR 97201, USA",
                        StreetAddress = "456 Oak Ave",
                        Locality = "Portland",
                        Region = "OR",
                        PostalCode = "97201",
                        Country = "USA",
                        Type = "work",
                        Primary = true
                    }
                }, JsonOptions),
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
                EmailsJson = JsonSerializer.Serialize(new[]
                {
                    new EmailData { Value = "bob.wilson@example.com", Type = "work", Primary = true }
                }, JsonOptions),
                PhoneNumbersJson = JsonSerializer.Serialize(new[]
                {
                    new PhoneNumberData { Value = "555-0103", Type = "work", Primary = true }
                }, JsonOptions),
                AddressesJson = JsonSerializer.Serialize(new[]
                {
                    new AddressData
                    {
                        Formatted = "789 Elm St, Austin, TX 78701, USA",
                        StreetAddress = "789 Elm St",
                        Locality = "Austin",
                        Region = "TX",
                        PostalCode = "78701",
                        Country = "USA",
                        Type = "work",
                        Primary = true
                    }
                }, JsonOptions),
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
                EmailsJson = JsonSerializer.Serialize(new[]
                {
                    new EmailData { Value = "alice.johnson@example.com", Type = "work", Primary = true }
                }, JsonOptions),
                PhoneNumbersJson = JsonSerializer.Serialize(new[]
                {
                    new PhoneNumberData { Value = "555-0104", Type = "work", Primary = true }
                }, JsonOptions),
                AddressesJson = JsonSerializer.Serialize(new[]
                {
                    new AddressData
                    {
                        Formatted = "321 Pine Rd, Seattle, WA 98101, USA",
                        StreetAddress = "321 Pine Rd",
                        Locality = "Seattle",
                        Region = "WA",
                        PostalCode = "98101",
                        Country = "USA",
                        Type = "work",
                        Primary = true
                    }
                }, JsonOptions),
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
                EmailsJson = JsonSerializer.Serialize(new[]
                {
                    new EmailData { Value = "charlie.brown@example.com", Type = "work", Primary = true }
                }, JsonOptions),
                PhoneNumbersJson = JsonSerializer.Serialize(new[]
                {
                    new PhoneNumberData { Value = "555-0105", Type = "work", Primary = true }
                }, JsonOptions),
                AddressesJson = JsonSerializer.Serialize(new[]
                {
                    new AddressData
                    {
                        Formatted = "654 Maple Dr, Boston, MA 02101, USA",
                        StreetAddress = "654 Maple Dr",
                        Locality = "Boston",
                        Region = "MA",
                        PostalCode = "02101",
                        Country = "USA",
                        Type = "work",
                        Primary = true
                    }
                }, JsonOptions),
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

