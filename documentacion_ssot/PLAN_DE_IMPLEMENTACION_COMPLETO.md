# PLAN DE IMPLEMENTACIÓN COMPLETO - SHOEMANAGER (ZAPAPP)
## Diagnóstico + Roadmap + Asignación de Agentes

> **Versión:** 1.0  
> **Fecha:** 18/06/2026  
> **Estado:** OFICIAL - Guía vinculante para todo el desarrollo  
> **Agente responsable:** COORD-01

---

## PARTE 1: DIAGNÓSTICO DEL ESTADO ACTUAL

### 🟢 CÓDIGO EXISTENTE Y FUNCIONAL (Completado)

| Módulo | Archivo | Estado | % Completado |
|--------|---------|--------|:------------:|
| **Core - Persistencia** | `BaseMaestra.cs` | ✅ Carga/guarda JSON, registro de pedidos/saldos, inventario predeterminado | 90% |
| **Core - Modelo Saldo** | `Saldo.cs` | ✅ Stock por talla, reserva de stock, validaciones | 100% |
| **Core - Modelo Pedido** | `Pedido.cs` | ✅ Máquina de 5 estados con transiciones, historial, validación | 100% |
| **Core - Modelo Cliente** | `Cliente.cs` | ✅ Teléfono como PK, validación, ToString | 100% |
| **Core - Modelo Mensaje** | `MensajeInterceptado.cs` | ✅ Estructura básica (Remitente, Contenido, Fecha) | 40% |
| **Core - Controlador** | `OrderController.cs` | ✅ Transiciones de estado (Creado→Confirmado→Enviado→Entregado/Cancelado) | 100% |
| **Windows - UI Escritorio** | `MainWindow.xaml/.cs` | ⚠️ Solo crea pedidos básicos. Sin DataGrid, sin reportes, sin búsqueda | 25% |
| **Android - UI Móvil** | `MainPage.xaml/.cs` | ⚠️ Solo crea pedidos básicos. Sin cámara, sin WhatsApp, sin fotos | 20% |
| **Tests Unitarios** | `UnitTest1.cs` | ⚠️ 4 tests básicos (constructor, controller, cliente, persistencia) | 20% |
| **Infraestructura** | `.csproj` de cada proyecto | ✅ Proyectos creados y referenciados correctamente | 100% |

### 🔴 CÓDIGO FALTANTE POR IMPLEMENTAR

| Fase del Plan Maestro | Componente | Prioridad | Depende de |
|:---------------------:|------------|:---------:|:----------:|
| **Fase 1** | ID Cronológico autoincremental (`ZAP-AAAAMMDD-000`) | Alta | - |
| **Fase 1** | Módulo STK-03 (Liquidación Dinámica - stock cero) | Alta | Saldo.cs |
| **Fase 2** | Servicio de Escucha de Notificaciones WhatsApp (Android) | Alta | - |
| **Fase 2** | Motor de Normalización Lingüística (mayúsculas, acentos, emoticonos) | Alta | - |
| **Fase 2** | Procesador Semántico (detección de stock, dirección, cliente, horario) | Alta | - |
| **Fase 2** | Diccionario de palabras clave por categoría | Media | - |
| **Fase 3** | DataGrid masivo con filtros y ordenamiento (Windows) | Alta | - |
| **Fase 3** | Gestión completa de CRUD de saldos (Windows) | Alta | - |
| **Fase 3** | Captura de cámara UPC-A (Android) | Alta | - |
| **Fase 3** | Formulario de registro con fotos frontal + perfil (Android) | Alta | - |
| **Fase 3** | Generador de enlaces wa.me (catálogo + confirmación) | Media | Fase 3 DataGrid |
| **Fase 4** | Reversa de stock en cancelación de pedido | Alta | Pedido.cs |
| **Fase 4** | Congelamiento de datos en estado "En Ruta" | Alta | Pedido.cs |
| **Fase 4** | Reporte de ingresos acumulados | Media | Fase 4 estados |
| **Fase 5** | Algoritmo de Merge bidireccional por USB | Alta | Fase 3 completo |
| **Fase 5** | Selector de carpeta MTP (Windows) | Alta | Fase 5 Merge |
| **Fase 5** | Sincronización binaria de imágenes (hash MD5) | Alta | Fase 5 Merge |
| **Fase 5** | Reporte de caja diario | Media | Fase 4 |
| **Fase 5** | Reporte de velocidad de venta | Media | Fase 4 |
| **Gobernanza** | Bug tracker (`BUGS_REGISTRO.md`) | Baja | - |
| **Gobernanza** | ADRs (Architecture Decision Records) | Baja | - |

