# ADR-003: Provider-Based Architecture

**Status:** Accepted

**Date:** 2026-07-02

## Context

DJ Library Manager is intended to support multiple DJ software platforms.

Each platform stores its library in a different format.

## Decision

Each DJ platform will be implemented as an independent provider module.

Examples include:

- VirtualDJ
- rekordbox
- Serato
- Engine DJ
- Traktor

Provider modules are responsible only for reading and writing their own formats.

Business logic remains provider independent.

## Consequences

### Advantages

- New providers can be added independently.
- Business logic remains reusable.
- Easier testing.
- Reduced coupling.

### Trade-offs

- Initial development requires more abstraction.

The flexibility gained outweighs this cost.

## Alternatives Considered

### Build Around VirtualDJ Only

Rejected because it would make future expansion significantly harder.

### Duplicate Business Logic Per Provider

Rejected because it would introduce unnecessary maintenance overhead.