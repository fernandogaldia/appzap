# CHANGELOG

## [v1.1.0] - 2026-06-16

### Added
- Committed el proyecto completo: `ShoeManager.Android`, `ShoeManager.Windows`, `ShoeManager.Core`, `ShoeManager.Core.Tests`, y la solución `ShoeManager.sln`.
- Localización completa al español en la interfaz de usuario y en los mensajes de validación de `ShoeManager.Core`.
- Añadido `.gitignore` para excluir artefactos de compilación (`bin/`, `obj/`) y logs temporales.
- Documentación inicial de cambios en `DOCUMENTACION_CAMBIOS.md` y `RELEASE_NOTES.md`.

### Changed
- Actualizado `RELEASE_NOTES.md` para incluir información del release `v1.1.0`.

### Fixed
- Verificación de compilación completa con `dotnet build -c Debug` sin errores en la solución.

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
