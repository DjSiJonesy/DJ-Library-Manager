# Workflow Controls

## Version

| Property | Value |
|----------|-------|
| Version | 1.0 |
| Status | Approved |
| Last Updated | August 2026 |

---

# Purpose

This document defines the reusable Workflow control architecture used throughout DIASISS.

Unlike the UI Standards document, which defines the application's visual appearance, this document defines the reusable controls used to build that interface.

The objective is to ensure every workflow page is built from a common set of reusable components.

---

# Design Philosophy

Workflow controls should be:

- Reusable
- Lightweight
- Provider independent
- MVVM friendly
- Easily testable
- Performance focused
- Visually consistent

Every new workflow should reuse existing controls wherever possible.

---

# Workflow Architecture

Every workflow page is composed from reusable controls.

```
Workflow
│
├── WorkflowHeader
│
├── Workflow Cards
│
├── Workflow Tables
│
├── Workflow Rows
│
└── Workflow Navigation
```

---

# Workflow Header

## Purpose

Provides a consistent header across every workspace.

## Responsibilities

- Display page title
- Display description
- Previous navigation
- Next navigation

## Public Properties

```
Title

Description

ShowPrevious

PreviousText

PreviousCommand

ShowNext

NextText

NextCommand
```

---

# Workflow Cards

Workflow cards group related information.

Two standard card types exist.

```
Primary

Secondary
```

Cards should never contain business logic.

They are responsible only for presentation.

---

# Workflow Tables

Workflow tables are the primary method of presenting structured information.

Every table should follow the same architecture.

```
Heading

Header Row

Scrollable Content

Totals Row (optional)
```

Workflow tables are responsible for

- Layout
- Column definitions
- Scroll behaviour
- Virtualization
- Totals

Workflow tables should never contain business logic.

---

# Workflow Rows

Workflow rows represent a single item displayed within a workflow table.

Examples include

```
WorkflowMediaLocationRow

WorkflowProviderImportRow

WorkflowMediaFolderRow
```

Responsibilities

- Display one model
- Display row actions
- Display status
- Display values

Rows should never calculate data.

---

# Workflow Actions

Workflow actions should always be command based.

Examples

```
Discover

Import

Re-Import

View

Analyse
```

Workflow controls should never directly manipulate application state.

Commands should be supplied by the ViewModel.

---

# Current Workflow Controls

## Headers

```
WorkflowHeader
```

---

## Discovery

```
WorkflowMediaLocationTable

WorkflowMediaLocationRow
```

---

## Import

```
WorkflowProviderImportTable

WorkflowProviderImportRow
```

---

## Media

```
WorkflowMediaFolderTable

WorkflowMediaFolderRow
```

---

# Future Workflow Controls

Future workspaces should extend the existing Workflow control library.

Examples

```
WorkflowAnalysisTable

WorkflowAnalysisRow

WorkflowDuplicateTable

WorkflowDuplicateRow

WorkflowMissingFileTable

WorkflowMissingFileRow

WorkflowRecoveryTable

WorkflowRecoveryRow

WorkflowSearchTable

WorkflowSearchRow

WorkflowSynchronisationTable

WorkflowSynchronisationRow
```

---

# Naming Convention

Workflow controls should follow a predictable naming pattern.

Tables

```
Workflow<Name>Table
```

Rows

```
Workflow<Name>Row
```

Cards

```
Workflow<Name>Card
```

Headers

```
WorkflowHeader
```

Avoid workspace-specific naming where a reusable control already exists.

---

# MVVM Responsibilities

## View

Responsible for

- Layout
- Visual appearance
- Bindings

Should never contain business logic.

---

## ViewModel

Responsible for

- Commands
- State
- Presentation data

Should never manipulate visual controls directly.

---

## Services

Responsible for

- Business logic
- Persistence
- Discovery
- Import
- Analysis
- Validation

Workflow controls should communicate only through the ViewModel.

---

# Performance

Workflow controls are expected to support extremely large DJ libraries.

Future workflow tables should support

- Virtualized rendering
- Lazy loading
- Fast filtering
- Fast sorting
- Low memory allocation

The target scale is

```
1,000,000+ media files
```

without requiring changes to the workflow architecture.

---

# Scroll Behaviour

Workflow tables should provide a consistent scrolling experience.

Requirements

- Stable column widths
- Stable header alignment
- Stable totals alignment
- Reserved scrollbar gutter
- No layout shift when scrollbars appear

Future workflow tables should implement virtualization rather than rendering every row.

---

# Extending the Workflow Library

When creating a new workflow page

DO

✓ Reuse an existing Workflow control

✓ Extend an existing Workflow control

✓ Follow the Workflow naming convention

✓ Follow the UI Standards

DON'T

✗ Duplicate an existing control

✗ Create page-specific implementations

✗ Embed business logic inside controls

✗ Create inconsistent layouts

---

# Reference Implementation

The Import workspace is considered the reference implementation of the Workflow control architecture.

Future controls should follow the same composition, layout and interaction model unless there is a clear technical reason not to.

---

# Future Direction

The Workflow library is expected to evolve into a complete UI framework for DIASISS.

Planned additions include

- Virtualized Workflow Tables
- Sortable columns
- Filter rows
- Search integration
- Multi-selection
- Context menus
- Keyboard navigation
- Column resizing
- Column persistence
- Export integration

These capabilities should be added by extending the Workflow control library rather than introducing new UI patterns.

---

# Design Principle

> **Every new workflow page should be constructed by composing existing Workflow controls. If a required control does not exist, it should be added to the Workflow library for reuse across the application rather than implemented solely for a single workspace.**