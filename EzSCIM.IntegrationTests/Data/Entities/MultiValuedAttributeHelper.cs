using System.Text.Json;

namespace EzSCIM.IntegrationTests.Data.Entities;

/// <summary>
/// Simple POCO for storing email data in JSON
/// </summary>
public class EmailData
{
    public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Primary { get; set; }
}

/// <summary>
/// Simple POCO for storing phone number data in JSON
/// </summary>
public class PhoneNumberData
{
    public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool Primary { get; set; }
}

/// <summary>
/// Simple POCO for storing address data in JSON
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
/// Helper methods for serializing/deserializing multi-valued attributes
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
        if (string.IsNullOrWhiteSpace(json))
            return new List<EmailData>();

        try
        {
            return JsonSerializer.Deserialize<List<EmailData>>(json, JsonOptions) ?? new List<EmailData>();
        }
        catch
        {
            return new List<EmailData>();
        }
    }

    public static string SerializeEmails(List<EmailData> emails)
    {
        if (emails == null || emails.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(emails, JsonOptions);
    }

    public static List<PhoneNumberData> DeserializePhones(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<PhoneNumberData>();

        try
        {
            return JsonSerializer.Deserialize<List<PhoneNumberData>>(json, JsonOptions) ?? new List<PhoneNumberData>();
        }
        catch
        {
            return new List<PhoneNumberData>();
        }
    }

    public static string SerializePhones(List<PhoneNumberData> phones)
    {
        if (phones == null || phones.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(phones, JsonOptions);
    }

    public static List<AddressData> DeserializeAddresses(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<AddressData>();

        try
        {
            return JsonSerializer.Deserialize<List<AddressData>>(json, JsonOptions) ?? new List<AddressData>();
        }
        catch
        {
            return new List<AddressData>();
        }
    }

    public static string SerializeAddresses(List<AddressData> addresses)
    {
        if (addresses == null || addresses.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(addresses, JsonOptions);
    }
}

