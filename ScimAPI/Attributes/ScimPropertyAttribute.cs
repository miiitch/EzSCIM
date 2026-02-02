namespace ScimAPI.Attributes
{
    /// <summary>
    /// Marks a property as part of a SCIM schema and defines its metadata.
    /// Only properties with this attribute will be included in the generated schema.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ScimPropertyAttribute : Attribute
    {
        /// <summary>
        /// The SCIM attribute name (e.g., "userName", "displayName").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The SCIM data type: "string", "boolean", "decimal", "integer", "dateTime", "reference", "complex", "binary".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Whether this attribute is required (default: false).
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Whether this attribute can have multiple values (default: false).
        /// </summary>
        public bool MultiValued { get; set; } = false;

        /// <summary>
        /// A human-readable description of the attribute.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Uniqueness constraint: "none", "server", "global" (default: "none").
        /// </summary>
        public string Uniqueness { get; set; } = "none";

        /// <summary>
        /// Mutability: "readOnly", "readWrite", "immutable", "writeOnly" (default: "readWrite").
        /// </summary>
        public string Mutability { get; set; } = "readWrite";

        /// <summary>
        /// When the attribute is returned: "always", "never", "default", "request" (default: "default").
        /// </summary>
        public string Returned { get; set; } = "default";

        /// <summary>
        /// Whether string comparison is case-sensitive (default: false).
        /// </summary>
        public bool CaseExact { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the ScimPropertyAttribute class.
        /// </summary>
        /// <param name="name">The SCIM attribute name</param>
        /// <param name="type">The SCIM data type</param>
        public ScimPropertyAttribute(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
