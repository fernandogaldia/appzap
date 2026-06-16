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

## [v1.2.0] - 2026-06-16

### Added
- Implementado flujo completo de pedido y stock en `ShoeManager.Core`.
- Añadidos estados de pedido en español y control de transiciones (`Pendiente`, `Creado`, `Confirmado`, `Enviado`, `Entregado`, `Cancelado`).
- Añadida reserva de stock por talla al registrar pedidos.
- Añadida inicialización de inventario predeterminado en `BaseMaestra` para la primera ejecución.
- Añadida UI funcional de Windows en `ShoeManager.Windows` para crear pedidos y ver artículos.
- Documentación actualizada en `DOCUMENTACION_CAMBIOS.md`, `RELEASE_NOTES.md` y `CHANGELOG.md`.

### Changed
- Mejorado `OrderController` para manejar transiciones de estado en lugar de solo crear pedidos.
- Actualizado `BaseMaestra.RegistrarPedido` para validar inventario y almacenar pedidos correctamente.

### Fixed
- Corregida la lógica de carga y guardado de datos para preservar saldos y pedidos.
- Verificación de compilación exitosa de la solución completa.

### Notes
- El proyecto compila correctamente con `dotnet build`.
- Las pruebas de `ShoeManager.Core.Tests` pasan con éxito.
