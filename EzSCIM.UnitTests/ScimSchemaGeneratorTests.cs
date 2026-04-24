using EzSCIM.Attributes;
using EzSCIM.Helpers;
using EzSCIM.Models;
using Shouldly;
using Xunit;

namespace EzSCIM.UnitTests;

/// <summary>
/// Tests for the ScimSchemaGenerator with different configurations of User and Group models.
/// </summary>
public class ScimSchemaGeneratorTests
{
    #region Basic Schema Generation Tests

    [Fact]
    public void UserSchema_ShouldBePreCalculated_AndNotNull()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldBe("urn:ietf:params:scim:schemas:core:2.0:User");
        schema.Name.ShouldBe("User");
        schema.Description.ShouldBe("User Account");
    }

    [Fact]
    public void GroupSchema_ShouldBePreCalculated_AndNotNull()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldBe("urn:ietf:params:scim:schemas:core:2.0:Group");
        schema.Name.ShouldBe("Group");
        schema.Description.ShouldBe("Group");
    }

    [Fact]
    public void UserSchema_ShouldHaveCorrectNumberOfAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;

        // Assert
        schema.Attributes.ShouldNotBeEmpty();
        schema.Attributes.Count.ShouldBeGreaterThan(10); // Au moins 10 attributs
    }

    [Fact]
    public void GroupSchema_ShouldHaveCorrectNumberOfAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;

        // Assert
        schema.Attributes.ShouldNotBeEmpty();
        schema.Attributes.Count.ShouldBe(3); // displayName, externalId, members
    }

    #endregion

    #region User Schema Attribute Tests

    [Fact]
    public void UserSchema_ShouldHaveUserNameAttribute_WithCorrectProperties()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var userNameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "userName");

        // Assert
        userNameAttr.ShouldNotBeNull();
        userNameAttr.Type.ShouldBe("string");
        userNameAttr.Required.ShouldBeTrue();
        userNameAttr.Uniqueness.ShouldBe("server");
        userNameAttr.Mutability.ShouldBe("readWrite");
        userNameAttr.Returned.ShouldBe("default");
    }

    [Fact]
    public void UserSchema_ShouldHaveExternalIdAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var externalIdAttr = schema.Attributes.FirstOrDefault(a => a.Name == "externalId");

        // Assert
        externalIdAttr.ShouldNotBeNull();
        externalIdAttr.Type.ShouldBe("string");
        externalIdAttr.Required.ShouldBeFalse();
    }

    [Fact]
    public void UserSchema_ShouldHaveDisplayNameAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var displayNameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "displayName");

        // Assert
        displayNameAttr.ShouldNotBeNull();
        displayNameAttr.Type.ShouldBe("string");
    }

    [Fact]
    public void UserSchema_ShouldHaveActiveAttribute_WithBooleanType()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var activeAttr = schema.Attributes.FirstOrDefault(a => a.Name == "active");

        // Assert
        activeAttr.ShouldNotBeNull();
        activeAttr.Type.ShouldBe("boolean");
        activeAttr.Required.ShouldBeFalse();
    }

    [Fact]
    public void UserSchema_ShouldNotHaveSystemAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;

        // Assert - Ces propriétés ne doivent PAS être dans le schéma (pas d'attribut ScimProperty)
        schema.Attributes.Any(a => a.Name == "id").ShouldBeFalse();
        schema.Attributes.Any(a => a.Name == "schemas").ShouldBeFalse();
        schema.Attributes.Any(a => a.Name == "meta").ShouldBeFalse();
        schema.Attributes.Any(a => a.Name == "customAttributes").ShouldBeFalse();
    }

    #endregion

    #region Complex Type Tests

    [Fact]
    public void UserSchema_NameAttribute_ShouldBeComplex_WithSubAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var nameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "name");

        // Assert
        nameAttr.ShouldNotBeNull();
        nameAttr.Type.ShouldBe("complex");
        nameAttr.MultiValued.ShouldBeFalse();
        nameAttr.SubAttributes.ShouldNotBeNull();
        nameAttr.SubAttributes.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void UserSchema_NameAttribute_ShouldHaveGivenNameSubAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var nameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "name");
        var givenName = nameAttr?.SubAttributes?.FirstOrDefault(sa => sa.Name == "givenName");

        // Assert
        givenName.ShouldNotBeNull();
        givenName.Type.ShouldBe("string");
    }

    [Fact]
    public void UserSchema_NameAttribute_ShouldHaveFamilyNameSubAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var nameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "name");
        var familyName = nameAttr?.SubAttributes?.FirstOrDefault(sa => sa.Name == "familyName");

        // Assert
        familyName.ShouldNotBeNull();
        familyName.Type.ShouldBe("string");
    }

    [Fact]
    public void UserSchema_NameAttribute_ShouldHaveAllExpectedSubAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var nameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "name");

        // Assert
        nameAttr.ShouldNotBeNull();
        nameAttr.SubAttributes.ShouldNotBeNull();
        
        var subAttrNames = nameAttr.SubAttributes.Select(sa => sa.Name).ToList();
        subAttrNames.ShouldContain("formatted");
        subAttrNames.ShouldContain("familyName");
        subAttrNames.ShouldContain("givenName");
        subAttrNames.ShouldContain("middleName");
        subAttrNames.ShouldContain("honorificPrefix");
        subAttrNames.ShouldContain("honorificSuffix");
    }

    #endregion

    #region Multi-Valued Complex Type Tests

    [Fact]
    public void UserSchema_EmailsAttribute_ShouldBeMultiValuedComplex()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var emailsAttr = schema.Attributes.FirstOrDefault(a => a.Name == "emails");

        // Assert
        emailsAttr.ShouldNotBeNull();
        emailsAttr.Type.ShouldBe("complex");
        emailsAttr.MultiValued.ShouldBeTrue();
        emailsAttr.SubAttributes.ShouldNotBeNull();
    }

    [Fact]
    public void UserSchema_EmailsAttribute_ShouldHaveValueSubAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var emailsAttr = schema.Attributes.FirstOrDefault(a => a.Name == "emails");
        var value = emailsAttr?.SubAttributes?.FirstOrDefault(sa => sa.Name == "value");

        // Assert
        value.ShouldNotBeNull();
        value.Type.ShouldBe("string");
    }

    [Fact]
    public void UserSchema_EmailsAttribute_ShouldHaveTypeAndPrimarySubAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var emailsAttr = schema.Attributes.FirstOrDefault(a => a.Name == "emails");

        // Assert
        emailsAttr.ShouldNotBeNull();
        var subAttrNames = emailsAttr.SubAttributes.Select(sa => sa.Name).ToList();
        subAttrNames.ShouldContain("value");
        subAttrNames.ShouldContain("type");
        subAttrNames.ShouldContain("primary");
    }

    [Fact]
    public void UserSchema_PhoneNumbersAttribute_ShouldBeMultiValuedComplex()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var phoneNumbersAttr = schema.Attributes.FirstOrDefault(a => a.Name == "phoneNumbers");

        // Assert
        phoneNumbersAttr.ShouldNotBeNull();
        phoneNumbersAttr.Type.ShouldBe("complex");
        phoneNumbersAttr.MultiValued.ShouldBeTrue();
        phoneNumbersAttr.SubAttributes.ShouldNotBeNull();
        phoneNumbersAttr.SubAttributes.Count.ShouldBe(3); // value, type, primary
    }

    [Fact]
    public void UserSchema_AddressesAttribute_ShouldBeMultiValuedComplex()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var addressesAttr = schema.Attributes.FirstOrDefault(a => a.Name == "addresses");

        // Assert
        addressesAttr.ShouldNotBeNull();
        addressesAttr.Type.ShouldBe("complex");
        addressesAttr.MultiValued.ShouldBeTrue();
        addressesAttr.SubAttributes.ShouldNotBeNull();
        addressesAttr.SubAttributes.Count.ShouldBeGreaterThan(5); // formatted, streetAddress, locality, region, postalCode, country, type, primary
    }

    [Fact]
    public void UserSchema_GroupsAttribute_ShouldBeReadOnly()
    {
        // Act
        var schema = ScimSchemaGenerator.UserSchema;
        var groupsAttr = schema.Attributes.FirstOrDefault(a => a.Name == "groups");

        // Assert
        groupsAttr.ShouldNotBeNull();
        groupsAttr.Type.ShouldBe("complex");
        groupsAttr.MultiValued.ShouldBeTrue();
        groupsAttr.Mutability.ShouldBe("readOnly");
    }

    #endregion

    #region Group Schema Attribute Tests

    [Fact]
    public void GroupSchema_ShouldHaveDisplayNameAttribute_Required()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;
        var displayNameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "displayName");

        // Assert
        displayNameAttr.ShouldNotBeNull();
        displayNameAttr.Type.ShouldBe("string");
        displayNameAttr.Required.ShouldBeTrue();
    }

    [Fact]
    public void GroupSchema_ShouldHaveExternalIdAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;
        var externalIdAttr = schema.Attributes.FirstOrDefault(a => a.Name == "externalId");

        // Assert
        externalIdAttr.ShouldNotBeNull();
        externalIdAttr.Type.ShouldBe("string");
        externalIdAttr.Required.ShouldBeFalse();
    }

    [Fact]
    public void GroupSchema_MembersAttribute_ShouldBeMultiValuedComplex()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;
        var membersAttr = schema.Attributes.FirstOrDefault(a => a.Name == "members");

        // Assert
        membersAttr.ShouldNotBeNull();
        membersAttr.Type.ShouldBe("complex");
        membersAttr.MultiValued.ShouldBeTrue();
        membersAttr.SubAttributes.ShouldNotBeNull();
    }

    [Fact]
    public void GroupSchema_MembersAttribute_ShouldHaveValueSubAttribute_Required()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;
        var membersAttr = schema.Attributes.FirstOrDefault(a => a.Name == "members");
        var value = membersAttr?.SubAttributes?.FirstOrDefault(sa => sa.Name == "value");

        // Assert
        value.ShouldNotBeNull();
        value.Type.ShouldBe("string");
        value.Required.ShouldBeTrue();
    }

    [Fact]
    public void GroupSchema_MembersAttribute_ShouldHaveRefSubAttribute()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;
        var membersAttr = schema.Attributes.FirstOrDefault(a => a.Name == "members");
        var refAttr = membersAttr?.SubAttributes?.FirstOrDefault(sa => sa.Name == "$ref");

        // Assert
        refAttr.ShouldNotBeNull();
        refAttr.Type.ShouldBe("reference");
    }

    [Fact]
    public void GroupSchema_ShouldNotHaveSystemAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.GroupSchema;

        // Assert
        schema.Attributes.Any(a => a.Name == "id").ShouldBeFalse();
        schema.Attributes.Any(a => a.Name == "schemas").ShouldBeFalse();
        schema.Attributes.Any(a => a.Name == "meta").ShouldBeFalse();
    }

    #endregion

    #region Custom Type Tests

    [Fact]
    public void GetSchema_WithCustomUserType_ShouldIncludeInheritedAndNewProperties()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<TestCustomUser>();

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldBe("urn:test:custom:user");
        
        // Doit inclure les propriétés héritées de ScimUser
        schema.Attributes.Any(a => a.Name == "userName").ShouldBeTrue();
        schema.Attributes.Any(a => a.Name == "displayName").ShouldBeTrue();
        
        // Doit inclure les nouvelles propriétés
        schema.Attributes.Any(a => a.Name == "employeeNumber").ShouldBeTrue();
        schema.Attributes.Any(a => a.Name == "department").ShouldBeTrue();
    }

    [Fact]
    public void GetSchema_WithMinimalCustomType_ShouldGenerateValidSchema()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<TestMinimalUser>();

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldBe("urn:test:minimal:user");
        schema.Name.ShouldBe("MinimalUser");
        schema.Attributes.Count.ShouldBe(1); // Seulement requiredField
    }

    [Fact]
    public void GetSchema_WithTypeWithoutAttribute_ShouldReturnEmptySchema()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<TestUserWithoutAttribute>();

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldContain("unknown");
        schema.Attributes.ShouldBeEmpty();
    }

    [Fact]
    public void GetSchema_WithComplexCustomType_ShouldDiscoverNestedAttributes()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<TestUserWithComplexType>();

        // Assert
        schema.ShouldNotBeNull();
        var badgeAttr = schema.Attributes.FirstOrDefault(a => a.Name == "badge");
        badgeAttr.ShouldNotBeNull();
        badgeAttr.Type.ShouldBe("complex");
        badgeAttr.SubAttributes.ShouldNotBeNull();
        badgeAttr.SubAttributes.Any(sa => sa.Name == "badgeNumber").ShouldBeTrue();
        badgeAttr.SubAttributes.Any(sa => sa.Name == "issueDate").ShouldBeTrue();
    }

    #endregion

    #region Thread-Safety Tests

    [Fact]
    public void UserSchema_MultipleAccess_ShouldReturnSameInstance()
    {
        // Act
        var schema1 = ScimSchemaGenerator.UserSchema;
        var schema2 = ScimSchemaGenerator.UserSchema;

        // Assert
        ReferenceEquals(schema1, schema2).ShouldBeTrue();
    }

    [Fact]
    public void GroupSchema_MultipleAccess_ShouldReturnSameInstance()
    {
        // Act
        var schema1 = ScimSchemaGenerator.GroupSchema;
        var schema2 = ScimSchemaGenerator.GroupSchema;

        // Assert
        ReferenceEquals(schema1, schema2).ShouldBeTrue();
    }

    #endregion

    #region Test Helper Classes

    [ScimResource("urn:test:custom:user", "CustomUser", "Custom User with extensions")]
    private class TestCustomUser : ScimUser
    {
        [ScimProperty("employeeNumber", "string", Required = true)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [ScimProperty("department", "string")]
        public string? Department { get; set; }
    }

    [ScimResource("urn:test:minimal:user", "MinimalUser", "Minimal User")]
    private class TestMinimalUser
    {
        [ScimProperty("requiredField", "string", Required = true)]
        public string RequiredField { get; set; } = string.Empty;
    }

    private class TestUserWithoutAttribute
    {
        public string SomeProperty { get; set; } = string.Empty;
    }

    [ScimResource("urn:test:complex:user", "ComplexUser", "User with complex type")]
    private class TestUserWithComplexType : ScimUser
    {
        [ScimProperty("badge", "complex")]
        public TestBadgeInfo? Badge { get; set; }
    }

    private class TestBadgeInfo
    {
        [ScimProperty("badgeNumber", "string")]
        public string? BadgeNumber { get; set; }

        [ScimProperty("issueDate", "dateTime")]
        public DateTime? IssueDate { get; set; }
    }

    #endregion

    #region Base Classes Schema Tests

    [Fact]
    public void ScimUserBase_ShouldHaveRequiredAttributesOnly()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<ScimUserBase>();
        var attributeNames = schema.Attributes.Select(a => a.Name).ToList();

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldBe("urn:ietf:params:scim:schemas:core:2.0:User");
        schema.Name.ShouldBe("User");
        
        // Vérifier que seul userName est présent (les autres sont des propriétés système)
        schema.Attributes.Count.ShouldBe(1);
        attributeNames.ShouldContain("userName");
    }

    [Fact]
    public void ScimUserBase_UserName_ShouldBeRequired()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<ScimUserBase>();
        var userNameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "userName");

        // Assert
        userNameAttr.ShouldNotBeNull();
        userNameAttr.Required.ShouldBeTrue();
        userNameAttr.Type.ShouldBe("string");
        userNameAttr.Uniqueness.ShouldBe("server");
    }

    [Fact]
    public void ScimUserBase_ShouldHaveSystemProperties()
    {
        // Arrange
        var user = new ScimUserBase
        {
            UserName = "testuser"
        };

        // Assert
        user.Id.ShouldNotBeNullOrEmpty();
        user.Schemas.ShouldNotBeEmpty();
        user.Schemas.ShouldContain("urn:ietf:params:scim:schemas:core:2.0:User");
        user.Meta.ShouldNotBeNull();
    }

    [Fact]
    public void ScimGroupBase_ShouldHaveRequiredAttributesOnly()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<ScimGroupBase>();
        var attributeNames = schema.Attributes.Select(a => a.Name).ToList();

        // Assert
        schema.ShouldNotBeNull();
        schema.Id.ShouldBe("urn:ietf:params:scim:schemas:core:2.0:Group");
        schema.Name.ShouldBe("Group");
        
        // Vérifier que seul displayName est présent (les autres sont des propriétés système)
        schema.Attributes.Count.ShouldBe(1);
        attributeNames.ShouldContain("displayName");
    }

    [Fact]
    public void ScimGroupBase_DisplayName_ShouldBeRequired()
    {
        // Act
        var schema = ScimSchemaGenerator.GetSchema<ScimGroupBase>();
        var displayNameAttr = schema.Attributes.FirstOrDefault(a => a.Name == "displayName");

        // Assert
        displayNameAttr.ShouldNotBeNull();
        displayNameAttr.Required.ShouldBeTrue();
        displayNameAttr.Type.ShouldBe("string");
    }

    [Fact]
    public void ScimGroupBase_ShouldHaveSystemProperties()
    {
        // Arrange
        var group = new ScimGroupBase
        {
            DisplayName = "Test Group"
        };

        // Assert
        group.Id.ShouldNotBeNullOrEmpty();
        group.Schemas.ShouldNotBeEmpty();
        group.Schemas.ShouldContain("urn:ietf:params:scim:schemas:core:2.0:Group");
        group.Meta.ShouldNotBeNull();
    }

    [Fact]
    public void ScimUser_ShouldHaveMoreAttributesThanBase()
    {
        // Act
        var baseSchema = ScimSchemaGenerator.GetSchema<ScimUserBase>();
        var fullSchema = ScimSchemaGenerator.GetSchema<ScimUser>();

        // Assert
        fullSchema.Attributes.Count.ShouldBeGreaterThan(baseSchema.Attributes.Count);
        fullSchema.Attributes.Count.ShouldBeGreaterThan(10); // Plusieurs attributs optionnels
    }

    [Fact]
    public void ScimGroup_ShouldHaveMoreAttributesThanBase()
    {
        // Act
        var baseSchema = ScimSchemaGenerator.GetSchema<ScimGroupBase>();
        var fullSchema = ScimSchemaGenerator.GetSchema<ScimGroup>();

        // Assert
        fullSchema.Attributes.Count.ShouldBeGreaterThan(baseSchema.Attributes.Count);
        
        // Vérifier que les attributs optionnels sont présents
        var attributeNames = fullSchema.Attributes.Select(a => a.Name).ToList();
        attributeNames.ShouldContain("displayName");
        attributeNames.ShouldContain("externalId");
        attributeNames.ShouldContain("members");
    }

    [Fact]
    public void ScimUserBase_And_ScimUser_ShouldHaveSameSchemaId()
    {
        // Act
        var baseSchema = ScimSchemaGenerator.GetSchema<ScimUserBase>();
        var fullSchema = ScimSchemaGenerator.GetSchema<ScimUser>();

        // Assert
        baseSchema.Id.ShouldBe(fullSchema.Id);
        baseSchema.Name.ShouldBe(fullSchema.Name);
    }

    [Fact]
    public void ScimGroupBase_And_ScimGroup_ShouldHaveSameSchemaId()
    {
        // Act
        var baseSchema = ScimSchemaGenerator.GetSchema<ScimGroupBase>();
        var fullSchema = ScimSchemaGenerator.GetSchema<ScimGroup>();

        // Assert
        baseSchema.Id.ShouldBe(fullSchema.Id);
        baseSchema.Name.ShouldBe(fullSchema.Name);
    }

    #endregion
}
