namespace EzSCIM.Attributes
{
    /// <summary>
    /// Marks a class as a SCIM resource and defines its schema metadata.
    /// Used to generate SCIM schema definitions automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ScimResourceAttribute : Attribute
    {
        /// <summary>
        /// The SCIM schema URN (e.g., "urn:ietf:params:scim:schemas:core:2.0:User").
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// The name of the resource (e.g., "User", "Group").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A human-readable description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the ScimResourceAttribute class.
        /// </summary>
        /// <param name="schema">The SCIM schema URN</param>
        /// <param name="name">The resource name</param>
        /// <param name="description">The resource description</param>
        public ScimResourceAttribute(string schema, string name, string description)
        {
            Schema = schema;
            Name = name;
            Description = description;
        }
    }
}

