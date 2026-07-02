# Changelog

All notable changes to DJ Library Manager will be documented in this file.

The format is based on Keep a Changelog and the project follows Semantic Versioning.

---

## [Unreleased]

### Added
## [Unreleased]

### Added
- Added provider-independent media translation layer.
- Added `ConvertFrom-UnixTime` shared service.
- Added safe XML child node helper.
- Added safe XML attribute helper.
- Added VirtualDJ media object translation.

### Changed
- VirtualDJ timestamps are now converted to native PowerShell `DateTime` objects.
- XML parsing now safely handles missing nodes and attributes.
- VirtualDJ provider now returns provider-independent DJLM media objects.

---

## [0.1.0-alpha] - 2026-07-02

### Added

#### Project Foundation
- Created GitHub repository.
- Established project folder structure.
- Configured Visual Studio Code development environment.
- Added project documentation including:
  - Vision
  - Architecture
  - Roadmap
  - Coding Standards
  - Contributing Guide
- Added application bootstrap (`Start.ps1`).

#### Core Module
- Created modular PowerShell architecture.
- Added configuration service.
- Added project root discovery helper.
- Added logging framework.
- Added coloured console output.
- Added automatic log file creation.
- Enabled Strict Mode throughout the project.

#### VirtualDJ Module
- Added VirtualDJ module.
- Added database import capability.
- Added initial library statistics engine.
- Added first-generation XML Schema Explorer.

#### Development
- Added Git workflow.
- Added module reload developer script.
- Established sprint-based development process.
- Adopted semantic versioning.
- Introduced public/private module architecture.

### Changed
- Renamed the project from **VirtualDJ Library Tool** to **DJ Library Manager (DJLM)**.
- Refined the project vision from a VirtualDJ-specific utility to a provider-independent DJ library platform.
- Improved module architecture to clearly separate public and private functionality.
- Began moving from XML-centric processing towards provider-independent media objects.

### Fixed
- Corrected module export behaviour.
- Resolved Strict Mode compatibility issues in the configuration service.
- Improved module loading reliability.
- Fixed logging framework initialisation.

### Known Limitations
- VirtualDJ Schema Explorer currently treats some XML data values as schema paths.
- XML attribute handling is being refactored to improve schema discovery.
- Import currently returns raw XML objects prior to the provider-independent media model.

---

## Future Releases

### Planned for v0.2.0
- Improved XML Schema Explorer.
- Media discovery engine.
- Provider-independent media objects.
- Enhanced VirtualDJ statistics.

### Planned for v0.3.0
- Library scanning engine.
- Missing file detection.
- Drive analysis.

### Planned for v0.4.0
- Intelligent matching engine.
- Confidence scoring.
- Duplicate detection.

### Planned for v0.5.0
- Recovery engine.
- Preview mode.
- Undo support.