using System;

namespace ShoeManager.Core {
    public class OrderController {
        public string CreateOrder(Pedido pedido) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.Validate();
            if (string.IsNullOrWhiteSpace(pedido.ID)) pedido.ID = Guid.NewGuid().ToString();
            pedido.HistorialEstados ??= new System.Collections.Generic.List<string>();
            pedido.HistorialEstados.Add("Creado");
            pedido.Estado = "Creado";
            return pedido.ID;
        }
    }
}
