using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EzSCIM.IntegrationTests.Data;
using EzSCIM.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace EzSCIM.IntegrationTests;

/// <summary>
/// Integration tests that reproduce failures found by https://scimvalidator.microsoft.com/
/// Each test corresponds to a specific compliance failure observed across validation runs 01-04.
/// 
/// Methodology:
///   - For each anomaly reported by the SCIM Validator, a regression test is created FIRST
///   - The test must fail before the fix and pass after the fix
///   - Test names reference the validator test name and the specific error message
///
/// Source data: docs/scim-test-results/scim-results-01.json through scim-results-04.json
/// </summary>
[Collection("ScimValidatorCompliance")]
public class ScimValidatorComplianceTests : IClassFixture<ScimWebApplicationFactory>, IAsyncLifetime
{
    private readonly ScimWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly ScimDbContext _context;
    private readonly ITestOutputHelper _output;
    private IDbContextTransaction _transaction = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ScimValidatorComplianceTests(
        ScimWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ScimDbContext>();
    }

    public async Task InitializeAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        _output.WriteLine($"[{DateTime.Now:HH:mm:ss}] SCIM Validator compliance test started");
    }

    public async Task DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
        _scope?.Dispose();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test user with all standard attributes populated, returns the created user.
    /// </summary>
    private async Task<ScimUser> CreateTestUserAsync(string? userNameSuffix = null)
    {
        var userName = $"scimvalidator_{userNameSuffix ?? Guid.NewGuid().ToString("N")[..8]}@example.com";
        var user = new
        {
            schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
            userName,
            externalId = Guid.NewGuid().ToString(),
            name = new
            {
                formatted = "Original Formatted",
                familyName = "OriginalFamily",
                givenName = "OriginalGiven",
                middleName = "OriginalMiddle",
                honorificPrefix = "Mr",
                honorificSuffix = "Jr"
            },
            displayName = "ORIGINAL_DISPLAY",
            nickName = "ORIGINAL_NICK",
            profileUrl = "ORIGINAL_PROFILE",
            title = "ORIGINAL_TITLE",
            userType = "ORIGINAL_TYPE",
            preferredLanguage = "en-US",
            locale = "ORIGINAL_LOCALE",
            timezone = "America/New_York",
            active = true,
            emails = new[] { new { value = "original@example.com", primary = "true" } },
            phoneNumbers = new[] { new { value = "1-555-0100", primary = "true" } },
            addresses = new[] { new {
                formatted = "ORIGINAL_FORMATTED",
                streetAddress = "123 Original St",
                locality = "ORIGINAL_CITY",
                region = "ORIGINAL_STATE",
                postalCode = "00000",
                country = "Original Country",
                primary = "true"
            }}
        };

        var response = await _client.PostAsJsonAsync("/scim/Users", user);
        response.StatusCode.ShouldBe(HttpStatusCode.Created,
            $"Failed to create test user: {await response.Content.ReadAsStringAsync()}");
        var created = await response.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);
        created.ShouldNotBeNull();
        _output.WriteLine($"[SETUP] Created user {created.Id} ({userName})");
        return created;
    }

    /// <summary>
    /// Creates a test group and returns the created group.
    /// </summary>
    private async Task<ScimGroup> CreateTestGroupAsync(string? displayNameSuffix = null)
    {
        var displayName = $"SCIMVALIDATOR_{displayNameSuffix ?? Guid.NewGuid().ToString("N")[..8]}";
        var group = new
        {
            schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:Group" },
            externalId = Guid.NewGuid().ToString(),
            displayName
        };

        var response = await _client.PostAsJsonAsync("/scim/Groups", group);
        response.StatusCode.ShouldBe(HttpStatusCode.Created,
            $"Failed to create test group: {await response.Content.ReadAsStringAsync()}");
        var created = await response.Content.ReadFromJsonAsync<ScimGroup>(JsonOptions);
        created.ShouldNotBeNull();
        _output.WriteLine($"[SETUP] Created group {created.Id} ({displayName})");
        return created;
    }

    /// <summary>
    /// Sends a raw PATCH request with JSON content.
    /// </summary>
    private async Task<HttpResponseMessage> SendPatchAsync(string url, object patchBody)
    {
        var json = JsonSerializer.Serialize(patchBody, JsonOptions);
        _output.WriteLine($"[PATCH] {url}");
        _output.WriteLine($"[BODY] {json}");
        var content = new StringContent(json, Encoding.UTF8, "application/scim+json");
        var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await _client.SendAsync(request);
    }

    #endregion

    #region Failure: Patch User - Replace Attributes (filtered path on multi-valued attributes)
    // Validator: "Patch User - Replace Attributes"
    // Runs affected: 02, 03, 04
    // Errors:
    //   - "The value of emails[primary eq true].value is Missing from the fetched Resource"
    //   - "The value of phoneNumbers[primary eq true].value is Missing from the fetched Resource"
    //   - "The value of addresses[primary eq true].* is Missing from the fetched Resource"
    //
    // Root cause: PATCH replace with filtered path like "emails[primary eq true].value"
    //             combined with a replace without path for scalar attributes in same request.

    [Fact]
    public async Task PatchUser_ReplaceFilteredEmailPrimaryValue_ShouldPersist()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "replace", path = "emails[primary eq true].value", value = "updated@newdomain.com" }
            }
        };

        // Act - PATCH
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act - GET to verify persistence (this is what the validator does)
        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetchedUser.ShouldNotBeNull();
        fetchedUser.Emails.ShouldNotBeEmpty("emails should not be empty after patch");
        var primaryEmail = fetchedUser.Emails.FirstOrDefault(e => e.Primary);
        primaryEmail.ShouldNotBeNull("Should have a primary email");
        primaryEmail.Value.ShouldBe("updated@newdomain.com",
            "emails[primary eq true].value should be updated");
    }

    [Fact]
    public async Task PatchUser_ReplaceFilteredPhonePrimaryValue_ShouldPersist()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "replace", path = "phoneNumbers[primary eq true].value", value = "9-999-0001" }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetchedUser.ShouldNotBeNull();
        fetchedUser.PhoneNumbers.ShouldNotBeEmpty();
        var primaryPhone = fetchedUser.PhoneNumbers.FirstOrDefault(p => p.Primary);
        primaryPhone.ShouldNotBeNull("Should have a primary phone number");
        primaryPhone.Value.ShouldBe("9-999-0001",
            "phoneNumbers[primary eq true].value should be updated");
    }

    [Fact]
    public async Task PatchUser_ReplaceFilteredAddressPrimaryFields_ShouldPersist()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "replace", path = "addresses[primary eq true].formatted", value = "NEW_FORMATTED" },
                new { op = "replace", path = "addresses[primary eq true].streetAddress", value = "999 New Street" },
                new { op = "replace", path = "addresses[primary eq true].locality", value = "NEW_CITY" },
                new { op = "replace", path = "addresses[primary eq true].region", value = "NEW_STATE" },
                new { op = "replace", path = "addresses[primary eq true].postalCode", value = "99999" },
                new { op = "replace", path = "addresses[primary eq true].country", value = "New Country" }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetchedUser.ShouldNotBeNull();
        fetchedUser.Addresses.ShouldNotBeEmpty();
        var primaryAddr = fetchedUser.Addresses.FirstOrDefault(a => a.Primary);
        primaryAddr.ShouldNotBeNull("Should have a primary address");
        primaryAddr.Formatted.ShouldBe("NEW_FORMATTED");
        primaryAddr.StreetAddress.ShouldBe("999 New Street");
        primaryAddr.Locality.ShouldBe("NEW_CITY");
        primaryAddr.Region.ShouldBe("NEW_STATE");
        primaryAddr.PostalCode.ShouldBe("99999");
        primaryAddr.Country.ShouldBe("New Country");
    }

    [Fact]
    public async Task PatchUser_ReplaceFilteredMultiValuedAndScalarsCombined_ShouldPersistAll()
    {
        // Arrange: reproduces the exact validator scenario — 9 operations in one PATCH
        var user = await CreateTestUserAsync();
        var newExternalId = Guid.NewGuid().ToString();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "replace", path = "emails[primary eq true].value", value = "patched@combined.com" },
                new { op = "replace", path = "phoneNumbers[primary eq true].value", value = "0-000-0000" },
                new { op = "replace", path = "addresses[primary eq true].formatted", value = "COMBINED_FORMATTED" },
                new { op = "replace", path = "addresses[primary eq true].streetAddress", value = "1 Combined Ave" },
                new { op = "replace", path = "addresses[primary eq true].locality", value = "COMBINED_CITY" },
                new { op = "replace", path = "addresses[primary eq true].region", value = "COMBINED_REGION" },
                new { op = "replace", path = "addresses[primary eq true].postalCode", value = "11111" },
                new { op = "replace", path = "addresses[primary eq true].country", value = "CombinedLand" },
                // Operation 9: replace without path — scalar attributes
                new {
                    op = "replace",
                    value = new {
                        externalId = newExternalId,
                        displayName = "COMBINED_DISPLAY",
                        nickName = "COMBINED_NICK",
                        profileUrl = "COMBINED_PROFILE",
                        title = "COMBINED_TITLE",
                        userType = "COMBINED_TYPE",
                        preferredLanguage = "fr-FR",
                        locale = "COMBINED_LOCALE",
                        timezone = "Europe/Paris",
                        active = true
                    }
                }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // GET to verify (same as validator does)
        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert scalar attributes
        fetched.ShouldNotBeNull();
        fetched.ExternalId.ShouldBe(newExternalId);
        fetched.DisplayName.ShouldBe("COMBINED_DISPLAY");
        fetched.NickName.ShouldBe("COMBINED_NICK");
        fetched.PreferredLanguage.ShouldBe("fr-FR");
        fetched.Timezone.ShouldBe("Europe/Paris");

        // Assert filtered multi-valued attributes
        var primaryEmail = fetched.Emails.FirstOrDefault(e => e.Primary);
        primaryEmail.ShouldNotBeNull("emails[primary eq true] should exist");
        primaryEmail.Value.ShouldBe("patched@combined.com");

        var primaryPhone = fetched.PhoneNumbers.FirstOrDefault(p => p.Primary);
        primaryPhone.ShouldNotBeNull("phoneNumbers[primary eq true] should exist");
        primaryPhone.Value.ShouldBe("0-000-0000");

        var primaryAddr = fetched.Addresses.FirstOrDefault(a => a.Primary);
        primaryAddr.ShouldNotBeNull("addresses[primary eq true] should exist");
        primaryAddr.Formatted.ShouldBe("COMBINED_FORMATTED");
        primaryAddr.StreetAddress.ShouldBe("1 Combined Ave");
        primaryAddr.Locality.ShouldBe("COMBINED_CITY");
        primaryAddr.Region.ShouldBe("COMBINED_REGION");
        primaryAddr.PostalCode.ShouldBe("11111");
        primaryAddr.Country.ShouldBe("CombinedLand");
    }

    #endregion

    #region Failure: Get Group by ID excluding members
    // Validator: "Get group by id excluding members"
    // Runs affected: 01, 02, 03, 04
    // Error: "The response should exclude the attribute: members"
    //
    // Root cause: excludedAttributes=members returns "members":[] instead of omitting the field

    [Fact]
    public async Task GetGroupById_ExcludingMembers_ShouldOmitMembersFromResponse()
    {
        // Arrange
        var group = await CreateTestGroupAsync();

        // Act — exact request the validator sends
        var response = await _client.GetAsync($"/scim/Groups/{group.Id}?excludedAttributes=members");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {body}");

        // Assert: "members" property should NOT appear in JSON at all
        var jsonNode = JsonNode.Parse(body);
        jsonNode.ShouldNotBeNull();
        var membersNode = jsonNode["members"];
        membersNode.ShouldBeNull(
            "The 'members' attribute should be completely absent from the response " +
            "when excludedAttributes=members is specified. Got: " + body);
    }

    #endregion

    #region Failure: Filter Groups excluding members
    // Validator: "Filter for an existing group by displayName excluding members"
    // Runs affected: 01, 02, 03, 04
    // Error: "The response should exclude the attribute: members"

    [Fact]
    public async Task GetGroups_FilterByDisplayName_ExcludingMembers_ShouldOmitMembersFromResponse()
    {
        // Arrange
        var group = await CreateTestGroupAsync();

        // Act — exact query the validator sends
        var filter = $"displayName eq \"{group.DisplayName}\"";
        var url = $"/scim/Groups?excludedAttributes=members&filter={Uri.EscapeDataString(filter)}";
        var response = await _client.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"[RESPONSE] {body}");

        // Assert: each resource in the list should NOT have "members"
        var jsonNode = JsonNode.Parse(body);
        jsonNode.ShouldNotBeNull();
        var resources = jsonNode["resources"]?.AsArray();
        resources.ShouldNotBeNull();
        resources.Count.ShouldBeGreaterThan(0, "Should find the group we just created");

        foreach (var resource in resources)
        {
            var membersNode = resource?["members"];
            membersNode.ShouldBeNull(
                "Each group resource should not contain 'members' when excludedAttributes=members. " +
                "Got: " + resource?.ToJsonString());
        }
    }

    #endregion

    #region Preview Failure: Patch User - Multiple Operations on different attributes
    // Validator: "Patch User - Multiple Operations on different attributes"
    // Runs affected: 01, 02, 03, 04
    // Errors:
    //   - "The value of externalId should be <new> instead of <old>" (op:add not applied)
    //   - "The value of nickName should not be in the fetched Resource" (op:remove not applied)
    //
    // Root cause: op:add without path doesn't apply value object; op:remove for scalar attrs not supported

    [Fact]
    public async Task PatchUser_RemoveNickName_ShouldSetToNull()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        user.NickName.ShouldNotBeNullOrEmpty("User should have a nickName initially");

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "remove", path = "nickName" }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert: nickName should be null/absent
        fetched.ShouldNotBeNull();
        fetched.NickName.ShouldBeNull(
            "nickName should be null after op:remove. " +
            "Validator error: 'The value of nickName should not be in the fetched Resource'");
    }

    [Fact]
    public async Task PatchUser_AddExternalId_WithoutPath_ShouldApplyValue()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var newExternalId = Guid.NewGuid().ToString();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "add", value = new { externalId = newExternalId } }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.ExternalId.ShouldBe(newExternalId,
            "externalId should be updated by op:add with value object. " +
            "Validator error: 'The value of externalId should be <new> instead of <old>'");
    }

    [Fact]
    public async Task PatchUser_MultipleOperations_RemoveAddReplace_ShouldApplyAllInOrder()
    {
        // Arrange: reproduces the exact validator scenario
        var user = await CreateTestUserAsync();
        var newExternalId = Guid.NewGuid().ToString();
        var newDisplayName = "UPDATED_DISPLAY_" + Guid.NewGuid().ToString("N")[..6];

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                // Op 1: remove nickName
                new { op = "remove", path = "nickName" },
                // Op 2: add externalId
                new { op = "add", value = new { externalId = newExternalId } },
                // Op 3: replace displayName
                new { op = "replace", value = new { displayName = newDisplayName } }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert all 3 operations applied
        fetched.ShouldNotBeNull();
        fetched.NickName.ShouldBeNull("nickName should be removed (op:remove)");
        fetched.ExternalId.ShouldBe(newExternalId, "externalId should be set (op:add)");
        fetched.DisplayName.ShouldBe(newDisplayName, "displayName should be updated (op:replace)");
    }

    #endregion

    #region Failure: Patch User - Multiple Operations on same attribute (preview)
    // Validator: "Patch User - Multiple Operations on same attribute"
    // Runs affected: 02
    // Error: "The value of externalId should be <final> instead of <initial>"
    //
    // Scenario: remove externalId, then add new one, then replace with another
    // The last operation should win.

    [Fact]
    public async Task PatchUser_MultipleOperationsOnSameAttribute_LastOperationWins()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var intermediateExternalId = Guid.NewGuid().ToString();
        var finalExternalId = Guid.NewGuid().ToString();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "remove", path = "externalId" },
                new { op = "add", value = new { externalId = intermediateExternalId } },
                new { op = "replace", value = new { externalId = finalExternalId } }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert: the last replace should win
        fetched.ShouldNotBeNull();
        fetched.ExternalId.ShouldBe(finalExternalId,
            "externalId should be the value from the last (replace) operation");
    }

    #endregion

    #region Failure: Patch Group - Replace Attributes (externalId via value object)
    // Validator: "Patch Group - Replace Attributes"
    // Runs affected: 01, 02
    // Error: "The value of externalId should be <new> instead of <old>"
    //
    // Root cause: replace without path with value object containing externalId not applied to group

    [Fact]
    public async Task PatchGroup_ReplaceExternalId_ViaValueObject_ShouldUpdate()
    {
        // Arrange
        var group = await CreateTestGroupAsync();
        var newExternalId = Guid.NewGuid().ToString();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "replace", value = new { externalId = newExternalId } }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Groups/{group.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Groups/{group.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimGroup>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.ExternalId.ShouldBe(newExternalId,
            "Group externalId should be updated via replace value object");
    }

    #endregion

    #region Failure: Update Group displayName (via replace value object)
    // Validator: "Update Group displayName"
    // Runs affected: 01, 02
    // Error: "The value of displayName should be <new> instead of <old>"

    [Fact]
    public async Task PatchGroup_ReplaceDisplayName_ViaValueObject_ShouldUpdate()
    {
        // Arrange
        var group = await CreateTestGroupAsync();
        var newDisplayName = "UPDATED_GROUP_" + Guid.NewGuid().ToString("N")[..6];

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "replace", value = new { displayName = newDisplayName } }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Groups/{group.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Groups/{group.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimGroup>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.DisplayName.ShouldBe(newDisplayName,
            "Group displayName should be updated via replace value object");
    }

    #endregion

    #region Failure: Patch Group - Replace Members (path=members)
    // Validator: "Patch Group - Add Member"
    // Ensures replace with path=members correctly replaces the members list

    [Fact]
    public async Task PatchGroup_ReplaceMembers_WithPath_ShouldReplaceEntireList()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var group = await CreateTestGroupAsync();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new {
                    op = "replace",
                    path = "members",
                    value = new[] { new { value = user.Id } }
                }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Groups/{group.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Groups/{group.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimGroup>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.Members.ShouldNotBeNull();
        fetched.Members.Count.ShouldBe(1, "Members list should have exactly 1 member after replace");
        fetched.Members.First().Value.ShouldBe(user.Id);
    }

    #endregion

    #region Failure: Patch User - Replace Attributes Verbose (run 02 comprehensive)
    // Validator: "Patch User - Replace Attributes"  
    // Run 02: ALL scalar attributes + filtered multi-valued not applied
    // This was a comprehensive failure where replace-without-path value object was ignored

    [Fact]
    public async Task PatchUser_ReplaceAllScalarAttributes_ViaValueObject_ShouldUpdateAll()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var newExternalId = Guid.NewGuid().ToString();

        // This replicates the exact value object from the validator
        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new {
                    op = "replace",
                    value = new
                    {
                        externalId = newExternalId,
                        displayName = "NEW_DISPLAY",
                        nickName = "NEW_NICK",
                        profileUrl = "NEW_PROFILE",
                        title = "NEW_TITLE",
                        userType = "NEW_TYPE",
                        preferredLanguage = "fr-FR",
                        locale = "NEW_LOCALE",
                        timezone = "Europe/Paris",
                        active = true
                    }
                }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.ExternalId.ShouldBe(newExternalId);
        fetched.DisplayName.ShouldBe("NEW_DISPLAY");
        fetched.NickName.ShouldBe("NEW_NICK");
        fetched.ProfileUrl.ShouldBe("NEW_PROFILE");
        fetched.Title.ShouldBe("NEW_TITLE");
        fetched.UserType.ShouldBe("NEW_TYPE");
        fetched.PreferredLanguage.ShouldBe("fr-FR");
        fetched.Locale.ShouldBe("NEW_LOCALE");
        fetched.Timezone.ShouldBe("Europe/Paris");
        fetched.Active.ShouldBeTrue();
    }

    [Fact]
    public async Task PatchUser_ReplaceNameDotNotation_ViaValueObject_ShouldUpdateAll()
    {
        // Arrange: validator sends "name.formatted", "name.familyName" etc. as keys
        var user = await CreateTestUserAsync();

        // Use raw JSON because anonymous types can't have dots in property names
        var json = JsonSerializer.Serialize(new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new[]
            {
                new
                {
                    op = "replace",
                    value = (object)null!
                }
            }
        });

        // Manually build the value object with dot-notation property names
        var valueObj = new JsonObject
        {
            ["name.formatted"] = "NewFormatted",
            ["name.familyName"] = "NewFamily",
            ["name.givenName"] = "NewGiven",
            ["name.middleName"] = "NewMiddle",
            ["name.honorificPrefix"] = "Dr",
            ["name.honorificSuffix"] = "III"
        };

        var patchNode = JsonNode.Parse(json)!;
        patchNode["Operations"]![0]!["value"] = valueObj;

        // Act
        var content = new StringContent(patchNode.ToJsonString(), Encoding.UTF8, "application/scim+json");
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/scim/Users/{user.Id}") { Content = content };
        var patchResponse = await _client.SendAsync(request);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.Name.ShouldNotBeNull();
        fetched.Name.Formatted.ShouldBe("NewFormatted");
        fetched.Name.FamilyName.ShouldBe("NewFamily");
        fetched.Name.GivenName.ShouldBe("NewGiven");
        fetched.Name.MiddleName.ShouldBe("NewMiddle");
        fetched.Name.HonorificPrefix.ShouldBe("Dr");
        fetched.Name.HonorificSuffix.ShouldBe("III");
    }

    #endregion

    #region Additional remove operations for scalar attributes

    [Fact]
    public async Task PatchUser_RemoveExternalId_ShouldSetToNull()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        user.ExternalId.ShouldNotBeNullOrEmpty();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "remove", path = "externalId" }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.ExternalId.ShouldBeNull("externalId should be null after remove operation");
    }

    [Fact]
    public async Task PatchUser_RemoveDisplayName_ShouldSetToNull()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        var patchBody = new
        {
            schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
            Operations = new object[]
            {
                new { op = "remove", path = "displayName" }
            }
        };

        // Act
        var patchResponse = await SendPatchAsync($"/scim/Users/{user.Id}", patchBody);
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/scim/Users/{user.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ScimUser>(JsonOptions);

        // Assert
        fetched.ShouldNotBeNull();
        fetched.DisplayName.ShouldBeNull("displayName should be null after remove operation");
    }

    #endregion
}

/// <summary>
/// Collection definition for SCIM Validator compliance tests.
/// Uses separate PostgreSQL container for test isolation.
/// </summary>
[CollectionDefinition("ScimValidatorCompliance")]
public class ScimValidatorComplianceCollection : ICollectionFixture<ScimWebApplicationFactory>
{
}




