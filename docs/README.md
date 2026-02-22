﻿# Documentation Index

This documentation is organized by topic and purpose for easy navigation.

## 🎯 Quick Navigation

### New to the Project?
Start here:
1. **[Quick Start Guide](./guides/quickstart.md)** - Get up and running in 5 minutes
2. **[Authentication Setup](./auth/setup.md)** - Configure JWT authentication
3. **[Development Setup](./guides/development-setup.md)** - Set up your development environment

### Looking for Specific Topics?

- **🔐 Authentication** → See [docs/auth/](./auth/README.md)
- **🔍 Filtering** → See [docs/filters/](./filters/README.md)
- **📦 Repository Integration** → See [docs/migration/](./migration/README.md)
- **📋 Schema System** → See [docs/schema/](./schema/README.md)
- **🧪 Testing** → See [docs/tests/](./tests/README.md)
- **📊 Status Reports** → See [docs/status/](./status/README.md)

---

## 📚 Directory Structure

```
docs/
├── README.md                    # This file
├── auth/                        # Authentication & Security
│   ├── setup.md                # JWT authentication setup guide
│   ├── index.md                # Authentication reference
│   ├── jwt-service-quick-fix.md
│   ├── jwt-di-resolution.md
│   └── pre-production-checklist.md
├── filters/                     # SCIM Filtering
│   ├── overview.md             # Filter system overview
│   ├── reference.md            # Complete filter reference
│   ├── parser.md               # Parser implementation
│   ├── implementation-guide.md # Adding new filters
│   ├── examples.md             # Usage examples
│   ├── value-types.md          # Supported value types
│   ├── nested-filters.md       # Complex filter expressions
│   ├── error-handling.md       # Error handling in filters
│   └── url-encoding.md         # Filter URL encoding
├── guides/                      # General Guides
│   ├── quickstart.md           # 5-minute quick start
│   ├── development-setup.md    # Development environment setup
│   ├── powershell-scripts.md   # Available PowerShell scripts
│   ├── provider-modes.md       # Provider mode selection
│   ├── useful-commands.md      # Useful commands reference
│   ├── visual-separation.md    # UI/UX considerations
│   ├── next-tasks.md           # Recommended next steps
│   └── next-tasks-checklist.md # Task checklist
├── migration/                   # Repository Integration & Migration
│   ├── quick-start-repository.md # 15-minute repository integration
│   ├── repository-mapping-overview.md
│   ├── repository-mapping-index.md
│   ├── repository-adapter-guide.md
│   ├── mapping-readme.md
│   ├── groups-and-constants-extension.md
│   └── interface-separation.md
├── schema/                      # Schema System
│   ├── system-overview.md      # Schema system overview
│   ├── extension-guide.md      # Creating schema extensions
│   ├── models-required-optional.md
│   └── expected-actual-pattern.md
├── tests/                       # Testing Documentation
│   ├── test-suite-update.md    # Test suite updates
│   ├── filter-tests.md         # Filter testing documentation
│   ├── filter-error-tests.md   # Error testing for filters
│   ├── base-classes-tests.md   # Base class testing
│   ├── base-classes-summary.md
│   └── entra-integration.md    # Entra ID integration tests
├── status/                      # Status Reports
│   ├── implementation-status.md # Current implementation status
│   ├── migration-summary.md    # Migration summary
│   ├── migration-complete.md   # Migration completion report
│   ├── completion-report.md    # Overall completion report
│   ├── session-summary.md      # Session completion summary
│   ├── today-summary.md        # Today's work summary
│   ├── tests-summary.md        # Test suite summary
│   ├── scim-run06-patch-error-analysis.md  # SCIM Validator Run 06 analysis
│   ├── scim-run06-tests-implementation.md  # Run 06 test implementation
│   ├── jwt-extension-delivery.md
│   ├── delivery-manifest.md    # Project delivery manifest
│   └── phase-4-completion.md   # Phase 4 completion status
└── archive/                     # Archive (Completed/Historical Documents)
    └── [Historical documentation files]
```

---

## 📖 Documentation Naming Convention

All new documentation files follow the naming pattern: `<topic>-<context>.md`

Examples:
- `setup.md` - Setup/installation guides
- `reference.md` - Reference documentation
- `examples.md` - Usage examples
- `implementation-guide.md` - Implementation details
- `quick-*.md` - Quick start guides

### File Organization Rule

```
docs/<theme>/<topic>.md
```

Where:
- `<theme>`: auth, filters, guides, migration, schema, tests, status, archive
- `<topic>`: Descriptive name in lowercase with hyphens

**Examples:**
- `docs/auth/setup.md`
- `docs/filters/implementation-guide.md`
- `docs/migration/quick-start-repository.md`

---

## 🚀 Key Starting Points by Role

### For Developers
1. [Quick Start Guide](./guides/quickstart.md)
2. [Development Setup](./guides/development-setup.md)
3. [Filter Examples](./filters/examples.md)
4. [Repository Integration](./migration/quick-start-repository.md)

### For DevOps/Operations
1. [Authentication Setup](./auth/setup.md)
2. [Pre-Production Checklist](./auth/pre-production-checklist.md)
3. [PowerShell Scripts](./guides/powershell-scripts.md)

### For System Architects
1. [Implementation Status](./status/implementation-status.md)
2. [Repository Mapping Overview](./migration/repository-mapping-overview.md)
3. [Schema System Overview](./schema/system-overview.md)
4. [Interface Separation](./migration/interface-separation.md)

### For QA/Testing
1. [Test Suite Documentation](./tests/test-suite-update.md)
2. [Filter Testing](./tests/filter-tests.md)
3. [Entra Integration Testing](./tests/entra-integration.md)

---

## 📋 Status & Completion

### Implementation Status
- See [docs/status/implementation-status.md](./status/implementation-status.md)

### Recent Changes
- See [docs/status/today-summary.md](./status/today-summary.md)

### Migration Progress
- See [docs/status/migration-summary.md](./status/migration-summary.md)

---

## 🔧 Contributing

When creating new documentation:

1. **Location**: Place files in appropriate `docs/<theme>/` folder
2. **Language**: Write all documentation in English
3. **Format**: Use Markdown (.md) format
4. **Naming**: Use lowercase with hyphens (e.g., `my-guide.md`)
5. **Links**: Use relative paths (e.g., `./guides/quickstart.md`)
6. **Update Index**: Add new files to this README and relevant theme index

### Documentation Quality Standards

- [ ] Clear, concise English language
- [ ] Include examples when relevant
- [ ] Add step-by-step instructions for guides
- [ ] Include links to related documentation
- [ ] Update table of contents
- [ ] No French language content
- [ ] Consistent formatting and structure

---

## 🔄 Migrating from Root Level

The following documents have been moved from the repository root to `docs/` with updated filenames:

| Old Location | New Location |
|---|---|
| `QUICKSTART.md` | `docs/guides/quickstart.md` |
| `AUTHENTICATION_SETUP.md` | `docs/auth/setup.md` |
| `AUTH_INDEX.md` | `docs/auth/index.md` |
| `DEVELOPMENT_INSTRUCTIONS.md` | `docs/guides/development-setup.md` |
| And many more... | See status report |

See [docs/status/migration-summary.md](./status/migration-summary.md) for complete migration details.

---

## 📞 Quick Links

- **Main README** → [README.md](../../README.md)
- **Changelog** → [CHANGELOG.md](../../CHANGELOG.md)
- **Start Here** → [START_HERE.md](../../START_HERE.md)

---

**Last Updated**: February 21, 2026  
**Convention Version**: 1.0  
**Language**: English only

