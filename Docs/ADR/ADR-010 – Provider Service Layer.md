# ADR-010 - Provider Service Layer

- **Status:** Accepted
- **Date:** 2026-07-09

---

# Context

DJ Library Manager was designed around isolated provider modules responsible for interacting with their native database formats.

Initially, higher-level modules such as Recovery called provider-specific functions directly. For example, the Recovery module invoked `Update-VirtualDJMediaPaths` and `Save-VirtualDJDatabase`.

While this approach worked for a single provider, it introduced undesirable coupling between the Recovery engine and individual provider implementations.

As additional providers were introduced (Rekordbox, Serato, Engine DJ and Traktor), every new provider would require changes to the Recovery module and other application components.

This violated the project's provider-independent design goals.

---

# Decision

Introduce a **Provider Service Layer** within the Core module.

The Provider Service Layer provides provider-independent operations that dispatch requests to the appropriate provider implementation based on the supplied provider database object.

Initially the service layer consists of:

- `Save-Database`
- `Update-MediaPaths`

Higher-level modules interact only with these services and never directly with provider-specific functions.

Provider modules continue to implement their own provider-specific operations, but expose a common public interface.

---

# Architecture

```
Recovery
Dashboard
Analysis
        │
        ▼
Core Provider Services
        │
        ├── Save-Database
        └── Update-MediaPaths
                │
                ▼
        Provider Modules
                │
    ┌───────────┴───────────┐
    ▼                       ▼
VirtualDJ             Rekordbox
```

---

# Consequences

## Positive

- Recovery is completely provider-independent.
- Higher-level modules contain no provider-specific logic.
- Providers expose a consistent public interface.
- Additional providers can be added with minimal impact to the application.
- Provider-specific storage technologies remain isolated.
- Reduced code duplication.

## Negative

- Core contains a small amount of provider dispatch logic.
- New provider services require updates to the dispatcher until a future registration mechanism is introduced.

---

# Alternatives Considered

## Direct provider calls

Continue calling provider functions directly from Recovery.

**Rejected**

This would require Recovery and other modules to understand every supported provider, increasing coupling and maintenance effort.

---

## Provider registration framework

Implement a dynamic provider registration mechanism.

**Deferred**

Although more flexible, the current number of providers does not justify the additional complexity.

A registration mechanism may be introduced in a future release if the number of provider services or providers increases significantly.

---

# Implementation

The following Core services were introduced:

- `Save-Database`
- `Update-MediaPaths`

The following provider interfaces were standardised:

- `Import-<Provider>Database`
- `Get-<Provider>MediaItems`
- `Get-<Provider>Statistics`
- `Save-<Provider>Database`
- `Update-<Provider>MediaPaths`

Recovery now invokes only the Core Provider Services.

---

# Validation

The architecture has been validated using two fundamentally different storage technologies.

| Provider | Storage | Status |
|----------|---------|--------|
| VirtualDJ | XML | Validated |
| Rekordbox | SQLCipher (SQLite) | Validated |

Both providers successfully completed the following workflow:

1. Import database.
2. Read media.
3. Translate to `DJLMMediaItem`.
4. Analyse library.
5. Generate recovery plan.
6. Update media paths.
7. Save provider database.

No provider-specific code exists within the Recovery module.

---

# Rationale

The Provider Service Layer preserves the provider-independent design philosophy of DJ Library Manager while allowing providers to implement their own storage technologies.

This decision establishes a stable architectural boundary between application logic and provider implementations, enabling future provider development without modifying higher-level modules.