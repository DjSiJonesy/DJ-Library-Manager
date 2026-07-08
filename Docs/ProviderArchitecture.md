# Provider Architecture

## Overview

DJ Library Manager (DJLM) is built around a provider-independent architecture.

Each supported DJ application is implemented as an isolated provider module responsible for reading and writing its native database format.

The remainder of the application (Library, Analysis, Recovery and Dashboard) operates exclusively on the common `DJLMMediaItem` model and has no knowledge of provider-specific storage technologies.

This separation allows new providers to be added without modifying the core application.

---

# Architecture

```
                   DJ Library Manager

                           │
                  Provider Selection
                           │
      ┌────────────────────┼────────────────────┐
      ▼                    ▼                    ▼
 VirtualDJ           Rekordbox             Future Providers
    XML              SQLCipher SQLite
      │                    │
      └────────────────────┴────────────────────┐
                                                │
                                   Provider Translation
                                                │
                                                ▼
                                      DJLMMediaItem[]
                                                │
         ┌──────────────────┬───────────────────┬──────────────────┐
         ▼                  ▼                   ▼                  ▼
      Library           Analysis            Recovery          Dashboard
```

---

# Design Goals

The provider architecture has been designed to:

- Support multiple DJ applications.
- Isolate provider-specific implementations.
- Provide a common public API.
- Return a common media model.
- Minimise duplicated logic.
- Hide storage implementation details.
- Allow providers to evolve independently.

---

# Supported Providers

| Provider | Status | Storage |
|----------|--------|---------|
| VirtualDJ | ✅ Implemented | XML |
| Rekordbox | ✅ Implemented | SQLCipher (SQLite) |
| Serato | 🚧 Planned | Database / Crates |
| Engine DJ | 🚧 Planned | SQLite |
| Traktor | 🚧 Planned | XML |

---

# Public Provider Contract

Every provider should expose the same public interface.

| Function | Purpose |
|----------|---------|
| Import-<Provider>Database | Opens the provider database. |
| Get-<Provider>MediaItems | Returns `DJLMMediaItem` objects. |
| Get-<Provider>Statistics | Returns provider statistics. |
| Save-<Provider>Database | Persists provider changes. |
| Update-<Provider>MediaPaths | Updates media locations within the provider database. |

Optional developer utilities may also be provided.

Examples include:

- Get-<Provider>Tables
- Get-<Provider>Schema

These utilities are intended for development and diagnostics rather than normal application use.

---

# Provider Responsibilities

Providers are responsible for:

- Opening databases.
- Reading provider records.
- Writing provider changes.
- Translating provider records into `DJLMMediaItem` objects.
- Updating media paths.
- Managing provider-specific resources.
- Logging provider operations.

Providers are **not** responsible for:

- Library scanning.
- Duplicate detection.
- Library analysis.
- Recovery decisions.
- Dashboard presentation.
- User interaction.

---

# Private Provider Functions

Private helper functions encapsulate provider-specific implementation details.

Examples:

### VirtualDJ

- ConvertTo-DJLMMediaItem
- Get-XmlAttribute
- Get-XmlChildNode

### Rekordbox

- ConvertTo-DJLMMediaItem
- Open-RekordboxDatabase
- Invoke-RekordboxQuery

Private functions are never exported from the provider module.

---

# Provider Processing Pipeline

Every provider follows the same logical workflow.

```
Import Database
        │
        ▼
Provider Database Object
        │
        ▼
Read Provider Records
        │
        ▼
ConvertTo-DJLMMediaItem
        │
        ▼
DJLMMediaItem[]
        │
        ▼
Library
        │
        ▼
Analysis
        │
        ▼
Recovery
        │
        ▼
Dashboard
```

Regardless of the underlying storage technology, every provider produces the same collection of `DJLMMediaItem` objects.

---

# Storage Technologies

Different providers use different storage technologies.

| Provider | Storage Technology |
|----------|--------------------|
| VirtualDJ | XML |
| Rekordbox | SQLCipher SQLite |
| Serato | Proprietary database / crate files |
| Engine DJ | SQLite |
| Traktor | XML |

Storage implementation details remain isolated within each provider.

---

# Common Media Model

Every provider returns the same provider-independent media object.

Typical properties include:

- Provider
- MediaType
- FilePath
- FileSize
- Artist
- Title
- Album
- Genre
- Year
- BPM
- Key
- Duration
- DateFirstSeen
- DateLastModified

The remainder of DJLM operates exclusively on this model.

---

# Provider Database Objects

Each provider returns its own database object.

Examples:

- DJLM.VirtualDJDatabase
- DJLM.RekordboxDatabase

These objects encapsulate provider-specific resources such as XML documents or SQL database connections.

---

# Infrastructure

Some providers require supporting infrastructure.

For example, the Rekordbox provider uses a small C# helper library to encapsulate SQLCipher database access.

Responsibilities include:

- Opening encrypted databases.
- Executing SQL queries.
- Managing database connections.
- Isolating native library interaction.

PowerShell providers consume this infrastructure rather than implementing low-level database access themselves.

---

# Provider Validation

Every provider should successfully complete the following validation checklist.

- ✅ Import database
- ✅ Read media
- ✅ Translate to `DJLMMediaItem`
- ✅ Provider statistics
- ✅ Library analysis
- ✅ Dashboard integration
- ⏳ Save database
- ⏳ Update media paths

This ensures all providers behave consistently regardless of their underlying storage implementation.

---

# Error Handling

Providers should:

- Validate configuration.
- Validate database existence.
- Throw meaningful exceptions.
- Never silently ignore failures.
- Log significant operations.
- Dispose of provider resources correctly.

---

# Design Principles

When implementing a provider:

1. One responsibility per function.
2. Keep provider-specific logic isolated.
3. Return only `DJLMMediaItem` objects.
4. Hide storage implementation details.
5. Keep the public API consistent.
6. Use private helper functions where appropriate.
7. Prefer readability over optimisation.
8. Avoid duplicating logic between providers.

---

# Future Enhancements

Potential future enhancements include:

- Automatic provider discovery.
- Plugin provider architecture.
- Shared SQL provider infrastructure.
- Shared XML provider infrastructure.
- Automatic SQLCipher key discovery.
- Cross-provider synchronisation.
- Provider capability reporting.

---

# Summary

DJLM uses a provider-independent architecture in which each provider is responsible only for interacting with its native database format.

Once provider data has been translated into the common `DJLMMediaItem` model, every subsequent module operates independently of the source application.

This architecture has now been validated using two fundamentally different storage technologies:

- VirtualDJ (XML)
- Rekordbox (SQLCipher SQLite)

This provides a scalable foundation for adding additional providers while keeping the core application unchanged.