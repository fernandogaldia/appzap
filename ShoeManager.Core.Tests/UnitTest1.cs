using System.Collections.Generic;
using Xunit;
using ShoeManager.Core;

namespace ShoeManager.Core.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Pedido_Constructor_CalculatesTotalAndSetsEstado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 2, PrecioPactado = 10m },
                new DetallePedido { SaldoID = "s2", Talla = "43", Cantidad = 1, PrecioPactado = 20m }
            };
            var p = new Pedido("+123456", items);
            Assert.NotNull(p.ID);
            Assert.Equal(40m, p.MontoTotal);
            Assert.Equal("Nuevo", p.Estado);
            Assert.Single(p.HistorialEstados);
        }

        [Fact]
        public void OrderController_CreateOrder_ReturnsIdAndUpdatesEstado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 15m }
            };
            var p = new Pedido("+111", items);
            var ctrl = new OrderController();
            var id = ctrl.CreateOrder(p);
            Assert.Equal(p.ID, id);
            Assert.Equal("Creado", p.Estado);
            Assert.Contains("Creado", p.HistorialEstados);
        }

        [Fact]
        public void Cliente_ValidateAndToString()
        {
            var c = new Cliente("+999", "Juan", "Calle 1");
            c.Validate();
            Assert.Equal("Juan (+999)", c.ToString());
        }

        [Fact]
        public void BaseMaestra_RegistrarPedido_PersistsAndReloads()
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), $"shoes_manager_test_{Guid.NewGuid()}.json");

            try
            {
                var baseMaestra = new BaseMaestra();
                var items = new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 2, PrecioPactado = 10m }
                };
                var pedido = new Pedido("+123456", items);
                string id = baseMaestra.RegistrarPedido(pedido, tmpFile);

                var reloaded = BaseMaestra.Cargar(tmpFile);
                Assert.Single(reloaded.Pedidos);
                Assert.Equal(id, reloaded.Pedidos[0].ID);
                Assert.Equal("Creado", reloaded.Pedidos[0].Estado);
                Assert.Equal(20m, reloaded.Pedidos[0].MontoTotal);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }
    }
}
