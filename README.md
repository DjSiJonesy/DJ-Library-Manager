# DJ Library Manager

![PowerShell](https://img.shields.io/badge/PowerShell-7.6+-5391FE?logo=powershell)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Version](https://img.shields.io/badge/Version-0.9.0--alpha-orange)
![Status](https://img.shields.io/badge/Status-Active%20Development-brightgreen)

---

## Overview

DJ Library Manager (DJLM) is a provider-independent PowerShell application for analysing, organising and recovering professional DJ music libraries.

Although currently focused on **VirtualDJ**, the architecture has been designed to support multiple DJ platforms including rekordbox, Serato, Engine DJ and Traktor without changing the core application.

DJLM helps DJs understand the health of their music collection before safely repairing problems such as missing tracks, moved files and duplicate media.

---

## Current Features

### Provider Support

- ✅ VirtualDJ database import
- ✅ Provider-independent media model
- 🚧 rekordbox (planned)
- 🚧 Serato (planned)
- 🚧 Engine DJ (planned)

### Library Analysis

- ✅ Music library scanning
- ✅ Duplicate track detection
- ✅ Missing file detection
- ✅ Moved file detection
- ✅ Orphan file detection
- ✅ Library statistics
- ✅ Library health analysis
- ✅ Intelligent matching engine

### Dashboard

- ✅ Console dashboard
- ✅ Health score
- ✅ Library summary
- ✅ Analysis summary
- ✅ Recommendations
- ✅ Dynamic application versioning

### Application

- ✅ Modular architecture
- ✅ Application bootstrap
- ✅ Configuration management
- ✅ Structured logging
- ✅ Provider-independent design

---

## Project Structure

```
Modules/
    Analysis
    Core
    Dashboard
    Library
    Recovery
    VirtualDJ

Config/
Data/
Docs/
Logs/
Reports/
Samples/
Tests/
Tools/
```

---

## Architecture

DJLM is built around a provider-independent architecture.

```
                 DJ Library Manager

                        │
                 Application Core
                        │
        ┌───────────────┼───────────────┐
        ▼               ▼               ▼
   VirtualDJ      rekordbox       Serato
        │               │               │
        └───────────────┼───────────────┘
                        │
              Provider Translation
                        │
                        ▼
                 DJLM Media Model
                        │
        ┌───────────────┼───────────────┐
        ▼               ▼               ▼
     Library       Analysis        Recovery
                        │
                        ▼
                    Dashboard
```

---

## Running DJLM

Launch the application from the project root:

```powershell
.\Start.ps1
```

DJLM will automatically:

1. Load all modules.
2. Load the application configuration.
3. Import the VirtualDJ database.
4. Scan configured music libraries.
5. Analyse the library.
6. Calculate library health.
7. Display the dashboard.

---

## Roadmap

### Completed

- ✅ Modular architecture
- ✅ Provider-independent media model
- ✅ VirtualDJ provider
- ✅ Library scanning
- ✅ Analysis engine
- ✅ Dashboard
- ✅ Application bootstrap

### In Progress

- 🚧 Recovery Engine
- 🚧 Path repair
- 🚧 Report generation

### Planned

- rekordbox support
- Serato support
- Engine DJ support
- Audio fingerprinting
- AI-assisted recommendations
- Plugin architecture

---

## Requirements

- Windows 10/11
- PowerShell 7.6 or later
- VirtualDJ (currently supported provider)

---

## Documentation

Additional documentation can be found in the **Docs** folder, including:

- Vision
- System Architecture
- Architecture Decision Records (ADRs)
- Development Roadmap
- Coding Standards

---

## Contributing

DJ Library Manager is currently under active development.

Feedback, feature suggestions and issue reports are welcome through GitHub Issues.

---

## License

License to be confirmed.