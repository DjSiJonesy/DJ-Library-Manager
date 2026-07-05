# ADR-006 – Analysis Engine

**Status:** Accepted

**Date:** 05 July 2026

---

# Context

DJ Library Manager initially focused on importing and translating provider-specific
libraries into a common media object model.

As the project evolved it became necessary to perform analysis independently of the
source provider.

The Analysis Engine was introduced to provide provider-independent diagnostics,
health checking and future repair capabilities across any supported DJ library.

The engine consumes standard DJLM media objects and physical filesystem objects,
allowing all analysis logic to remain independent of VirtualDJ, rekordbox or any
future provider.

---

# Decision

The Analysis Engine will be implemented as a dedicated module.

Analysis functions will operate exclusively on DJLM domain objects and will never
read provider-specific databases directly.

The module exposes a single orchestration function:

```
Get-LibraryAnalysis
```

which coordinates all available analysis routines.

Current analysis includes:

- Duplicate Detection
- Missing File Detection
- Moved File Detection
- Orphan File Detection

Each analysis remains independent and reusable.

The orchestration layer is responsible for resolving overlap between analysis
results before presenting a single analysis object to consumers.

---

# Architecture

```
Provider
    │
    ▼
Media Objects
    │
    │
Filesystem
    │
    ▼
Library Files
    │
    ▼
Get-LibraryAnalysis
    │
    ├── Find-DuplicateTracks
    ├── Find-MissingFiles
    ├── Find-MovedFiles
    └── Find-Orphans
    │
    ▼
DJLM.LibraryAnalysis
    │
    ▼
Dashboard / Reports / Repair
```

---

# Responsibilities

## Provider Modules

Provider modules are responsible only for:

- Reading provider databases
- Translating provider data into DJLM media objects

Providers never perform analysis.

---

## Library Module

The Library module is responsible for discovering physical media files.

It performs no analysis.

Its responsibility is to represent the current state of the filesystem.

---

## Analysis Module

The Analysis module is responsible for analysing relationships between:

- Library media objects
- Physical files

It performs no user interface operations.

---

## Dashboard Module

The Dashboard consumes a completed
`DJLM.LibraryAnalysis`
object and presents the results.

It performs no analysis itself.

---

# Analysis Workflow

```
Import Provider
        │
        ▼
Media Objects
        │
        ▼
Get-LibraryFiles
        │
        ▼
Get-LibraryAnalysis
        │
        ├── Duplicate Analysis
        ├── Missing File Analysis
        ├── Moved File Analysis
        └── Orphan Analysis
        │
        ▼
Library Analysis Object
```

---

# Analysis Categories

## Duplicate Tracks

Identifies multiple library entries representing the same media.

---

## Missing Files

Identifies database entries whose recorded path no longer exists and for which no
replacement file has been located.

---

## Moved Files

Identifies missing media that has been relocated elsewhere on disk with sufficient
confidence to be considered the same file.

Moved files are excluded from the Missing File report.

---

## Orphan Files

Identifies media files that exist on disk but are not referenced by the library.

Files already classified as moved are excluded from the Orphan report.

---

# Performance

The Analysis Engine favours indexed lookups over brute-force comparison.

Examples include:

- HashSet lookups
- Hashtable indexes
- Candidate grouping

This approach reduced duplicate analysis from approximately **40 minutes** to
approximately **15 seconds** on a library containing over 7,800 media items.

---

# Consequences

Advantages:

- Provider-independent analysis
- Reusable analysis components
- Clear separation of responsibilities
- Scalable architecture
- Suitable for future providers
- Foundation for automated repair tools

Trade-offs:

- Additional orchestration layer
- Increased number of domain objects
- Slightly more complex analysis pipeline

These trade-offs are considered acceptable in exchange for improved modularity,
maintainability and future extensibility.

---

# Future Direction

The Analysis Engine provides the foundation for future features including:

- Library repair
- Automatic path correction
- Duplicate consolidation
- Library integrity checking
- Media migration tools
- Advanced reporting
- Additional provider support

The Dashboard, Reports and future Repair modules will consume the
`DJLM.LibraryAnalysis`
object without requiring knowledge of the underlying analysis algorithms.