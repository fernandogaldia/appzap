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

Notas:
- Quedan en el repositorio archivos no comiteados y generados (`build_*`, `obj/`, `bin/`) que no forman parte de este release formal.
- Este release cubre la experiencia visible y los mensajes de validación en la aplicación actual.
