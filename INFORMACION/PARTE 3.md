# DOCUMENTACIÓN TÉCNICA SSOT – SISTEMA DE GESTIÓN DE VENTAS DE ZAPATILLAS POR WHATSAPP

**Versión:** 1.0  
**Fecha:** 2026-06-09  
**Tipo:** Especificación técnica para desarrollo (sin mockups ni elementos visuales)

---

## PARTE 3: INTEGRACIÓN CON WHATSAPP, ALMACENAMIENTO LOCAL Y SINCRONIZACIÓN USB

---

### 1. INTEGRACIÓN CON WHATSAPP

La aplicación debe interactuar con WhatsApp instalado en el mismo dispositivo Android (no se soporta integración con WhatsApp Business API). Se utilizan dos mecanismos: monitoreo de notificaciones (para detectar mensajes entrantes) y URL Scheme (para abrir WhatsApp con mensajes pre‑cargados).

#### 1.1 Monitoreo de notificaciones (Android)

- **Permiso requerido:** `android.permission.NOTIFICATION_LISTENER`. El usuario debe activarlo manualmente desde ajustes del sistema (la app debe guiarlo).
- **Servicio en segundo plano:** Se implementa un `NotificationListenerService` que escucha todas las notificaciones.
- **Filtrado:** Se identifica el paquete `com.whatsapp` (o `com.whatsapp.w4b` para WhatsApp Business). Solo se procesan notificaciones de WhatsApp.
- **Extracción de datos:** De la notificación se obtiene:
  - El texto del mensaje (campo `EXTRA_TEXT`).
  - El título (normalmente el número de teléfono o nombre del contacto).
  - La hora de la notificación.
- **Evento generado:** Cada notificación se envía a la capa de Dart a través de un `EventChannel`. La aplicación principal recibe el evento y lo procesa.

#### 1.2 Procesamiento de mensajes entrantes

Cuando se recibe un evento, el sistema ejecuta el siguiente flujo:

1. **Registrar conversación:** Se crea o actualiza el registro de cliente (usando el número de teléfono extraído del título). Se almacena una nueva entrada en `conversaciones` con `tipo = "info"` (por defecto).
2. **Detección de consulta de talla:** Se aplica la regex `\b([1-9][0-9]?(?:\.[5])?)\b` sobre el texto. Si se encuentra un número entre 35 y 45 (incluyendo medios), se considera una consulta de talla:
   - Se actualiza `conversacion.tipo = "consulta"` y `conversacion.tallaConsultada`.
   - Se genera una **alerta sonora** (corta) y una notificación visual en el dispositivo.
   - Se guarda el evento en el historial.
3. **Detección de intención de compra:** Se buscan palabras clave en el texto (`quiero`, `comprar`, `pedir`, `llevo`, `me interesa`). Si se detectan:
   - Se inicia el flujo de pedido.
   - Se responde automáticamente (vía apertura de WhatsApp con mensaje pre‑cargado) pidiendo nombre, dirección y forma de pago.
   - Se actualiza `conversacion.tipo = "pedido"`.
   - Se genera una **alerta sonora** (larga) de nuevo pedido pendiente.
4. **Extracción de datos de respuesta:** Si el cliente responde al cuestionario, se aplican las regex definidas en la Parte 2 para extraer nombre, dirección y forma de pago. Una vez reunidos los tres, se crea un pedido en estado `pendiente` y se muestra al vendedor para su confirmación.

#### 1.3 Envío de mensajes (abrir WhatsApp con texto pre‑cargado)

Para enviar un catálogo o cualquier mensaje al cliente, la aplicación debe:

- Construir la URL: `https://wa.me/{telefono}?text={texto_url_encoded}`
- Utilizar `url_launcher` (Flutter) con `launchUrl` y modo `LaunchMode.externalApplication`.
- **Texto del catálogo:** Se genera mediante plantilla fija. Cada producto se muestra en una línea separada con el formato:
━━━━━━━━━━━━━━━━━━━━
👟 {marca} {modelo}
🎨 Color: {color}
💰 Precio: S/{precio}
📦 Stock: {stock} unidades

