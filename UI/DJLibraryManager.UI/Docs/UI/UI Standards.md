# DIASISS UI Standards

## Version

| Property | Value |
|----------|-------|
| Version | 1.0 |
| Status | Approved |
| Last Updated | August 2026 |

---
The Import workspace is considered the reference implementation of the DIASISS user interface. 
All future workflow pages should follow its visual language, interaction model and reusable control architecture unless there is a clear functional reason not to.
---

# Purpose

This document defines the visual design standards for the DIASISS user interface.

The objective is to ensure every workspace provides a consistent experience regardless of which feature the user is using.

The standards described here apply to all current and future UI development.

---

# Design Principles

The DIASISS interface has been designed around five core principles.

## 1. Consistency

Controls should behave consistently throughout the application.

Users should never have to learn multiple ways of completing the same task.

---

## 2. Information Density

DJ libraries can contain millions of media files.

The interface should present the maximum amount of useful information without appearing cluttered.

---

## 3. Workflow Driven

The application guides users through a logical workflow.

Every workspace should naturally lead into the next stage.

---

## 4. Performance

The interface must remain responsive even when handling extremely large media libraries.

UI performance takes precedence over unnecessary visual effects.

---

## 5. Reuse

New controls should only be introduced when an existing Workflow control cannot reasonably be extended.

Duplicated UI implementations should be avoided.

---

# Workflow Layout

Every workflow page should use the same overall structure.

```
Workflow Header

Primary Content

Secondary Content

Navigation
```

Example workspaces include:

- Discovery
- Import
- Analysis
- Recovery
- Structure
- Synchronisation

---

# Workflow Header

All workflow pages should use the reusable `WorkflowHeader` control.

The header consists of:

- Title
- Description
- Previous button (optional)
- Next button (optional)

Navigation buttons should always appear in the top-right corner.

---

# Cards

Two card styles exist throughout the application.

## Primary Card

Used for the main content of a workspace.

```
Classes="card"
```

Padding

```
12
```

---

## Secondary Card

Used for supporting information.

```
Classes="card-secondary"
```

Padding

```
12
```

---

# Tables

Tables are the primary method of displaying structured information.

Every workflow table should follow the same layout.

```
Heading

Column Headers

Divider

Scrollable Rows

Divider (optional)

Totals Row (optional)
```

---

# Table Typography

| Element | Standard |
|----------|----------|
| Section Heading | section |
| Column Heading | 12pt SemiBold |
| Data | 12pt |
| Totals | 12pt SemiBold |

---

# Table Rows

Standard row height

```
30px
```

Compact row height

```
28px
```

Rows should never exceed 36px unless displaying images.

---

# Standard Column Types

Rather than defining widths individually for each table, workflow tables should be composed from standard column types.

## Icon

Width

```
30px
```

Examples

- Folder
- Drive
- Status

---

## Drive

Width

```
60px
```

---

## Name / Path / Folder

Width

```
*
```

This column always expands to fill the available space.

Long text should always use

```
TextTrimming="CharacterEllipsis"
```

---

## Numeric

Width

```
90px
```

Alignment

```
Right
```

Used for

- Files
- Audio
- Video
- Tracks
- Folders
- Missing
- Duplicate
- Recovered

---

## Date

Width

```
180px
```

---

## Status

Default width

```
220px
```

The Status column may contain

- Status indicator
- Status text
- Inline workflow actions

---

# Scrollable Tables

Workflow tables should support both small and extremely large datasets.

The visual layout must not change depending on whether a scrollbar is currently visible.

To achieve this, every workflow table reserves a fixed scrollbar gutter.

Standard scrollbar gutter

```
18px
```

The gutter must be applied consistently to:

- Column headers
- Scrollable content
- Totals row

The final column must never be obscured by the scrollbar.

---

# Numeric Alignment

All numeric values are right aligned.

Column headings should use the same alignment as the values beneath them.

Example

```
Folders          12,340
Audio            10,275
Video             2,065
```

---

# Icons

Emoji may be used where appropriate to improve visual recognition.

Examples

```
📁 Folder
🎵 Audio
🎬 Video
💾 Size
🏷 Name
```

Workflow actions should use Unicode symbols instead of emoji where a cleaner appearance is required.

Examples

```
→
⟳
✓
✕
```

---

# Buttons

## Primary

Minimum Height

```
32px
```

Padding

```
12,4
```

---

## Secondary

Minimum Height

```
32px
```

Padding

```
12,4
```

---

## Table Actions

Table actions should be presented as inline links.

Standard appearance

- Transparent background
- No border
- Underlined text
- 12pt font

Examples

```
Discover →
View →
Import →
Re-Import →
```

Large buttons should not appear inside workflow tables.

---

# Spacing

| Element | Standard |
|----------|-----------|
| Card Padding | 12 |
| Workflow Gap | 20 |
| Table Row Margin | 2 |
| Section Gap | 8 |

---

# Performance Requirements

DIASISS is expected to manage professional DJ libraries containing hundreds of thousands or millions of media files.

Workflow tables should therefore be designed with scalability in mind.

Requirements

- Virtualized row rendering
- Lazy UI generation
- Fast scrolling
- Fast filtering
- Fast sorting
- Minimal memory allocation

---

# Standard Workflow Controls

The following controls form the DIASISS UI design system.

## Navigation

- WorkflowHeader

---

## Discovery

- WorkflowMediaLocationTable
- WorkflowMediaLocationRow

---

## Import

- WorkflowProviderImportTable
- WorkflowProviderImportRow

---

## Media

- WorkflowMediaFolderTable
- WorkflowMediaFolderRow

---

## Future Controls

New workflow tables should extend the existing design language.

Examples include

- WorkflowAnalysisTable
- WorkflowDuplicateTable
- WorkflowMissingFileTable
- WorkflowRecoveryTable
- WorkflowSearchTable
- WorkflowSynchronisationTable

---

# Design Rule

The Import workspace currently represents the reference implementation for workflow table design.

Future workflow tables should follow its layout, typography, spacing and interaction model.

Where possible, existing Workflow controls should be extended rather than introducing new visual patterns.

Consistency across the application is preferred over workspace-specific customisation.

---

# Future Enhancements

The following capabilities should be supported by future workflow tables where appropriate.

- Virtualization
- Sorting
- Filtering
- Search
- Multi-selection
- Keyboard navigation
- Context menus
- Inline actions
- Persistent column widths
- Column visibility
- Export support