using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ShoeManager.Core;

namespace ShoeManager.Windows
{
    // ============================================================
    // CONVERTERS
    // ============================================================

    public class BoolToEstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool esActivo)
                return esActivo ? "✅ Activo" : "❌ Liquidado";
            return "❓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ItemTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal precio && parameter is string prop && prop == "Cantidad")
                return precio; // El total se calcula en el binding
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class EstadoToVisibilidadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string estado && parameter is string requerido)
            {
                if (requerido == "Cancelable")
                    return (estado == EstadosPedido.Creado || estado == EstadosPedido.Confirmado) 
                        ? Visibility.Visible : Visibility.Collapsed;
                return estado == requerido ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ============================================================
    // MAIN WINDOW
    // ============================================================

    public partial class MainWindow : Window
    {
        private readonly OrderController _orderController = new();
        private BaseMaestra _baseMaestra;
        private readonly ObservableCollection<DetallePedido> _items = new();
        private readonly string _dataFilePath;
        private readonly LoggerLocal _logger;
        private bool _needsRefresh = false;

        public MainWindow()
        {
            InitializeComponent();

            // Inicializar persistencia
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dataFolder = System.IO.Path.Combine(localAppData, "ShoeManager");
            System.IO.Directory.CreateDirectory(dataFolder);
            _dataFilePath = System.IO.Path.Combine(dataFolder, "shoes_manager_data.json");

            // Inicializar logger
            string logFolder = System.IO.Path.Combine(dataFolder, "logs");
            _logger = new LoggerLocal(logFolder);

            // Cargar datos
            _baseMaestra = BaseMaestra.Cargar(_dataFilePath);

            // Configurar bindings
            ItemsListView.ItemsSource = _items;

            // Cargar datos en las tablas
            CargarSaldos();
            CargarClientes();
            CargarHistorial();
            ActualizarEstadisticas();

            _logger.Info("Aplicación Windows iniciada correctamente");
            StatusTextBlock.Text = "✅ Sistema listo";
            DataFileText.Text = Path.GetFileName(_dataFilePath);
        }

        // ============================================================
        // CARGA DE DATOS
        // ============================================================

        private void CargarSaldos(string filtro = "")
        {
            var saldos = string.IsNullOrWhiteSpace(filtro)
                ? _baseMaestra.Saldos
                : _baseMaestra.Saldos.Where(s =>
                    s.Marca.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    s.Modelo.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    s.UPC.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    s.ID.Contains(filtro, StringComparison.OrdinalIgnoreCase)).ToList();

            SaldosDataGrid.ItemsSource = saldos;
            ActualizarEstadisticas();
        }

        private void CargarClientes()
        {
            ClientesDataGrid.ItemsSource = _baseMaestra.Clientes.ToList();
        }

        private void CargarHistorial()
        {
            HistorialDataGrid.ItemsSource = _baseMaestra.Pedidos.OrderByDescending(p => p.FechaCreacion).ToList();
        }

        private void ActualizarEstadisticas()
        {
            int activos = _baseMaestra.Saldos.Count(s => s.EsActivo);
            int liquidados = _baseMaestra.Saldos.Count(s => !s.EsActivo);
            int pedidos = _baseMaestra.Pedidos.Count;
            int clientes = _baseMaestra.Clientes.Count;

            StatsText.Text = $"{activos} activos | {liquidados} liquidados | {pedidos} pedidos | {clientes} clientes";
            HeaderStatusText.Text = $"{_baseMaestra.Saldos.Count} saldos totales";
        }

        // ============================================================
        // COMANDOS DE TECLADO (Ctrl+N, Ctrl+F, Ctrl+S)
        // ============================================================

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem == InventarioTab)
                NuevoSaldoButton_Click(sender, new RoutedEventArgs());
            else if (MainTabControl.SelectedItem == PedidosTab)
                PhoneTextBox.Focus();
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _baseMaestra.Guardar(_dataFilePath);
            StatusTextBlock.Text = "✅ Datos guardados correctamente";
            _logger.Info("Guardado manual ejecutado");
        }

        // ============================================================
        // TAB INVENTARIO
        // ============================================================

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarSaldos(SearchTextBox.Text?.Trim() ?? "");
        }

        private void SaldosDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool haySeleccion = SaldosDataGrid.SelectedItem != null;
            EditarSaldoButton.IsEnabled = haySeleccion;
            EliminarSaldoButton.IsEnabled = haySeleccion;
        }

        private void SaldosDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditarSaldoButton_Click(sender, new RoutedEventArgs());
        }

        private void NuevoSaldoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogSaldo(_baseMaestra, null);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (dialog.SaldoResult is not null)
                    {
                        _baseMaestra.RegistrarOActualizarSaldo(dialog.SaldoResult, _dataFilePath);
                        CargarSaldos(SearchTextBox.Text?.Trim() ?? "");
                        StatusTextBlock.Text = $"✅ Saldo {dialog.SaldoResult.UPC} creado correctamente";
                        _logger.Operacion("admin", "CREAR_SALDO", dialog.SaldoResult.ID);
                    }
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"❌ Error: {ex.Message}";
                    _logger.Error("Error al crear saldo", ex);
                }
            }
        }

        private void EditarSaldoButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaldosDataGrid.SelectedItem is not Saldo saldo) return;

            var dialog = new DialogSaldo(_baseMaestra, saldo);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _baseMaestra.RegistrarOActualizarSaldo(dialog.SaldoResult, _dataFilePath);
                    CargarSaldos(SearchTextBox.Text?.Trim() ?? "");
                    StatusTextBlock.Text = $"✅ Saldo {dialog.SaldoResult.UPC} actualizado";
                    _logger.Operacion("admin", "EDITAR_SALDO", dialog.SaldoResult.ID);
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"❌ Error: {ex.Message}";
                    _logger.Error("Error al editar saldo", ex);
                }
            }
        }

        private void EliminarSaldoButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaldosDataGrid.SelectedItem is not Saldo saldo) return;

            var result = MessageBox.Show(
                $"¿Estás seguro de eliminar el saldo {saldo.Marca} {saldo.Modelo} ({saldo.UPC})?\n\n" +
                $"Stock restante: {saldo.StockTotal} unidades",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _baseMaestra.Saldos.Remove(saldo);
                _baseMaestra.Guardar(_dataFilePath);
                CargarSaldos(SearchTextBox.Text?.Trim() ?? "");
                StatusTextBlock.Text = $"🗑️ Saldo {saldo.UPC} eliminado";
                _logger.Operacion("admin", "ELIMINAR_SALDO", saldo.ID);
            }
        }

        // ============================================================
        // TAB PEDIDOS
        // ============================================================

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
            ActualizarTotalPedido();
        }

        private void ActualizarTotalPedido()
        {
            decimal total = _items.Sum(i => i.PrecioPactado * i.Cantidad);
            OrderTotalText.Text = $"Total: {total:C2}";
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            string telefono = PhoneTextBox.Text?.Trim() ?? string.Empty;
            if (_items.Count == 0)
            {
                StatusTextBlock.Text = "Agregue al menos un artículo antes de crear el pedido.";
                return;
            }

            if (string.IsNullOrWhiteSpace(telefono) || !Regex.IsMatch(telefono, "^\\+?\\d{7,15}$"))
            {
                StatusTextBlock.Text = "Ingrese un teléfono válido con 7 a 15 dígitos, opcionalmente con prefijo +.";
                return;
            }

            try
            {
                var pedido = new Pedido(telefono, new System.Collections.Generic.List<DetallePedido>(_items));
                _orderController.CreateOrder(pedido);
                _baseMaestra.RegistrarPedido(pedido, _dataFilePath);
                StatusTextBlock.Text = $"✅ Pedido creado. ID: {pedido.ID} - Total: {pedido.MontoTotal:C2}";
                _logger.Operacion("admin", "CREAR_PEDIDO", pedido.ID);
                _items.Clear();
                PhoneTextBox.Clear();
                ActualizarTotalPedido();
                CargarHistorial();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ Error: {ex.Message}";
                _logger.Error("Error al crear pedido", ex);
            }
        }

        // ============================================================
        // TAB CLIENTES
        // ============================================================

        private void GuardarClienteButton_Click(object sender, RoutedEventArgs e)
        {
            string telefono = ClienteTelefonoTextBox.Text?.Trim() ?? string.Empty;
            string nombre = ClienteNombreTextBox.Text?.Trim() ?? string.Empty;
            string direccion = ClienteDireccionTextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(telefono))
            {
                StatusTextBlock.Text = "El teléfono es requerido.";
                return;
            }

            try
            {
                var existente = _baseMaestra.Clientes.Find(c => c.Telefono == telefono);
                if (existente != null)
                {
                    existente.Nombre = nombre;
                    existente.Direccion = direccion;
                    StatusTextBlock.Text = $"✅ Cliente {telefono} actualizado";
                }
                else
                {
                    _baseMaestra.Clientes.Add(new Cliente(telefono, nombre, direccion));
                    StatusTextBlock.Text = $"✅ Cliente {telefono} registrado";
                }

                _baseMaestra.Guardar(_dataFilePath);
                CargarClientes();
                ActualizarEstadisticas();
                _logger.Operacion("admin", "GUARDAR_CLIENTE", telefono);

                ClienteTelefonoTextBox.Clear();
                ClienteNombreTextBox.Clear();
                ClienteDireccionTextBox.Clear();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ Error: {ex.Message}";
                _logger.Error("Error al guardar cliente", ex);
            }
        }

        // ============================================================
        // TAB HISTORIAL
        // ============================================================

        private void HistorialDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerarEnlaceButton.IsEnabled = HistorialDataGrid.SelectedItem != null;
        }

        private void CambiarEstadoPedido_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string pedidoId) return;

            var pedido = _baseMaestra.Pedidos.Find(p => p.ID == pedidoId);
            if (pedido == null) return;

            try
            {
                string nuevoEstado = button.Content.ToString() switch
                {
                    string s when s.Contains("En Ruta") => EstadosPedido.Enviado,
                    string s when s.Contains("Entregar") => EstadosPedido.Entregado,
                    _ => null
                };

                if (nuevoEstado == null) return;

                pedido.CambiarEstado(nuevoEstado);
                _baseMaestra.Guardar(_dataFilePath);
                CargarHistorial();
                ActualizarEstadisticas();
                StatusTextBlock.Text = $"✅ Pedido {pedido.ID} → {nuevoEstado}";
                _logger.Operacion("admin", $"CAMBIAR_ESTADO_{nuevoEstado.ToUpper()}", pedido.ID);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ Error: {ex.Message}";
                _logger.Error("Error al cambiar estado", ex);
            }
        }

        private void CancelarPedidoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string pedidoId) return;

            var result = MessageBox.Show(
                $"¿Cancelar el pedido {pedidoId}?\n\n" +
                "El stock de todos los artículos será reintegrado automáticamente.",
                "Confirmar cancelación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _baseMaestra.CancelarPedido(pedidoId, _dataFilePath);
                CargarHistorial();
                CargarSaldos(SearchTextBox.Text?.Trim() ?? "");
                ActualizarEstadisticas();
                StatusTextBlock.Text = $"✅ Pedido {pedidoId} cancelado. Stock reintegrado.";
                _logger.Operacion("admin", "CANCELAR_PEDIDO", pedidoId);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ Error: {ex.Message}";
                _logger.Error("Error al cancelar pedido", ex);
            }
        }

        private void ReportesButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder para SPRINT 5
            var reportes = new ReportesWindow(_baseMaestra);
            reportes.ShowDialog();
        }

        private void GenerarEnlaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (HistorialDataGrid.SelectedItem is not Pedido pedido) return;

            string telefono = pedido.ClienteTelefono.Replace("+", "").Trim();
            string mensaje = $"📦 *Pedido {pedido.ID}*\n";
            mensaje += $"Total: {pedido.MontoTotal:C2}\n";
            mensaje += $"Estado: {pedido.Estado}\n";
            mensaje += $"Artículos:\n";
            foreach (var item in pedido.Items)
            {
                mensaje += $"- {item.SaldoID} (Talla {item.Talla}) x{item.Cantidad} = {item.PrecioPactado * item.Cantidad:C2}\n";
            }

            string enlace = $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje)}";

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = enlace,
                    UseShellExecute = true
                });
                StatusTextBlock.Text = $"🔗 Enlace wa.me generado para pedido {pedido.ID}";
                _logger.Operacion("admin", "GENERAR_ENLACE", pedido.ID);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ No se pudo abrir el enlace: {ex.Message}";
            }
        }

        // ============================================================
        // EVENTOS GENERALES
        // ============================================================

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _baseMaestra = BaseMaestra.Cargar(_dataFilePath);
            CargarSaldos(SearchTextBox.Text?.Trim() ?? "");
            CargarClientes();
            CargarHistorial();
            ActualizarEstadisticas();
            StatusTextBlock.Text = "🔄 Datos recargados";
            _logger.Info("Recarga manual de datos");
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem == HistorialTab)
            {
                CargarHistorial();
            }
            else if (MainTabControl.SelectedItem == ClientesTab)
            {
                CargarClientes();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _baseMaestra.Guardar(_dataFilePath);
            _logger.Info("Aplicación Windows cerrada");
            base.OnClosing(e);
        }
    }
}