---

## PARTE 2: PLAN DE TRABAJO POR SPRINTS

### 🏃 SPRINT 1: COMPLETAR NÚCLEO LÓGICO (Fase 1 + Fase 4)
**Objetivo:** Terminar toda la lógica de negocio en ShoeManager.Core

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 1.1 | Implementar ID Cronológico autoincremental (`ZAP-AAAAMMDD-000`) en `BaseMaestra` | ALGO-03 | `BaseMaestra.cs` | IDs generados secuencialmente por fecha |
| 1.2 | Implementar STK-03: Liquidación Dinámica cuando stock total = 0 | ALGO-03 | `Saldo.cs` | Producto se marca como "Liquidado" y se oculta de catálogos activos |
| 1.3 | Implementar reversa de stock al cancelar pedido | ALGO-03 | `Pedido.cs`, `BaseMaestra.cs` | Al cancelar → stock se reintegra automáticamente |
| 1.4 | Implementar congelamiento de datos en estado "En Ruta" | ALGO-03 | `Pedido.cs` | Precios, modelos y cantidades inmodificables |
| 1.5 | Agregar campo `RutaFotoFrontal` a `Saldo` | ALGO-03 | `Saldo.cs` | Ruta de imagen frontal adicional |
| 1.6 | Agregar campo `EstadoLiquidacion` y filtro `EsActivo` a `Saldo` | ALGO-03 | `Saldo.cs` | Propiedad `bool EsActivo` para ocultar liquidados |

**Estimación:** 2-3 interacciones  
**Tests requeridos:** TEST-08 debe crear tests para cada nueva funcionalidad

---

### 🏃 SPRINT 2: INTERFAZ WINDOWS COMPLETA (Fase 3 Desktop)
**Objetivo:** Transformar MainWindow en una central de administración masiva de datos

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 2.1 | Rediseñar MainWindow con DataGrid de alto rendimiento | WIN-04 | `MainWindow.xaml`, `MainWindow.xaml.cs` | 10,000+ registros en < 2 segundos |
| 2.2 | Implementar CRUD completo de saldos (alta, edición, eliminación lógica) | WIN-04 | `MainWindow.xaml`, `MainWindow.xaml.cs` | Formulario de edición con todos los campos |
| 2.3 | Implementar búsqueda y filtros por Marca/Modelo/UPC | WIN-04 | `MainWindow.xaml.cs` | Filtrado en tiempo real al escribir |
| 2.4 | Implementar gestión de clientes (CRUD) | WIN-04 | `ClientesWindow.xaml`, nuevas ventanas | ABM completo de clientes |
| 2.5 | Implementar visor de pedidos con cambio de estado manual | WIN-04 | `PedidosWindow.xaml` | Botones: Confirmar, En Ruta, Entregar, Cancelar |
| 2.6 | Implementar atajos de teclado: CTRL+N, CTRL+F, CTRL+E, CTRL+R | WIN-04 | `MainWindow.xaml.cs` | Atajos funcionando sin conflictos |
| 2.7 | Implementar logging local de operaciones | WIN-04 | Nuevo: `LoggerLocal.cs` | Log rotatorio Info/Warning/Error |
| 2.8 | Implementar indicador visual de stock crítico | WIN-04 | `MainWindow.xaml` | Color rojo/amarillo en filas con stock bajo |

**Estimación:** 4-5 interacciones  
**Tests requeridos:** TEST-08 debe probar UI y flujos críticos

---

