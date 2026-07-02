# DJ Library Manager Domain Model

## Overview

The domain model defines the core business objects used throughout DJ Library Manager.

These objects represent concepts that exist independently of any individual DJ software platform.

Provider modules are responsible for translating native data into these shared domain objects.

The remainder of the application operates exclusively on the domain model.

---

# Core Objects

```
DJLMLibrary
│
├── DJLMMediaItem
│
├── DJLMPlaylist
│
├── DJLMScanResult
│
└── DJLMReport
```

---

# DJLMLibrary

Represents an imported music library.

Contains:

- Media Items
- Playlists
- Statistics
- Provider Information

One application session may contain multiple libraries.

---

# DJLMMediaItem

Represents a single piece of media.

Examples include:

- Audio Track
- Music Video
- Karaoke Track
- Sample
- Stem
- Future media types

Media items are provider independent.

---

# DJLMPlaylist

Represents an ordered collection of media items.

Playlists are independent of the provider that created them.

---

# DJLMScanResult

Represents the results of analysing a library.

Examples:

- Missing files
- Duplicate files
- Metadata conflicts
- Broken references

Scan results never modify data.

---

# DJLMReport

Represents formatted information intended for presentation.

Reports may be rendered as:

- Console
- Markdown
- HTML
- PDF
- GUI

Reports never perform analysis.

---

# Object Relationships

```
                 DJLMLibrary
                       │
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
  Media Items      Playlists      Scan Results
        │
        ▼
   Matching Engine
        │
        ▼
 Recovery Engine
        │
        ▼
     Reports
```

---

# Provider Translation

```
VirtualDJ XML
        │
        ▼
VirtualDJ Provider
        │
        ▼
DJLMMediaItem

rekordbox XML
        │
        ▼
rekordbox Provider
        │
        ▼
DJLMMediaItem

Serato Database
        │
        ▼
Serato Provider
        │
        ▼
DJLMMediaItem
```

Every provider produces the same logical objects.

---

# Domain Rules

The domain model must never contain provider-specific terminology.

For example:

Avoid:

- Song
- Database.xml
- Collection.xml
- Crate

Prefer:

- Media Item
- Library
- Playlist
- Report

The domain model should remain stable even as additional providers are added.

---

# Guiding Principle

Provider modules understand DJ software.

The domain model understands DJs.

Everything else in DJ Library Manager is built on top of that distinction.