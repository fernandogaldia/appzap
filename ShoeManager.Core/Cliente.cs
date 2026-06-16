using System;
namespace ShoeManager.Core {
    public class Cliente {
        public string Telefono { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? PreferenciaHoraria { get; set; }

        public Cliente() { }

        public Cliente(string telefono, string? nombre = null, string? direccion = null) {
            if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es requerido", nameof(telefono));
            Telefono = telefono;
            Nombre = nombre;
            Direccion = direccion;
        }

        public void Validate() {
            if (string.IsNullOrWhiteSpace(Telefono)) throw new InvalidOperationException("El teléfono es requerido");
        }

        public override string ToString() {
            return string.IsNullOrWhiteSpace(Nombre) ? Telefono : $"{Nombre} ({Telefono})";
        }
    }
}
