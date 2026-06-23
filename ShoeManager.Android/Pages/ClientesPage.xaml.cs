using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using ShoeManager.Core;

namespace ShoeManager.Android.Pages
{
    public partial class ClientesPage : ContentPage
    {
        private readonly BaseMaestra _baseMaestra;
        private readonly string _dataFilePath;
        private readonly ObservableCollection<Cliente> _clientes = new();

        public ClientesPage(BaseMaestra baseMaestra, string dataFilePath)
        {
            InitializeComponent();
            _baseMaestra = baseMaestra;
            _dataFilePath = dataFilePath;
            ClientesCollection.ItemsSource = _clientes;
            CargarClientes();
        }

        private void CargarClientes()
        {
            _clientes.Clear();
            foreach (var c in _baseMaestra.Clientes)
                _clientes.Add(c);
        }

        private void OnGuardarClienteClicked(object sender, EventArgs e)
        {
            string telefono = TelefonoEntry.Text?.Trim() ?? string.Empty;
            string nombre = NombreEntry.Text?.Trim() ?? string.Empty;
            string direccion = DireccionEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(telefono))
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = "El teléfono es requerido.";
                return;
            }

            try
            {
                var existente = _baseMaestra.Clientes.Find(c => c.Telefono == telefono);
                if (existente != null)
                {
                    existente.Nombre = nombre;
                    existente.Direccion = direccion;
                    StatusLabel.Text = $"✅ Cliente {telefono} actualizado";
                }
                else
                {
                    _baseMaestra.Clientes.Add(new Cliente(telefono, nombre, direccion));
                    StatusLabel.Text = $"✅ Cliente {telefono} registrado";
                }
                _baseMaestra.Guardar(_dataFilePath);
                CargarClientes();
                TelefonoEntry.Text = NombreEntry.Text = DireccionEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = $"❌ Error: {ex.Message}";
            }
        }
    }
}