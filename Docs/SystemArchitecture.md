# DJ Library Manager System Architecture

## Overview

DJ Library Manager (DJLM) is a modular, provider-independent application for analysing, organising, maintaining and recovering DJ music libraries.

The system is designed around a common domain model that separates provider-specific implementations from business logic, allowing support for multiple DJ software platforms without changing the core application.

Every module has a single responsibility and communicates through standardised DJLM domain objects.

---

# Architectural Principles

The architecture is built upon six core principles:

1. Separation of Responsibilities
2. Provider Independence
3. Modular Design
4. Safety Before Automation
5. Composability
6. Extensibility

Every architectural decision should reinforce one or more of these principles.

---

# High Level Architecture

```text
                           DJ Library Manager

                                  │
                           Application Bootstrap
                               (Start.ps1)
                                  │
          ┌───────────────────────┼────────────────────────┐
          │                       │                        │
          ▼                       ▼                        ▼
    Configuration             Logging               Environment
           \                     │                      /
            \                    │                     /
             └───────────────────┼────────────────────┘
                                 │
                             Core Module
                                 │
       ┌──────────────┬──────────┼──────────────┬──────────────┐
       ▼              ▼          ▼              ▼              ▼
  VirtualDJ      rekordbox    Serato      Engine DJ     Future Providers
       │              │          │              │
       └──────────────┴──────────┴──────────────┘
                      │
                      ▼
          Provider Translation Layer
                      │
                      ▼
              DJLM Domain Model
                      │
          ┌───────────┼───────────────┐
          ▼           ▼               ▼
      Library      Analysis       Recovery
          │           │
          │           ▼
          │    DJLM.LibraryAnalysis
          │           │
          └───────────┼───────────────┐
                      ▼               ▼
                 Reporting      Dashboard / GUI
```

---

# Runtime Pipeline

The normal execution flow through the application is:

```text
Import Provider Database
          │
          ▼
Translate Provider Data
          │
          ▼
DJLM Media Objects
          │
          ├──────────────────────────────┐
          ▼                              ▼
Get-LibraryFiles                 Analysis Engine
          │                              │
          └──────────────┬───────────────┘
                         ▼
               Get-LibraryAnalysis
                         │
                         ▼
              DJLM.LibraryAnalysis
                         │
                         ▼
              Dashboard / Reports
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

The Core module contains no provider-specific logic.

---

## Provider Modules

Provider modules understand the native formats used by DJ software.

Examples include:

- VirtualDJ
- rekordbox
- Serato
- Engine DJ
- Traktor

Responsibilities include:

- Reading provider databases
- Parsing native formats
- Translating provider data into DJLM media objects

Provider modules never perform analysis.

---

## Library

Responsible for representing the physical music collection independently of any provider.

Responsibilities include:

- Library scanning
- File discovery
- File inventory
- Filesystem abstraction

The Library module performs no analysis.

It exposes standardised library file objects for consumption by the Analysis module.

---

## Analysis

Responsible for analysing relationships between DJLM media objects and physical library files.

Responsibilities include:

- Duplicate detection
- Missing file detection
- Moved file detection
- Orphan detection
- Match scoring
- Library analysis orchestration

The Analysis module is provider-independent.

It performs no user interface operations.

---

## Recovery

Responsible for safe repair operations.

Examples include:

- Path repair
- Missing file recovery
- Drive migration
- Undo support

Recovery modules consume analysis results and apply safe, auditable modifications.

---

## Reporting

Responsible for producing reports from analysis results.

Examples include:

- Console reports
- Markdown reports
- HTML reports
- PDF reports

Reporting modules never modify data.

---

## Dashboard

Responsible for presenting library health and analysis summaries.

Responsibilities include:

- Console dashboard
- Summary statistics
- Health overview
- Recommendations

The Dashboard consumes a completed `DJLM.LibraryAnalysis` object.

It performs no analysis itself.

---

# Domain Objects

The application communicates between modules using standardised domain objects.

Current domain objects include:

- DJLM Media Item
- Library File
- Library Analysis
- Duplicate Match
- Moved File Match

Provider-specific objects never leave their own module.

---

# Dependency Rules

Modules may only depend upon lower-level modules.

```text
Dashboard / GUI
        │
        ▼
Reporting
        │
        ▼
Recovery
        │
        ▼
Analysis
        │
        ▼
Library
        │
        ▼
Provider Modules
        │
        ▼
Core
```

The Core module depends on nothing.

Higher-level modules must never be referenced by lower-level modules.

---

# Design Goals

DJ Library Manager should always remain:

- Safe
- Predictable
- Explainable
- Composable
- Extensible
- Testable

When architectural decisions require compromise, these principles should guide the solution.

---

# Future Architecture

The current architecture has been intentionally designed to support future expansion without redesigning the core system.

Potential future capabilities include:

- Multiple provider support
- Plugin architecture
- Audio fingerprinting
- Automatic library repair
- Library synchronisation
- Cloud services
- Cross-platform user interfaces
- Advanced analytics
- AI-assisted library management

These capabilities should be implemented by extending existing modules rather than modifying the core architecture.

---

# Summary

The architecture of DJ Library Manager is centred around a provider-independent domain model.

Providers translate native data into standard DJLM objects.

Business logic operates exclusively on those objects.

Presentation modules consume completed analysis results without knowledge of the underlying providers or analysis algorithms.

This separation enables the application to grow through additional providers, analysis engines and repair capabilities while maintaining a clean, modular and testable architecture.