### 🏃 SPRINT 3: INTERFAZ ANDROID + CÁMARA (Fase 3 Mobile)
**Objetivo:** Dotar a la app Android de captura de cámara y formularios completos

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 3.1 | Implementar captura de código de barras UPC-A con cámara | DROID-05 | Nuevo: `ScannerPage.xaml/.cs`, paquete ZXing | Decodificación < 1 segundo |
| 3.2 | Rediseñar MainPage con formulario completo de registro de saldo | DROID-05 | `MainPage.xaml/.cs` | Campos: UPC, Marca, Modelo, Precio, Stock por talla |
| 3.3 | Implementar captura de fotos frontal y perfil | DROID-05 | Nuevo: `CameraPage.xaml/.cs` | 480p mínimo, almacenamiento en carpeta dedicada |
| 3.4 | Implementar manejo de permisos Android 13+ (granular) | DROID-05 | `MainPage.xaml.cs`, `AndroidManifest.xml` | Solicitud: Cámara, Notificaciones, Almacenamiento |
| 3.5 | Implementar indicador visual de modo offline | DROID-05 | `AppShell.xaml` | Banner "📡 Sin conexión" permanente |
| 3.6 | Implementar gestión de clientes desde móvil | DROID-05 | Nuevo: `ClientesPage.xaml/.cs` | ABM de clientes |
| 3.7 | Implementar visor de pedidos con cambio de estado (Android) | DROID-05 | Nuevo: `PedidosPage.xaml/.cs` | Mismos flujos que Windows |

**Estimación:** 4-5 interacciones  
**Tests requeridos:** TEST-08 debe probar permisos y flujo de cámara

---

### 🏃 SPRINT 4: WHATSAPP INTEGRATION (Fase 2)
**Objetivo:** Implementar el módulo de escucha y procesamiento de WhatsApp

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 4.1 | Implementar Servicio de Escucha en segundo plano (Android) | DROID-05 | Nuevo: `NotificationListenerService.cs` | Filtro: solo `com.whatsapp` y `com.whatsapp.w4b` |
| 4.2 | Implementar Motor de Normalización Lingüística | ALGO-03 | Nuevo: `NormalizadorLinguistico.cs` en Core | Mayúsculas, sin acentos, sin emoticonos |
| 4.3 | Implementar Diccionario de palabras clave | ALGO-03 | Nuevo: `DiccionarioKeywords.cs` en Core | Categorías: Stock, Dirección, Cliente, Horario |
| 4.4 | Implementar Procesador Semántico (extracción de datos) | ALGO-03 | Nuevo: `ProcesadorSemantico.cs` en Core | Detección de talla, dirección, nombre, horario |
| 4.5 | Implementar alerta de disponibilidad de stock | DROID-05 + ALGO-03 | Nuevo flujo en Android | Consulta a BaseMaestra local |
| 4.6 | Implementar creación automática de pedido pendiente desde WhatsApp | DROID-05 | Nuevo flujo en Android | Pedido en estado Pendiente creado desde mensaje |

**Estimación:** 4-5 interacciones  
**Tests requeridos:** TEST-08 con casos de prueba para cada patrón lingüístico

---

### 🏃 SPRINT 5: REPORTES Y GENERADOR WA.ME (Fase 3 + Fase 4)
**Objetivo:** Implementar reportes financieros y generación de enlaces

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 5.1 | Implementar Reporte de Caja Diario | WIN-04 | Nuevo: `ReportesWindow.xaml/.cs` | Filtro por fecha, ingresos brutos |
| 5.2 | Implementar Reporte de Ingresos Acumulados | WIN-04 | `ReportesWindow.xaml/.cs` | Suma de pedidos "Entregado" |
| 5.3 | Implementar Reporte de Velocidad de Venta | WIN-04 | `ReportesWindow.xaml/.cs` | Días desde creación hasta último pedido |
| 5.4 | Implementar generador de enlace wa.me (catálogo) | WIN-04 | Nuevo: `GeneradorEnlaces.cs` en Core | Enlace con mensaje de catálogo precargado |
| 5.5 | Implementar generador de enlace wa.me (confirmación) | WIN-04 | `GeneradorEnlaces.cs` en Core | Enlace con datos del pedido |
| 5.6 | Exportar reportes a TXT | WIN-04 | `ReportesWindow.xaml.cs` | Archivo TXT con formato legible |

**Estimación:** 3-4 interacciones  
**Tests requeridos:** TEST-08 con datos de prueba para verificar cálculos

---

