using System;
using System.Collections.Generic;
using System.Linq;
namespace ShoeManager.Core {
    public class Saldo {
        public string ID { get; set; } = string.Empty;
        public string UPC { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string RutaFotoPerfil { get; set; } = string.Empty;
        public string RutaFotoFrontal { get; set; } = string.Empty;
        public Dictionary<string, int> StockPorTalla { get; set; } = new Dictionary<string, int>();
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaModificacion { get; set; } = DateTime.Now;

        /// <summary>
        /// STK-03: Indica si el saldo está activo en el catálogo.
        /// Se vuelve false automáticamente cuando todas las tallas tienen stock = 0.
        /// </summary>
        public bool EsActivo { get; set; } = true;

        /// <summary>
        /// STK-03: Fecha en que el producto fue marcado como liquidado.
        /// </summary>
        public DateTime? FechaLiquidacion { get; set; }

        public int StockTotal => StockPorTalla.Values.Sum();

        public int ObtenerStock(string talla) {
            if (string.IsNullOrWhiteSpace(talla)) return 0;
            return StockPorTalla.TryGetValue(talla, out int cantidad) ? cantidad : 0;
        }

        public bool TieneStock(string talla, int cantidad) {
            if (string.IsNullOrWhiteSpace(talla)) return false;
            if (cantidad <= 0) return false;
            return ObtenerStock(talla) >= cantidad;
        }

        /// <summary>
        /// STK-03: Verifica si el producto tiene stock en al menos una talla.
        /// </summary>
        public bool TieneStockDisponible() {
            return StockPorTalla.Values.Any(cantidad => cantidad > 0);
        }

        public void ReservarStock(string talla, int cantidad) {
            if (string.IsNullOrWhiteSpace(talla)) throw new ArgumentException("La talla es requerida", nameof(talla));
            if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
            if (!StockPorTalla.ContainsKey(talla)) throw new InvalidOperationException($"No hay stock para la talla {talla}");
            int disponible = StockPorTalla[talla];
            if (disponible < cantidad) throw new InvalidOperationException($"Stock insuficiente para la talla {talla}: disponible {disponible}, requerido {cantidad}");
            StockPorTalla[talla] = disponible - cantidad;
            FechaModificacion = DateTime.Now;

            // STK-03: Liquidación Dinámica - si todas las tallas llegan a cero, marcar como inactivo
            EjecutarLiquidacionSiCorresponde();
        }

        /// <summary>
        /// STK-03: Reintegra stock cuando un pedido es cancelado.
        /// Si el producto estaba liquidado, lo reactiva.
        /// </summary>
        public void ReintegrarStock(string talla, int cantidad) {
            if (string.IsNullOrWhiteSpace(talla)) throw new ArgumentException("La talla es requerida", nameof(talla));
            if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));

            if (StockPorTalla.ContainsKey(talla)) {
                StockPorTalla[talla] += cantidad;
            } else {
                StockPorTalla[talla] = cantidad;
            }
            FechaModificacion = DateTime.Now;

            // STK-03: Reactivar producto si tenía stock nuevamente
            if (!EsActivo && TieneStockDisponible()) {
                EsActivo = true;
                FechaLiquidacion = null;
            }
        }

        /// <summary>
        /// STK-03: Evalúa si todas las tallas están en cero y aplica liquidación.
        /// </summary>
        private void EjecutarLiquidacionSiCorresponde() {
            if (!TieneStockDisponible() && EsActivo) {
                EsActivo = false;
                FechaLiquidacion = DateTime.Now;
            }
        }
    }
}
