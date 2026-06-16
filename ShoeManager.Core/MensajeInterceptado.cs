using System;
namespace ShoeManager.Core {
    public class MensajeInterceptado {
        public string Remitente { get; set; } = string.Empty;
        public string ContenidoOriginal { get; set; } = string.Empty;
        public DateTime FechaCaptura { get; set; }
    }
}
