# DOCUMENTACIÓN TÉCNICA SSOT (Single Source of Truth)
## SISTEMA DE GESTIÓN DE VENTAS DE ZAPATILLAS POR WHATSAPP

**Versión:** 1.0  
**Fecha:** 2026-06-09  
**Tipo:** Documento técnico para desarrollo (sin elementos visuales)

---

## PARTE 1: INTRODUCCIÓN, VISIÓN GENERAL Y ARQUITECTURA

---

### 1. PROPÓSITO DEL DOCUMENTO

Este documento es la **Fuente Única de Verdad (SSOT)** para el desarrollo del sistema. Describe exclusivamente los requisitos técnicos, la arquitectura, los modelos de datos, las reglas de negocio, los flujos de trabajo y las integraciones necesarias. No contiene elementos de diseño de interfaz de usuario (mockups) ni wireframes; esos se definen por separado.

**Objetivo:** Que un desarrollador pueda implementar el sistema sin ambigüedades.

---

### 2. VISIÓN GENERAL DEL SISTEMA

**Nombre:** ShoeManager

**Tipo:** Aplicación de escritorio (Windows) y móvil (Android) para un único vendedor de zapatillas.

**Funcionalidad principal:** Gestionar inventario, detectar consultas de clientes por WhatsApp, enviar catálogos, procesar pedidos, controlar entregas y mantener historial, todo sin conexión a internet y sin sistemas de pago integrados.

**Filosofía de diseño:**
- **Offline-first:** 100% funcional sin internet.
- **Privacidad total:** Todos los datos se almacenan localmente en el dispositivo del vendedor.
- **Sin dependencias externas:** No se utilizan servidores, APIs de pago ni servicios en la nube.
- **Sincronización manual:** Los datos se sincronizan entre Android y Windows solo mediante cable USB.
- **Sin pasarelas de pago:** El vendedor gestiona los pagos externamente (transferencia, contraentrega, etc.).

---

### 3. ALCANCE DEL PROYECTO

#### 3.1 Incluido
- **Gestión de inventario:** ABM (altas, bajas, modificaciones) de zapatillas, con foto frontal, foto de perfil, precio, marca, modelo, color, y stock por talla (tallas de 35 a 45, soporte para medios puntos como 38.5).
- **Detección de mensajes de WhatsApp:** Monitoreo de notificaciones para identificar consultas de talla (ej. "talla 43") e intenciones de compra (palabras clave: "quiero", "comprar", "pedir").
- **Generación y envío de catálogo:** Filtrado automático por talla (solo productos con stock > 0), creación de texto con marca, modelo, precio y stock, y apertura de WhatsApp con mensaje pre-cargado.
- **Gestión de pedidos:** Captura de nombre, dirección y forma de pago del cliente; verificación de stock; confirmación manual por el vendedor; descuento automático de stock; cambio de estados: pendiente → confirmado → enviado → entregado.
- **Historial:** Registro de clientes (por número de teléfono), conversaciones y pedidos completados.
- **Sincronización USB:** Fusión bidireccional de archivos JSON entre Android y Windows basada en timestamp de modificación (solo copia lo nuevo/modificado).
- **Reportes:** Exportación de ventas a Excel/CSV y reporte semanal resumido (total ventas, cantidad de pedidos, productos más vendidos).
- **Configuración:** Horario laboral (para silenciar alertas), zonas de envío (lista editable), costo de envío fijo, activación/desactivación de sonido de alertas.

#### 3.2 Excluido
- Pasarelas de pago (por decisión explícita).
- Chatbot automático (las respuestas son manuales).
- Integración con servicios de courier (el seguimiento es manual).
- Múltiples vendedores o cuentas.
- Aplicación web o app para clientes (todo por WhatsApp).
- Facturación electrónica.
- Notificaciones push al cliente (solo mensajes por WhatsApp iniciados por el vendedor).
- Sincronización automática por internet o nube.
- API pública.

---

### 4. PLATAFORMAS OBJETIVO

| Plataforma | Función | Versión mínima | Obligatoria |
|------------|---------|----------------|--------------|
| Android | Aplicación principal (para uso en movimiento, con cámara y monitoreo de notificaciones) | Android 10 (API 29) | Sí |
| Windows | Aplicación secundaria (para gestión de stock masiva, reportes y sincronización) | Windows 10 (64-bit) | Opcional (recomendada) |
| WhatsApp | Canal de comunicación con clientes (instalado en el mismo celular Android) | Cualquier versión | Sí |

Requisito transversal: **ninguna de las dos versiones requiere conexión a internet** para funcionar.

---

### 5. USUARIOS DEL SISTEMA

- **Único usuario:** el vendedor (tú). Tiene todos los permisos: ABM de productos, gestión de stock, confirmación de pedidos, exportación, configuración.
- **Los clientes no son usuarios del sistema:** interactúan únicamente por WhatsApp.

---

### 6. CASOS DE USO (DESCRIPCIÓN TÉCNICA)

