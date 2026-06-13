# DOCUMENTACIÓN TÉCNICA SSOT – SISTEMA DE GESTIÓN DE VENTAS DE ZAPATILLAS POR WHATSAPP

**Versión:** 1.0  
**Fecha:** 2026-06-09  
**Tipo:** Especificación técnica para desarrollo (sin mockups ni elementos visuales)

---

## PARTE 2: MODELO DE DATOS Y REGLAS DE NEGOCIO

---

### 1. ENTIDADES Y ESTRUCTURA DE DATOS

El sistema almacena toda la información en archivos JSON ubicados en una carpeta fija (`ZapatillasApp` en la raíz del almacenamiento externo en Android y en `Documentos/ZapatillasApp` en Windows). A continuación se definen las entidades, sus atributos, tipos, restricciones y relaciones.

#### 1.1 Zapatilla (producto)

- **Identificador único:** `id` (string). Formato `zap_YYYYMMDD_NNN` donde `YYYYMMDD` es la fecha de creación y `NNN` un número secuencial de tres dígitos.
- **Marca** (string, obligatorio): nombre comercial, entre 1 y 50 caracteres.
- **Modelo** (string, obligatorio): nombre del modelo, entre 1 y 100 caracteres.
- **Color** (string, obligatorio): descripción del color principal, libre.
- **Foto frontal** (string, obligatorio): ruta relativa a la imagen frontal dentro de la subcarpeta `fotos/`. Ejemplo: `"fotos/zap_001_frontal.jpg"`.
- **Foto de perfil** (string, opcional): ruta relativa a la imagen lateral.
- **Precio** (decimal, obligatorio): valor en soles, mayor que 0, con dos decimales.
- **Stock por talla** (mapa de string a entero, obligatorio): claves que representan tallas (valores admitidos: `"35"`, `"35.5"`, `"36"`, `"36.5"`, ..., hasta `"45"`). Los valores son enteros mayores o iguales a cero. Si una talla no aparece en el mapa, se interpreta como stock 0.
- **Fecha de creación** (string ISO 8601 UTC, obligatorio): `YYYY-MM-DDThh:mm:ss`.
- **Última modificación** (string ISO 8601 UTC, obligatorio): se actualiza cada vez que se modifica cualquier campo.

#### 1.2 Cliente

- **Teléfono** (string, clave primaria): número de WhatsApp con código de país, formato `+51XXXXXXXXX` (10 a 15 dígitos).
- **Nombre** (string, opcional): se completa cuando el cliente lo proporciona en su primer pedido.
- **Dirección predeterminada** (string, opcional): se guarda la última dirección usada por el cliente.
- **Historial de pedidos** (lista de strings, obligatoria): array de `id` de pedidos que han sido entregados.
- **Último contacto** (string ISO 8601 UTC, obligatorio): fecha y hora de la última interacción (mensaje recibido o pedido).

#### 1.3 Pedido

- **ID** (string): formato `ped_YYYYMMDD_NNN`.
- **Teléfono del cliente** (string): referencia al cliente.
- **Nombre del cliente** (string): copia del nombre al momento del pedido (puede diferir del guardado en cliente).
- **Dirección de entrega** (string): dirección para este pedido.
- **Fecha del pedido** (ISO): cuando el cliente manifestó la intención de compra.
- **Fecha de envío** (ISO, opcional): cuando el vendedor marcó como "enviado".
- **Fecha de entrega** (ISO, opcional): cuando el vendedor marcó como "entregado".
- **Estado** (string): uno de `pendiente`, `confirmado`, `enviado`, `entregado`, `rechazado`.
- **Forma de pago** (string): `transferencia` o `contraentrega`.
- **Total** (decimal): suma de `precio * cantidad` de los productos.
- **Productos** (lista de objetos ItemPedido): cada objeto contiene:
  - `zapatillaId` (string)
  - `marca`, `modelo`, `talla`, `precio`, `cantidad` (siempre 1 por ahora).

#### 1.4 Conversación

