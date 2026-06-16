# CHANGELOG

## [Unreleased]

### Added
- Created `ShoeManager.Core.Tests` project with unit tests for `Pedido`, `OrderController`, and `Cliente`.
- Added `OrderController` to manage the creation of orders in `ShoeManager.Core`.
- Implemented validation and total calculation in `Pedido`.
- Added customer validation and friendly `ToString()` in `Cliente`.
- Added `README.md` documenting project structure and environment setup.

### Fixed
- Added Android SDK and OpenJDK 17 setup information to enable successful full solution builds.
- Corrected `Cliente.cs` syntax during implementation.

### Notes
- Full solution `ShoeManager.sln` builds successfully after Android SDK configuration.
- Unit tests pass for the core logic.
- An Android emulator `ShoeManager_33` was created and the app package `com.companyname.shoemanager.android` was launched successfully.
- The app process was verified as running in the emulator.
