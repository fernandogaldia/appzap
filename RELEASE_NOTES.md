Release v1.0.0 - Localización completa a español

Resumen:
- Se completó la localización de la aplicación a español en todos los proyectos principales.
- Se añadieron mensajes de error y validación en español en la capa `ShoeManager.Core`.
- Se verificó la compilación correcta de la solución completa.

Cambios incluidos:
- `ShoeManager.Core/Cliente.cs`
  - Traducción de mensajes de validación de teléfono.
- `ShoeManager.Core/Pedido.cs`
  - Traducción de todas las excepciones y mensajes de validación.
- `ShoeManager.Core/BaseMaestra.cs`
  - Cambio de retorno `SUCCESS` a `ÉXITO`.
- `ShoeManager.Android/MainPage.xaml`
  - UI en español: etiquetas, botones, placeholders.
- `ShoeManager.Android/MainPage.xaml.cs`
  - Mensajes de estado y validación en español.
- `ShoeManager.Android/AppShell.xaml`
  - Navegación y título en español.
- `ShoeManager.Windows/MainWindow.xaml`
  - Título de ventana traducido.
- `DOCUMENTACION_CAMBIOS.md`
  - Documentación detallada de los cambios realizados.

Verificación:
- `dotnet build -c Debug` completado sin errores.
- Logs de compilación: `build_log.txt`, `build_verify.txt`, `build_all_verify.txt`.

Tag creado:
- `v1.0.0`
- `v1.1.0`

Cambios del release v1.1.0:
- Commit `54f6746` agrega el proyecto completo y documentación inicial.
- Se incluyó el proyecto MAUI Android, el proyecto WPF Windows y el proyecto Core.
- Se añadieron archivos de configuración y recursos necesarios para compilar la solución.
- Se añadió `.gitignore` para excluir artefactos de compilación y logs.

Notas:
- Los archivos `build_*`, `obj/`, `bin/` se excluyen de este release y no quedan en el repo.
- Este release cubre el estado completo del proyecto con soporte de localización en español.

Release v1.2.0 - Lógica completa de pedidos e inventario

Resumen:
- Se completó la lógica de estado de pedidos y la reserva de stock en `ShoeManager.Core`.
- Se añadió una interfaz de usuario funcional en `ShoeManager.Windows` para crear pedidos y gestionar artículos.
- Se actualizó la documentación del proyecto para reflejar los cambios realizados.

Cambios incluidos:
- `ShoeManager.Core/Pedido.cs`
  - Agregado flujo de estados de pedido y validación de transiciones.
  - Se asegura el historial de estados y fechas de modificación.
- `ShoeManager.Core/OrderController.cs`
  - Creación de pedidos con transición al estado `Creado`.
  - Métodos añadidos para confirmar, enviar, entregar y cancelar pedidos.
- `ShoeManager.Core/Saldo.cs`
  - Añadidos métodos para verificar y reservar stock por talla.
- `ShoeManager.Core/BaseMaestra.cs`
  - Validación de inventario al registrar pedidos.
  - Inicialización de inventario predeterminado en primera ejecución.
- `ShoeManager.Windows/MainWindow.xaml` y `ShoeManager.Windows/MainWindow.xaml.cs`
  - Interfaz funcional de escritorio para crear y gestionar pedidos.

Verificación:
- `dotnet build` completado sin errores.
- `dotnet test` para `ShoeManager.Core.Tests` pasado con éxito.
