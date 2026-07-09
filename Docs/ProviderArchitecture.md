# Provider Architecture

## Overview

DJ Library Manager (DJLM) is built around a provider-independent architecture that separates provider-specific database implementations from the remainder of the application.

Each supported DJ application is implemented as an isolated provider module responsible only for interacting with its native database format.

Once provider data has been translated into the common `DJLMMediaItem` model, the remainder of the application operates entirely independently of the originating DJ software.

A Provider Service Layer within the Core module provides a common set of services that allow higher-level modules to perform provider operations without containing provider-specific logic.

This architecture enables new providers to be added with minimal impact on the rest of the application.

---

# Architecture

```
                         DJ Library Manager

                                │
                         Application Core
                                │
          ┌─────────────────────┼─────────────────────┐
          ▼                     ▼                     ▼
     Discovery             Provider Modules     Library Services
                                   │
        ┌──────────────┬───────────┼───────────┬──────────────┐
        ▼              ▼           ▼           ▼              ▼
   VirtualDJ      Rekordbox     Serato     Engine DJ      Traktor
        │              │
        │              │
        └──────────────┴──────────────────────────────┐
                                                      │
                                          Provider Translation
                                                      │
                                                      ▼
                                            DJLMMediaItem[]
                                                      │
                     ┌────────────────────────────────┼────────────────────────────────┐
                     ▼                                ▼                                ▼
                 Analysis                         Recovery                       Dashboard
                                                      │
                                                      ▼
                                           Core Provider Services
                                                      │
                                   ┌──────────────────┴──────────────────┐
                                   ▼                                     ▼
                           Save-Database                     Update-MediaPaths
```

---

# Design Goals

The provider architecture has been designed to:

- Support multiple DJ applications.
- Isolate provider-specific implementations.
- Provide a consistent public provider interface.
- Return a common media model.
- Centralise provider-independent operations.
- Hide storage implementation details.
- Allow providers to evolve independently.

---

# Supported Providers

| Provider | Read | Analyse | Recovery | Write | Storage |
|----------|:----:|:-------:|:--------:|:-----:|---------|
| VirtualDJ | ✅ | ✅ | ✅ | ✅ | XML |
| Rekordbox | ✅ | ✅ | ✅ | ✅ | SQLCipher (SQLite) |
| Serato | 🚧 | 🚧 | 🚧 | 🚧 | Proprietary |
| Engine DJ | 🚧 | 🚧 | 🚧 | 🚧 | SQLite |
| Traktor | 🚧 | 🚧 | 🚧 | 🚧 | XML |

---

# Public Provider Contract

Every provider exposes a consistent public interface.

| Function | Purpose |
|----------|---------|
| Import-<Provider>Database | Opens the provider database. |
| Get-<Provider>MediaItems | Returns `DJLMMediaItem` objects. |
| Get-<Provider>Statistics | Returns provider statistics. |
| Save-<Provider>Database | Persists provider changes. |
| Update-<Provider>MediaPaths | Updates media locations within the provider database. |

Developer utilities may also be provided where appropriate.

Examples include:

- Get-<Provider>Schema
- Get-<Provider>Tables

These utilities are intended for diagnostics and development rather than normal application operation.

---

# Core Provider Services

Provider-independent operations are implemented within the Core module.

These services determine the appropriate provider implementation based on the supplied provider database object.

Current services include:

| Function | Purpose |
|----------|---------|
| Save-Database | Dispatches database save operations to the appropriate provider. |
| Update-MediaPaths | Dispatches media path updates to the appropriate provider. |

Higher-level modules interact only with these services and never directly with provider-specific implementations.

---

# Provider Responsibilities

Providers are responsible for:

- Opening provider databases.
- Reading provider records.
- Writing provider changes.
- Translating provider records into `DJLMMediaItem`.
- Updating media paths.
- Managing provider resources.
- Logging provider operations.

Providers are **not** responsible for:

- Library scanning.
- Library analysis.
- Recovery planning.
- Dashboard presentation.
- User interaction.
- Cross-provider workflows.

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
Recovery Plan
        │
        ▼
Core Provider Services
        │
        ▼
Provider Update
        │
        ▼
Save Database
```

Regardless of storage technology, every provider produces and consumes the same provider-independent model.

---

# Provider Database Objects

Each provider returns a provider-specific database object.

Examples:

- DJLM.VirtualDJDatabase
- DJLM.RekordboxDatabase

These encapsulate provider-specific resources such as XML documents, SQL connections or other native objects.

---

# Common Media Model

Every provider translates native records into the common `DJLMMediaItem` model.

Typical properties include:

- Provider
- ProviderId
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
- DateAdded
- LastModified
- Properties

Once translated, all subsequent processing is completely provider-independent.

---

# Storage Technologies

Provider storage implementations remain completely isolated.

| Provider | Storage |
|----------|---------|
| VirtualDJ | XML |
| Rekordbox | SQLCipher SQLite |
| Serato | Proprietary database / crate files |
| Engine DJ | SQLite |
| Traktor | XML |

No other module needs knowledge of these storage technologies.

---

# Infrastructure

Some providers require additional infrastructure.

For example, the Rekordbox provider uses a C# helper library to encapsulate SQLCipher database access.

Responsibilities include:

- Opening encrypted databases.
- Executing SQL queries.
- Managing database connections.
- Isolating native library interaction.

This keeps PowerShell providers focused solely on provider behaviour.

---

# Provider Validation

Every provider should successfully complete the following validation checklist.

- ✅ Import database
- ✅ Read media
- ✅ Translate to `DJLMMediaItem`
- ✅ Provider statistics
- ✅ Library analysis
- ✅ Recovery plan generation
- ✅ Dashboard integration
- ✅ Update media paths
- ✅ Save database

This validation ensures consistent behaviour regardless of storage technology.

---

# Error Handling

Providers should:

- Validate configuration.
- Validate database existence.
- Throw meaningful exceptions.
- Log significant operations.
- Dispose of provider resources correctly.
- Never silently ignore failures.

---

# Design Principles

When implementing a provider:

1. One responsibility per function.
2. Keep provider-specific logic isolated.
3. Return only `DJLMMediaItem` objects.
4. Hide storage implementation details.
5. Maintain a consistent public API.
6. Use private helper functions where appropriate.
7. Prefer readability over optimisation.
8. Avoid duplicating logic between providers.
9. Keep provider-independent logic within Core services.

---

# Future Enhancements

Potential future enhancements include:

- Automatic provider discovery.
- Plugin-based provider architecture.
- Shared SQL provider infrastructure.
- Shared XML provider infrastructure.
- Provider capability reporting.
- Cross-provider synchronisation.

---

# Summary

DJ Library Manager uses a provider-independent architecture that cleanly separates provider-specific database implementations from the remainder of the application.

Providers are responsible only for interacting with their native storage technologies and translating data to and from the common `DJLMMediaItem` model.

Provider-independent operations are centralised within the Core Provider Services layer, allowing Analysis, Recovery and Dashboard modules to remain completely unaware of provider-specific implementations.

This architecture has been validated against two fundamentally different storage technologies:

- VirtualDJ (XML)
- Rekordbox (SQLCipher SQLite)

It provides a scalable foundation for supporting additional DJ applications while keeping the core application stable, maintainable and provider-independent.