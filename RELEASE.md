# Release v1.2.0

## Resumen
Lanzamiento de la versión `v1.2.0` con la implementación completa del flujo de pedidos, control de stock por talla, persistencia de datos y la interfaz de escritorio funcional en `ShoeManager.Windows`.

## Características principales
- Flujo de pedidos completo en `ShoeManager.Core`.
- Estados de pedido en español: `Pendiente`, `Creado`, `Confirmado`, `Enviado`, `Entregado`, `Cancelado`.
- Validación de transiciones de estado y registro de historial de estados.
- Reserva de stock al registrar pedidos.
- Inicialización de inventario predeterminado para la primera ejecución.
- UI de Windows funcional para crear pedidos y ver artículos del pedido.
- Documentación de release y changelog actualizados.

## Archivos clave
- `ShoeManager.Core/Pedido.cs`
- `ShoeManager.Core/OrderController.cs`
- `ShoeManager.Core/Saldo.cs`
- `ShoeManager.Core/BaseMaestra.cs`
- `ShoeManager.Windows/MainWindow.xaml`
- `ShoeManager.Windows/MainWindow.xaml.cs`
- `CHANGELOG.md`
- `RELEASE_NOTES.md`
- `DOCUMENTACION_CAMBIOS.md`

## Validación
- `dotnet build` de la solución completado sin errores.
- `dotnet test ShoeManager.Core.Tests\ShoeManager.Core.Tests.csproj` pasado con 4 pruebas correctas.

## Nota de versión
Esta versión cierra la brecha funcional entre la lógica de pedidos y la interfaz de usuario de escritorio, dejando la aplicación en un estado sólido para crear pedidos, validar stock y guardar los datos de forma persistente.

## Tag
- `v1.2.0`
