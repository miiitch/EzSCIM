﻿# Global Instructions for EzSCIM Repository

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

### Bug-First Testing Methodology (MANDATORY)

When the user reports an anomaly or bug:

1. **ALWAYS create a regression test FIRST** before writing any fix
2. The test must **reproduce the failure** (red) before the fix is applied
3. Place compliance/regression tests in `EzSCIM.IntegrationTests/ScimValidatorComplianceTests.cs`
4. Each test must document:
   - The validator test name (e.g., "Patch User - Replace Attributes")
   - Which validation runs were affected (e.g., runs 01, 02, 03, 04)
   - The exact error message from the validator
   - The root cause explanation
5. Test names should follow the pattern: `<Endpoint>_<Operation>_Should<ExpectedBehavior>`
6. After creating the test, verify it fails, then implement the fix, then verify it passes

### SCIM Validator Compliance

- Validation results from https://scimvalidator.microsoft.com/ are stored in `docs/scim-test-results/`
- Each validation run is saved as `scim-results-XX.json`
- For every failure reported by the validator, a corresponding integration test MUST exist
- Tests must verify both the PATCH response AND a subsequent GET (the validator checks persistence)

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
├── .github/                    # GitHub configuration and workflows
│   └── copilot-instructions.md
├── docs/                       # Documentation
│   ├── README.md              # Entry point → public/ or internal/
│   ├── public/                # NuGet package user documentation
│   │   ├── README.md          # Choose IQueryable or EF Core model
│   │   ├── authentication.md  # JWT setup (both models)
│   │   ├── iqueryable/        # Model 1: any data source via IQueryable
│   │   └── efcore/            # Model 2: EF Core / DbContext
│   ├── internal/              # Contributor documentation
│   │   ├── architecture.md    # Multi-provider DbContext, request flow
│   │   ├── development-setup.md
│   │   ├── testing.md         # Testcontainers, xUnit collections
│   │   ├── scim-validator.md  # Validator runs, known issues
│   │   └── issues/            # Known bugs
│   ├── scim-test-results/     # Raw SCIM validator JSON exports
│   └── archive/               # Historical/completed documentation
├── EzSCIM/                    # Core SCIM library (controllers, models, services)
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Filtering/
│   └── Repositories/
├── EzSCIM.EfCore/             # EF Core abstractions (EfScimRepositoryBase, IScimEntity)
├── EzSCIM.Demo.Data/          # Shared data layer (provider-agnostic)
│   ├── ScimDbContextBase.cs   # Base DbContext (no provider-specific config)
│   ├── DemoScimRepository.cs  # IScimRepository implementation
│   ├── DemoUserEntityExtensions.cs
│   ├── DemoGroupEntityExtensions.cs
│   ├── Entities/              # DemoUserEntity, DemoGroupEntity, helpers
│   └── Repositories/          # DemoUserGroupRepository (uses ScimDbContextBase)
├── EzSCIM.EntraID.Demo/       # Demo SCIM API service (SQL Server / Azure SQL)
│   ├── Data/DemoScimDbContext.cs  # SQL Server DbContext (nvarchar(max) columns)
│   ├── Migrations/            # EF Core migrations for SQL Server
│   └── Program.cs             # App startup with Aspire integration
├── EzSCIM.EntraID.AppHost/    # Aspire orchestration (SQL Server container)
├── EzSCIM.ServiceDefaults/    # Shared service configuration (health, telemetry)
├── EzSCIM.UnitTests/          # Unit tests (in-memory, no DB)
├── EzSCIM.IntegrationTests/   # Integration tests (PostgreSQL via Testcontainers)
│   ├── Data/PostgreSqlScimDbContext.cs  # PostgreSQL DbContext (jsonb columns)
│   ├── Data/SeedData.cs       # Test seed data
│   └── ScimWebApplicationFactory.cs
└── CHANGELOG.md               # Version history
```

## Documentation Organization

The `docs/` directory is split into two audiences:

- **`docs/public/`** — NuGet package user documentation (IQueryable path, EF Core path, authentication, SCIM attributes, schema extensions)
- **`docs/internal/`** — Contributor documentation (architecture, development setup, testing, SCIM validator)
- **`docs/archive/`** — Historical/completed documentation (not maintained)

### File Placement Rules

| Audience | Location |
|---|---|
| NuGet package users | `docs/public/iqueryable/` or `docs/public/efcore/` or `docs/public/` |
| Authentication (both models) | `docs/public/authentication.md` |
| Contributors / maintainers | `docs/internal/` |
| Known bugs | `docs/internal/issues/` |
| Historical / obsolete | `docs/archive/` |

### Creating New Documentation

When creating new Markdown files:

1. **Determine the audience** — package user (`docs/public/`) or contributor (`docs/internal/`)
2. **Use lowercase filenames with hyphens** (e.g., `my-new-guide.md`)
3. **Write in English only** (no French or other languages)
4. **Include a clear title/header** at the top
5. **Update `docs/README.md`** to include a reference to the new file
6. **Use relative paths** for internal links
7. **No content duplication** — consolidate related information instead of duplicating files

### Link Format Standards

```markdown
# ✅ Correct
See [EF Core setup](./efcore/getting-started.md)
See [Authentication](../authentication.md)

# ❌ Incorrect
See [Setup](/docs/public/efcore/getting-started.md)
```

### Documentation Quality Checklist

- [ ] File is in `docs/public/` (user-facing) or `docs/internal/` (contributor)
- [ ] Filename uses lowercase with hyphens
- [ ] Language is 100% English
- [ ] Includes clear title and structure
- [ ] All internal links are relative paths
- [ ] File is referenced in `docs/README.md`
- [ ] No duplicate content
- [ ] Follows Microsoft writing style (clear, concise, technical)

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

**Last Updated**: April 24, 2026  
**Version**: 1.3  
**Notable Changes**: 
- Added `EzSCIM.Demo.Data` shared library (provider-agnostic data layer)
- Added `EzSCIM.EfCore` abstraction library
- Multi-provider DbContext architecture (ScimDbContextBase → SQL Server / PostgreSQL)
- Integration tests now use PostgreSqlScimDbContext via Testcontainers
- Removed duplicated data layer from EzSCIM.IntegrationTests (~700 lines eliminated)
- Restructured `docs/` into `docs/public/` (NuGet users) and `docs/internal/` (contributors)
- Two integration paths documented: IQueryable and EF Core
- Authentication documented as a standalone section
- SCIM 2.0 attribute reference added as public doc
- Old `docs/status/`, `docs/auth/`, `docs/filters/` etc. archived
