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
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
