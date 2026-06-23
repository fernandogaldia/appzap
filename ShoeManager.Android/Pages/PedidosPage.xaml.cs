using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using ShoeManager.Core;

namespace ShoeManager.Android.Pages
{
    public partial class PedidosPage : ContentPage
    {
        private readonly BaseMaestra _baseMaestra;
        private readonly ObservableCollection<Pedido> _pedidos = new();

        public PedidosPage(BaseMaestra baseMaestra)
        {
            InitializeComponent();
            _baseMaestra = baseMaestra;
            PedidosCollection.ItemsSource = _pedidos;
            CargarPedidos();
        }

        private void CargarPedidos()
        {
            _pedidos.Clear();
            foreach (var p in _baseMaestra.Pedidos.OrderByDescending(p => p.FechaCreacion))
                _pedidos.Add(p);
            EmptyLabel.IsVisible = _pedidos.Count == 0;
        }
    }
}