# Changelog

All notable changes to DJ Library Manager (DJLM) will be documented in this file.

The format follows **Keep a Changelog** and the project adheres to **Semantic Versioning**.

---

## [Unreleased]

### Planned

#### Recovery
- Automatic path repair.
- Preview mode.
- Undo support.
- Relink moved media.
- Provider database repair.

#### Reporting
- Markdown analysis reports.
- HTML reports.
- PDF reports.

#### Providers
- rekordbox support.
- Serato support.
- Engine DJ support.

#### Future
- Audio fingerprinting.
- AI-assisted recommendations.
- Plugin architecture.

---

## [0.9.0-alpha] - 2026-07-05

### Added

#### Application

- Added `Start-DJLM` application orchestrator.
- Simplified `Start.ps1` into a lightweight bootstrapper.
- Added automatic module discovery and loading.

#### Dashboard

- Integrated Library Statistics, Health and Analysis into a single dashboard.
- Added Library Analysis summary section.
- Added Files Scanned summary.
- Added dynamic application version display.
- Added overall library health status.
- Improved dashboard presentation.

#### Core

- Added application manifest (`DJLM.psd1`).
- Added `Get-DJLMVersion`.
- Introduced application metadata service.
- Improved application startup workflow.

#### Configuration

- Expanded application configuration for provider support.
- Added provider configuration structure.
- Added configurable library paths.

### Changed

- Refactored application startup around `Start-DJLM`.
- Separated bootstrap logic from application orchestration.
- Replaced hard-coded version information with dynamic application versioning.
- Improved Dashboard presentation and readability.
- Updated configuration structure to support future providers.

### Improved

- Reduced manual startup from multiple commands to a single application entry point.
- Simplified application initialisation.
- Improved separation of responsibilities across Core, Dashboard and Provider modules.
- Established application-level metadata independent of module manifests.

---

## [0.8.0] - 2026-07-05

### Added

#### Analysis Module

- Added provider-independent Analysis module.
- Added duplicate detection engine.
- Added missing file detection.
- Added orphan file detection.
- Added moved file detection.
- Added library analysis orchestration.

#### Analysis Engine

- Added `Compare-MediaItem`.
- Added `Get-MatchScore`.
- Added `Test-StrongMatch`.
- Added `Find-DuplicateTracks`.
- Added `Find-MissingFiles`.
- Added `Find-Orphans`.
- Added `Find-MovedFiles`.
- Added `Get-LibraryAnalysis`.

#### Library Module

- Added filesystem scanning.
- Added `Get-LibraryFiles`.
- Added standardised Library File objects.

#### VirtualDJ

- Added media duration support.
- Improved provider-independent media model.

### Changed

- Refactored library analysis into a dedicated Analysis module.
- Introduced `DJLM.LibraryAnalysis` domain object.
- Improved orchestration between Library, Analysis and Dashboard modules.
- Analysis now separates:
  - Duplicate Tracks
  - Moved Files
  - Missing Files
  - Orphan Files

### Improved

- Optimised duplicate detection algorithm.
- Reduced duplicate analysis time from approximately **40 minutes** to approximately **15 seconds**.
- Introduced indexed lookups using `HashSet` and `Hashtable` collections.
- Improved overall scalability of the Analysis Engine.

---

## [0.1.0-alpha] - 2026-07-02

### Added

#### Project Foundation

- Created GitHub repository.
- Established project folder structure.
- Configured Visual Studio Code development environment.
- Added project documentation.
- Added application bootstrap (`Start.ps1`).

#### Core Module

- Created modular PowerShell architecture.
- Added configuration service.
- Added logging framework.
- Added coloured console output.
- Added automatic log file creation.

#### VirtualDJ Module

- Added VirtualDJ provider.
- Added VirtualDJ database import.
- Added XML translation framework.

### Changed

- Renamed project from **VirtualDJ Library Tool** to **DJ Library Manager**.
- Introduced provider-independent architecture.

### Fixed

- Corrected module exports.
- Improved Strict Mode compatibility.
- Improved module loading.

---

## Future

Planned future capabilities include:

- Provider-independent recovery engine.
- Additional DJ software providers.
- Audio fingerprinting.
- AI-assisted recommendations.
- Plugin architecture.
- Cross-platform support.