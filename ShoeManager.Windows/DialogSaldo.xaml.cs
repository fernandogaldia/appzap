using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using ShoeManager.Core;

namespace ShoeManager.Windows
{
    public partial class DialogSaldo : Window
    {
        public Saldo? SaldoResult { get; private set; }
        private readonly BaseMaestra _baseMaestra;
        private readonly Saldo? _saldoOriginal;
        private readonly bool _esEdicion;

        public DialogSaldo(BaseMaestra baseMaestra, Saldo? saldoExistente)
        {
            InitializeComponent();
            _baseMaestra = baseMaestra;
            _esEdicion = saldoExistente != null;

            if (_esEdicion)
            {
                _saldoOriginal = saldoExistente!;
                Title = "Editar Saldo";
                TitleText.Text = "Editar Saldo";
                CargarDatosSaldo(_saldoOriginal);
            }
            else
            {
                Title = "Nuevo Saldo";
                TitleText.Text = "Nuevo Saldo";
            }
        }

        private void CargarDatosSaldo(Saldo saldo)
        {
            UPCTextBox.Text = saldo.UPC;
            MarcaTextBox.Text = saldo.Marca;
            ModeloTextBox.Text = saldo.Modelo;
            PrecioTextBox.Text = saldo.Precio.ToString("F2");
            FotoFrontalTextBox.Text = saldo.RutaFotoFrontal;
            FotoPerfilTextBox.Text = saldo.RutaFotoPerfil;

            // Cargar stock por talla
            var lineas = saldo.StockPorTalla.Select(kvp => $"{kvp.Key}={kvp.Value}");
            StockTextBox.Text = string.Join(Environment.NewLine, lineas);
        }

        private void MarcaTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Convertir a mayúsculas en tiempo real
            int cursorPos = MarcaTextBox.CaretIndex;
            MarcaTextBox.Text = MarcaTextBox.Text.ToUpperInvariant();
            MarcaTextBox.CaretIndex = cursorPos > MarcaTextBox.Text.Length ? MarcaTextBox.Text.Length : cursorPos;
        }

        private void BuscarFotoFrontal_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Seleccionar foto frontal"
            };
            if (dialog.ShowDialog() == true)
                FotoFrontalTextBox.Text = dialog.FileName;
        }

        private void BuscarFotoPerfil_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Seleccionar foto de perfil"
            };
            if (dialog.ShowDialog() == true)
                FotoPerfilTextBox.Text = dialog.FileName;
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos obligatorios
                string upc = UPCTextBox.Text?.Trim() ?? string.Empty;
                string marca = MarcaTextBox.Text?.Trim().ToUpperInvariant() ?? string.Empty;
                string modelo = ModeloTextBox.Text?.Trim() ?? string.Empty;
                string precioStr = PrecioTextBox.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(upc))
                {
                    MessageBox.Show("El UPC es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UPCTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(marca))
                {
                    MessageBox.Show("La marca es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MarcaTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(modelo))
                {
                    MessageBox.Show("El modelo es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ModeloTextBox.Focus();
                    return;
                }

                if (!decimal.TryParse(precioStr, out decimal precio) || precio <= 0)
                {
                    MessageBox.Show("Ingrese un precio válido mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PrecioTextBox.Focus();
                    return;
                }

                // Parsear stock por talla
                var stockPorTalla = new Dictionary<string, int>();
                if (!string.IsNullOrWhiteSpace(StockTextBox.Text))
                {
                    var lineas = StockTextBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var linea in lineas)
                    {
                        var partes = linea.Split('=');
                        if (partes.Length == 2 &&
                            int.TryParse(partes[1].Trim(), out int cantidad) &&
                            cantidad > 0)
                        {
                            string talla = partes[0].Trim();
                            if (!string.IsNullOrWhiteSpace(talla))
                                stockPorTalla[talla] = cantidad;
                        }
                    }
                }

                if (stockPorTalla.Count == 0)
                {
                    MessageBox.Show("Debe registrar al menos una talla con stock mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StockTextBox.Focus();
                    return;
                }

                // Construir el saldo
                var saldo = new Saldo
                {
                    UPC = upc,
                    Marca = marca,
                    Modelo = modelo,
                    Precio = precio,
                    StockPorTalla = stockPorTalla,
                    RutaFotoFrontal = FotoFrontalTextBox.Text?.Trim() ?? string.Empty,
                    RutaFotoPerfil = FotoPerfilTextBox.Text?.Trim() ?? string.Empty
                };

                // Si es edición, preservar el ID y fechas originales
                if (_esEdicion && _saldoOriginal != null)
                {
                    saldo.ID = _saldoOriginal.ID;
                    saldo.FechaCreacion = _saldoOriginal.FechaCreacion;
                    saldo.EsActivo = _saldoOriginal.EsActivo;
                    saldo.FechaLiquidacion = _saldoOriginal.FechaLiquidacion;
                }

                SaldoResult = saldo;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}