using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using ShoeManager.Core;

namespace ShoeManager.Core.Tests
{
    public class UnitTests
    {
        // ============================================================
        // TESTS EXISTENTES (VERIFICAR QUE SIGAN FUNCIONANDO)
        // ============================================================

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
            Assert.Equal(EstadosPedido.Pendiente, p.Estado);
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
            Assert.Equal(EstadosPedido.Creado, p.Estado);
            Assert.Contains(EstadosPedido.Creado, p.HistorialEstados);
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
                baseMaestra.InicializarInventarioPredeterminado();
                var items = new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = baseMaestra.Saldos[0].ID, Talla = "42", Cantidad = 2, PrecioPactado = 10m }
                };
                var pedido = new Pedido("+123456", items);
                string id = baseMaestra.RegistrarPedido(pedido, tmpFile);

                var reloaded = BaseMaestra.Cargar(tmpFile);
                Assert.Single(reloaded.Pedidos);
                Assert.Equal(id, reloaded.Pedidos[0].ID);
                Assert.Equal(EstadosPedido.Creado, reloaded.Pedidos[0].Estado);
                Assert.Equal(20m, reloaded.Pedidos[0].MontoTotal);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        // ============================================================
        // SPRINT 1 - TESTS NUEVOS: ID CRONOLÓGICO
        // ============================================================

        [Fact]
        public void BaseMaestra_GenerarIdZap_FormatoCorrecto()
        {
            var bm = new BaseMaestra();
            string id = bm.GenerarIdZap();
            Assert.Matches(@"^ZAP-\d{8}-\d{3}$", id);
            Assert.StartsWith($"ZAP-{DateTime.Now:yyyyMMdd}-", id);
        }

        [Fact]
        public void BaseMaestra_GenerarIdPed_FormatoCorrecto()
        {
            var bm = new BaseMaestra();
            string id = bm.GenerarIdPed();
            Assert.Matches(@"^PED-\d{8}-\d{3}$", id);
            Assert.StartsWith($"PED-{DateTime.Now:yyyyMMdd}-", id);
        }

        [Fact]
        public void BaseMaestra_GenerarIdZap_IncrementaSecuencial()
        {
            var bm = new BaseMaestra();
            string id1 = bm.GenerarIdZap();
            string id2 = bm.GenerarIdZap();
            string id3 = bm.GenerarIdZap();

            // Los IDs deben ser diferentes y secuenciales
            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id2, id3);

            // Extraer los números secuenciales
            int num1 = int.Parse(id1.Substring(id1.Length - 3));
            int num2 = int.Parse(id2.Substring(id2.Length - 3));
            int num3 = int.Parse(id3.Substring(id3.Length - 3));

            Assert.Equal(num1 + 1, num2);
            Assert.Equal(num2 + 1, num3);
        }

        [Fact]
        public void BaseMaestra_RegistrarSaldoNuevo_UsaIdCronologico()
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), $"shoes_manager_test_{Guid.NewGuid()}.json");
            try
            {
                var bm = new BaseMaestra();
                var saldo = new Saldo
                {
                    UPC = "TEST-123456",
                    Marca = "NIKE",
                    Modelo = "Air Max",
                    Precio = 99.99m,
                    StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
                };
                bm.RegistrarOActualizarSaldo(saldo, tmpFile);

                Assert.StartsWith("ZAP-", saldo.ID);
                Assert.Matches(@"^ZAP-\d{8}-\d{3}$", saldo.ID);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        // ============================================================
        // SPRINT 1 - TESTS NUEVOS: STK-03 LIQUIDACIÓN DINÁMICA
        // ============================================================

        [Fact]
        public void Saldo_ReservarStock_ActivaLiquidacionCuandoStockCero()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 1 } }
            };
            Assert.True(saldo.EsActivo);
            Assert.Null(saldo.FechaLiquidacion);

            saldo.ReservarStock("42", 1);

            Assert.False(saldo.EsActivo);
            Assert.NotNull(saldo.FechaLiquidacion);
            Assert.Equal(0, saldo.StockTotal);
        }

        [Fact]
        public void Saldo_ReservarStock_NoLiquidaSiAunHayStock()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 }, { "43", 3 } }
            };

            saldo.ReservarStock("42", 2); // Quedan 3 en 42, y 3 en 43

            Assert.True(saldo.EsActivo);
            Assert.Null(saldo.FechaLiquidacion);
        }

        [Fact]
        public void Saldo_TieneStockDisponible_CuandoHayStock()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
            };
            Assert.True(saldo.TieneStockDisponible());
        }

        [Fact]
        public void Saldo_TieneStockDisponible_FalsoCuandoTodoCero()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 0 }, { "43", 0 } }
            };
            Assert.False(saldo.TieneStockDisponible());
        }

        // ============================================================
        // SPRINT 1 - TESTS NUEVOS: REVERSA DE STOCK
        // ============================================================

        [Fact]
        public void Saldo_ReintegrarStock_ReactivaProductoLiquidado()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 1 } }
            };
            saldo.ReservarStock("42", 1); // Se liquida
            Assert.False(saldo.EsActivo);

            saldo.ReintegrarStock("42", 2); // Reactiva

            Assert.True(saldo.EsActivo);
            Assert.Null(saldo.FechaLiquidacion);
            Assert.Equal(2, saldo.StockPorTalla["42"]);
        }

        [Fact]
        public void BaseMaestra_CancelarPedido_ReintegraStock()
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), $"shoes_manager_test_{Guid.NewGuid()}.json");
            try
            {
                var bm = new BaseMaestra();
                bm.InicializarInventarioPredeterminado();
                string saldoId = bm.Saldos[0].ID;
                int stockAntes = bm.Saldos[0].ObtenerStock("42");

                var items = new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = saldoId, Talla = "42", Cantidad = 2, PrecioPactado = 10m }
                };
                var pedido = new Pedido("+123456", items);
                bm.RegistrarPedido(pedido, tmpFile);

                // Verificar que se reservó stock
                Assert.Equal(stockAntes - 2, bm.Saldos[0].ObtenerStock("42"));

                // Cancelar y verificar que se reintegró
                bm.CancelarPedido(pedido.ID, tmpFile);
                Assert.Equal(stockAntes, bm.Saldos[0].ObtenerStock("42"));
                Assert.Equal(EstadosPedido.Cancelado, pedido.Estado);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        [Fact]
        public void BaseMaestra_CancelarPedido_LanzaErrorSiEntregado()
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), $"shoes_manager_test_{Guid.NewGuid()}.json");
            try
            {
                var bm = new BaseMaestra();
                bm.InicializarInventarioPredeterminado();
                var items = new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = bm.Saldos[0].ID, Talla = "42", Cantidad = 1, PrecioPactado = 10m }
                };
                var pedido = new Pedido("+123456", items);
                bm.RegistrarPedido(pedido, tmpFile);

                pedido.CambiarEstado(EstadosPedido.Confirmado);
                pedido.CambiarEstado(EstadosPedido.Enviado);
                pedido.CambiarEstado(EstadosPedido.Entregado);

                Assert.Throws<InvalidOperationException>(() => bm.CancelarPedido(pedido.ID, tmpFile));
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        // ============================================================
        // SPRINT 1 - TESTS NUEVOS: CONGELAMIENTO DE DATOS
        // ============================================================

        [Fact]
        public void Pedido_DatosCongelados_CuandoEnviado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            Assert.False(p.DatosCongelados);

            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            p.CambiarEstado(EstadosPedido.Enviado);

            Assert.True(p.DatosCongelados);
        }

        [Fact]
        public void Pedido_DatosCongelados_CuandoEntregado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            p.CambiarEstado(EstadosPedido.Enviado);
            p.CambiarEstado(EstadosPedido.Entregado);

            Assert.True(p.DatosCongelados);
        }

        [Fact]
        public void Pedido_ValidarNoCongelado_LanzaErrorCuandoCongelado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            p.CambiarEstado(EstadosPedido.Enviado);

            Assert.Throws<InvalidOperationException>(() => p.ValidarNoCongelado());
        }

        [Fact]
        public void Pedido_ValidarNoCongelado_NoLanzaCuandoActivo()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);

            // No debe lanzar excepción
            p.ValidarNoCongelado();
        }

        // ============================================================
        // SPRINT 1 - TESTS NUEVOS: REGISTRAR PEDIDO USA ID CRONOLÓGICO
        // ============================================================

        [Fact]
        public void BaseMaestra_RegistrarPedido_UsaIdCronologico()
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), $"shoes_manager_test_{Guid.NewGuid()}.json");
            try
            {
                var bm = new BaseMaestra();
                bm.InicializarInventarioPredeterminado();
                var items = new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = bm.Saldos[0].ID, Talla = "42", Cantidad = 1, PrecioPactado = 10m }
                };
                // Usar constructor sin parámetros para que ID esté vacío
                // y RegistrarPedido le asigne el ID cronológico
                var pedido = new Pedido
                {
                    ClienteTelefono = "+123456",
                    Items = items
                };
                string id = bm.RegistrarPedido(pedido, tmpFile);

                Assert.StartsWith("PED-", id);
                Assert.Matches(@"^PED-\d{8}-\d{3}$", id);
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }
    }
}
