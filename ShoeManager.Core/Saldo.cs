using System;
using System.Collections.Generic;
namespace ShoeManager.Core {
    public class Saldo {
        public string ID { get; set; } = string.Empty;
        public string UPC { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string RutaFotoPerfil { get; set; } = string.Empty;
        public Dictionary<string, int> StockPorTalla { get; set; } = new Dictionary<string, int>();
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaModificacion { get; set; } = DateTime.Now;

        public int ObtenerStock(string talla) {
            if (string.IsNullOrWhiteSpace(talla)) return 0;
            return StockPorTalla.TryGetValue(talla, out int cantidad) ? cantidad : 0;
        }

        public bool TieneStock(string talla, int cantidad) {
            if (string.IsNullOrWhiteSpace(talla)) return false;
            if (cantidad <= 0) return false;
            return ObtenerStock(talla) >= cantidad;
        }

        public void ReservarStock(string talla, int cantidad) {
            if (string.IsNullOrWhiteSpace(talla)) throw new ArgumentException("La talla es requerida", nameof(talla));
            if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
            if (!StockPorTalla.ContainsKey(talla)) throw new InvalidOperationException($"No hay stock para la talla {talla}");
            int disponible = StockPorTalla[talla];
            if (disponible < cantidad) throw new InvalidOperationException($"Stock insuficiente para la talla {talla}: disponible {disponible}, requerido {cantidad}");
            StockPorTalla[talla] = disponible - cantidad;
            FechaModificacion = DateTime.Now;
        }
    }
}
