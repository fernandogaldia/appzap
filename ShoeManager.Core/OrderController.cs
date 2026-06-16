using System;

namespace ShoeManager.Core {
    public class OrderController {
        public string CreateOrder(Pedido pedido) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.Validate();
            if (string.IsNullOrWhiteSpace(pedido.ID)) pedido.ID = Guid.NewGuid().ToString();
            if (pedido.Estado == EstadosPedido.Pendiente) {
                pedido.CambiarEstado(EstadosPedido.Creado);
            }
            return pedido.ID;
        }

        public void ConfirmOrder(Pedido pedido) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.CambiarEstado(EstadosPedido.Confirmado);
        }

        public void ShipOrder(Pedido pedido) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.CambiarEstado(EstadosPedido.Enviado);
        }

        public void DeliverOrder(Pedido pedido) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.CambiarEstado(EstadosPedido.Entregado);
        }

        public void CancelOrder(Pedido pedido) {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            pedido.CambiarEstado(EstadosPedido.Cancelado);
        }
    }
}
