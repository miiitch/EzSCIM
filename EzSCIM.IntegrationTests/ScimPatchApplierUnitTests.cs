﻿using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.IntegrationTests.Data;
using EzSCIM.Models;
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
        var user = new UserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            EmailsJson = "[{\"value\":\"original@example.com\",\"type\":\"work\",\"primary\":true}]",
            Active = true
        };

        var operations = new[]
        {
            new ScimPatchOperation
            {
                Op = "replace",
                Path = "emails[primary eq true].value",
                Value = "updated@example.com"
            }
        };

        _output.WriteLine($"Original EmailsJson: {user.EmailsJson}");

        // Act
        var result = UserEntityPatchApplier.ApplyPatch(user, operations);

        _output.WriteLine($"ApplyPatch returned: {result}");
        _output.WriteLine($"Updated EmailsJson: {user.EmailsJson}");

        // Assert
        Assert.True(result, "ApplyPatch should return true");
        Assert.Contains("updated@example.com", user.EmailsJson);
    }

    [Fact]
    public void ApplyPatch_FilteredPhonePath_ShouldUpdatePhone()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            PhoneNumbersJson = "[{\"value\":\"111-222-3333\",\"type\":\"work\",\"primary\":true}]",
            Active = true
        };

        var operations = new[]
        {
            new ScimPatchOperation
            {
                Op = "replace",
                Path = "phoneNumbers[primary eq true].value",
                Value = "999-888-7777"
            }
        };

        _output.WriteLine($"Original PhoneNumbersJson: {user.PhoneNumbersJson}");

        // Act
        var result = UserEntityPatchApplier.ApplyPatch(user, operations);

        _output.WriteLine($"ApplyPatch returned: {result}");
        _output.WriteLine($"Updated PhoneNumbersJson: {user.PhoneNumbersJson}");

        // Assert
        Assert.True(result, "ApplyPatch should return true");
        Assert.Contains("999-888-7777", user.PhoneNumbersJson);
    }

    [Fact]
    public void ApplyPatch_DirectArrayPath_ShouldUpdateEmail()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test2@example.com",
            EmailsJson = "[{\"value\":\"original2@example.com\",\"type\":\"work\",\"primary\":true}]",
            Active = true
        };

        var operations = new[]
        {
            new ScimPatchOperation
            {
                Op = "replace",
                Path = "emails[0].value",
                Value = "updated2@example.com"
            }
        };

        _output.WriteLine($"Original EmailsJson: {user.EmailsJson}");

        // Act
        var result = UserEntityPatchApplier.ApplyPatch(user, operations);

        _output.WriteLine($"ApplyPatch returned: {result}");
        _output.WriteLine($"Updated EmailsJson: {user.EmailsJson}");

        // Assert
        Assert.True(result, "ApplyPatch should return true");
        Assert.Contains("updated2@example.com", user.EmailsJson);
    }
}

