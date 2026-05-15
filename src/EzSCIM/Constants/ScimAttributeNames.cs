namespace EzSCIM.Constants
{
    /// <summary>
    /// Constants for SCIM attribute names.
    /// Use these instead of magic strings for type safety and refactoring support.
    /// </summary>
    public static class ScimAttributeNames
    {
        /// <summary>
        /// Common attributes for all SCIM resources
        /// </summary>
        public static class Common
        {
            public const string Id = "id";
            public const string ExternalId = "externalId";
            public const string Meta = "meta";
            public const string Schemas = "schemas";
        }

        /// <summary>
        /// User-specific SCIM attributes (urn:ietf:params:scim:schemas:core:2.0:User)
        /// </summary>
        public static class User
        {
            // Required
            public const string UserName = "userName";

            // Name attributes
            public const string Name = "name";
            public const string NameFormatted = "name.formatted";
            public const string NameFamilyName = "name.familyName";
            public const string NameGivenName = "name.givenName";
            public const string NameMiddleName = "name.middleName";
            public const string NameHonorificPrefix = "name.honorificPrefix";
            public const string NameHonorificSuffix = "name.honorificSuffix";

            // Basic attributes
            public const string DisplayName = "displayName";
            public const string NickName = "nickName";
            public const string ProfileUrl = "profileUrl";
            public const string Title = "title";
            public const string UserType = "userType";
            public const string PreferredLanguage = "preferredLanguage";
            public const string Locale = "locale";
            public const string Timezone = "timezone";
            public const string Active = "active";
            public const string Password = "password";

            // Multi-valued attributes
            public const string Emails = "emails";
            public const string EmailsValue = "emails.value";
            public const string EmailsType = "emails.type";
            public const string EmailsPrimary = "emails.primary";

            public const string PhoneNumbers = "phoneNumbers";
            public const string PhoneNumbersValue = "phoneNumbers.value";
            public const string PhoneNumbersType = "phoneNumbers.type";
            public const string PhoneNumbersPrimary = "phoneNumbers.primary";

            public const string Ims = "ims";
            public const string ImsValue = "ims.value";
            public const string ImsType = "ims.type";
            public const string ImsPrimary = "ims.primary";

            public const string Photos = "photos";
            public const string PhotosValue = "photos.value";
            public const string PhotosType = "photos.type";
            public const string PhotosPrimary = "photos.primary";

            public const string Addresses = "addresses";
            public const string AddressesFormatted = "addresses.formatted";
            public const string AddressesStreetAddress = "addresses.streetAddress";
            public const string AddressesLocality = "addresses.locality";
            public const string AddressesRegion = "addresses.region";
            public const string AddressesPostalCode = "addresses.postalCode";
            public const string AddressesCountry = "addresses.country";
            public const string AddressesType = "addresses.type";
            public const string AddressesPrimary = "addresses.primary";

            public const string Groups = "groups";
            public const string GroupsValue = "groups.value";
            public const string GroupsRef = "groups.$ref";
            public const string GroupsDisplay = "groups.display";
            public const string GroupsType = "groups.type";

            public const string Entitlements = "entitlements";
            public const string Roles = "roles";
            public const string X509Certificates = "x509Certificates";
        }

        /// <summary>
        /// Group-specific SCIM attributes (urn:ietf:params:scim:schemas:core:2.0:Group)
        /// </summary>
        public static class Group
        {
            // Required
            public const string DisplayName = "displayName";

            // Members
            public const string Members = "members";
            public const string MembersValue = "members.value";
            public const string MembersRef = "members.$ref";
            public const string MembersDisplay = "members.display";
            public const string MembersType = "members.type";
        }

        /// <summary>
        /// Enterprise User extension attributes (urn:ietf:params:scim:schemas:extension:enterprise:2.0:User)
        /// </summary>
        public static class EnterpriseUser
        {
            public const string EmployeeNumber = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:employeeNumber";
            public const string CostCenter = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:costCenter";
            public const string Organization = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:organization";
            public const string Division = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:division";
            public const string Department = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:department";
            public const string Manager = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:manager";
            public const string ManagerValue = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:manager.value";
            public const string ManagerRef = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:manager.$ref";
            public const string ManagerDisplayName = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User:manager.displayName";

            // Short versions (for use in filters)
            public const string EmployeeNumberShort = "employeeNumber";
            public const string CostCenterShort = "costCenter";
            public const string OrganizationShort = "organization";
            public const string DivisionShort = "division";
            public const string DepartmentShort = "department";
            public const string ManagerShort = "manager";
        }

        /// <summary>
        /// Filter operators
        /// </summary>
        public static class Operators
        {
            public const string Equals = "eq";
            public const string NotEquals = "ne";
            public const string Contains = "co";
            public const string StartsWith = "sw";
            public const string EndsWith = "ew";
            public const string Present = "pr";
            public const string GreaterThan = "gt";
            public const string GreaterThanOrEqual = "ge";
            public const string LessThan = "lt";
            public const string LessThanOrEqual = "le";
            public const string And = "and";
            public const string Or = "or";
            public const string Not = "not";
        }
    }
}