Al final se agrega la invitación a responder con nombre, dirección y forma de pago.
- **Envío de fotos:** No se pueden enviar automáticamente. La aplicación solo abre la conversación con el texto; el vendedor debe adjuntar las fotos manualmente en WhatsApp (por diseño).

#### 1.4 Detección de “visto”

- Cuando el cliente abre el mensaje, WhatsApp emite una notificación de “visto” (cambia el estado del mensaje). El `NotificationListenerService` captura esa notificación y la envía a la app.
- La app asocia ese evento con la conversación más reciente de ese cliente y actualiza `clienteVio = true` y `clienteVioFecha`.
- Se genera una alerta (sonido configurable) de “Cliente vio el catálogo”.

#### 1.5 Permisos necesarios (Android)

Además del `NOTIFICATION_LISTENER`, se requieren:

- `INTERNET` (para las URLs de WhatsApp).
- `VIBRATE` (para alertas con vibración).
- `POST_NOTIFICATIONS` (Android 13+).
- `CAMERA` y `READ_EXTERNAL_STORAGE` (opcionales, solo si la app permite tomar fotos de productos directamente).

El usuario debe conceder estos permisos en el primer inicio; la app debe explicar por qué son necesarios.

---

### 2. ALMACENAMIENTO LOCAL

#### 2.1 Estructura de carpetas

La aplicación crea una carpeta raíz en cada plataforma:

