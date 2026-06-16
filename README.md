# ShoeManager

## Resumen

Este repositorio contiene la solución `ShoeManager.sln` con tres proyectos:

- `ShoeManager.Core`: lógica de dominio, modelos y controladores.
- `ShoeManager.Windows`: aplicación de escritorio Windows.
- `ShoeManager.Android`: aplicación .NET MAUI multiplataforma.
- `ShoeManager.Core.Tests`: proyecto de pruebas unitarias para la lógica del núcleo.

## Cambios recientes

Se implementaron las siguientes mejoras en el núcleo (`ShoeManager.Core`):

1. **Validación de pedidos**
   - `Pedido.cs` ahora incluye un constructor con validaciones de `ClienteTelefono`, `Items`, `Cantidad` y `PrecioPactado`.
   - Se añadió el método `Validate()` para verificar un pedido existente y recalcular `MontoTotal`.
   - El pedido inicializa `Estado` como `Nuevo` y guarda un historial de estados.

2. **Controlador de órdenes**
   - Se añadió `OrderController.cs` con `CreateOrder(Pedido)`.
   - El método valida el pedido, asigna `ID` si falta y actualiza `Estado` a `Creado`.

3. **Refactorización de cliente**
   - `Cliente.cs` ahora incluye constructor obligatorio para `Telefono`.
   - Se añadió validación con `Validate()` y `ToString()` para representación legible.

4. **Pruebas unitarias**
   - Se creó `ShoeManager.Core.Tests` con cobertura para:
     - creación de pedido y cálculo de total,
     - creación de orden y actualización de estado,
     - validación y formato de cliente.

## Cómo compilar

```powershell
cd c:\Users\USUARIO\Desktop\appzap
dotnet build ShoeManager.sln -c Debug -p:AndroidSdkDirectory="$env:USERPROFILE\Android\Sdk"
```

## Cómo ejecutar pruebas

```powershell
dotnet test ShoeManager.Core.Tests/ShoeManager.Core.Tests.csproj -v minimal
```

## Dependencias del entorno

- .NET 8 SDK
- Android SDK instalado y configurado en `ANDROID_SDK_ROOT`
- OpenJDK 17 disponible en `JAVA_HOME`

## Notas

- La compilación completa se verificó correctamente después de instalar el Android SDK y OpenJDK.
- El proyecto de pruebas unitarias se creó y pasó todas las pruebas.
- Se creó un emulador Android `ShoeManager_33` y se lanzó la app `com.companyname.shoemanager.android`.
- La app se ejecuta en el emulador y se verificó que el proceso está activo.
