# SCIM Base Classes Tests

## Summary

This document describes **10 new tests** added in `ScimAPI.Tests\ScimSchemaGeneratorTests.cs` to verify that the base classes `ScimUserBase` and `ScimGroupBase` contain the minimum required fields according to RFC 7643.

## Added Tests

### Tests for ScimUserBase

#### 1. `ScimUserBase_ShouldHaveRequiredAttributesOnly()`
- **Goal**: Verify that `ScimUserBase` contains only the REQUIRED attributes
- **Verifies**:
  - The schema has the correct ID: `urn:ietf:params:scim:schemas:core:2.0:User`
  - The schema name is "User"
  - Only one attribute is present (`userName`)
  - The `userName` attribute is present

#### 2. `ScimUserBase_UserName_ShouldBeRequired()`
- **Goal**: Verify that the `userName` attribute is correctly configured
- **Verifies**:
  - `userName` is marked as REQUIRED (`Required = true`)
  - Type is `"string"`
  - Uniqueness is set to `"server"`

#### 3. `ScimUserBase_ShouldHaveSystemProperties()`
- **Goal**: Verify that system properties are initialized
- **Verifies**:
  - `Id` is generated automatically
  - `Schemas` contains the correct URI
  - `Meta` is initialized

### Tests for ScimGroupBase

#### 4. `ScimGroupBase_ShouldHaveRequiredAttributesOnly()`
- **Goal**: Verify that `ScimGroupBase` contains only the REQUIRED attributes
- **Verifies**:
  - The schema has the correct ID: `urn:ietf:params:scim:schemas:core:2.0:Group`
  - The schema name is "Group"
  - Only one attribute is present (`displayName`)
  - The `displayName` attribute is present

#### 5. `ScimGroupBase_DisplayName_ShouldBeRequired()`
- **Goal**: Verify that the `displayName` attribute is correctly configured
- **Verifies**:
  - `displayName` is marked as REQUIRED (`Required = true`)
  - Type is `"string"`

#### 6. `ScimGroupBase_ShouldHaveSystemProperties()`
- **Goal**: Verify that system properties are initialized
- **Verifies**:
  - `Id` is generated automatically
  - `Schemas` contains the correct URI
  - `Meta` is initialized

### Base vs Full Class Comparison Tests

#### 7. `ScimUser_ShouldHaveMoreAttributesThanBase()`
- **Goal**: Verify that `ScimUser` contains more attributes than `ScimUserBase`
- **Verifies**:
  - `ScimUser` has more attributes than `ScimUserBase`
  - `ScimUser` has at least 10 attributes (including optional ones)

#### 8. `ScimGroup_ShouldHaveMoreAttributesThanBase()`
- **Goal**: Verify that `ScimGroup` contains more attributes than `ScimGroupBase`
- **Verifies**:
  - `ScimGroup` has more attributes than `ScimGroupBase`
  - `ScimGroup` contains optional attributes: `externalId`, `members`

#### 9. `ScimUserBase_And_ScimUser_ShouldHaveSameSchemaId()`
- **Goal**: Verify that the base and full class share the same schema
- **Verifies**:
  - Both have the same schema ID
  - Both have the same schema name

#### 10. `ScimGroupBase_And_ScimGroup_ShouldHaveSameSchemaId()`
- **Goal**: Verify that the base and full class share the same schema
- **Verifies**:
  - Both have the same schema ID
  - Both have the same schema name

## Class Structure

### ScimUserBase (Required Attributes Only)
```csharp
[ScimResource("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")]
public class ScimUserBase
{
    public string Id { get; set; }
    [ScimProperty(..., Required = true)]
    public string UserName { get; set; }
    public List<string> Schemas { get; set; }
    public ScimMeta Meta { get; set; }
}
```

### ScimUser (Required + Optional Attributes)
```csharp
public class ScimUser : ScimUserBase
{
    // +15 optional attributes
    public string? ExternalId { get; set; }
    public ScimName Name { get; set; }
    public string? DisplayName { get; set; }
    // ... etc
}
```

### ScimGroupBase (Required Attributes Only)
```csharp
[ScimResource("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")]
public class ScimGroupBase
{
    public string Id { get; set; }
    [ScimProperty(..., Required = true)]
    public string DisplayName { get; set; }
    public List<string> Schemas { get; set; }
    public ScimMeta Meta { get; set; }
}
```

### ScimGroup (Required + Optional Attributes)
```csharp
public class ScimGroup : ScimGroupBase
{
    // +2 optional attributes
    public string? ExternalId { get; set; }
    public List<ScimMember> Members { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; }
}
```

## Running the Tests

### Via PowerShell Script
```powershell
.\Run-BaseClassesTests.ps1
```

### Via dotnet CLI
```powershell
# All base class tests
dotnet test --filter "FullyQualifiedName~Base"

# Specific test
dotnet test --filter "FullyQualifiedName~ScimUserBase_ShouldHaveRequiredAttributesOnly"

# All schema tests
dotnet test ScimAPI.Tests/ScimAPI.Tests.csproj --filter "FullyQualifiedName~ScimSchemaGeneratorTests"
```

### Via Visual Studio / Rider
- Open `ScimSchemaGeneratorTests.cs`
- Navigate to the `#region Base Classes Schema Tests` section
- Run tests individually or as a group

## Modified Files

1. **ScimAPI.Tests\ScimSchemaGeneratorTests.cs**
   - Added 10 new tests in a new `#region Base Classes Schema Tests`
   - Lines 507–657

2. **Run-BaseClassesTests.ps1** (new file)
   - PowerShell script to easily run all base class tests
   - Displays a colorized summary of results

## RFC 7643 Compliance

These tests ensure that:

✅ **ScimUserBase** contains only the REQUIRED attribute `userName`
✅ **ScimGroupBase** contains only the REQUIRED attribute `displayName`
✅ System properties (Id, Schemas, Meta) are present in the base classes
✅ Full classes (`ScimUser`, `ScimGroup`) inherit from the base and add optional attributes
✅ The `[ScimResource]` attribute is on the base classes (inheritable)
✅ Generated schemas correctly reflect the hierarchy

## Benefits of This Architecture

1. **Clear separation** between required and optional attributes
2. **Flexibility**: ability to use the base class for minimal operations
3. **Validation**: tests guarantee SCIM compliance
4. **Extensibility**: easy to add new optional attributes
5. **Type-safety**: the compiler enforces presence of required attributes

## Creation Date

2026-02-02
