using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using ShoeManager.Core;

namespace ShoeManager.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly OrderController _orderController = new();
    private readonly BaseMaestra _baseMaestra;
    private readonly ObservableCollection<DetallePedido> _items = new ObservableCollection<DetallePedido>();
    private readonly string _dataFilePath;

    public MainWindow()
    {
        InitializeComponent();

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dataFolder = System.IO.Path.Combine(localAppData, "ShoeManager");
        System.IO.Directory.CreateDirectory(dataFolder);
        _dataFilePath = System.IO.Path.Combine(dataFolder, "shoes_manager_data.json");
        _baseMaestra = BaseMaestra.Cargar(_dataFilePath);
        ItemsListView.ItemsSource = _items;
        StatusTextBlock.Text = $"Pedidos cargados: {_baseMaestra.Pedidos.Count} | Inventario: {_baseMaestra.Saldos.Count} artículos";
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        string saldoId = ItemIdTextBox.Text?.Trim() ?? string.Empty;
        string talla = SizeTextBox.Text?.Trim() ?? string.Empty;
        bool cantidadOk = int.TryParse(QuantityTextBox.Text, out int cantidad);
        bool precioOk = decimal.TryParse(PriceTextBox.Text, out decimal precio);

        if (string.IsNullOrWhiteSpace(saldoId) || string.IsNullOrWhiteSpace(talla) || !cantidadOk || !precioOk || cantidad <= 0 || precio < 0)
        {
            StatusTextBlock.Text = "Complete los datos del artículo correctamente antes de agregar.";
            return;
        }

        _items.Add(new DetallePedido
        {
            SaldoID = saldoId,
            Talla = talla,
            Cantidad = cantidad,
            PrecioPactado = precio
        });

        ItemIdTextBox.Clear();
        SizeTextBox.Clear();
        QuantityTextBox.Clear();
        PriceTextBox.Clear();
        StatusTextBlock.Text = $"Artículo agregado: {saldoId}, talla {talla}.";
    }

    private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
    {
        string telefono = PhoneTextBox.Text?.Trim() ?? string.Empty;
        if (_items.Count == 0)
        {
            StatusTextBlock.Text = "Agregue al menos un artículo antes de crear el pedido.";
            return;
        }

        if (string.IsNullOrWhiteSpace(telefono) || !System.Text.RegularExpressions.Regex.IsMatch(telefono, "^\\+?\\d{7,15}$"))
        {
            StatusTextBlock.Text = "Ingrese un teléfono válido con 7 a 15 dígitos, opcionalmente con prefijo +.";
            return;
        }

        try
        {
            var pedido = new Pedido(telefono, new System.Collections.Generic.List<DetallePedido>(_items));
            _orderController.CreateOrder(pedido);
            _baseMaestra.RegistrarPedido(pedido, _dataFilePath);
            StatusTextBlock.Text = $"Pedido creado y guardado. ID: {pedido.ID} - Total: {pedido.MontoTotal:C2}";
            _items.Clear();
            PhoneTextBox.Clear();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = ex.Message;
        }
    }
}