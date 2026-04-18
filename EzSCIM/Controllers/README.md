# EzSCIM Controllers

## Overview

The SCIM controllers have been moved into the EzSCIM library to allow reuse in any ASP.NET Core application.

## Available Controllers

### ScimUsersController
- **Route**: `scim/Users`
- **Features**:
  - `GET /scim/Users` - List users with filtering and pagination
  - `GET /scim/Users/{id}` - Retrieve a user by ID
  - `POST /scim/Users` - Create a new user
  - `PUT /scim/Users/{id}` - Replace an existing user
  - `PATCH /scim/Users/{id}` - Apply partial modifications
  - `DELETE /scim/Users/{id}` - Delete a user

### ScimGroupsController
- **Route**: `scim/Groups`
- **Features**:
  - `GET /scim/Groups` - List groups with filtering and pagination
  - `GET /scim/Groups/{id}` - Retrieve a group by ID
  - `POST /scim/Groups` - Create a new group
  - `PUT /scim/Groups/{id}` - Replace an existing group
  - `PATCH /scim/Groups/{id}` - Apply partial modifications
  - `DELETE /scim/Groups/{id}` - Delete a group

## Usage

### 1. Configuration in Program.cs

```csharp
using EzSCIM.Controllers;
using EzSCIM.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register the SCIM repository
builder.Services.AddSingleton<IScimRepository, YourScimRepositoryImplementation>();

// Add SCIM controllers
builder.Services.AddScimControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configure authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Your JWT configuration
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. AddScimControllers() Extension Method

The `AddScimControllers()` method:
- Automatically registers the SCIM controllers from the EzSCIM library
- Returns an `IMvcBuilder` to allow chaining with other configurations (e.g., `AddJsonOptions`)
- Loads controllers via `AddApplicationPart`

### 3. Example Requests

```bash
# List users
GET /scim/Users

# Filter users
GET /scim/Users?filter=userName eq "john.doe@example.com"&startIndex=1&count=10

# Create a user
POST /scim/Users
{
  "userName": "john.doe@example.com",
  "name": {
    "givenName": "John",
    "familyName": "Doe"
  },
  "emails": [{
    "value": "john.doe@example.com",
    "type": "work",
    "primary": true
  }]
}

# Replace a user
PUT /scim/Users/{id}
{
  "userName": "john.doe@example.com",
  "active": true
}

# Patch a user
PATCH /scim/Users/{id}
{
  "Operations": [{
    "op": "replace",
    "path": "active",
    "value": false
  }]
}

# Delete a user
DELETE /scim/Users/{id}
```

## Authentication

Controllers are decorated with `[Authorize]`, therefore:
- Authentication must be configured in your application
- All requests must include a valid authentication token
- Expected format: `Authorization: Bearer {token}`

## Response Format

Controllers return:
- **200 OK** - Successful operation
- **201 Created** - Resource created (POST)
- **204 No Content** - Successfully deleted (DELETE)
- **400 Bad Request** - Invalid filter or incorrect data
- **404 Not Found** - Resource not found
- **409 Conflict** - Resource already exists
- **500 Internal Server Error** - Internal error

All responses use the `application/scim+json` format.

## Migration from Demo Controllers

If you were using controllers in `EzSCIM.EntraID.Demo`:

1. Demo controllers are now obsolete and marked with `[Obsolete]`
2. They remain available at `scim/demo/Users` and `scim/demo/Groups` for backward compatibility
3. The new EzSCIM controllers are at `scim/Users` and `scim/Groups`
4. Update your clients to use the new routes

## Customization

To customize the controllers:

1. **Option 1**: Inherit from EzSCIM controllers
```csharp
public class CustomUsersController : ScimUsersController
{
    public CustomUsersController(IScimRepository repository, ILogger<CustomUsersController> logger)
        : base(repository, logger)
    {
    }
    
    // Add custom methods
}
```

2. **Option 2**: Create your own controllers and use `IScimRepository`
```csharp
[ApiController]
[Route("custom/users")]
public class MyCustomController : ControllerBase
{
    private readonly IScimRepository _repository;
    
    public MyCustomController(IScimRepository repository)
    {
        _repository = repository;
    }
    
    // Your custom methods
}
```

## Required Dependencies

Controllers require:
- `IScimRepository` - Must be registered in the DI container
- `ILogger<T>` - Automatically provided by ASP.NET Core
- ASP.NET Core 10.0 or higher

## Important Notes

1. Controllers use SCIM filtering via `FilterParser`
2. Pagination is handled with `startIndex` and `count`
3. Filter errors return a `ScimError` with status code 400
4. All controllers are thread-safe and can be used in multi-threaded environments

## See Also

- [SCIM Filter Documentation](../SCIM-FILTER-DOCUMENTATION.md)
- [Repository Guide](../REPOSITORY-ADAPTER-GUIDE.md)
- [Quick Start](../QUICKSTART.md)