- **ID** (string): formato `conv_YYYYMMDD_NNN`.
- **Teléfono del cliente** (string).
- **Fecha** (ISO): momento del mensaje entrante.
- **Tipo** (string): `consulta`, `pedido` o `info`.
- **Talla consultada** (entero, solo si tipo = `consulta`).
- **Respuesta enviada** (booleano): indica si el vendedor ya contestó.
- **Cliente vio** (booleano): detectado por cambio de estado en la notificación de WhatsApp.
- **Fecha de visto** (ISO, opcional).

#### 1.5 Archivos de almacenamiento

- `datos.json`: contiene arrays de zapatillas, clientes, pedidos y conversaciones, más un campo `version` y `ultimaActualizacion`.
- `metadata_fotos.json`: para cada foto, guarda `timestamp`, `tamaño_bytes`, `hash_md5` (opcional), `zapatilla_id` y `sync_status` (`sincronizada`, `pendiente_desktop`, `pendiente_android`).
- `sync_control.json`: guarda `ultimaSincronizacion`, `dispositivoId`, `syncToken`, `historialSync`.
- `configuracion.json`: guarda `horarioInicio`, `horarioFin`, `horarioActivo`, `zonasEnvio` (array de strings), `costoEnvioFijo`, `alertaSonido`, `reporteDiario`, `reporteSemanal`.

---

### 2. REGLAS DE NEGOCIO

#### 2.1 Reglas de stock

- **STK-01:** El stock se maneja por talla individualmente.
- **STK-02:** Al confirmar un pedido, si la talla queda con stock 1 antes de descontar, se muestra una alerta de "última unidad". Si queda 0 después del descuento, también se registra pero sin alerta adicional.
- **STK-03:** Los productos con stock 0 para una talla no aparecen en el catálogo filtrado por esa talla.
- **STK-04:** El descuento de stock solo ocurre cuando el vendedor confirma el pedido, no en el estado `pendiente`.
- **STK-05:** No se puede confirmar un pedido si `stockPorTalla[talla] < cantidad_solicitada`.

#### 2.2 Reglas de pedidos

- **PED-01:** Antes de crear un pedido, el sistema debe solicitar nombre, dirección y forma de pago. Solo después de recibirlos se crea el pedido en estado `pendiente`.
- **PED-02:** El estado inicial es `pendiente`.
- **PED-03:** Solo el vendedor puede cambiar el estado a `confirmado`, `enviado` o `entregado`.
- **PED-04:** El stock no se descuenta hasta la confirmación.
- **PED-05:** Un pedido puede contener múltiples productos (ítems).
- **PED-06:** Al marcar `entregado`, el `id` del pedido se agrega al `historialPedidos` del cliente.

#### 2.3 Reglas de catálogo (generación de mensaje)

- **CAT-01:** El catálogo enviado por WhatsApp contiene solo productos con stock > 0 para la talla solicitada.
- **CAT-02:** El formato del mensaje es texto plano, con separadores visuales (`━━━━`), y para cada producto se incluye: marca, modelo, color, precio y stock.
- **CAT-03:** Las fotos no se incluyen en el texto del mensaje. Si se desea enviar imágenes, se envían como archivos adjuntos (la app abre WhatsApp con el texto y luego el vendedor puede adjuntar manualmente).
- **CAT-04:** Si el stock es 0 (caso borde), se muestra la palabra "Agotado".

#### 2.4 Reglas de notificaciones y alertas

- **NOT-01:** Toda consulta de talla y nuevo pedido generan una notificación en el sistema Android/Windows con sonido (configurable).
- **NOT-02:** Se usan sonidos diferentes para consulta (corto) y pedido (largo).
- **NOT-03:** Si `horarioActivo` es `true` y la hora actual está fuera del rango `horarioInicio`-`horarioFin`, las alertas son silenciosas (solo notificación visual).
- **NOT-04:** Cuando se detecta que el cliente ha visto el mensaje (por el cambio de estado en la notificación de WhatsApp), se registra `clienteVio = true` y se emite una alerta (con sonido) de "Cliente vio catálogo".

#### 2.5 Reglas de clientes