### 🏃 SPRINT 6: FUSIÓN INCREMENTAL POR USB (Fase 5)
**Objetivo:** Implementar el algoritmo de sincronización bidireccional

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 6.1 | Implementar módulo de exploración de carpeta MTP | WIN-04 | Nuevo: `SincronizacionWindow.xaml/.cs` | Diálogo FolderBrowserDialog nativo |
| 6.2 | Implementar lectura paralela de ambos archivos (PC + Celular) | ALGO-03 | Nuevo: `MotorFusion.cs` en Core | Carga simultánea en RAM |
| 6.3 | Implementar resolución de conflictos por timestamp (milisegundos) | ALGO-03 | `MotorFusion.cs` | El más reciente sobreescribe al más antiguo |
| 6.4 | Implementar inserción de registros huérfanos | ALGO-03 | `MotorFusion.cs` | Registros únicos se agregan sin duplicar |
| 6.5 | Implementar escritura atómica secuencial (PC → Celular) | WIN-04 + ALGO-03 | `MotorFusion.cs`, `SincronizacionWindow.xaml.cs` | Garantía de consistencia |
| 6.6 | Implementar sincronización binaria de imágenes (hash MD5) | ALGO-03 | `MotorFusion.cs` | Cálculo de hash, transmisión solo de faltantes |
| 6.7 | Implementar reporte post-merge (cambios realizados) | WIN-04 | `SincronizacionWindow.xaml.cs` | Resumen: N registros actualizados, N imágenes sincronizadas |

**Estimación:** 4-5 interacciones  
**Tests requeridos:** TEST-08 con simulaciones de merge con datos de prueba

---

### 🏃 SPRINT 7: PRUEBAS Y CIERRE (Transversal)
**Objetivo:** Asegurar calidad y documentar todo

| # | Tarea | Agente | Archivos afectados | Criterio de éxito |
|---|-------|--------|-------------------|-------------------|
| 7.1 | Crear suite completa de tests unitarios para Core | TEST-08 | `UnitTest1.cs` + nuevos tests | Cobertura > 80% de Core |
| 7.2 | Crear tests de integración Windows ↔ Android (merge simulado) | TEST-08 | Nuevo: `IntegrationTests.cs` | Sin pérdida ni duplicación |
| 7.3 | Crear tests de regresión para la máquina de estados | TEST-08 | Nuevos tests | Todas las transiciones válidas e inválidas |
| 7.4 | Ejecutar auditoría de privacidad completa | PRIV-07 | Escaneo de todo el código | Cero URLs, tokens o APIs externas |
| 7.5 | Verificar cumplimiento Offline-First | PRIV-07 | Revisión de dependencias | Sin dependencias de red en runtime |
| 7.6 | Crear bug tracker inicial | TEST-08 | `documentacion_ssot/BUGS_REGISTRO.md` | Lista de bugs conocidos si existen |
| 7.7 | Documentar ADRs de decisiones clave | COORD-01 | `documentacion_ssot/ADRs/` | ADR-001 en adelante |
| 7.8 | Post-mortem del desarrollo completo | COORD-01 | `documentacion_ssot/LEcciones_APRENDIDAS.md` | Lecciones documentadas |

**Estimación:** 2-3 interacciones  
**Ejecución:** Paralelo con sprints anteriores

---

## PARTE 3: DIAGRAMA DE DEPENDENCIAS

```
SPRINT 1 (Núcleo Lógico)
    │
    ├──→ SPRINT 2 (Windows UI) ──→ SPRINT 5 (Reportes)
    │         └──→ SPRINT 6 (Merge USB)
    │
    └──→ SPRINT 3 (Android UI) ──→ SPRINT 4 (WhatsApp)
    
SPRINT 7 (Pruebas) ←── Ejecución en paralelo con todos
```

**Reglas de dependencia:**
- SPRINT 2 y SPRINT 3 pueden ejecutarse en paralelo (no comparten archivos)
- SPRINT 5 depende de SPRINT 2 (reportes en Windows) y SPRINT 1 (estados)
- SPRINT 4 depende de SPRINT 3 (base Android funcional)
- SPRINT 6 depende de SPRINT 2 (selector carpeta) y SPRINT 1 (merge lógico)
- SPRINT 7 es continuo desde el inicio

---

## PARTE 4: ARQUITECTURA DE NUEVOS ARCHIVOS

