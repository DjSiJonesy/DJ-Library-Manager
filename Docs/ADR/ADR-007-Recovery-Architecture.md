# ADR-007: Recovery Engine Architecture

**Status:** Accepted

**Date:** 2026-07-05

---

# Context

DJ Library Manager currently provides comprehensive analysis of a DJ library by identifying:

- Duplicate tracks
- Missing files
- Moved files
- Orphan files
- Metadata quality issues

Analysis alone is insufficient.

The primary goal of DJLM is to help users safely recover and maintain their music libraries.

A Recovery Engine is therefore required to convert analysis results into safe, explainable repair operations.

---

# Decision

Recovery will be implemented as a separate application layer.

The Recovery Engine will never perform analysis itself.

Instead it will consume provider-independent analysis results and generate a recovery plan.

```
Analysis
        │
        ▼
Recovery Plan
        │
        ▼
Preview
        │
        ▼
User Approval
        │
        ▼
Execution
        │
        ▼
Verification
        │
        ▼
Undo Log
```

Recovery is therefore completely independent from both provider implementations and reporting.

---

# Recovery Principles

The Recovery Engine is built around the following principles.

## Safety First

Recovery operations must never modify a user's library without explicit approval.

Every operation must support preview mode.

---

## Explainable

Every proposed change must include an explanation.

Example:

```
Track:
ATB - Ecstasy.mp3

Current Path

G:\Old Drive\Dance\ATB - Ecstasy.mp3

Suggested Path

D:\Music\Active\ATB - Ecstasy.mp3

Confidence

98%

Reason

Filename, artist and duration all match.
```

The user should always understand why DJLM is proposing a repair.

---

## Provider Independence

Recovery operates exclusively on DJLM domain objects.

Recovery never manipulates VirtualDJ XML directly.

Provider modules remain responsible for reading and writing provider-specific formats.

---

## Confidence Driven

Every recovery action includes a confidence score.

Example:

| Confidence | Behaviour |
|------------|-----------|
| 100% | Safe automatic repair (optional) |
| 90–99% | Recommend repair |
| 70–89% | User review required |
| Below 70% | Do not repair |

Confidence scoring may evolve as additional matching algorithms are introduced.

---

## Reversible

Every modification must generate an undo record.

Recovery operations should be reversible whenever technically possible.

---

# Recovery Pipeline

The Recovery Engine consists of five logical stages.

## Stage 1

Generate Recovery Plan

Convert analysis findings into recovery actions.

No files are modified.

---

## Stage 2

Preview

Display every proposed change.

The user may choose which operations to perform.

---

## Stage 3

Execution

Apply approved recovery operations.

Each action is independently logged.

---

## Stage 4

Verification

Re-analyse affected items.

Confirm the repair succeeded.

---

## Stage 5

Undo

Allow previous recovery operations to be reversed where possible.

---

# Initial Recovery Operations

Sprint 10 will introduce support for:

- Repair moved file paths
- Relink provider database entries
- Import orphan files

Future versions may add:

- Remove stale provider entries
- Metadata repair
- Duplicate consolidation
- Playlist repair
- Artwork recovery

---

# Module Responsibilities

```
Analysis
        │
        ▼
Recovery
        │
        ▼
Provider
        │
        ▼
Reporting
```

Analysis identifies problems.

Recovery decides how to repair them.

Providers perform provider-specific updates.

Reporting presents the results.

Each layer remains independent.

---

# Consequences

Benefits include:

- Provider-independent recovery
- Safe repair workflow
- Preview before execution
- Undo capability
- Explainable recovery decisions
- Extensible recovery architecture

This architecture supports future providers without requiring changes to the Recovery Engine itself.