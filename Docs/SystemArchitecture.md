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
                  ┌──────────────┴──────────────┐
                  ▼                             ▼
          Provider Services              Provider Modules
                                                │
        ┌──────────────┬──────────┬─────────────┼──────────────┐
        ▼              ▼          ▼             ▼              ▼
   VirtualDJ      Rekordbox    Serato      Engine DJ      Traktor
        │              │
        └──────────────┴──────────────────────────────┐
                                                      │
                                          Provider Translation
                                                      │
                                                      ▼
                                              DJLM Domain Model
                                                      │
                ┌─────────────────────────────────────┼────────────────────────────────────┐
                ▼                                     ▼                                    ▼
            Library                              Analysis                             Recovery
                                                                                │
                                                                                ▼
                                                                      Provider Services
                                                                                │
                                                                                ▼
                                                                       Provider Updates

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
          ▼
Library Scan
          │
          ▼
Analysis Engine
          │
          ▼
Recovery Plan
          │
          ▼
Provider Services
          │
          ▼
Provider Database
          │
          ▼
Dashboard / Reports

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
- Provider services

The Core module contains no provider-specific logic.

---

## Provider Modules

Provider modules understand the native formats used by DJ software.

Examples include:

- VirtualDJ
- Rekordbox
- Serato
- Engine DJ
- Traktor

Responsibilities include:

- Reading provider databases
- Writing provider databases
- Parsing native formats
- Translating provider data into DJLM media objects
- Updating provider media paths

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

- Recovery plan generation
- Recovery execution
- Preview mode
- Approval workflow
- Undo support (future)

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
- Recovery Plan
- Recovery Action
- Provider Database

Provider-specific objects never leave their own module.

---

# Dependency Rules

Modules may only depend upon lower-level modules.

```text
                Dashboard
                    │
                Reporting
                    │
                Recovery
                    │
                Analysis
                    │
                Library
                    │
        ┌───────────┴───────────┐
        ▼                       ▼
Core Provider Services     Provider Modules
            │                     │
            └───────────┬─────────┘
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

DJ Library Manager is built around a provider-independent domain model and a provider service layer.

Provider modules are responsible only for interacting with native database formats and translating data to and from the common DJLMMediaItem model.

Analysis, Recovery and Dashboard modules operate exclusively on provider-independent domain objects and interact with provider implementations only through the Core Provider Services.

This architecture enables new providers and new application features to be added with minimal impact on the remainder of the system while maintaining a clean, modular and testable design.