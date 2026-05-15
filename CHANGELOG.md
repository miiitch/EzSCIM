# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.1] - 2024-04-24

### Added
- `EzSCIM` NuGet package — SCIM 2.0 server implementation for ASP.NET Core
- `EzSCIM.EfCore` NuGet package — Entity Framework Core base repository
- Two integration models: IQueryable (any data source) and EF Core / DbContext
- SCIM 2.0 Users and Groups endpoints (GET, POST, PUT, PATCH, DELETE)
- SCIM filter translation to LINQ expressions via `GenericScimFilterTranslator`
- Schema extensions via `[ScimProperty]` attribute
- JWT Bearer token authentication handler
- `EfScimRepositoryBase<TUser, TGroup, TContext>` with automatic CRUD, timestamps, and unique-constraint detection
- `IScimEntity` interface for EF Core entities
- Multi-provider DbContext support (SQL Server and PostgreSQL)
- Microsoft Entra ID provisioning compatibility
- ASP.NET Core Aspire demo application
- Integration tests using Testcontainers (PostgreSQL)
- Unit tests

[Unreleased]: https://github.com/miiitch/EzSCIM/compare/v0.0.1...HEAD
[0.0.1]: https://github.com/miiitch/EzSCIM/releases/tag/v0.0.1
