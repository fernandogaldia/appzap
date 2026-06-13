# DOCUMENTACIÓN TÉCNICA SSOT – SISTEMA DE GESTIÓN DE VENTAS DE ZAPATILLAS POR WHATSAPP

**Versión:** 1.0  
**Fecha:** 2026-06-09  
**Tipo:** Especificación técnica para desarrollo (sin mockups ni elementos visuales)

---

## PARTE 4: SEGURIDAD, REQUISITOS NO FUNCIONALES, STACK TECNOLÓGICO, INSTALACIÓN Y GLOSARIO

---

### 1. SEGURIDAD Y PRIVACIDAD

#### 1.1 Datos almacenados localmente

- **Nombres de clientes:** se almacenan en `datos.json` dentro del dispositivo del vendedor.
- **Direcciones:** se almacenan en `datos.json`.
- **Números de teléfono:** se almacenan en `datos.json`.
- **Historial de compras:** se almacena en `datos.json`.
- **Fotos de productos:** se almacenan en la subcarpeta `fotos/`.

#### 1.2 Principios de privacidad

- **Todo es local:** ningún dato sale del dispositivo del vendedor. No se realizan llamadas a internet excepto para abrir WhatsApp (URL scheme) o para futuras actualizaciones opcionales.
- **Sin servidores externos:** no se utilizan APIs de terceros para almacenar o procesar datos.
- **Sin compartición de datos:** la aplicación no comparte información con otras aplicaciones ni servicios.
- **Control del vendedor:** el vendedor decide cuándo hacer respaldos manuales (exportación a `respaldos/`) y cuándo sincronizar por USB.

#### 1.3 Permisos y transparencia

- La aplicación debe solicitar permisos únicamente cuando son necesarios y explicar su uso.
- Permisos obligatorios:
  - `NOTIFICATION_LISTENER`: para detectar mensajes de WhatsApp.
  - `INTERNET`: para abrir enlaces de WhatsApp (solo salida).
  - `VIBRATE`: para alertas.
- Permisos opcionales (solicitar bajo demanda):
  - `CAMERA` y `READ_EXTERNAL_STORAGE`: si el vendedor quiere tomar o seleccionar fotos desde la app.
- En Android 13+ se debe pedir `POST_NOTIFICATIONS` para mostrar notificaciones propias de la app.

#### 1.4 Sin sistemas de pago integrados

Por decisión explícita, la aplicación **no incluye** ninguna pasarela de pago. El vendedor gestiona los pagos externamente (transferencia bancaria, contraentrega, Yape, Plin, etc.). El campo `formaPago` en el pedido es meramente informativo.

---

### 2. REQUISITOS NO FUNCIONALES

#### 2.1 Rendimiento

| Métrica | Objetivo | Condición de prueba |
|---------|----------|---------------------|
| Carga inicial de la app | < 2 segundos | Dispositivo de gama media (4 GB RAM) con 200 zapatillas, 500 clientes, 1000 pedidos. |
| Filtrado de catálogo por talla | < 0.5 segundos | Inventario de 200 zapatillas. |
| Apertura de WhatsApp con catálogo | < 1 segundo (desde que se pulsa el botón) | Conexión USB o sin internet (la apertura es local). |
| Sincronización USB (bidireccional) | < 10 segundos | 100 registros modificados + 50 fotos nuevas (cada foto < 2 MB). |
| Exportación a Excel/CSV | < 5 segundos | 1000 pedidos. |
| Escritura de JSON tras modificación | < 0.2 segundos | Archivo `datos.json` de hasta 10 MB. |

#### 2.2 Usabilidad

- Interfaz basada en texto y botones claros; máximo 3 clics/toques para cualquier acción principal (ver catálogo, confirmar pedido, sincronizar).
- Retroalimentación visual inmediata: indicadores de progreso (spinners, barras) para operaciones que puedan tardar más de 1 segundo.
- Diálogos de confirmación antes de acciones destructivas (eliminar producto, rechazar pedido, sobrescribir datos en sincronización).
- Consistencia de diseño entre Android y Windows: usar Material 3 en ambas plataformas.

#### 2.3 Disponibilidad y tolerancia a fallos

