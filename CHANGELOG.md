# Changelog

All notable changes to DJ Library Manager (DJLM) will be documented in this file.

The format follows **Keep a Changelog** and the project adheres to **Semantic Versioning**.

---

## [Unreleased]

### Planned

#### Discovery

- Automatic detection of installed DJ software.
- Automatic discovery of provider databases.
- Automatic discovery of music libraries.
- Automatic drive detection and classification.
- Provider configuration wizard.
- Environment discovery for future GUI.

#### Organisation

- Rule-based library organisation.
- Library organisation planning.
- Preview file moves.
- Automatic folder creation.
- Undo support.

#### Reporting

- Markdown reports.
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
- Added dynamic application versioning via `DJLM.psd1`.

#### Dashboard

- Integrated Library Statistics, Health and Analysis into a single dashboard.
- Added Library Analysis summary section.
- Added Files Scanned summary.
- Added dynamic application version display.
- Added overall library health score and status.
- Improved dashboard presentation and formatting.

#### Recovery Module

- Added provider-independent Recovery module.
- Added `Get-RecoveryPlan`.
- Added `Show-RecoveryPlan`.
- Added `Approve-RecoveryPlan`.
- Added `Invoke-RecoveryPlan`.
- Added `Repair-MovedFiles`.
- Added Recovery Plan domain model.
- Added Recovery Action domain model.
- Added recovery preview workflow.
- Added recovery approval workflow.
- Added recovery execution orchestration.

#### VirtualDJ

- Added `Save-VirtualDJDatabase`.
- Added `Update-VirtualDJMediaPath`.
- Added provider write support.
- Added in-memory database update workflow.
- Added structured provider update result objects.
- Added optional database backup support.

#### Core

- Added application manifest (`DJLM.psd1`).
- Added `Get-DJLMVersion`.
- Added `Get-ProviderConfiguration`.
- Introduced application metadata service.
- Moved shared console helper functions into the Core module.
- Improved application startup workflow.

#### Configuration

- Expanded application configuration for multi-provider support.
- Added provider configuration structure.
- Added configurable library paths.
- Added recovery configuration.
- Added organisation configuration.

#### Documentation

- Added ADR-007 Recovery Architecture.
- Added ADR-008 Library Organisation.

### Changed

- Refactored application startup around `Start-DJLM`.
- Separated bootstrap logic from application orchestration.
- Replaced hard-coded version information with dynamic application versioning.
- Refactored shared console helper functions into the Core module.
- Updated configuration structure to support multiple DJ providers.
- Improved Dashboard presentation and readability.
- Recovery workflow now follows:
  - Analyse
  - Plan
  - Approve
  - Execute
  - Save

### Improved

- Reduced manual startup from multiple commands to a single application entry point.
- Simplified application initialisation.
- Improved separation of responsibilities across Core, Dashboard, Recovery and Provider modules.
- Established application-level metadata independent of module manifests.
- Introduced the first complete end-to-end recovery pipeline.
- Introduced safe provider write support using in-memory updates before persistence.
- Strengthened provider-independent architecture.

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

## [0.9.0-alpha] - 2026-07-09

### Added
- Added provider service layer within the Core module.
- Added generic `Save-Database` dispatcher.
- Added generic `Update-MediaPaths` dispatcher.
- Added Rekordbox media path update support.
- Added bulk Rekordbox media path update support.
- Added provider-independent Recovery execution.

### Changed
- Refactored Recovery module to remove provider-specific dependencies.
- Refactored VirtualDJ media path updates to use the provider-independent interface.
- Standardised provider configuration under `Providers.*`.
- Restored automatic library scanning using `Library.Path` from configuration.
- Moved Dashboard rendering helpers back into the Dashboard module.

### Fixed
- Corrected Rekordbox media type detection.
- Corrected VirtualDJ configuration loading.
- Corrected Dashboard helper loading.
- Improved Recovery execution tracking.