### ShoeManager.Core (Nuevos archivos)
```
ShoeManager.Core/
├── NormalizadorLinguistico.cs    # SPRINT 4 - Limpieza de texto WhatsApp
├── DiccionarioKeywords.cs        # SPRINT 4 - Palabras clave por categoría
├── ProcesadorSemantico.cs        # SPRINT 4 - Extracción de datos de mensajes
├── MotorFusion.cs                # SPRINT 6 - Algoritmo de merge bidireccional
├── GeneradorEnlaces.cs           # SPRINT 5 - Enlaces wa.me
├── LoggerLocal.cs                # SPRINT 2 - Logging de operaciones
└── Exceptions.cs                 # SPRINT 1 - Excepciones personalizadas
```

### ShoeManager.Windows (Nuevos archivos)
```
ShoeManager.Windows/
├── Views/
│   ├── InventarioWindow.xaml/.cs  # SPRINT 2 - DataGrid de saldos
│   ├── ClientesWindow.xaml/.cs    # SPRINT 2 - CRUD de clientes
│   ├── PedidosWindow.xaml/.cs     # SPRINT 2 - Visor de pedidos
│   ├── ReportesWindow.xaml/.cs    # SPRINT 5 - Reportes financieros
│   └── SincronizacionWindow.xaml/.cs # SPRINT 6 - Merge USB
└── Services/
    └── SincronizadorUSB.cs        # SPRINT 6 - Operaciones MTP
```

### ShoeManager.Android (Nuevos archivos)
```
ShoeManager.Android/
├── Pages/
│   ├── ScannerPage.xaml/.cs       # SPRINT 3 - Escáner UPC-A
│   ├── CameraPage.xaml/.cs        # SPRINT 3 - Captura de fotos
│   ├── ClientesPage.xaml/.cs      # SPRINT 3 - CRUD de clientes
│   └── PedidosPage.xaml/.cs       # SPRINT 3 - Visor de pedidos
├── Services/
│   ├── NotificationListenerService.cs  # SPRINT 4 - Escucha WhatsApp
│   └── WhatsAppProcessor.cs       # SPRINT 4 - Procesa mensajes
└── Platforms/Android/
    └── AndroidManifest.xml        # SPRINT 3 - Permisos actualizados
```

---

## PARTE 5: MÉTRICAS DE PROGRESO

| Sprint | Archivos nuevos | Archivos modificados | Tests nuevos | % del total |
|:------:|:---------------:|:--------------------:|:------------:|:-----------:|
| Sprint 1 | 1 | 4 | 8+ | 15% |
| Sprint 2 | 6 | 2 | 5+ | 25% |
| Sprint 3 | 7 | 3 | 5+ | 25% |
| Sprint 4 | 5 | 1 | 10+ | 15% |
| Sprint 5 | 2 | 1 | 3+ | 8% |
| Sprint 6 | 3 | 1 | 5+ | 10% |
| Sprint 7 | 3 | 0 | 15+ | 2% |
| **Total** | **27** | **12** | **51+** | **100%** |

---

## PARTE 6: RIESGOS Y MITIGACIONES

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|:-----------:|:-------:|------------|
| El Service de Notificaciones de WhatsApp cambia su API interna | Media | Alto | El sistema no depende de la API de WhatsApp, solo del sistema de notificaciones Android nativo |
| Android 14+ restringe aún más los NotificationListener | Media | Alto | Preparar fallback con AccessibilityService como alternativa |
| MTP en Windows 10/11 tiene tiempos de respuesta lentos | Alta | Medio | Implementar timeouts y progreso visual en la UI |
| El merge bidireccional puede perder datos si falla la escritura atómica | Baja | Crítico | Implementar backup temporal antes de cada merge + rollback automático |
| Archivo JSON puede corromperse en escritura concurrente | Media | Alto | Implementar FileStream con bloqueo exclusivo + archivo `.bak` automático |

---

```
═══════════════════════════════════════════════════════════════
  PLAN DE IMPLEMENTACIÓN COMPLETO - V1.0
  7 SPRINTS | 27 NUEVOS ARCHIVOS | 51+ TESTS | ~26 INTERACCIONES ESTIMADAS
═══════════════════════════════════════════════════════════════
```

> **Próximo paso:** Revisar y aprobar el plan. Una vez aprobado, iniciaremos con **SPRINT 1: COMPLETAR NÚCLEO LÓGICO**.