- La aplicación debe ser completamente funcional sin conexión a internet (modo offline).
- En caso de corrupción de un archivo JSON (ej. error de escritura), la aplicación debe mostrar un mensaje de error y permitir restaurar desde el respaldo automático más reciente (la app debe mantener al menos los últimos 3 respaldos en `respaldos/`).
- La sincronización USB debe ser atómica: si falla en mitad del proceso, se debe restaurar el estado anterior usando las copias de seguridad.

#### 2.4 Mantenibilidad

- Código fuente organizado por módulos (separación clara de lógica de negocio, interfaz de usuario, servicios de almacenamiento, integración con WhatsApp).
- Documentación del código (comentarios en inglés o español) para funciones críticas.
- El sistema de logs (`logs/`) debe registrar eventos importantes (inicio, errores, sincronizaciones, cambios de estado de pedidos) con nivel de detalle configurable.

#### 2.5 Portabilidad

- La aplicación para Windows debe distribuirse como un ejecutable `.exe` autónomo (no requiere instalación adicional). Puede generarse con `flutter build windows --release`.
- La aplicación para Android se distribuye como un archivo `.apk` o `.aab` (firmado con clave de depuración para uso personal).

---

### 3. STACK TECNOLÓGICO

#### 3.1 Lenguaje y framework

| Componente | Tecnología | Versión | Justificación |
|------------|------------|---------|----------------|
| Framework principal | Flutter | 3.22+ (stable) | Multiplataforma (Android y Windows) con un solo código base. |
| Lenguaje | Dart | 3.4+ | Moderno, tipado, fácil de aprender, excelente integración con Flutter. |
| IDE principal | Visual Studio Code | Última estable | Ligero, extensiones Flutter/Dart, depuración integrada. |
| IDE alternativo | Android Studio | Última estable | Necesario para emulador y gestión de SDK de Android. |

#### 3.2 Librerías y paquetes (pub.dev)

| Paquete | Propósito | Versión recomendada |
|---------|-----------|----------------------|
| `flutter_local_notifications` | Mostrar notificaciones propias de la app (alertas de consultas/pedidos) | ^17.0.0 |
| `notification_listener_service` | Escuchar notificaciones de WhatsApp (requiere implementación nativa adicional) | ^1.0.0 |
| `url_launcher` | Abrir WhatsApp con mensaje pre‑cargado | ^6.3.0 |
| `path_provider` | Obtener rutas de almacenamiento local | ^2.1.0 |
| `shared_preferences` | Guardar configuración simple (ej. último token de sincronización) | ^2.3.0 |
| `file_picker` | Seleccionar archivos para importar respaldos | ^8.0.0 |
| `excel` | Exportar datos a formato Excel (`.xlsx`) | ^4.0.0 |
| `csv` | Exportar datos a CSV (alternativa ligera) | ^5.1.0 |
| `crypto` | Calcular hash MD5 para fotos | ^3.0.0 |
| `image_picker` | Tomar o seleccionar fotos para productos | ^1.1.0 |
| `flutter_image_compress` | Redimensionar fotos a 1024px de ancho y calidad 80% | ^2.3.0 |

**Nota:** El paquete `notification_listener_service` requiere código nativo (Kotlin/Java) en Android para implementar `NotificationListenerService`. Se debe escribir un plugin personalizado o usar canales de plataforma.

#### 3.3 Herramientas de desarrollo adicionales

- **Git**: control de versiones (repositorio local o remoto privado).
- **Postman** (opcional): para pruebas de APIs (aunque no se usen APIs externas).
- **Android SDK Platform 35**: para compilar y probar en Android 15.

---

### 4. REQUERIMIENTOS DE INSTALACIÓN (para el entorno de desarrollo)

#### 4.1 Hardware mínimo (Windows)

- **Sistema operativo:** Windows 10 (64-bit) o Windows 11.
- **RAM:** 8 GB (16 GB recomendados si se usa emulador de Android).
- **Disco duro:** 10 GB libres (más espacio para proyectos y emulador).
- **Procesador:** 64-bit, 2 núcleos (4 núcleos recomendados).

#### 4.2 Hardware para pruebas (Android)