- **CLI-01:** Un cliente se crea automáticamente la primera vez que envía un mensaje, con solo el `telefono` y `ultimoContacto`.
- **CLI-02:** El `telefono` es único y no se pueden duplicar.
- **CLI-03:** Cada vez que se entrega un pedido, su ID se agrega a `historialPedidos`.
- **CLI-04:** Cuando un cliente proporciona su nombre o dirección en un pedido, se actualiza el campo correspondiente en el objeto Cliente.
- **CLI-05:** El campo `clienteDireccion` en el pedido es una copia independiente; puede ser diferente de `direccionDefault` para ese envío.

---

### 3. EXTRACCIÓN DE DATOS DESDE TEXTO (REGEX)

El procesador de mensajes debe aplicar las siguientes expresiones regulares al texto entrante (en orden de prioridad):

| Dato | Patrón | Ejemplo | Nota |
|------|--------|---------|------|
| Talla | `\b([1-9][0-9]?(?:\.[5])?)\b` | `43`, `38.5` | Captura enteros y medios puntos |
| Nombre | `([A-Z][a-z]+(?:\s+[A-Z][a-z]+)+)` | `Juan Pérez` | Dos o más palabras con mayúscula inicial |
| Dirección | `(?:calle|av|jr|psje|Mz|Lt|Calle|Avenida|Jirón|Pasaje)\s+.+` | `Av. Siempre Viva 123` | Detecta palabras clave de dirección |
| Forma de pago | `transferencia|contraentrega|yape|plin|efectivo` | `contraentrega` | Se normaliza a `transferencia` o `contraentrega` |

**Procedimiento de extracción para un pedido:**

1. Si el mensaje contiene palabras clave de compra (`quiero`, `comprar`, `pedir`, `llevo`), iniciar flujo de pedido.
2. Buscar en el mismo mensaje o en mensajes posteriores del mismo cliente (misma conversación) los campos nombre, dirección y forma de pago utilizando los patrones.
3. Si falta alguno, el sistema debe responder preguntando específicamente ese campo.
4. Una vez reunidos los tres campos, crear el pedido en estado `pendiente`.

---

### 4. SISTEMA DE ARCHIVOS Y SINCRONIZACIÓN

#### 4.1 Estructura de directorios

- **Android:** `/storage/emulated/0/ZapatillasApp/`
- **Windows:** `%USERPROFILE%\Documents\ZapatillasApp\`

Subdirectorios y archivos:
- `datos.json`
- `metadata_fotos.json`
- `sync_control.json`
- `configuracion.json`
- `fotos/` (carpeta con imágenes)
- `respaldos/` (carpeta para exportaciones manuales)
- `logs/` (opcional, para depuración)

#### 4.2 Sincronización USB (bidireccional)

El algoritmo de sincronización (invocado manualmente por el vendedor desde la versión de Windows) debe:

1. Detectar la ruta del celular conectado (por ejemplo, `E:\ZapatillasApp\`).
2. Cargar `datos.json` de ambos dispositivos en memoria.
3. Para cada entidad (zapatillas, clientes, pedidos, conversaciones):
   - Crear un mapa por `id`.
   - Recorrer la unión de IDs de ambos mapas.
   - Si el ID solo está en celular → copiar a Windows.
   - Si solo está en Windows → copiar al celular.
   - Si está en ambos → comparar `ultimaModificacion` (o `fechaModificacion` según la entidad). El registro con fecha más reciente prevalece y se copia al otro dispositivo.
4. Guardar el JSON resultante en ambos dispositivos (sobrescribir).
5. Para fotos: comparar `metadata_fotos.json`. Copiar solo aquellos archivos cuyo `timestamp` en origen sea más reciente que en destino. Si la foto no existe en destino, copiar también. Si el `timestamp` es igual, omitir.
6. Actualizar `sync_control.json` en ambos con la nueva fecha y generar un nuevo `syncToken`.

#### 4.3 Manejo de conflictos

- En caso de empate de timestamp (misma fecha), se conserva el registro de Windows (desktop) por convención, pero se puede añadir una regla de desempate usando el `dispositivoId` (alfabéticamente).
- Si hay conflicto de fotos con igual timestamp pero diferente hash MD5, se considera que la foto ha cambiado y se debe copiar la más reciente según el hash (no hay regla automática, se notifica al vendedor).

---

**FIN DE LA PARTE 2**