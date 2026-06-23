using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace ShoeManager.Android.Pages
{
    public partial class ScannerPage : ContentPage
    {
        public string ScannedUPC { get; private set; } = string.Empty;

        public ScannerPage()
        {
            InitializeComponent();
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        ScanResultLabel.Text = "Permiso de cámara denegado.";
                        return;
                    }
                }

                ScanResultLabel.Text = "Cámara activada. Escanea el código UPC-A...";
                // En un entorno real, aquí se integraría ZXing.Mobile o similar
                // Por ahora simulamos la captura
                string upcSimulado = await SimularEscaneo();
                ManualUPCEntry.Text = upcSimulado;
                ScanResultLabel.Text = $"Código detectado: {upcSimulado}";
            }
            catch (Exception ex)
            {
                ScanResultLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async Task<string> SimularEscaneo()
        {
            await Task.Delay(1500); // Simula tiempo de escaneo
            return "123456789012"; // UPC-A simulado de 12 dígitos
        }

        private void OnUseCodeClicked(object sender, EventArgs e)
        {
            string upc = ManualUPCEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(upc) || upc.Length < 8)
            {
                DisplayAlert("Error", "Ingrese un código UPC válido (mínimo 8 dígitos)", "OK");
                return;
            }
            ScannedUPC = upc;
            Navigation.PopAsync();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}