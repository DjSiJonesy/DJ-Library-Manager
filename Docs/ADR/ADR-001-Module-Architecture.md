# ADR-001: Modular Architecture

**Status:** Accepted

**Date:** 2026-07-02

## Context

DJ Library Manager is expected to grow into a large application supporting multiple DJ software providers, reporting engines, recovery tools and future extensions.

Maintaining a single PowerShell script or monolithic module would become increasingly difficult as the project grows.

## Decision

The application will be divided into independent PowerShell modules.

Each module will have a consistent structure:

- Public
- Private
- Tests

Only functions within the Public folder will be exported.

Private functions are implementation details and are not intended for external use.

Modules communicate only through their public interfaces.

## Consequences

### Advantages

- Clear separation of responsibilities.
- Easier testing.
- Easier maintenance.
- Better scalability.
- Future provider modules can be added without affecting existing code.

### Trade-offs

- Slightly more files.
- Additional module loading during startup.

These trade-offs are considered acceptable.

## Alternatives Considered

### Single Script

Rejected because maintainability decreases rapidly as the application grows.

### Single Large Module

Rejected because it couples unrelated functionality together.