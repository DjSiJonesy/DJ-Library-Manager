# ADR-008: Library Organisation

**Status:** Accepted

**Date:** 2026-07-05

---

# Context

Professional DJ music libraries naturally become fragmented over time.

Common causes include:

- Multiple hard drives
- Downloads stored in temporary folders
- Music pool imports
- Streaming service exports
- Event-specific folders
- Manual file management
- Changes to storage devices

Although DJ software can often recover from some changes, no provider currently offers comprehensive library organisation across multiple platforms.

DJ Library Manager aims to become the primary owner of the physical music library while remaining provider-independent.

---

# Decision

DJLM will manage the physical organisation of the user's music library.

Provider applications such as VirtualDJ, rekordbox and Serato will consume the organised library rather than defining its structure.

DJLM becomes the authoritative source for library organisation.

---

# Design Principles

## Single Source of Truth

The physical music library is the authoritative source.

DJ provider databases are considered indexes that reference the library.

```
Physical Library
        │
        ▼
DJLM
        │
        ├──────────────┐
        ▼              ▼
 VirtualDJ      rekordbox
        │              │
        └──────┬───────┘
               ▼
           Other Providers
```

---

## Organisation Before Synchronisation

Files should first be organised into a logical structure.

Provider databases are then updated to reflect the new locations.

Providers never determine where files should be stored.

---

## Preview Before Execution

All organisation operations must support preview mode.

Users should be able to review:

- Files to be moved
- Destination folders
- Files to be renamed
- Duplicate handling
- Estimated changes

No filesystem modifications occur until explicitly approved.

---

## Provider Independence

The Organisation Engine operates only on DJLM domain objects.

It never manipulates provider databases directly.

Provider modules remain responsible for updating their own native formats.

---

## Rule-Based Organisation

Library organisation is driven by configurable rules.

Examples include:

- Genre
- BPM
- Decade
- Event type
- Media type
- User-defined collections
- File location
- Drive allocation

Rules may be extended without changing the Organisation Engine.

---

## Drive Awareness

DJLM recognises all connected storage devices.

Users may define one or more roles for each drive, for example:

- Active Library
- Archive
- Video
- Backup
- Music Pools

The Organisation Engine determines the appropriate destination for each file.

---

## Safe File Movement

Every move operation follows the same workflow.

```
Current Location
        │
        ▼
Validate Destination
        │
        ▼
Move File
        │
        ▼
Verify Move
        │
        ▼
Update Provider Databases
        │
        ▼
Record Undo Information
```

Each stage must succeed before progressing to the next.

---

# Folder Strategy

DJLM encourages a consistent logical library structure.

Example:

```
DJ Library

├── Active
│   ├── Commercial
│   ├── Dance
│   ├── House
│   ├── Drum & Bass
│   ├── Rock
│   └── Pop
│
├── Events
│   ├── Weddings
│   ├── Karaoke
│   ├── Corporate
│   └── Seasonal
│
├── Video
│
├── Music Pools
│
└── Archive
```

Users may customise this structure through configuration.

---

# Synchronisation

After organisation completes successfully:

- VirtualDJ paths are updated.
- rekordbox paths are updated.
- Serato paths are updated.
- Future providers are updated.

Provider synchronisation is always performed after successful file verification.

---

# Undo Support

Every organisation operation produces an undo record.

Each record stores:

- Original path
- New path
- Timestamp
- Provider updates performed
- Verification status

Where technically possible, all organisation operations should be reversible.

---

# Future Enhancements

The Organisation Engine is designed to support future capabilities including:

- Intelligent folder recommendations
- Automatic duplicate consolidation
- Audio fingerprint matching
- Metadata enrichment
- AI-assisted organisation
- Scheduled maintenance
- Cross-provider synchronisation

These capabilities should extend the existing architecture without requiring redesign.

---

# Consequences

Benefits include:

- Consistent library structure
- Provider-independent organisation
- Safer file management
- Automatic provider synchronisation
- Fully previewable operations
- Undo support
- Multi-drive awareness
- Future extensibility

DJLM becomes the authoritative manager of a professional DJ music library while remaining independent of any individual DJ software platform.