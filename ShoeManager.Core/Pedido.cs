using System;
using System.Collections.Generic;
using System.Linq;
namespace ShoeManager.Core {
    public static class EstadosPedido {
        public const string Pendiente = "Pendiente";
        public const string Creado = "Creado";
        public const string Confirmado = "Confirmado";
        public const string Enviado = "Enviado";
        public const string Entregado = "Entregado";
        public const string Cancelado = "Cancelado";
    }

    public class DetallePedido {
        public string? SaldoID { get; set; }
        public string? Talla { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioPactado { get; set; }
    }

    public class Pedido {
        public string ID { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public List<DetallePedido> Items { get; set; } = new List<DetallePedido>();
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = EstadosPedido.Pendiente;
        public List<string> HistorialEstados { get; set; } = new List<string>();
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaModificacion { get; set; } = DateTime.Now;

        public Pedido() {
            Estado = EstadosPedido.Pendiente;
            HistorialEstados = new List<string> { Estado };
        }

        public Pedido(string clienteTelefono, List<DetallePedido> items) {
            if (string.IsNullOrWhiteSpace(clienteTelefono)) throw new ArgumentException("El teléfono del cliente es requerido", nameof(clienteTelefono));
            if (items == null || items.Count == 0) throw new ArgumentException("Se requieren artículos", nameof(items));
            foreach (var it in items) {
                if (it == null) throw new ArgumentException("El artículo no puede ser nulo", nameof(items));
                if (string.IsNullOrWhiteSpace(it.SaldoID)) throw new ArgumentException("El SaldoID del artículo es requerido", nameof(it.SaldoID));
                if (string.IsNullOrWhiteSpace(it.Talla)) throw new ArgumentException("La talla del artículo es requerida", nameof(it.Talla));
                if (it.Cantidad <= 0) throw new ArgumentException("La cantidad del artículo debe ser mayor que cero", nameof(it.Cantidad));
                if (it.PrecioPactado < 0) throw new ArgumentException("El precio pactado del artículo no puede ser negativo", nameof(it.PrecioPactado));
            }
            ID = Guid.NewGuid().ToString();
            ClienteTelefono = clienteTelefono;
            Items = items;
            MontoTotal = Items.Sum(i => i.PrecioPactado * i.Cantidad);
            Estado = EstadosPedido.Pendiente;
            HistorialEstados = new List<string> { Estado };
            FechaCreacion = DateTime.Now;
            FechaModificacion = DateTime.Now;
        }

        public void Validate() {
            if (string.IsNullOrWhiteSpace(ClienteTelefono)) throw new InvalidOperationException("El teléfono del cliente es requerido");
            if (Items == null || Items.Count == 0) throw new InvalidOperationException("El pedido debe tener al menos un artículo");
            foreach (var it in Items) {
                if (it == null) throw new InvalidOperationException("El artículo es nulo");
                if (string.IsNullOrWhiteSpace(it.SaldoID)) throw new InvalidOperationException("El SaldoID del artículo es requerido");
                if (string.IsNullOrWhiteSpace(it.Talla)) throw new InvalidOperationException("La talla del artículo es requerida");
                if (it.Cantidad <= 0) throw new InvalidOperationException("La cantidad del artículo debe ser mayor que cero");
                if (it.PrecioPactado < 0) throw new InvalidOperationException("El precio pactado del artículo no puede ser negativo");
            }
            MontoTotal = Items.Sum(i => i.PrecioPactado * i.Cantidad);
            FechaModificacion = DateTime.Now;
        }

        public void CambiarEstado(string nuevoEstado) {
            if (string.IsNullOrWhiteSpace(nuevoEstado)) throw new ArgumentException("El estado es requerido", nameof(nuevoEstado));
            if (Estado == nuevoEstado) return;
            if (!PermiteTransicion(Estado, nuevoEstado)) throw new InvalidOperationException($"Transición de estado inválida: {Estado} -> {nuevoEstado}");
            Estado = nuevoEstado;
            HistorialEstados.Add(nuevoEstado);
            FechaModificacion = DateTime.Now;
        }

        private bool PermiteTransicion(string estadoActual, string nuevoEstado) {
            return estadoActual switch {
                EstadosPedido.Pendiente => nuevoEstado == EstadosPedido.Creado || nuevoEstado == EstadosPedido.Cancelado,
                EstadosPedido.Creado => nuevoEstado == EstadosPedido.Confirmado || nuevoEstado == EstadosPedido.Cancelado,
                EstadosPedido.Confirmado => nuevoEstado == EstadosPedido.Enviado || nuevoEstado == EstadosPedido.Cancelado,
                EstadosPedido.Enviado => nuevoEstado == EstadosPedido.Entregado,
                EstadosPedido.Entregado => false,
                EstadosPedido.Cancelado => false,
                _ => false,
            };
        }
    }
}