- Dispositivo físico con Android 10 o superior (para pruebas reales de notificaciones).
- Alternativa: emulador de Android (API 29+) pero el monitoreo de notificaciones no funciona en emulador (requiere dispositivo físico).

#### 4.3 Software necesario (todo gratuito)

| Software | Propósito | Enlace de descarga |
|----------|-----------|---------------------|
| Flutter SDK | Framework | [flutter.dev](https://flutter.dev) |
| Git for Windows | Requerido por Flutter | [git-scm.com](https://git-scm.com) |
| Visual Studio 2022 Community | Compilar para Windows (necesita workload "Desarrollo de escritorio con C++") | [visualstudio.microsoft.com](https://visualstudio.microsoft.com) |
| Android Studio | SDK de Android, emulador | [developer.android.com/studio](https://developer.android.com/studio) |
| VS Code (opcional) | Editor ligero | [code.visualstudio.com](https://code.visualstudio.com) |

#### 4.4 Configuración inicial (comandos)

```powershell
# Verificar instalación de Flutter
flutter doctor

# Aceptar licencias de Android
flutter doctor --android-licenses

# Crear proyecto
flutter create zapatillas_app

# Compilar para Windows
flutter build windows

# Compilar APK para Android
flutter build apk --release

4.5 Checklist de verificación para el desarrollador
Flutter doctor muestra todos los checkmarks verdes.

Visual Studio 2022 instalado con workload de C++.

Android Studio instalado y SDK Platform 35 descargado.

Dispositivo Android físico conectado y con depuración USB habilitada.

Permiso de notificaciones concedido a la aplicación (en el dispositivo de pruebas).

5. MATRIZ DE DECISIONES TÉCNICAS (resumen)
Decisión	Opción elegida	Alternativas descartadas	Razón
Persistencia de datos	Archivos JSON locales	SQLite, Hive, Firebase	Simplicidad, facilidad de sincronización manual, portabilidad.
Sincronización entre dispositivos	USB manual + merge por timestamp	Sincronización por nube (Drive, Firebase)	Privacidad total, sin costos, control manual.
Detección de mensajes WhatsApp	Monitoreo de notificaciones (NotificationListenerService)	WhatsApp Business API, leer base de datos de WhatsApp	Gratuito, sin necesidad de cuenta comercial, funciona offline.
Framework multiplataforma	Flutter	React Native, .NET MAUI, nativo Android + Windows	Un solo código para ambas plataformas, madurez en escritorio, buena comunidad.
Manejo de fotos	Copia incremental por timestamp + hash	Copia total cada sincronización	Ahorro de tiempo y ancho de banda USB.
Sin pasarela de pagos	No implementar	Integrar Yape, Plin, PayPal	Requisito explícito del usuario, simplifica desarrollo y evita riesgos legales.
Formato de tallas	Soporte para medios puntos (ej. 38.5)	Solo tallas enteras	Basado en datos reales de zapatillas (ej. Converse 38.5).

6. GLOSARIO DE TÉRMINOS
Término	Definición
ABM	Altas, Bajas y Modificaciones (CRUD).
Bidireccional	Sincronización en ambos sentidos (celular → PC y PC → celular).
EAN-13	Código de barras de 13 dígitos usado en productos.
ISO 8601	Formato de fecha/hora estándar: YYYY-MM-DDThh:mm:ss.
Merge	Fusión de dos conjuntos de datos basada en reglas (ej. timestamp más reciente).
MTP	Media Transfer Protocol, usado para conectar Android por USB y acceder a archivos.
MTP	Medio punto (talla 38.5). Se representa como string "38.5".
NotificationListenerService	Servicio de Android que escucha notificaciones del sistema.
Offline-first	La aplicación funciona sin conexión a internet, sincronizando solo cuando es posible.
Regex	Expresión regular para extraer patrones de texto.
SSOT	Single Source of Truth – documento único de especificaciones.
Timestamp	Marca de tiempo (fecha y hora) usada para comparar versiones.
URL Scheme	Mecanismo para abrir una aplicación con parámetros desde una URL (ej. https://wa.me/...).

FIN DE LA PARTE 4