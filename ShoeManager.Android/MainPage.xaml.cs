using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using ShoeManager.Core;
using ShoeManager.Android.Pages;

namespace ShoeManager.Android;

public partial class MainPage : ContentPage
{
    private readonly OrderController _orderController = new();
    private readonly ObservableCollection<DetallePedido> _items = new();
    private readonly BaseMaestra _baseMaestra;
    private readonly string _dataFilePath;

    public MainPage()
    {
        InitializeComponent();
        _dataFilePath = Path.Combine(FileSystem.AppDataDirectory, "shoes_manager_data.json");
        _baseMaestra = BaseMaestra.Cargar(_dataFilePath);
        ItemsCollection.ItemsSource = _items;
        ActualizarEstadisticas();
    }

    private void ActualizarEstadisticas()
    {
        int activos = _baseMaestra.Saldos.Count(s => s.EsActivo);
        int pedidos = _baseMaestra.Pedidos.Count;
        StatsLabel.Text = $"{activos} saldos activos | {pedidos} pedidos";
    }

    // ============================================================
    // NAVEGACIÓN
    // ============================================================

    private async void OnScannerClicked(object sender, EventArgs e)
    {
        var scannerPage = new ScannerPage();
        await Navigation.PushAsync(scannerPage);
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
        var cameraPage = new CameraPage(true);
        await Navigation.PushAsync(cameraPage);
    }

    private async void OnClientesClicked(object sender, EventArgs e)
    {
        var clientesPage = new ClientesPage(_baseMaestra, _dataFilePath);
        await Navigation.PushAsync(clientesPage);
    }

    private async void OnPedidosClicked(object sender, EventArgs e)
    {
        var pedidosPage = new PedidosPage(_baseMaestra);
        await Navigation.PushAsync(pedidosPage);
    }

    private async void OnScanFromOrderClicked(object sender, EventArgs e)
    {
        var scannerPage = new ScannerPage();
        await Navigation.PushAsync(scannerPage);
        // Cuando regrese, el UPC estará en scannerPage.ScannedUPC
    }

    // ============================================================
    // CREACIÓN DE PEDIDOS
    // ============================================================

    private void OnAddItemClicked(object sender, EventArgs e)
    {
        string saldoId = ItemIdEntry.Text?.Trim() ?? string.Empty;
        string talla = SizeEntry.Text?.Trim() ?? string.Empty;
        bool cantidadOk = int.TryParse(QuantityEntry.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out int cantidad);
        bool precioOk = decimal.TryParse(PriceEntry.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal precio);

        if (string.IsNullOrWhiteSpace(saldoId) || string.IsNullOrWhiteSpace(talla) || !cantidadOk || !precioOk)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Complete los datos del artículo correctamente.";
            return;
        }

        _items.Add(new DetallePedido
        {
            SaldoID = saldoId,
            Talla = talla,
            Cantidad = cantidad,
            PrecioPactado = precio
        });

        ItemIdEntry.Text = SizeEntry.Text = QuantityEntry.Text = PriceEntry.Text = string.Empty;
        StatusLabel.TextColor = Colors.Black;
        StatusLabel.Text = $"✅ Artículo agregado: {saldoId} (Talla {talla})";
    }

    private void OnCreateOrderClicked(object sender, EventArgs e)
    {
        string telefono = PhoneEntry.Text?.Trim() ?? string.Empty;

        if (_items.Count == 0)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Agregue al menos un artículo.";
            return;
        }

        if (string.IsNullOrWhiteSpace(telefono) || !Regex.IsMatch(telefono, "^\\+?\\d{7,15}$"))
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Ingrese un teléfono válido con 7 a 15 dígitos.";
            return;
        }

        try
        {
            var pedido = new Pedido(telefono, _items.ToList());
            string id = _orderController.CreateOrder(pedido);
            _baseMaestra.RegistrarPedido(pedido, _dataFilePath);
            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = $"✅ Pedido {id} creado. Total: {pedido.MontoTotal:C2}";
            _items.Clear();
            PhoneEntry.Text = string.Empty;
            ActualizarEstadisticas();
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }
}