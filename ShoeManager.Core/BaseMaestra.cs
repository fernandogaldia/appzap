using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ShoeManager.Core {
    public class BaseMaestra {
        /// <summary>
        /// Contadores para IDs cronológicos autoincrementales por día.
        /// Key: "YYYYMMDD", Value: último contador usado.
        /// </summary>
        private static readonly Dictionary<string, int> _contadoresZap = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> _contadoresPed = new Dictionary<string, int>();

        public string Version { get; set; } = "1.0.0";
        public string DispositivoMaestroID { get; set; } = "PC-CENTRAL";
        public DateTime UltimaSincronizacion { get; set; } = DateTime.Now;
        public List<Saldo> Saldos { get; set; } = new List<Saldo>();
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public List<MensajeInterceptado> Mensajes { get; set; } = new List<MensajeInterceptado>();

        public static BaseMaestra Cargar(string rutaArchivo) {
            if (!File.Exists(rutaArchivo)) {
                var nuevaBase = new BaseMaestra();
                nuevaBase.InicializarInventarioPredeterminado();
                nuevaBase.Guardar(rutaArchivo);
                return nuevaBase;
            }
            string json = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<BaseMaestra>(json) ?? new BaseMaestra();
        }

        public void Guardar(string rutaArchivo) {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(rutaArchivo, json);
        }

        /// <summary>
        /// Genera un ID cronológico autoincremental con formato ZAP-AAAAMMDD-NNN.
        /// Ejemplo: ZAP-20260618-001, ZAP-20260618-002, ...
        /// </summary>
        public string GenerarIdZap() {
            string hoy = DateTime.Now.ToString("yyyyMMdd");
            if (!_contadoresZap.ContainsKey(hoy)) {
                // Buscar el máximo existente para hoy en la lista de saldos
                int maxExistente = 0;
                foreach (var s in Saldos) {
                    if (s.ID.StartsWith($"ZAP-{hoy}-")) {
                        string numeroStr = s.ID.Substring($"ZAP-{hoy}-".Length);
                        if (int.TryParse(numeroStr, out int num) && num > maxExistente)
                            maxExistente = num;
                    }
                }
                _contadoresZap[hoy] = maxExistente;
            }
            _contadoresZap[hoy]++;
            return $"ZAP-{hoy}-{_contadoresZap[hoy]:D3}";
        }

        /// <summary>
        /// Genera un ID cronológico autoincremental con formato PED-AAAAMMDD-NNN.
        /// Ejemplo: PED-20260618-001, PED-20260618-002, ...
        /// </summary>
        public string GenerarIdPed() {
            string hoy = DateTime.Now.ToString("yyyyMMdd");
            if (!_contadoresPed.ContainsKey(hoy)) {
                int maxExistente = 0;
                foreach (var p in Pedidos) {
                    if (p.ID.StartsWith($"PED-{hoy}-")) {
                        string numeroStr = p.ID.Substring($"PED-{hoy}-".Length);
                        if (int.TryParse(numeroStr, out int num) && num > maxExistente)
                            maxExistente = num;
                    }
                }
                _contadoresPed[hoy] = maxExistente;
            }
            _contadoresPed[hoy]++;
            return $"PED-{hoy}-{_contadoresPed[hoy]:D3}";
        }

        public void InicializarInventarioPredeterminado() {
            if (Saldos.Any()) return;

            Saldos.Add(new Saldo {
                ID = GenerarIdZap(),
                UPC = "STK-0001",
                Marca = "AZAP",
                Modelo = "Deportivo 42",
                Precio = 59.90m,
                StockPorTalla = new Dictionary<string, int> {
                    { "40", 10 },
                    { "41", 8 },
                    { "42", 12 },
                    { "43", 6 }
                },
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now
            });
        }

        public Saldo? ObtenerSaldoPorId(string id) {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return Saldos.Find(s => string.Equals(s.ID, id, StringComparison.OrdinalIgnoreCase) || string.Equals(s.UPC, id, StringComparison.OrdinalIgnoreCase));
        }

        public string RegistrarPedido(Pedido pedido, string rutaArchivo) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.Validate();
            if (string.IsNullOrWhiteSpace(pedido.ID)) pedido.ID = GenerarIdPed();

            foreach (var item in pedido.Items) {
                var saldo = ObtenerSaldoPorId(item.SaldoID ?? string.Empty);
                if (saldo == null) {
                    throw new InvalidOperationException($"No se encontró inventario para el artículo {item.SaldoID}");
                }
                if (!saldo.TieneStock(item.Talla ?? string.Empty, item.Cantidad)) {
                    throw new InvalidOperationException($"Stock insuficiente para el artículo {item.SaldoID} talla {item.Talla}");
                }
                saldo.ReservarStock(item.Talla ?? string.Empty, item.Cantidad);
            }

            if (pedido.HistorialEstados == null || pedido.HistorialEstados.Count == 0) {
                pedido.HistorialEstados = new List<string> { pedido.Estado };
            }

            if (pedido.Estado != EstadosPedido.Creado) {
                pedido.CambiarEstado(EstadosPedido.Creado);
            }

            Pedidos.Add(pedido);
            Guardar(rutaArchivo);
            return pedido.ID;
        }

        /// <summary>
        /// Cancela un pedido y reintegra el stock de todos sus artículos.
        /// </summary>
        public void CancelarPedido(string pedidoId, string rutaArchivo) {
            var pedido = Pedidos.Find(p => string.Equals(p.ID, pedidoId, StringComparison.OrdinalIgnoreCase));
            if (pedido == null) throw new InvalidOperationException($"No se encontró el pedido {pedidoId}");

            // Solo se puede cancelar si no está entregado ni ya cancelado
            if (pedido.Estado == EstadosPedido.Entregado)
                throw new InvalidOperationException($"No se puede cancelar un pedido ya entregado: {pedidoId}");
            if (pedido.Estado == EstadosPedido.Cancelado)
                throw new InvalidOperationException($"El pedido {pedidoId} ya está cancelado");

            // Reintegrar stock de cada artículo
            foreach (var item in pedido.Items) {
                var saldo = ObtenerSaldoPorId(item.SaldoID ?? string.Empty);
                if (saldo != null) {
                    saldo.ReintegrarStock(item.Talla ?? string.Empty, item.Cantidad);
                }
            }

            pedido.CambiarEstado(EstadosPedido.Cancelado);
            Guardar(rutaArchivo);
        }

        public string RegistrarOActualizarSaldo(Saldo nuevoSaldo, string rutaArchivo) {
            if (nuevoSaldo == null) throw new ArgumentNullException(nameof(nuevoSaldo));
            if (string.IsNullOrWhiteSpace(nuevoSaldo.UPC)) throw new ArgumentException("El UPC es requerido", nameof(nuevoSaldo.UPC));

            var existente = Saldos.Find(s => string.Equals(s.UPC, nuevoSaldo.UPC, StringComparison.OrdinalIgnoreCase) || string.Equals(s.ID, nuevoSaldo.ID, StringComparison.OrdinalIgnoreCase));
            bool tieneStock = nuevoSaldo.StockPorTalla.Values.Any(cantidad => cantidad > 0);
            if (!tieneStock) {
                throw new InvalidOperationException("STK-02: Error de consistencia. Debe registrar stock mayor a cero.");
            }
            nuevoSaldo.Marca = nuevoSaldo.Marca.ToUpperInvariant();
            nuevoSaldo.FechaModificacion = DateTime.Now;

            if (existente != null) {
                existente.Marca = nuevoSaldo.Marca;
                existente.Modelo = nuevoSaldo.Modelo;
                existente.Precio = nuevoSaldo.Precio;
                existente.RutaFotoPerfil = nuevoSaldo.RutaFotoPerfil;
                existente.RutaFotoFrontal = nuevoSaldo.RutaFotoFrontal;
                existente.StockPorTalla = new Dictionary<string, int>(nuevoSaldo.StockPorTalla);
                existente.FechaModificacion = DateTime.Now;
            } else {
                if (string.IsNullOrWhiteSpace(nuevoSaldo.ID)) nuevoSaldo.ID = GenerarIdZap();
                nuevoSaldo.FechaCreacion = DateTime.Now;
                Saldos.Add(nuevoSaldo);
            }

            Guardar(rutaArchivo);
            return "ÉXITO";
        }
    }
}
