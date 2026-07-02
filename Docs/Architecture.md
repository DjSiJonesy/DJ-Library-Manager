# DJ Library Manager Architecture

## Overview

DJ Library Manager (DJLM) is designed as a modular, provider-independent application for analysing, organising and recovering DJ music libraries.

The architecture separates provider-specific functionality from the business logic that powers the application.

This allows support for additional DJ software platforms without requiring changes to the core application.

---

# Architectural Principles

The architecture is built around five core principles:

1. Separation of Responsibilities
2. Provider Independence
3. Modular Design
4. Safety Before Automation
5. Extensibility

Every architectural decision should reinforce one or more of these principles.

---

# High Level Architecture

```
                         DJ Library Manager

                                 │
                     Application Bootstrap
                           (Start.ps1)
                                 │
        ┌────────────────────────┼────────────────────────┐
        │                        │                        │
        ▼                        ▼                        ▼
     Configuration           Logging              Environment
            \                   |                     /
             \                  |                    /
              └─────────────────┼───────────────────┘
                                │
                             Core Module
                                │
    ┌───────────────┬───────────────┬─────────────────┐
    ▼               ▼               ▼                 ▼
VirtualDJ      rekordbox       Serato          Future Providers
    │               │               │
    └───────────────┼───────────────┘
                    │
            Provider Translation Layer
                    │
                    ▼
             DJLM Domain Model
                    │
    ┌───────────────┼───────────────────────────────┐
    ▼               ▼               ▼               ▼
 Statistics    Library Scan    Matching      Recovery
                                                │
                                                ▼
                                            Reporting
                                                │
                                                ▼
                                                GUI
```

---

# Module Responsibilities

## Core

Provides application-wide services.

Responsibilities include:

- Configuration
- Logging
- Environment
- Shared utilities
- Application startup

Core contains no provider-specific logic.

---

## Provider Modules

Each provider is responsible only for understanding its own data format.

Examples:

- VirtualDJ
- rekordbox
- Serato
- Engine DJ
- Traktor

Providers convert native data into DJLM objects.

Providers do not perform business logic.

---

## Library

Responsible for analysing music collections independently of the provider.

Examples:

- Library scanning
- Metadata comparison
- Duplicate detection
- Drive analysis

---

## Matching

Responsible for determining relationships between media items.

Examples:

- Filename similarity
- Metadata comparison
- Confidence scoring
- Audio fingerprinting (future)

---

## Recovery

Responsible for safe repair operations.

Examples:

- Path repair
- Missing file recovery
- Drive migration
- Undo support

---

## Reporting

Responsible for presenting information.

Examples:

- Console output
- Markdown reports
- HTML reports
- PDF reports
- GUI

Reporting never modifies data.

---

# Dependency Rules

Modules may only depend upon lower-level services.

Example:

GUI

↓

Reporting

↓

Recovery

↓

Matching

↓

Library

↓

Provider Modules

↓

Core

Core depends on nothing.

---

# Design Goals

DJ Library Manager should always be:

- Safe
- Predictable
- Explainable
- Extensible
- Testable

---

# Future Architecture

The architecture has been intentionally designed to support future expansion including:

- Multiple provider support
- Plugin architecture
- Audio fingerprinting
- Cloud services
- Synchronisation
- Cross-platform user interfaces

These features should be possible without redesigning the core architecture.