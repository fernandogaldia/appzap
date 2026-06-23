using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShoeManager.Core
{
    /// <summary>
    /// Generador de enlaces interactivos de WhatsApp (wa.me).
    /// No usa APIs externas - solo genera URLs con protocolo wa.me.
    /// </summary>
    public static class GeneradorEnlaces
    {
        /// <summary>
        /// Genera un enlace wa.me con el catálogo dinámico filtrado por talla.
        /// </summary>
        public static string GenerarEnlaceCatalogo(string telefonoCliente, List<Saldo> saldosActivos, string? tallaFiltro = null)
        {
            string telefono = LimpiarTelefono(telefonoCliente);
            var mensaje = new StringBuilder();

            mensaje.AppendLine("🛍️ *CATÁLOGO DISPONIBLE*");
            mensaje.AppendLine();

            var saldosFiltrados = tallaFiltro != null
                ? saldosActivos.Where(s => s.EsActivo && s.TieneStock(tallaFiltro, 1)).ToList()
                : saldosActivos.Where(s => s.EsActivo).ToList();

            if (!saldosFiltrados.Any())
            {
                mensaje.AppendLine("(No hay productos disponibles actualmente)");
            }
            else
            {
                foreach (var saldo in saldosFiltrados.Take(20)) // Máximo 20 productos por mensaje
                {
                    string tallasStr = string.Join(", ", saldo.StockPorTalla
                        .Where(kvp => kvp.Value > 0)
                        .Select(kvp => $"{kvp.Key}({kvp.Value})"));

                    mensaje.AppendLine($"*{saldo.Marca} {saldo.Modelo}*");
                    mensaje.AppendLine($"💰 S/ {saldo.Precio:F2}");
                    mensaje.AppendLine($"📏 Tallas: {tallasStr}");
                    mensaje.AppendLine($"🔢 Cód: {saldo.UPC}");
                    mensaje.AppendLine();
                }
            }

            mensaje.AppendLine("📍 *Contraentrega*");
            mensaje.AppendLine("Se acepta efectivo o transferencia.");

            return $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje.ToString())}";
        }

        /// <summary>
        /// Genera un enlace wa.me con confirmación de pedido.
        /// </summary>
        public static string GenerarEnlaceConfirmacion(string telefonoCliente, Pedido pedido)
        {
            string telefono = LimpiarTelefono(telefonoCliente);
            var mensaje = new StringBuilder();

            mensaje.AppendLine("📦 *CONFIRMACIÓN DE PEDIDO*");
            mensaje.AppendLine();
            mensaje.AppendLine($"🆔 Pedido: {pedido.ID}");
            mensaje.AppendLine();
            mensaje.AppendLine("*Artículos:*");

            foreach (var item in pedido.Items)
            {
                mensaje.AppendLine($"• {item.SaldoID} (Talla {item.Talla})");
                mensaje.AppendLine($"  x{item.Cantidad} = S/ {item.PrecioPactado * item.Cantidad:F2}");
            }

            mensaje.AppendLine();
            mensaje.AppendLine($"💰 *TOTAL: S/ {pedido.MontoTotal:F2}*");
            mensaje.AppendLine($"📅 Fecha: {pedido.FechaCreacion:dd/MM/yyyy HH:mm}");
            mensaje.AppendLine($"📌 Estado: {pedido.Estado}");
            mensaje.AppendLine();
            mensaje.AppendLine("✅ *Contraentrega* - Pagas al recibir");

            return $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje.ToString())}";
        }

        /// <summary>
        /// Genera un enlace wa.me para consulta de disponibilidad de un producto específico.
        /// </summary>
        public static string GenerarEnlaceConsultaStock(string telefonoVendedor, Saldo saldo, string talla)
        {
            string telefono = LimpiarTelefono(telefonoVendedor);
            string mensaje = $"Hola, ¿tienes disponible el {saldo.Marca} {saldo.Modelo} en talla {talla}?";

            return $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje)}";
        }

        private static string LimpiarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return string.Empty;
            return telefono.Trim().Replace("+", "").Replace(" ", "").Replace("-", "");
        }
    }
}