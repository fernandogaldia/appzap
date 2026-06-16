Resumen de cambios: localización a español

Hechos:
- Se actualizaron todos los textos visibles y mensajes de validación en los proyectos de la solución.

Archivos modificados:
- [ShoeManager.Core/Cliente.cs](ShoeManager.Core/Cliente.cs)
  - "Telefono is required" -> "El teléfono es requerido"
  - "Telefono is required" (Validate) -> "El teléfono es requerido"

- [ShoeManager.Core/Pedido.cs](ShoeManager.Core/Pedido.cs)
  - Constructor y validaciones: todas las excepciones en inglés fueron traducidas a español. Ejemplos:
    - "ClienteTelefono is required" -> "El teléfono del cliente es requerido"
    - "Items are required" -> "Se requieren artículos"
    - "Item cannot be null" -> "El artículo no puede ser nulo"
    - "Item SaldoID is required" -> "El SaldoID del artículo es requerido"
    - "Item Talla is required" -> "La talla del artículo es requerida"
    - "Item Cantidad must be > 0" -> "La cantidad del artículo debe ser mayor que cero"
    - "Item PrecioPactado cannot be negative" -> "El precio pactado del artículo no puede ser negativo"
    - Mensajes de `Validate()` también traducidos.

- [ShoeManager.Core/BaseMaestra.cs](ShoeManager.Core/BaseMaestra.cs)
  - Se cambió el retorno "SUCCESS" por "ÉXITO" en `RegistrarOActualizarSaldo`.
  - Mensaje ya existente de validación de stock en español conservado: "STK-02: Error de consistencia. Debe registrar stock mayor a cero."

- [ShoeManager.Android/MainPage.xaml](ShoeManager.Android/MainPage.xaml)
- [ShoeManager.Android/MainPage.xaml.cs](ShoeManager.Android/MainPage.xaml.cs)
- [ShoeManager.Android/AppShell.xaml](ShoeManager.Android/AppShell.xaml)
  - Ya se encontraban traducidos: labels, placeholders, botones y mensajes de estado en español.

- [ShoeManager.Windows/MainWindow.xaml](ShoeManager.Windows/MainWindow.xaml)
  - Título cambiado a "Gestor de Calzado - Escritorio".

Verificación:
- Ejecuté `dotnet build` y la solución compila correctamente. Logs:
  - [build_verify.txt](build_verify.txt)
  - [build_all_verify.txt](build_all_verify.txt)
  - [build_log.txt](build_log.txt)

Siguientes pasos sugeridos (opcional):
- Revisar y traducir comentarios de desarrollador si así lo prefieres.
- Añadir recursos de localización (`.resx`) para soportar múltiples idiomas en el futuro.
- Hacer commit de los cambios y crear un tag/release.

Si quieres, hago el commit y preparo un changelog más formal o traduzco comentarios/strings restantes en documentación y scripts.