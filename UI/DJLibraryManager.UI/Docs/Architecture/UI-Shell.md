# DJ Library Manager UI Shell Architecture

**Status:** Accepted

**Version:** 1.0

**Date:** 22 July 2026

---

# Overview

This document defines the permanent user interface architecture for DJ Library Manager.

The application shall use a **Shell + Workspace** architecture.

The Shell is persistent throughout the lifetime of the application.

Only the active workspace changes.

This architecture replaces the original page navigation model used during the early prototype stage.

---

# Design Goals

The Shell architecture has been designed to satisfy the following goals:

- Permanent navigation
- Provider-independent design
- Modular workspaces
- Consistent user experience
- Future expansion without redesign

The architecture must support an unlimited number of DJ providers together with future DJLM modules.

---

# Shell Layout

The application window is divided into two permanent regions.

```
+-----------------------------------------------------------------------+
| DJ Library Manager                                                    |
+-----------------------------------------------------------------------+
|                                                                       |
| Navigation Pane        |              Workspace                       |
|                        |                                              |
| Providers              |                                              |
| Music Locations        |          Active Workspace                    |
| DJLM Modules           |                                              |
|                        |                                              |
+-----------------------------------------------------------------------+
```

The left-hand navigation pane never changes.

The right-hand workspace displays the currently selected feature.

---

# Navigation Pane

The Navigation Pane is permanently visible.

It contains three logical sections.

---

## 1. Providers

The Providers section displays every supported DJ application.

Example:

```
Providers

✓ VirtualDJ

✓ Rekordbox

○ Serato

○ Engine DJ

○ Traktor

○ Mixxx
```

Installed providers display as available.

Unavailable providers remain visible but disabled.

Selecting a provider activates its workspace.

---

## 2. Music Locations

The Music Locations section displays all detected top-level music folders.

Example

```
Music Locations

D:\Music

E:\Archive

NAS\DJ Library
```

Selecting a location may display folder statistics or future maintenance tools.

---

## 3. DJ Library Manager

The final section contains features provided by DJ Library Manager itself.

Initially this consists of:

```
Library

Analysis

Recovery

Reports

Settings
```

Additional modules may be added in future without changing the Shell.

---

# Workspace

The Workspace occupies the entire right-hand side of the application.

Exactly one workspace is active at any time.

Examples include

- VirtualDJ Workspace
- Rekordbox Workspace
- Serato Workspace
- Library Workspace
- Analysis Workspace
- Recovery Workspace
- Reports Workspace
- Settings Workspace

The Shell is unaware of workspace implementation details.

---

# Workspace Architecture

Every workspace derives from

```
WorkspaceViewModel
```

Example

```
WorkspaceViewModel

    ProviderWorkspaceViewModel

    LibraryWorkspaceViewModel

    AnalysisWorkspaceViewModel

    RecoveryWorkspaceViewModel

    ReportsWorkspaceViewModel

    SettingsWorkspaceViewModel
```

This provides a common contract for all application features.

---

# Workspace Manager

Workspace switching is handled by a single service.

```
WorkspaceManager
```

Responsibilities include

- Maintaining the active workspace
- Switching workspaces
- Clearing workspaces
- Future workspace lifecycle management

The Shell interacts only with the Workspace Manager.

Individual workspaces never communicate directly with one another.

---

# Provider Workspaces

Each supported provider owns an independent workspace.

Responsibilities include

- Provider information
- Library import
- Import history
- Database information
- Statistics
- Provider maintenance
- Provider-specific operations

Provider workspaces must never contain DJLM-wide functionality.

---

# DJLM Workspaces

DJLM workspaces operate on the unified DJ Library Manager library.

Examples include

- Library
- Analysis
- Recovery
- Reports
- Settings

These modules remain provider-independent.

---

# Navigation Rules

Selecting an item within the Navigation Pane changes the active workspace.

```
Navigation

↓

WorkspaceManager

↓

Workspace

↓

User Interface
```

The Navigation Pane itself never changes.

---

# Provider Independence

The Shell has no knowledge of individual DJ providers.

Provider-specific functionality is encapsulated entirely within Provider Workspaces.

Adding support for a new provider should require:

- New Provider module
- New Provider Workspace

No changes should be required to the Shell architecture.

---

# Future Expansion

The architecture is designed to support future modules including, but not limited to:

- Smart Playlists
- Duplicate Management
- Audio Fingerprinting
- Artwork Management
- Cloud Synchronisation
- Collection Health
- Library Optimisation
- Batch Metadata Editing

Each module becomes a new Workspace.

The Shell remains unchanged.

---

# Architectural Principles

The Shell follows these principles.

## Single Responsibility

The Shell provides navigation.

Workspaces provide functionality.

The Workspace Manager provides workspace orchestration.

Each has one responsibility.

---

## Open / Closed Principle

New functionality is added through new Workspaces.

Existing Shell components should rarely require modification.

---

## Provider Independence

No provider-specific logic exists outside Provider Workspaces.

---

## Permanent Navigation

Navigation remains visible throughout the application.

Only the active workspace changes.

---

# Naming Convention

Classes

```
WorkspaceViewModel

ProviderWorkspaceViewModel

LibraryWorkspaceViewModel

AnalysisWorkspaceViewModel

WorkspaceManager
```

Views

```
WorkspaceHost

ProviderWorkspaceView

LibraryWorkspaceView

AnalysisWorkspaceView
```

---

# Implementation Roadmap

Phase 1

✓ WorkspaceViewModel

✓ ProviderWorkspaceViewModel

✓ WorkspaceManager

---

Phase 2

WorkspaceHost

ActiveWorkspace

Workspace binding

---

Phase 3

Shell implementation

Permanent navigation

Workspace hosting

---

Phase 4

Provider workspaces

---

Phase 5

DJLM workspaces

---

# Summary

The DJ Library Manager user interface is based upon a permanent Shell architecture.

The Shell never changes.

Navigation never changes.

Only the active Workspace changes.

This architecture provides a scalable, provider-independent foundation capable of supporting future DJ software providers and future DJ Library Manager modules without redesigning the user interface.