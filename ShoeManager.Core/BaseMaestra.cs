using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ShoeManager.Core {
    public class BaseMaestra {
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

        public void InicializarInventarioPredeterminado() {
            if (Saldos.Any()) return;

            Saldos.Add(new Saldo {
                ID = Guid.NewGuid().ToString(),
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
            if (string.IsNullOrWhiteSpace(pedido.ID)) pedido.ID = Guid.NewGuid().ToString();

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
                existente.StockPorTalla = new Dictionary<string, int>(nuevoSaldo.StockPorTalla);
                existente.FechaModificacion = DateTime.Now;
            } else {
                if (string.IsNullOrWhiteSpace(nuevoSaldo.ID)) nuevoSaldo.ID = Guid.NewGuid().ToString();
                nuevoSaldo.FechaCreacion = DateTime.Now;
                Saldos.Add(nuevoSaldo);
            }

            Guardar(rutaArchivo);
            return "ÉXITO";
        }
    }
}
