using EzSCIM.Models;
using System.Text.Json;

namespace EzSCIM.IntegrationTests.Data;

/// <summary>
/// Extension methods for converting UserEntity with JSON columns to ScimUser
/// </summary>
public static class UserEntityExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Converts UserEntity (with JSON columns) to ScimUser
    /// </summary>
    public static ScimUser ToScimUser(this Entities.UserEntity user)
    {
        var scimUser = new ScimUser
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            NickName = user.NickName,
            ProfileUrl = user.ProfileUrl,
            Title = user.Title,
            UserType = user.UserType,
            PreferredLanguage = user.PreferredLanguage,
            Locale = user.Locale,
            Timezone = user.Timezone,
            Active = user.Active,
            ExternalId = user.ExternalId
        };

        // Map Name
        if (!string.IsNullOrEmpty(user.NameFormatted) || 
            !string.IsNullOrEmpty(user.FirstName) || 
            !string.IsNullOrEmpty(user.LastName))
        {
            scimUser.Name = new ScimName
            {
                Formatted = user.NameFormatted,
                GivenName = user.FirstName,
                FamilyName = user.LastName,
                MiddleName = user.NameMiddleName,
                HonorificPrefix = user.NameHonorificPrefix,
                HonorificSuffix = user.NameHonorificSuffix
            };
        }

        // Map Emails from JSON
        if (!string.IsNullOrWhiteSpace(user.EmailsJson))
        {
            try
            {
                var emails = JsonSerializer.Deserialize<List<Entities.EmailData>>(user.EmailsJson, JsonOptions);
                if (emails != null && emails.Count > 0)
                {
                    scimUser.Emails = emails.Select(e => new ScimEmail
                    {
                        Value = e.Value,
                        Type = e.Type,
                        Primary = e.Primary
                    }).ToList();
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        // Map PhoneNumbers from JSON
        if (!string.IsNullOrWhiteSpace(user.PhoneNumbersJson))
        {
            try
            {
                var phones = JsonSerializer.Deserialize<List<Entities.PhoneNumberData>>(user.PhoneNumbersJson, JsonOptions);
                if (phones != null && phones.Count > 0)
                {
                    scimUser.PhoneNumbers = phones.Select(p => new ScimPhoneNumber
                    {
                        Value = p.Value,
                        Type = p.Type,
                        Primary = p.Primary
                    }).ToList();
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        // Map Addresses from JSON
        if (!string.IsNullOrWhiteSpace(user.AddressesJson))
        {
            try
            {
                var addresses = JsonSerializer.Deserialize<List<Entities.AddressData>>(user.AddressesJson, JsonOptions);
                if (addresses != null && addresses.Count > 0)
                {
                    scimUser.Addresses = addresses.Select(a => new ScimAddress
                    {
                        Formatted = a.Formatted,
                        StreetAddress = a.StreetAddress,
                        Locality = a.Locality,
                        Region = a.Region,
                        PostalCode = a.PostalCode,
                        Country = a.Country,
                        Type = a.Type,
                        Primary = a.Primary
                    }).ToList();
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        // Set metadata
        scimUser.Meta = new ScimMeta
        {
            ResourceType = "User",
            Created = user.CreatedAt,
            LastModified = user.ModifiedAt,
            Location = $"/scim/Users/{user.Id}"
        };

        return scimUser;
    }

    /// <summary>
    /// Updates UserEntity from ScimUser (for CREATE/UPDATE operations)
    /// </summary>
    public static void UpdateFromScimUser(this Entities.UserEntity entity, ScimUser scimUser)
    {
        entity.UserName = scimUser.UserName;
        entity.DisplayName = scimUser.DisplayName;
        entity.NickName = scimUser.NickName;
        entity.ProfileUrl = scimUser.ProfileUrl;
        entity.Title = scimUser.Title;
        entity.UserType = scimUser.UserType;
        entity.PreferredLanguage = scimUser.PreferredLanguage;
        entity.Locale = scimUser.Locale;
        entity.Timezone = scimUser.Timezone;
        entity.Active = scimUser.Active;
        entity.ExternalId = scimUser.ExternalId;

        // Map Name
        if (scimUser.Name != null)
        {
            entity.NameFormatted = scimUser.Name.Formatted;
            entity.FirstName = scimUser.Name.GivenName;
            entity.LastName = scimUser.Name.FamilyName;
            entity.NameMiddleName = scimUser.Name.MiddleName;
            entity.NameHonorificPrefix = scimUser.Name.HonorificPrefix;
            entity.NameHonorificSuffix = scimUser.Name.HonorificSuffix;
        }

        // Map Emails to JSON
        if (scimUser.Emails != null && scimUser.Emails.Count > 0)
        {
            var emailsData = scimUser.Emails.Select(e => new Entities.EmailData
            {
                Value = e.Value,
                Type = e.Type,
                Primary = e.Primary
            }).ToList();
            entity.EmailsJson = JsonSerializer.Serialize(emailsData, JsonOptions);
        }
        else
        {
            entity.EmailsJson = "[]";
        }

        // Map PhoneNumbers to JSON
        if (scimUser.PhoneNumbers != null && scimUser.PhoneNumbers.Count > 0)
        {
            var phonesData = scimUser.PhoneNumbers.Select(p => new Entities.PhoneNumberData
            {
                Value = p.Value,
                Type = p.Type,
                Primary = p.Primary
            }).ToList();
            entity.PhoneNumbersJson = JsonSerializer.Serialize(phonesData, JsonOptions);
        }
        else
        {
            entity.PhoneNumbersJson = "[]";
        }

        // Map Addresses to JSON
        if (scimUser.Addresses != null && scimUser.Addresses.Count > 0)
        {
            var addressesData = scimUser.Addresses.Select(a => new Entities.AddressData
            {
                Formatted = a.Formatted,
                StreetAddress = a.StreetAddress,
                Locality = a.Locality,
                Region = a.Region,
                PostalCode = a.PostalCode,
                Country = a.Country,
                Type = a.Type,
                Primary = a.Primary
            }).ToList();
            entity.AddressesJson = JsonSerializer.Serialize(addressesData, JsonOptions);
        }
        else
        {
            entity.AddressesJson = "[]";
        }

        entity.ModifiedAt = DateTime.UtcNow;
    }
}

