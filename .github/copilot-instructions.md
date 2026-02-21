# Global Instructions for EzSCIM Repository

This document outlines the global coding and documentation standards for this repository.

## Language

All code, documentation, comments, and user-facing text in this repository **MUST** be written in **English**.

- **Code comments**: English only
- **Documentation**: English only
- **Commit messages**: English only
- **Issue descriptions**: English only
- **Variable/method names**: English only
- **Error messages**: English only

### Rationale

Using a single language (English) ensures:
- Global team collaboration is seamless
- Code is maintainable by international contributors
- Documentation is accessible to the wider open-source community
- Version control history is consistent and searchable

## Code Style Guidelines

### C# / .NET

- Follow Microsoft C# coding conventions
- Use `PascalCase` for class names and public members
- Use `camelCase` for local variables and private fields
- Use meaningful, English-only identifier names
- Add XML documentation comments (`///`) for public APIs

### Example

```csharp
/// <summary>
/// Validates a JWT token and returns the claims if valid
/// </summary>
/// <param name="token">The JWT token to validate</param>
/// <returns>Claims principal if valid, null otherwise</returns>
public ClaimsPrincipal? ValidateToken(string token)
{
    // Implementation...
}
```

### JSON / Configuration Files

- Use clear, descriptive keys in English
- Add comments explaining complex configurations
- Follow consistent indentation (2 or 4 spaces)

## Documentation Standards

### README Files

- Must be comprehensive and in English
- Include:
  - Project overview
  - Prerequisites
  - Installation instructions
  - Usage examples
  - Configuration options
  - Troubleshooting guide

### API Documentation

- Document all public endpoints
- Include request/response examples
- Explain authentication requirements
- Provide error codes and messages

### Technical Guides

- Use clear, professional English
- Include step-by-step instructions
- Provide examples and screenshots
- Add troubleshooting sections

## Commit Messages

Follow conventional commit format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

- **Type**: feat, fix, docs, style, refactor, test, chore
- **Scope**: Optional, describe the affected area
- **Subject**: Imperative, present tense, English only
- **Body**: Explain what and why, not how
- **Footer**: Reference issues (e.g., Closes #123)

### Example

```
feat(jwt-service): add token generation with custom expiration

Add support for custom token expiration duration to JWT token service.
Implement AddJwtTokenService extension method for DI registration.

Closes #45
```

## Pull Request Guidelines

- **Title**: Clear, concise, English description
- **Description**: Explain changes, testing performed
- **Labels**: Use appropriate labels
- **Review**: Request review from maintainers

## Issue Reporting

- **Title**: Concise English description
- **Description**: Include:
  - Steps to reproduce
  - Expected behavior
  - Actual behavior
  - Environment details
- **Labels**: Bug, enhancement, documentation, etc.

## Authentication & Security

- Never commit secrets, passwords, or API keys
- Use environment variables or configuration files (gitignored)
- Document secure practices in README
- Follow Microsoft security best practices

## Testing

- Write tests in English (method names, assertions)
- Include comments explaining complex test logic
- Use descriptive assertion messages

### Example

```csharp
[Fact]
public void GenerateToken_ReturnsValidJwt_WhenCalled()
{
    // Arrange
    var service = new JwtTokenService(_configuration, _logger);

    // Act
    var token = service.GenerateToken(expirationMinutes: 60);

    // Assert
    Assert.NotNull(token);
    Assert.NotEmpty(token);
}
```

## Dependencies

- Document all external dependencies
- Keep dependencies up to date
- Review security advisories regularly
- Use semantic versioning

## Logging

- Use English log messages
- Include appropriate log levels (Debug, Info, Warning, Error)
- Include context in log messages for debugging

### Example

```csharp
_logger.LogInformation("User {UserId} created successfully", userId);
_logger.LogWarning("Token validation failed for user {UserId}", userId);
_logger.LogError(ex, "Unexpected error processing request {RequestId}", requestId);
```

## CI/CD Pipeline

- All workflows must have English descriptions
- Document build/test/deploy steps
- Include meaningful error messages
- Log output should be in English

## Repository Structure

```
scimwork/
├── .github/               # GitHub configuration and workflows
│   └── copilot-instructions.md
├── EzSCIM/                # Core SCIM library
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   └── Repositories/
├── EzSCIM.EntraID.Demo/   # Demo SCIM API service
├── EzSCIM.EntraID.AppHost/ # Aspire orchestration
├── EzSCIM.ServiceDefaults/ # Shared service configuration
├── EzSCIM.UnitTests/      # Unit tests
├── EzSCIM.IntegrationTests/ # Integration tests
├── Documentation files    # Guides and specifications
└── README.md             # Main project documentation
```

## Breaking Changes

- Document breaking changes in CHANGELOG.md
- Include migration guides for significant changes
- Use version bumping appropriately (semantic versioning)

## Performance Considerations

- Document performance implications of changes
- Include benchmarks if relevant
- Log performance warnings if applicable

## Accessibility

- Use clear, inclusive language in documentation
- Avoid jargon without explanation
- Provide examples for complex concepts

---

**Last Updated**: February 20, 2026  
**Version**: 1.0