- **Android:** `/storage/emulated/0/ZapatillasApp/`
- **Windows:** `%USERPROFILE%\Documents\ZapatillasApp\`

Dentro se encuentran:

- `datos.json`: base de datos principal.
- `metadata_fotos.json`: metadatos de imágenes.
- `sync_control.json`: información de sincronización.
- `configuracion.json`: preferencias del usuario.
- `fotos/`: subcarpeta que contiene todas las imágenes (formato JPG, calidad 80%).
- `respaldos/`: carpeta donde se guardan las exportaciones manuales (ZIP o JSON copiado).
- `logs/` (opcional): archivos de registro para depuración.

#### 2.2 Mecanismo de persistencia

- **Lectura:** Al iniciar la app, se cargan todos los JSON en memoria. Si algún archivo no existe, se crea con la estructura por defecto.
- **Escritura:** Cada vez que se modifica una entidad (crear, editar, eliminar, cambiar estado, descontar stock), se actualiza el objeto en memoria y se **escribe inmediatamente** el archivo JSON completo (`datos.json` o el correspondiente) en disco. No se usan transacciones diferidas.
- **Concurrencia:** Como es una app de un solo usuario, no se requieren bloqueos complejos. Sin embargo, en Windows se debe evitar que dos instancias de la app accedan al mismo archivo (se puede usar un `FileLock` básico).

#### 2.3 Manejo de fotos

- **Captura:** El vendedor puede tomar fotos con la cámara o seleccionarlas de la galería. La app debe redimensionarlas (ancho máximo 1024 píxeles, calidad 80% JPG) para ahorrar espacio.
- **Nombre del archivo:** `{zapatilla_id}_{tipo}.jpg`, donde `tipo` es `frontal` o `perfil`.
- **Registro en `metadata_fotos.json`:** Al guardar una foto, se genera su entrada con:
- `timestamp`: fecha y hora actual UTC.
- `tamaño_bytes`: tamaño del archivo.
- `hash_md5`: (opcional pero recomendado) para detectar cambios reales.
- `zapatilla_id`: referencia.
- `sync_status`: `"sincronizada"` si se creó en el dispositivo actual; `"pendiente_desktop"` o `"pendiente_android"` según corresponda cuando se crea en un dispositivo y aún no se ha sincronizado.
- **Actualización:** Si se reemplaza una foto (por ejemplo, se toma una mejor), se actualiza el `timestamp` y se recalcula el hash. La sincronización posterior lo detectará como cambio.

---

### 3. SINCRONIZACIÓN ENTRE DISPOSITIVOS (USB)

La sincronización es **manual** y **bidireccional**. Solo se ejecuta cuando el vendedor conecta el celular Android a la PC Windows por USB y presiona el botón "Sincronizar USB" en la aplicación de Windows.

#### 3.1 Detección de la ruta del celular

- Al presionar el botón, la app de Windows debe listar las unidades disponibles y buscar una que contenga la carpeta `ZapatillasApp` en su raíz.
- Si encuentra múltiples, puede mostrar una lista para que el usuario elija, o tomar la primera (se puede configurar).
- La ruta típica es `X:\ZapatillasApp\` donde `X` es la letra de unidad asignada al celular en modo MTP.

#### 3.2 Algoritmo de fusión (merge)

El proceso debe ser atómico para evitar corrupción de datos. Se recomienda seguir estos pasos:

1. **Hacer una copia de seguridad** de los archivos `datos.json` y `metadata_fotos.json` de ambos dispositivos en la carpeta `respaldos/` con marca de tiempo (por si hay que revertir).
2. **Cargar en memoria** ambos `datos.json` y `metadata_fotos.json`.
3. **Fusionar `datos.json`:** Para cada tipo de entidad (zapatillas, clientes, pedidos, conversaciones):
 - Crear un mapa de ID a objeto para cada lado.
 - Para cada ID presente en la unión de ambos mapas:
   - Si solo en celular → agregar a la lista de Windows.
   - Si solo en Windows → agregar a la lista del celular.
   - Si en ambos → comparar el campo de timestamp que indica última modificación. Para zapatillas es `ultimaModificacion`, para clientes y pedidos también existe el mismo campo. **Gana el más reciente** y se copia al otro dispositivo (sobrescribe el más antiguo).
4. **Fusionar `metadata_fotos.json`** (para fotos):
 - Recorrer todas las claves (nombres de archivo) presentes en ambos metadatos.
 - Si una foto solo existe en un lado, se marca para copiar al otro lado.
 - Si existe en ambos, comparar `timestamp`. Si el timestamp del origen es mayor que el del destino, se copia el archivo y se actualiza el metadata en destino. Si son iguales, se omite (se considera ya sincronizado).
5. **Copiar archivos de fotos** (solo los que se han marcado):
 - Leer el archivo de origen, escribirlo en destino.
 - Actualizar la entrada `metadata_fotos.json` del destino con el `timestamp`, `tamaño_bytes` y `hash_md5` del origen.
6. **Escribir los JSON resultantes** en ambos dispositivos (sobrescribir los originales).
7. **Actualizar `sync_control.json`** en ambos dispositivos:
 - `ultimaSincronizacion = ahora()`
 - `syncToken` = nuevo UUID
 - Agregar la fecha al `historialSync` (limitar a los últimos 10 elementos para no crecer indefinidamente).
8. **Generar reporte** con las siguientes métricas:
 - Número de registros nuevos en cada dirección.
 - Número de registros actualizados (conflictos resueltos).
 - Número de fotos copiadas.
 - Tiempo total de la operación.

#### 3.3 Resolución de conflictos

- **Conflicto de datos con igual timestamp:** Por diseño, si dos registros tienen exactamente la misma `ultimaModificacion` (lo cual es raro pero posible), se da prioridad al registro de Windows. Se debe registrar esta acción en el reporte.
- **Conflicto de fotos con igual timestamp pero diferente hash:** Se considera que la foto ha cambiado a pesar del mismo timestamp (puede deberse a ajuste manual del reloj). El sistema debe:
- Mostrar un diálogo al usuario preguntando qué versión conservar (la del celular o la de Windows).
- O, como alternativa automática, copiar la de mayor tamaño (asumiendo que es de mejor calidad).
- **Fotos huérfanas:** Si una foto referenciada en `datos.json` no existe en la carpeta `fotos/`, se debe eliminar la referencia (o mostrar una advertencia). Por el contrario, si una foto en la carpeta no está referenciada, se puede dejar o eliminar (configurable).

#### 3.4 Requisitos adicionales de sincronización

- La sincronización no debe requerir que ambas aplicaciones estén ejecutándose al mismo tiempo. Windows actúa como maestro y es quien inicia el proceso.
- El celular debe tener la carpeta `ZapatillasApp` accesible por MTP; no se requiere depuración USB.
- Durante la sincronización, la app de Windows debe mostrar una barra de progreso y permitir la cancelación solo al inicio (una vez iniciada la copia de archivos, no se debe interrumpir).

---

**FIN DE LA PARTE 3**