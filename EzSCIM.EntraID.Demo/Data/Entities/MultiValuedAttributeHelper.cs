using System.Text.Json;

namespace EzSCIM.EntraID.Demo.Data.Entities;

/// <summary>
/// Simple POCO for storing email data as JSON in SQL Server.
/// </summary>
public class EmailData
{
    public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Primary { get; set; }
}

/// <summary>
/// Simple POCO for storing phone number data as JSON in SQL Server.
/// </summary>
public class PhoneNumberData
{
    public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Primary { get; set; }
}

/// <summary>
/// Simple POCO for storing address data as JSON in SQL Server.
/// </summary>
public class AddressData
{
    public string? Formatted { get; set; }
    public string? StreetAddress { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Type { get; set; }
    public bool Primary { get; set; }
}

/// <summary>
/// Simple POCO for storing group member data as JSON in SQL Server.
/// </summary>
public class MemberData
{
    public string Value { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
}

/// <summary>
/// Helper methods for serializing/deserializing multi-valued SCIM attributes stored as JSON.
/// </summary>
public static class MultiValuedAttributeHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static List<EmailData> DeserializeEmails(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<EmailData>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    public static string SerializeEmails(List<EmailData> emails)
        => (emails is { Count: > 0 }) ? JsonSerializer.Serialize(emails, JsonOptions) : "[]";

    public static List<PhoneNumberData> DeserializePhones(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<PhoneNumberData>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    public static string SerializePhones(List<PhoneNumberData> phones)
        => (phones is { Count: > 0 }) ? JsonSerializer.Serialize(phones, JsonOptions) : "[]";

    public static List<AddressData> DeserializeAddresses(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<AddressData>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    public static string SerializeAddresses(List<AddressData> addresses)
        => (addresses is { Count: > 0 }) ? JsonSerializer.Serialize(addresses, JsonOptions) : "[]";

    public static List<MemberData> DeserializeMembers(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<MemberData>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    public static string SerializeMembers(List<MemberData> members)
        => (members is { Count: > 0 }) ? JsonSerializer.Serialize(members, JsonOptions) : "[]";
}

