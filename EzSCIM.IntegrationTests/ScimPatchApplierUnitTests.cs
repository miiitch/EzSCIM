using EzSCIM.Demo.Data.Entities;
using EzSCIM.Demo.Data;
using EzSCIM.Models;
using EzSCIM.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit.Abstractions;

namespace EzSCIM.IntegrationTests;

public class ScimPatchApplierUnitTests
{
    private readonly ITestOutputHelper _output;

    public ScimPatchApplierUnitTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ApplyPatch_FilteredEmailPath_ShouldUpdateEmail()
    {
        // Arrange
        var user = new DemoUserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            EmailsJson = "[{\"value\":\"original@example.com\",\"type\":\"work\",\"primary\":true}]",
            Active = true
        };

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation
                {
                    Op = "replace",
                    Path = "emails[primary eq true].value",
                    Value = "updated@example.com"
                }
            }
        };

        _output.WriteLine($"Original EmailsJson: {user.EmailsJson}");

        // Act — round-trip via ScimUser (same as production code)
        var scimUser = user.ToScimUser();
        var result = ScimPatchService.ApplyPatch(scimUser, patchRequest);
        user.UpdateFromScimUser(scimUser);

        _output.WriteLine($"ApplyPatch returned: {result}");
        _output.WriteLine($"Updated EmailsJson: {user.EmailsJson}");

        // Assert
        Assert.True(result, "ApplyPatch should return true");
        Assert.Contains("updated@example.com", user.EmailsJson!);
    }

    [Fact]
    public void ApplyPatch_FilteredPhonePath_ShouldUpdatePhone()
    {
        // Arrange
        var user = new DemoUserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            PhoneNumbersJson = "[{\"value\":\"111-222-3333\",\"type\":\"work\",\"primary\":true}]",
            Active = true
        };

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation
                {
                    Op = "replace",
                    Path = "phoneNumbers[primary eq true].value",
                    Value = "999-888-7777"
                }
            }
        };

        _output.WriteLine($"Original PhoneNumbersJson: {user.PhoneNumbersJson}");

        // Act
        var scimUser = user.ToScimUser();
        var result = ScimPatchService.ApplyPatch(scimUser, patchRequest);
        user.UpdateFromScimUser(scimUser);

        _output.WriteLine($"ApplyPatch returned: {result}");
        _output.WriteLine($"Updated PhoneNumbersJson: {user.PhoneNumbersJson}");

        // Assert
        Assert.True(result, "ApplyPatch should return true");
        Assert.Contains("999-888-7777", user.PhoneNumbersJson!);
    }

    [Fact]
    public void ApplyPatch_DirectArrayPath_ShouldUpdateEmail()
    {
        // Arrange
        var user = new DemoUserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test2@example.com",
            EmailsJson = "[{\"value\":\"original2@example.com\",\"type\":\"work\",\"primary\":true}]",
            Active = true
        };

        var patchRequest = new ScimPatchRequest
        {
            Operations = new List<ScimPatchOperation>
            {
                new ScimPatchOperation
                {
                    Op = "replace",
                    Path = "emails",
                    Value = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
                        "[{\"value\":\"updated2@example.com\",\"type\":\"work\",\"primary\":true}]")
                }
            }
        };

        _output.WriteLine($"Original EmailsJson: {user.EmailsJson}");

        // Act
        var scimUser = user.ToScimUser();
        var result = ScimPatchService.ApplyPatch(scimUser, patchRequest);
        user.UpdateFromScimUser(scimUser);

        _output.WriteLine($"ApplyPatch returned: {result}");
        _output.WriteLine($"Updated EmailsJson: {user.EmailsJson}");

        // Assert
        Assert.True(result, "ApplyPatch should return true");
        Assert.Contains("updated2@example.com", user.EmailsJson!);
    }

    /// <summary>
    /// Regression test for SCIM Validator "PATCH /Users/Id" - "Patch User - Replace Attributes".
    /// Reproduces the exact scenario from docs/scim-test-results/PATCH _Users_Id_details.json
    ///
    /// Validator errors:
    ///   - "The value of emails[primary eq true].value is Missing from the fetched Resource"
    ///   - "The value of phoneNumbers[primary eq true].value is Missing from the fetched Resource"
    ///   - "The value of addresses[primary eq true].formatted is Missing from the fetched Resource"
    ///   - "The value of addresses[primary eq true].streetAddress is Missing from the fetched Resource"
    ///   - "The value of addresses[primary eq true].locality is Missing from the fetched Resource"
    ///   - "The value of addresses[primary eq true].region is Missing from the fetched Resource"
    ///   - "The value of addresses[primary eq true].postalCode is Missing from the fetched Resource"
    ///   - "The value of addresses[primary eq true].country is Missing from the fetched Resource"
    ///
    /// Root cause: When a PATCH request combines filtered path operations (ops 1-8)
    /// with a bulk replace without path (op 9), the multi-valued attributes modified by
    /// the filtered operations are lost after the round-trip through entity conversion.
    /// </summary>
    [Fact]
    public void PatchUser_ReplaceFilteredPathsCombinedWithBulkReplace_ShouldPersistAll()
    {
        // Arrange: Create user matching the validator's initial POST
        var user = new DemoUserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "delia.koch@hilpert.com",
            DisplayName = "OPFPBMYJFCDV",
            NickName = "IKASXZIGCAWN",
            ProfileUrl = "KQHXUEEQTVUI",
            Title = "HEVZHWNADGAZ",
            UserType = "VCBQOXGSYMXW",
            PreferredLanguage = "xh-ZA",
            Locale = "HNMJLVNCECCW",
            Timezone = "America/Belize",
            Active = true,
            ExternalId = "e76e1000-dfcd-447b-b3ab-9a9eb4022a47",
            NameFormatted = "Johnny",
            FirstName = "Tiara",
            LastName = "Cruz",
            NameMiddleName = "Moses",
            NameHonorificPrefix = "Yolanda",
            NameHonorificSuffix = "Marjolaine",
            EmailsJson = "[{\"value\":\"nedra@satterfield.name\",\"primary\":true}]",
            PhoneNumbersJson = "[{\"value\":\"20-636-2717\",\"primary\":true}]",
            AddressesJson = "[{\"formatted\":\"STKFAJKPAWRR\",\"streetAddress\":\"04540 Nicolas Greens\",\"locality\":\"DVYYCJIFSMYE\",\"region\":\"GUQXRJAHCLRD\",\"postalCode\":\"mz6 7em\",\"country\":\"Malaysia\",\"primary\":true}]"
        };

        // Build exact PATCH request from the validator (9 operations)
        var patchJson = """
        {
            "schemas":["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
            "Operations":[
                {"op":"replace","path":"emails[primary eq true].value","value":"marques@hintz.name"},
                {"op":"replace","path":"phoneNumbers[primary eq true].value","value":"58-036-1653"},
                {"op":"replace","path":"addresses[primary eq true].formatted","value":"ULYSMNPMXOFJ"},
                {"op":"replace","path":"addresses[primary eq true].streetAddress","value":"20838 Madisyn Station"},
                {"op":"replace","path":"addresses[primary eq true].locality","value":"JEARZXJUDQBX"},
                {"op":"replace","path":"addresses[primary eq true].region","value":"KMBYOLKJDNRI"},
                {"op":"replace","path":"addresses[primary eq true].postalCode","value":"xb30 9dn"},
                {"op":"replace","path":"addresses[primary eq true].country","value":"Switzerland"},
                {"op":"replace","value":{
                    "externalId":"3dde0ec3-cb34-47f7-8bd5-3c0e2fa2b54a",
                    "name.formatted":"Jailyn",
                    "name.familyName":"Lenora",
                    "name.givenName":"Eileen",
                    "name.middleName":"Jaydon",
                    "name.honorificPrefix":"Leone",
                    "name.honorificSuffix":"Noelia",
                    "displayName":"VMGRDOMSTBZW",
                    "nickName":"FQEAFPPACLAR",
                    "profileUrl":"QDGAPAWRMXRJ",
                    "title":"UBIESQLIXFJL",
                    "userType":"YLAXLLFSRLJH",
                    "preferredLanguage":"et",
                    "locale":"MRDSTVHXEUQC",
                    "timezone":"America/Goose_Bay",
                    "active":true
                }}
            ]
        }
        """;

        var patchRequest = JsonSerializer.Deserialize<ScimPatchRequest>(patchJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        _output.WriteLine($"Original EmailsJson: {user.EmailsJson}");
        _output.WriteLine($"Original PhoneNumbersJson: {user.PhoneNumbersJson}");
        _output.WriteLine($"Original AddressesJson: {user.AddressesJson}");

        // Act: Same round-trip as production (ToScimUser → ApplyPatch → UpdateFromScimUser)
        var scimUser = user.ToScimUser();
        var result = ScimPatchService.ApplyPatch(scimUser, patchRequest);
        user.UpdateFromScimUser(scimUser);

        _output.WriteLine($"\nAfter PATCH:");
        _output.WriteLine($"  EmailsJson: {user.EmailsJson}");
        _output.WriteLine($"  PhoneNumbersJson: {user.PhoneNumbersJson}");
        _output.WriteLine($"  AddressesJson: {user.AddressesJson}");
        _output.WriteLine($"  DisplayName: {user.DisplayName}");
        _output.WriteLine($"  ExternalId: {user.ExternalId}");

        // Assert: All operations must be persisted
        Assert.True(result, "ApplyPatch should return true");

        // Simulate the GET: convert entity back to ScimUser (like the controller does)
        var fetchedUser = user.ToScimUser();

        // Verify filtered path operations (ops 1-8) are persisted
        _output.WriteLine("\nVerifying fetched resource:");

        // emails[primary eq true].value
        var primaryEmail = fetchedUser.Emails?.FirstOrDefault(e => e.Primary);
        Assert.NotNull(primaryEmail);
        Assert.Equal("marques@hintz.name", primaryEmail.Value);
        _output.WriteLine($"  ✓ emails[primary eq true].value = {primaryEmail.Value}");

        // phoneNumbers[primary eq true].value
        var primaryPhone = fetchedUser.PhoneNumbers?.FirstOrDefault(p => p.Primary);
        Assert.NotNull(primaryPhone);
        Assert.Equal("58-036-1653", primaryPhone.Value);
        _output.WriteLine($"  ✓ phoneNumbers[primary eq true].value = {primaryPhone.Value}");

        // addresses[primary eq true].*
        var primaryAddr = fetchedUser.Addresses?.FirstOrDefault(a => a.Primary);
        Assert.NotNull(primaryAddr);
        Assert.Equal("ULYSMNPMXOFJ", primaryAddr.Formatted);
        Assert.Equal("20838 Madisyn Station", primaryAddr.StreetAddress);
        Assert.Equal("JEARZXJUDQBX", primaryAddr.Locality);
        Assert.Equal("KMBYOLKJDNRI", primaryAddr.Region);
        Assert.Equal("xb30 9dn", primaryAddr.PostalCode);
        Assert.Equal("Switzerland", primaryAddr.Country);
        _output.WriteLine($"  ✓ addresses[primary eq true] = all fields verified");

        // Verify bulk replace scalar attributes (op 9)
        Assert.Equal("3dde0ec3-cb34-47f7-8bd5-3c0e2fa2b54a", fetchedUser.ExternalId);
        Assert.Equal("VMGRDOMSTBZW", fetchedUser.DisplayName);
        Assert.Equal("Jailyn", fetchedUser.Name?.Formatted);
        Assert.Equal("Lenora", fetchedUser.Name?.FamilyName);
        Assert.Equal("Eileen", fetchedUser.Name?.GivenName);
        _output.WriteLine($"  ✓ scalar attributes from bulk replace verified");
    }

    /// <summary>
    /// Tests the full EF Core round-trip: load entity, apply PATCH, save, reload, verify.
    /// Uses SQLite in-memory to simulate the production EF persistence flow.
    /// This reproduces the exact scenario from the SCIM validator failure.
    /// </summary>
    [Fact]
    public void PatchUser_ReplaceFilteredPathsCombinedWithBulkReplace_EfRoundTrip_ShouldPersist()
    {
        // Arrange: Use SQLite in-memory with a shared connection to simulate EF persistence
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ScimDbContextBase>()
            .UseSqlite(connection)
            .Options;

        using var context = new ScimDbContextBase(options);
        context.Database.EnsureCreated();

        var userId = Guid.NewGuid().ToString();
        var user = new DemoUserEntity
        {
            Id = userId,
            UserName = "delia.koch@hilpert.com",
            DisplayName = "OPFPBMYJFCDV",
            Active = true,
            ExternalId = "e76e1000-dfcd-447b-b3ab-9a9eb4022a47",
            NameFormatted = "Johnny",
            FirstName = "Tiara",
            LastName = "Cruz",
            EmailsJson = "[{\"value\":\"nedra@satterfield.name\",\"primary\":true}]",
            PhoneNumbersJson = "[{\"value\":\"20-636-2717\",\"primary\":true}]",
            AddressesJson = "[{\"formatted\":\"STKFAJKPAWRR\",\"streetAddress\":\"04540 Nicolas Greens\",\"locality\":\"DVYYCJIFSMYE\",\"region\":\"GUQXRJAHCLRD\",\"postalCode\":\"mz6 7em\",\"country\":\"Malaysia\",\"primary\":true}]"
        };

        context.Users.Add(user);
        context.SaveChanges();

        // Detach to simulate a fresh request
        context.ChangeTracker.Clear();

        // Act: Simulate PatchUserAsync flow
        var existing = context.Users.Find(userId)!;
        var scimUser = existing.ToScimUser();

        var patchJson = """
        {
            "schemas":["urn:ietf:params:scim:api:messages:2.0:PatchOp"],
            "Operations":[
                {"op":"replace","path":"emails[primary eq true].value","value":"marques@hintz.name"},
                {"op":"replace","path":"phoneNumbers[primary eq true].value","value":"58-036-1653"},
                {"op":"replace","path":"addresses[primary eq true].formatted","value":"ULYSMNPMXOFJ"},
                {"op":"replace","path":"addresses[primary eq true].streetAddress","value":"20838 Madisyn Station"},
                {"op":"replace","path":"addresses[primary eq true].locality","value":"JEARZXJUDQBX"},
                {"op":"replace","path":"addresses[primary eq true].region","value":"KMBYOLKJDNRI"},
                {"op":"replace","path":"addresses[primary eq true].postalCode","value":"xb30 9dn"},
                {"op":"replace","path":"addresses[primary eq true].country","value":"Switzerland"},
                {"op":"replace","value":{
                    "externalId":"3dde0ec3-cb34-47f7-8bd5-3c0e2fa2b54a",
                    "name.formatted":"Jailyn",
                    "name.familyName":"Lenora",
                    "name.givenName":"Eileen",
                    "displayName":"VMGRDOMSTBZW",
                    "active":true
                }}
            ]
        }
        """;

        var patchRequest = JsonSerializer.Deserialize<ScimPatchRequest>(patchJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        ScimPatchService.ApplyPatch(scimUser, patchRequest);
        existing.UpdateFromScimUser(scimUser);

        _output.WriteLine($"After UpdateFromScimUser: EmailsJson = {existing.EmailsJson}");

        // Simulate EfScimRepositoryBase.UpdateUserAsync: FindAsync + SetValues
        var existingAgain = context.Users.Find(userId)!; // Same tracked instance
        _output.WriteLine($"Same instance? {ReferenceEquals(existing, existingAgain)}");
        context.Entry(existingAgain).CurrentValues.SetValues(existing);
        context.SaveChanges();

        // Clear tracker and reload to simulate GET
        context.ChangeTracker.Clear();
        var reloaded = context.Users.Find(userId)!;

        _output.WriteLine($"Reloaded EmailsJson: {reloaded.EmailsJson}");
        _output.WriteLine($"Reloaded PhoneNumbersJson: {reloaded.PhoneNumbersJson}");
        _output.WriteLine($"Reloaded AddressesJson: {reloaded.AddressesJson}");

        var fetchedUser = reloaded.ToScimUser();

        // Assert: All values must be persisted
        var primaryEmail = fetchedUser.Emails?.FirstOrDefault(e => e.Primary);
        Assert.NotNull(primaryEmail);
        Assert.Equal("marques@hintz.name", primaryEmail.Value);

        var primaryPhone = fetchedUser.PhoneNumbers?.FirstOrDefault(p => p.Primary);
        Assert.NotNull(primaryPhone);
        Assert.Equal("58-036-1653", primaryPhone.Value);

        var primaryAddr = fetchedUser.Addresses?.FirstOrDefault(a => a.Primary);
        Assert.NotNull(primaryAddr);
        Assert.Equal("ULYSMNPMXOFJ", primaryAddr.Formatted);
        Assert.Equal("20838 Madisyn Station", primaryAddr.StreetAddress);
        Assert.Equal("JEARZXJUDQBX", primaryAddr.Locality);
        Assert.Equal("KMBYOLKJDNRI", primaryAddr.Region);
        Assert.Equal("xb30 9dn", primaryAddr.PostalCode);
        Assert.Equal("Switzerland", primaryAddr.Country);

        Assert.Equal("3dde0ec3-cb34-47f7-8bd5-3c0e2fa2b54a", fetchedUser.ExternalId);
        Assert.Equal("VMGRDOMSTBZW", fetchedUser.DisplayName);
    }
}
