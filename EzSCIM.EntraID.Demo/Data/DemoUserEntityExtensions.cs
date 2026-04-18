using EzSCIM.EntraID.Demo.Data.Entities;
using EzSCIM.Models;

namespace EzSCIM.EntraID.Demo.Data;

/// <summary>
/// Extension methods to convert DemoUserEntity ↔ ScimUser.
/// Handles JSON multi-valued attributes (emails, phones, addresses).
/// </summary>
public static class DemoUserEntityExtensions
{
    /// <summary>
    /// Converts a <see cref="DemoUserEntity"/> to a <see cref="ScimUser"/>.
    /// </summary>
    public static ScimUser ToScimUser(this DemoUserEntity user)
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

        // Map name sub-attributes
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

        // Map emails from JSON
        var emails = MultiValuedAttributeHelper.DeserializeEmails(user.EmailsJson);
        if (emails.Count > 0)
        {
            scimUser.Emails = emails.Select(e => new ScimEmail
            {
                Value = e.Value,
                Type = e.Type,
                Primary = e.Primary
            }).ToList();
        }

        // Map phone numbers from JSON
        var phones = MultiValuedAttributeHelper.DeserializePhones(user.PhoneNumbersJson);
        if (phones.Count > 0)
        {
            scimUser.PhoneNumbers = phones.Select(p => new ScimPhoneNumber
            {
                Value = p.Value,
                Type = p.Type,
                Primary = p.Primary
            }).ToList();
        }

        // Map addresses from JSON
        var addresses = MultiValuedAttributeHelper.DeserializeAddresses(user.AddressesJson);
        if (addresses.Count > 0)
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
    /// Updates a <see cref="DemoUserEntity"/> from a <see cref="ScimUser"/>.
    /// </summary>
    public static void UpdateFromScimUser(this DemoUserEntity entity, ScimUser scimUser)
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

        if (scimUser.Name != null)
        {
            entity.NameFormatted = scimUser.Name.Formatted;
            entity.FirstName = scimUser.Name.GivenName;
            entity.LastName = scimUser.Name.FamilyName;
            entity.NameMiddleName = scimUser.Name.MiddleName;
            entity.NameHonorificPrefix = scimUser.Name.HonorificPrefix;
            entity.NameHonorificSuffix = scimUser.Name.HonorificSuffix;
        }
        else
        {
            entity.NameFormatted = null;
            entity.FirstName = null;
            entity.LastName = null;
            entity.NameMiddleName = null;
            entity.NameHonorificPrefix = null;
            entity.NameHonorificSuffix = null;
        }

        entity.EmailsJson = MultiValuedAttributeHelper.SerializeEmails(
            scimUser.Emails?.Select(e => new EmailData { Value = e.Value, Type = e.Type, Primary = e.Primary }).ToList() ?? []);

        entity.PhoneNumbersJson = MultiValuedAttributeHelper.SerializePhones(
            scimUser.PhoneNumbers?.Select(p => new PhoneNumberData { Value = p.Value, Type = p.Type, Primary = p.Primary }).ToList() ?? []);

        entity.AddressesJson = MultiValuedAttributeHelper.SerializeAddresses(
            scimUser.Addresses?.Select(a => new AddressData
            {
                Formatted = a.Formatted,
                StreetAddress = a.StreetAddress,
                Locality = a.Locality,
                Region = a.Region,
                PostalCode = a.PostalCode,
                Country = a.Country,
                Type = a.Type,
                Primary = a.Primary
            }).ToList() ?? []);

        entity.ModifiedAt = DateTime.UtcNow;
    }
}