| ID | Caso de uso | Actor | Descripción resumida |
|----|-------------|-------|----------------------|
| CU-01 | Cargar inventario inicial | Vendedor | Agregar zapatillas con foto frontal, foto perfil, precio, marca, modelo, color y stockPorTalla (diccionario talla → entero). |
| CU-02 | Recibir consulta de talla | Sistema | Escuchar notificaciones de WhatsApp; extraer número de talla; generar alerta sonora; registrar conversación como "consulta". |
| CU-03 | Responder con catálogo | Vendedor | Seleccionar talla → sistema filtra zapatillas con stock>0 → genera mensaje de texto + abre WhatsApp con mensaje pre-cargado. |
| CU-04 | Recibir pedido | Sistema | Detectar intención de compra (palabras clave); responder automáticamente pidiendo nombre, dirección, forma de pago; extraer datos de la respuesta; verificar stock; mostrar alerta de pedido pendiente. |
| CU-05 | Confirmar pedido | Vendedor | Revisar pedido; presionar "Confirmar"; sistema descuenta stock (actualiza stockPorTalla); cambia estado a "confirmado"; envía mensaje de confirmación al cliente. |
| CU-06 | Marcar como enviado | Vendedor | Cambiar estado a "enviado"; registrar fechaEnvio; enviar aviso al cliente. |
| CU-07 | Marcar como entregado | Vendedor | Cambiar estado a "entregado"; registrar fechaEntrega; actualizar historial del cliente. |
| CU-08 | Gestionar stock | Vendedor | Editar manualmente stockPorTalla de cualquier zapatilla. |
| CU-09 | Sincronizar por USB | Vendedor | Conectar celular a PC; la app de Windows lee el JSON del celular; fusiona registros por ID y fecha_modificacion (el más reciente gana); copia fotos nuevas; actualiza ambos JSON. |
| CU-10 | Exportar reportes | Vendedor | Generar archivo Excel o CSV con ventas del período seleccionado; opcionalmente exportar lista de clientes. |

---

### 7. ARQUITECTURA DEL SISTEMA

#### 7.1 Diagrama de componentes (textual)

El sistema se compone de los siguientes módulos dentro de la misma aplicación (multiplataforma):

- **Módulo de notificaciones de WhatsApp:** Servicio en segundo plano (Android) que escucha las notificaciones del sistema. Cuando detecta un mensaje de WhatsApp, extrae el texto y el número del remitente, y dispara eventos internos.
- **Procesador de mensajes:** Aplica expresiones regulares para identificar talla (ej. `\b([1-9][0-9]?)\b`), palabras clave de compra, y extrae nombre, dirección y forma de pago de la conversación.
- **Motor de stock:** Mantiene en memoria el diccionario `stockPorTalla` de cada zapatilla; proporciona métodos para consultar stock, descontar stock y verificar disponibilidad.
- **Generador de catálogo:** Toma una lista de zapatillas y una talla; genera un string en texto plano con formato predefinido (marca, modelo, precio, stock) para enviar por WhatsApp.
- **Gestor de pedidos:** Maneja la máquina de estados de cada pedido (pendiente, confirmado, enviado, entregado, rechazado). Almacena pedidos en JSON.
- **Exportador:** Convierte listas de pedidos y clientes a formato Excel (usando librería) o CSV.
- **Módulo de sincronización USB:** Escanea la ruta del celular conectado (MTP), compara archivos JSON, aplica merge, copia fotos solo si el timestamp es más reciente.
- **Almacenamiento local:** Todas las entidades se guardan en archivos JSON dentro de una carpeta fija (`ZapatillasApp` en la raíz del almacenamiento externo en Android, y en `Documentos/ZapatillasApp` en Windows).

#### 7.2 Flujo de datos interno (descripción)

1. El **módulo de notificaciones** recibe una notificación de WhatsApp.
2. **Procesador de mensajes** extrae la talla (si existe) y la intención.
3. Si es una consulta, se **genera una alerta sonora** y se guarda una conversación de tipo "consulta".
4. El vendedor abre la app, selecciona la talla consultada. El sistema **filtra el inventario** por esa talla (solo productos con stock > 0).
5. El vendedor pulsa "Enviar catálogo". El **generador de catálogo** produce el texto y abre WhatsApp con el mensaje pre-cargado.
6. Cuando el cliente responde con un pedido, el **procesador** extrae nombre, dirección y forma de pago, y crea un pedido en estado "pendiente".
7. El vendedor confirma el pedido; el **motor de stock** descuenta las unidades y el **gestor de pedidos** cambia el estado a "confirmado".
8. Los cambios se **guardan inmediatamente en los archivos JSON**.
9. Periódicamente (o manualmente), el **módulo de sincronización USB** fusiona los datos entre Android y Windows.

---

### 8. DECISIONES ARQUITECTÓNICAS CLAVE

| Decisión | Opción elegida | Justificación |
|----------|----------------|----------------|
| Persistencia | Archivos JSON | Simplicidad, portabilidad, fácil depuración, sin base de datos externa. |
| Sincronización | USB manual con merge por timestamp | Sin nube, privacidad total, evita conflictos complejos. |
| Detección de WhatsApp | Monitoreo de notificaciones (NotificationListenerService) | No requiere API oficial, funciona sin internet, gratuito. |
| Lenguaje/Framework | Flutter (Dart) | Multiplataforma (Android y Windows con un solo código), madurez en escritorio. |
| Manejo de fotos | Almacenamiento local + metadata_fotos.json con timestamp y hash | Permite sincronización incremental (solo copiar fotos nuevas o modificadas). |

---

**FIN DE LA PARTE 1**