namespace EzSCIM.Services
{
    /// <summary>
    /// Controls whether the token generation endpoint is enabled.
    /// </summary>
    public interface ITokenEndpointFeature
    {
        /// <summary>
        /// Gets a value indicating whether the token endpoint is enabled.
        /// </summary>
        bool IsEnabled { get; }
    }

    /// <summary>
    /// Default implementation for token endpoint enabling.
    /// </summary>
    public sealed class TokenEndpointFeature : ITokenEndpointFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenEndpointFeature"/> class.
        /// </summary>
        /// <param name="enabled">Whether the endpoint is enabled.</param>
        public TokenEndpointFeature(bool enabled)
        {
            IsEnabled = enabled;
        }

        /// <inheritdoc />
        public bool IsEnabled { get; }
    }
}

