# ADR-002: Centralised Logging

**Status:** Accepted

**Date:** 2026-07-02

## Context

Every component of DJ Library Manager requires consistent logging for diagnostics, troubleshooting and auditing.

Without a shared logging framework each module would implement logging independently, resulting in inconsistent output.

## Decision

A central Write-Log function will provide all logging services.

Logging will support:

- Console output
- Daily log files
- Severity levels
- Timestamps

All modules must use Write-Log rather than writing directly to the console or log files.

## Consequences

### Advantages

- Consistent logging.
- Easier troubleshooting.
- Centralised formatting.
- Future support for additional log destinations.

### Trade-offs

- All modules depend on the Core module.

This dependency is acceptable because logging is a core service.

## Alternatives Considered

### Write-Host Everywhere

Rejected because output would become inconsistent.

### Individual Module Logs

Rejected because troubleshooting would become fragmented.