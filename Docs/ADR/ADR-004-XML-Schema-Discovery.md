# ADR-004: XML Schema Discovery

**Status:** Accepted

**Date:** 2026-07-02

## Context

VirtualDJ databases contain optional elements and attributes.

Assuming every field exists would make the importer fragile and difficult to maintain.

During early development unexpected XML structures were discovered, including optional nodes and varying metadata between media types.

## Decision

DJ Library Manager will inspect and understand XML structures before mapping them into internal objects.

Schema discovery will be performed using dedicated tooling rather than hard-coded assumptions.

Importers will treat XML attributes as optional unless proven otherwise.

## Consequences

### Advantages

- Greater compatibility with future VirtualDJ versions.
- More reliable importing.
- Easier debugging.
- Better understanding of provider data.

### Trade-offs

- Slightly more development effort.

This investment reduces long-term maintenance costs.

## Alternatives Considered

### Hard-code XML Fields

Rejected because optional fields and provider updates would likely cause failures.

### Ignore Unknown Fields

Rejected because potentially valuable metadata could be lost.