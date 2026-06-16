using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Maui.Storage;
using ShoeManager.Core;

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
        StatusLabel.Text = $"Pedidos cargados: {_baseMaestra.Pedidos.Count}";
    }

    private void OnAddItemClicked(object sender, EventArgs e)
    {
        string telefono = PhoneEntry.Text?.Trim() ?? string.Empty;
        string saldoId = ItemIdEntry.Text?.Trim() ?? string.Empty;
        string talla = SizeEntry.Text?.Trim() ?? string.Empty;
        bool cantidadOk = int.TryParse(QuantityEntry.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out int cantidad);
        bool precioOk = decimal.TryParse(PriceEntry.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal precio);

        if (string.IsNullOrWhiteSpace(saldoId) || string.IsNullOrWhiteSpace(talla) || !cantidadOk || !precioOk)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Complete los datos del artículo correctamente antes de agregar.";
            return;
        }

        _items.Add(new DetallePedido
        {
            SaldoID = saldoId,
            Talla = talla,
            Cantidad = cantidad,
            PrecioPactado = precio
        });

        ItemIdEntry.Text = string.Empty;
        SizeEntry.Text = string.Empty;
        QuantityEntry.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        StatusLabel.TextColor = Colors.Black;
        StatusLabel.Text = $"Artículo agregado: {saldoId}, talla {talla}.";
    }

    private void OnCreateOrderClicked(object sender, EventArgs e)
    {
        string telefono = PhoneEntry.Text?.Trim() ?? string.Empty;

        if (_items.Count == 0)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Agregue al menos un artículo antes de crear el pedido.";
            return;
        }

        if (string.IsNullOrWhiteSpace(telefono) || !Regex.IsMatch(telefono, "^\\+?\\d{7,15}$"))
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Ingrese un teléfono válido con 7 a 15 dígitos, opcionalmente con prefijo +.";
            return;
        }

        try
        {
            var pedido = new Pedido(telefono, _items.ToList());
            string id = _orderController.CreateOrder(pedido);
            _baseMaestra.RegistrarPedido(pedido, _dataFilePath);
            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = $"Pedido creado y guardado. ID: {id} - Total: {pedido.MontoTotal:C2}";
            _items.Clear();
            PhoneEntry.Text = string.Empty;
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex.Message;
        }
    }
}

