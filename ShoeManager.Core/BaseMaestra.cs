using System;
using System.Collections.Generic;
using System.IO;
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

        public string RegistrarPedido(Pedido pedido, string rutaArchivo) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.Validate();
            if (string.IsNullOrWhiteSpace(pedido.ID)) pedido.ID = Guid.NewGuid().ToString();
            if (pedido.HistorialEstados == null || pedido.HistorialEstados.Count == 0)
            {
                pedido.HistorialEstados = new List<string> { pedido.Estado };
            }
            if (pedido.Estado != "Creado")
            {
                pedido.Estado = "Creado";
                pedido.HistorialEstados.Add(pedido.Estado);
            }
            Pedidos.Add(pedido);
            Guardar(rutaArchivo);
            return pedido.ID;
        }

        public string RegistrarOActualizarSaldo(Saldo nuevoSaldo, string rutaArchivo) {
            var existente = Saldos.Find(s => s.UPC == nuevoSaldo.UPC);
            if (existente != null) {
                return $"STK-01: El codigo UPC [{nuevoSaldo.UPC}] ya existe. ID: {existente.ID}";
            }
            bool tieneStock = false;
            foreach (var kvp in nuevoSaldo.StockPorTalla) {
                if (kvp.Value > 0) { tieneStock = true; break; }
            }
            if (!tieneStock) {
                throw new InvalidOperationException("STK-02: Error de consistencia. Debe registrar stock mayor a cero.");
            }
            nuevoSaldo.Marca = nuevoSaldo.Marca.ToUpper();
            Saldos.Add(nuevoSaldo);
            Guardar(rutaArchivo);
            return "ÉXITO";
        }
    }
}
