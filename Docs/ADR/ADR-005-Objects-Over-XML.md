# ADR-005: Provider-Independent Domain Model

**Status:** Accepted

**Date:** 2026-07-02

## Context

DJ Library Manager is intended to support multiple DJ software platforms including VirtualDJ, rekordbox, Serato, Engine DJ and Traktor.

Each platform stores its library using different file formats, metadata structures and terminology.

If the application were to use provider-specific data structures throughout the codebase, every new provider would require significant changes to the matching engine, reporting, recovery tools and user interface.

This would tightly couple the application to individual DJ platforms and make long-term maintenance increasingly difficult.

---

## Decision

DJ Library Manager will use a provider-independent domain model.

Each provider module is responsible for reading its own native data format and translating it into standard DJLM objects.

The remainder of the application will work exclusively with DJLM domain objects and will have no knowledge of provider-specific formats.

For example:

VirtualDJ XML

↓

DJLM Media Item

↓

Library Engine

↓

Matching Engine

↓

Recovery Engine

↓

Reports

↓

User Interface

No component outside of the provider module should directly access provider-specific XML, database structures or proprietary formats.

---

## Consequences

### Advantages

- Clean separation between providers and business logic.
- Additional DJ software providers can be added without redesigning the application.
- Matching, reporting and recovery engines become reusable.
- Simplified testing using provider-independent objects.
- Reduced coupling between modules.
- Improved long-term maintainability.

### Trade-offs

- Provider modules require an additional mapping stage.
- Initial implementation is slightly more complex than working directly with provider-specific data.

These trade-offs are considered worthwhile because they significantly reduce future maintenance effort.

---

## Alternatives Considered

### Use VirtualDJ XML Throughout The Application

Rejected.

This would tightly couple every component to VirtualDJ's XML structure and make future provider support significantly more difficult.

---

### Create Separate Business Logic For Every Provider

Rejected.

Maintaining duplicate implementations for VirtualDJ, rekordbox, Serato and other platforms would introduce unnecessary complexity and increase maintenance costs.

---

### Convert Providers Into A Shared DJLM Domain Model

Accepted.

A shared domain model allows every provider to present data in a consistent format while keeping provider-specific knowledge isolated within its own module.

---

## Architectural Principle

Every provider module is responsible for answering one question:

> "How do I convert my native library into DJLM objects?"

Every other module is responsible for answering questions about those objects.

This separation ensures that the Core, Library, Matching, Recovery and Reporting modules remain independent of any individual DJ platform.

---

## Future Considerations

As additional providers are implemented, each will expose the same logical capabilities:

- Import Library
- Export Library (future)
- Retrieve Statistics
- Validate Library
- Discover Metadata

The internal implementation may differ between providers, but the behaviour presented to the remainder of DJLM should remain consistent.

This approach establishes DJ Library Manager as a provider-independent platform rather than a VirtualDJ-specific utility.