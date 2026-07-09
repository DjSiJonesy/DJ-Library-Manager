# DJ Library Manager

![PowerShell](https://img.shields.io/badge/PowerShell-7.6+-5391FE?logo=powershell)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Version](https://img.shields.io/badge/Version-0.9.0--alpha-orange)
![Status](https://img.shields.io/badge/Status-Active%20Development-brightgreen)

---

# Overview

DJ Library Manager (DJLM) is a provider-independent PowerShell application for analysing, organising and recovering professional DJ music libraries.

Built around a common provider architecture, DJLM provides a consistent interface for multiple DJ applications while keeping provider-specific code isolated.

VirtualDJ and Rekordbox are fully supported providers, with Serato, Engine DJ and Traktor already scaffolded and ready for implementation.

The long-term goal is to provide a single toolkit capable of analysing, repairing and managing professional DJ music libraries regardless of the software used.

---

# Design Principles

DJ Library Manager is built around a number of core architectural principles.

- Provider-independent architecture
- Common DJLM media model
- Modular PowerShell design
- Safe, non-destructive operations
- Extensible provider framework
- Provider service layer
- Consistent developer tooling

---

# Current Features

## Provider Support

| Provider | Read | Analyse | Recovery | Write |
|----------|:----:|:-------:|:--------:|:-----:|
| VirtualDJ | ✅ | ✅ | ✅ | ✅ |
| Rekordbox | ✅ | ✅ | ✅ | ✅ |
| Serato | 🚧 | 🚧 | 🚧 | 🚧 |
| Engine DJ | 🚧 | 🚧 | 🚧 | 🚧 |
| Traktor | 🚧 | 🚧 | 🚧 | 🚧 |

---

## Library Analysis

- ✅ Music library scanning
- ✅ Duplicate track detection
- ✅ Missing file detection
- ✅ Moved file detection
- ✅ Orphan file detection
- ✅ Library statistics
- ✅ Library health analysis
- ✅ Intelligent matching engine

---

## Recovery

- ✅ Recovery plan generation
- ✅ Provider-independent recovery engine
- ✅ Path repair
- ✅ Database updates
- 🚧 Missing file recovery
- 🚧 Duplicate resolution
- 🚧 Orphan import

---

## Dashboard

- ✅ Console dashboard
- ✅ Library health score
- ✅ Library statistics
- ✅ Analysis summary
- ✅ Recommendations
- ✅ Dynamic application versioning

---

## Application

- ✅ Modular architecture
- ✅ Configuration management
- ✅ Structured logging
- ✅ Provider discovery
- ✅ Database discovery
- ✅ Provider service layer
- ✅ Provider-independent database operations
- ✅ Developer scaffolding tools

---

# Project Structure

```
Modules
│
├── Analysis
├── Core
├── Dashboard
├── Discovery
├── EngineDJ
├── Library
├── Recovery
├── Rekordbox
├── Serato
├── Traktor
└── VirtualDJ

Config
Data
Docs
Logs
Reports
Samples
Tests

Tools
├── Reload-DJLM.ps1
├── New-DJLMModule.ps1
├── New-DJLMFunction.ps1
└── Templates
```

---

# Architecture

```
                    DJ Library Manager

                           │
                     Application Core
                           │
     ┌──────────────┬──────────────┬──────────────┐
     ▼              ▼              ▼
 Discovery      Provider       Library Services
                 Modules
                     │
     ┌──────────┬──────────┬──────────┬──────────┬──────────┐
     ▼          ▼          ▼          ▼          ▼
 VirtualDJ  Rekordbox   Serato    EngineDJ   Traktor
                     │
                     ▼
          Provider Translation Layer
                     │
                     ▼
               DJLMMediaItem[]
                     │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
      Analysis   Recovery   Dashboard
                     │
                     ▼
          Core Provider Services
                     │
          ┌──────────┴──────────┐
          ▼                     ▼
     Save-Database      Update-MediaPaths
```

---

# Running DJLM

Launch the application from the project root.

```powershell
.\Start.ps1
```

DJLM automatically:

1. Loads all modules.
2. Loads the application configuration.
3. Discovers installed DJ software.
4. Discovers available provider databases.
5. Imports provider databases.
6. Scans configured music libraries.
7. Performs library analysis.
8. Calculates library health.
9. Generates recovery recommendations.
10. Displays the dashboard.

---

# Roadmap

## Completed

- ✅ Modular architecture
- ✅ Provider-independent architecture
- ✅ Common media model
- ✅ Provider service layer
- ✅ VirtualDJ provider
- ✅ Rekordbox provider
- ✅ Provider discovery
- ✅ Database discovery
- ✅ Library scanning
- ✅ Analysis engine
- ✅ Recovery engine
- ✅ Console dashboard
- ✅ Developer scaffolding tools

---

## In Progress

- 🚧 Serato provider
- 🚧 Engine DJ provider
- 🚧 Traktor provider
- 🚧 Advanced recovery actions

---

## Planned

- Library organisation
- Metadata repair
- Artwork management
- Audio quality analysis
- Report generation
- Smart collections
- Cross-provider synchronisation
- Plugin architecture
- Graphical user interface

---

# Requirements

- Windows 10 or Windows 11
- PowerShell 7.6 or later
- One or more supported DJ applications

---

# Documentation

Additional documentation is available in the **Docs** folder, including:

- Vision
- Architecture
- Domain Model
- Provider Architecture
- Architecture Decision Records (ADRs)
- Development Roadmap
- Coding Standards

---

# Developer Tools

DJLM includes built-in developer tooling to simplify development.

- Module scaffolding
- Function scaffolding
- Automatic module reloading
- Reusable project templates

These tools ensure all modules follow a consistent structure and coding standard.

---

# Contributing

DJ Library Manager is currently under active development.

Feedback, feature requests and issue reports are welcome.

---

# License

License to be confirmed.