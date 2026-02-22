using System;
using EzSCIM.IntegrationTests;
using EzSCIM.IntegrationTests.Data.Entities;
using EzSCIM.Models;
using System.Text.Json;

// Simple debug test for ScimPatchApplier
var user = new UserEntity
{
    Id = Guid.NewGuid().ToString(),
    UserName = "test@example.com",
    Email = "original@example.com",
    PhoneNumber = "111-222-3333",
    AddressFormatted = "Original Address"
};

Console.WriteLine($"Original Email: {user.Email}");
Console.WriteLine($"Original Phone: {user.PhoneNumber}");
Console.WriteLine($"Original Address: {user.AddressFormatted}");

var patchOps = new[]
{
    new ScimPatchOperation
    {
        Op = "replace",
        Path = "emails[primary eq true].value",
        Value = "newemail@test.com"
    },
    new ScimPatchOperation
    {
        Op = "replace",
        Path = "phoneNumbers[primary eq true].value",
        Value = "999-888-7777"
    },
    new ScimPatchOperation
    {
        Op = "replace",
        Path = "addresses[0].formatted",
        Value = "New Address"
    }
};

var modified = ScimPatchApplier.ApplyPatch(user, patchOps);

Console.WriteLine($"\nModified: {modified}");
Console.WriteLine($"New Email: {user.Email}");
Console.WriteLine($"New Phone: {user.PhoneNumber}");
Console.WriteLine($"New Address: {user.AddressFormatted}");

