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
        // SPRINT 1 - ID CRONOLÓGICO
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

            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id2, id3);

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
        // STK-03 LIQUIDACIÓN DINÁMICA
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

            saldo.ReservarStock("42", 2);

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
        // REVERSA DE STOCK
        // ============================================================

        [Fact]
        public void Saldo_ReintegrarStock_ReactivaProductoLiquidado()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 1 } }
            };
            saldo.ReservarStock("42", 1);
            Assert.False(saldo.EsActivo);

            saldo.ReintegrarStock("42", 2);

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

                Assert.Equal(stockAntes - 2, bm.Saldos[0].ObtenerStock("42"));

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
        // CONGELAMIENTO DE DATOS
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

            p.ValidarNoCongelado();
        }

        // ============================================================
        // REGISTRAR PEDIDO USA ID CRONOLÓGICO
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

        // ============================================================
        // TESTS: NORMALIZADOR LINGÜÍSTICO
        // ============================================================

        [Fact]
        public void Normalizador_RemoverAcentos_Correctamente()
        {
            string resultado = NormalizadorLinguistico.Normalizar("Café con Martínez y Pérez");
            Assert.DoesNotContain("é", resultado);
            Assert.DoesNotContain("í", resultado);
            Assert.Contains("CAFE", resultado);
            Assert.Contains("MARTINEZ", resultado);
            Assert.Contains("PEREZ", resultado);
        }

        [Fact]
        public void Normalizador_ConvertirMayusculas()
        {
            string resultado = NormalizadorLinguistico.Normalizar("Hola Mundo");
            Assert.Equal("HOLA MUNDO", resultado);
        }

        [Fact]
        public void Normalizador_RemoverEmoticonos()
        {
            string resultado = NormalizadorLinguistico.Normalizar("Hola 😊👍❤️");
            Assert.True(resultado.Contains("HOLA") || resultado.Trim() == "HOLA");
        }

        [Fact]
        public void Normalizador_EsTallaValida_Rango34_45()
        {
            Assert.True(NormalizadorLinguistico.EsTallaValida("42"));
            Assert.True(NormalizadorLinguistico.EsTallaValida("34"));
            Assert.True(NormalizadorLinguistico.EsTallaValida("45"));
            Assert.False(NormalizadorLinguistico.EsTallaValida("33"));
            Assert.False(NormalizadorLinguistico.EsTallaValida("46"));
            Assert.False(NormalizadorLinguistico.EsTallaValida(""));
        }

        // ============================================================
        // TESTS: DICCIONARIO KEYWORDS
        // ============================================================

        [Fact]
        public void DiccionarioKeywords_ContieneKeywordStock()
        {
            Assert.True(DiccionarioKeywords.ContienePalabraClave("QUIERO TALLA 42", DiccionarioKeywords.Stock));
            Assert.True(DiccionarioKeywords.ContienePalabraClave("CUAL ES EL PRECIO", DiccionarioKeywords.Stock));
            Assert.False(DiccionarioKeywords.ContienePalabraClave("AVENIDA SIEMPRE VIVA", DiccionarioKeywords.Stock));
        }

        [Fact]
        public void DiccionarioKeywords_ContieneKeywordDireccion()
        {
            Assert.True(DiccionarioKeywords.ContienePalabraClave("CALLE LOS OLIVOS", DiccionarioKeywords.Direccion));
            Assert.True(DiccionarioKeywords.ContienePalabraClave("AVENIDA SIEMPRE VIVA", DiccionarioKeywords.Direccion));
            Assert.False(DiccionarioKeywords.ContienePalabraClave("TALLA 42", DiccionarioKeywords.Direccion));
        }

        [Fact]
        public void DiccionarioKeywords_ContieneKeywordCliente()
        {
            Assert.True(DiccionarioKeywords.ContienePalabraClave("MI NOMBRE ES JUAN", DiccionarioKeywords.Cliente));
            Assert.False(DiccionarioKeywords.ContienePalabraClave("TALLA 42", DiccionarioKeywords.Cliente));
        }

        [Fact]
        public void DiccionarioKeywords_ContieneKeywordHorario()
        {
            Assert.True(DiccionarioKeywords.ContienePalabraClave("ENTREGAR EN LA MAÑANA", DiccionarioKeywords.Horario));
            Assert.True(DiccionarioKeywords.ContienePalabraClave("POR LA TARDE", DiccionarioKeywords.Horario));
        }

        [Fact]
        public void DiccionarioKeywords_EncontrarPosicionKeyword_RetornaPosicion()
        {
            string texto = "QUIERO TALLA 42 POR FAVOR";
            int pos = DiccionarioKeywords.EncontrarPosicionKeyword(texto, DiccionarioKeywords.Stock);
            Assert.True(pos >= 0);
        }

        [Fact]
        public void DiccionarioKeywords_EncontrarPosicionKeyword_RetornaMenos1SiNoEncuentra()
        {
            string texto = "HOLA MUNDO";
            int pos = DiccionarioKeywords.EncontrarPosicionKeyword(texto, DiccionarioKeywords.Direccion);
            Assert.Equal(-1, pos);
        }

        // ============================================================
        // TESTS: PROCESADOR SEMÁNTICO
        // ============================================================

        [Fact]
        public void ProcesadorSemantico_ExtraerTalla_DeTexto()
        {
            var resultado = ProcesadorSemantico.Procesar("Quiero talla 42");
            Assert.Equal("42", resultado.TallaDetectada);
        }

        [Fact]
        public void ProcesadorSemantico_DetectarConsultaStock()
        {
            var resultado = ProcesadorSemantico.Procesar("Tienes zapatos talla 42?");
            Assert.True(resultado.EsConsultaStock);
        }

        [Fact]
        public void ProcesadorSemantico_ExtraerNombreCliente()
        {
            var resultado = ProcesadorSemantico.Procesar("MI NOMBRE ES JUAN quiero talla 42");
            Assert.Contains("JUAN", resultado.NombreClienteDetectado);
        }

        [Fact]
        public void ProcesadorSemantico_ProcesarMensajeVacio()
        {
            var resultado = ProcesadorSemantico.Procesar("");
            Assert.Null(resultado.TallaDetectada);
            Assert.False(resultado.EsConsultaStock);
        }

        [Fact]
        public void ProcesadorSemantico_ResultadoTienePropiedades()
        {
            var resultado = ProcesadorSemantico.Procesar("Hola");
            Assert.NotNull(resultado.TextoOriginal);
            Assert.NotNull(resultado.TextoNormalizado);
        }

        // ============================================================
        // TESTS: GENERADOR DE ENLACES
        // ============================================================

        [Fact]
        public void GeneradorEnlaces_LimpiarTelefono()
        {
            string enlace = GeneradorEnlaces.GenerarEnlaceConfirmacion("+51 999 123 456",
                new Pedido("+51999123456", new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
                }));
            Assert.Contains("/51999123456", enlace);
        }

        [Fact]
        public void GeneradorEnlaces_GenerarEnlaceContieneWhatsApp()
        {
            string enlace = GeneradorEnlaces.GenerarEnlaceConfirmacion("+51999123456",
                new Pedido("+51999123456", new List<DetallePedido>
                {
                    new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
                }));
            Assert.StartsWith("https://wa.me/", enlace);
        }

        // ============================================================
        // TESTS: LOGGER LOCAL
        // ============================================================

        [Fact]
        public void LoggerLocal_RegistrarInfo_CreaArchivo()
        {
            string logFolder = Path.Combine(Path.GetTempPath(), $"logs_test_{Guid.NewGuid()}");
            try
            {
                var logger = new LoggerLocal(logFolder);
                logger.Info("Test message");
                
                string logFile = Path.Combine(logFolder, $"shoemanager_{DateTime.Now:yyyyMMdd}.log");
                Assert.True(File.Exists(logFile));
                string content = File.ReadAllText(logFile);
                Assert.Contains("INFO", content);
                Assert.Contains("Test message", content);
            }
            finally
            {
                if (Directory.Exists(logFolder)) Directory.Delete(logFolder, true);
            }
        }

        [Fact]
        public void LoggerLocal_RegistrarWarning_ContieneNivel()
        {
            string logFolder = Path.Combine(Path.GetTempPath(), $"logs_test_{Guid.NewGuid()}");
            try
            {
                var logger = new LoggerLocal(logFolder);
                logger.Warning("Warning message");
                
                string logFile = Path.Combine(logFolder, $"shoemanager_{DateTime.Now:yyyyMMdd}.log");
                string content = File.ReadAllText(logFile);
                Assert.Contains("WARN", content);
                Assert.Contains("Warning message", content);
            }
            finally
            {
                if (Directory.Exists(logFolder)) Directory.Delete(logFolder, true);
            }
        }

        [Fact]
        public void LoggerLocal_RegistrarError_ContieneNivel()
        {
            string logFolder = Path.Combine(Path.GetTempPath(), $"logs_test_{Guid.NewGuid()}");
            try
            {
                var logger = new LoggerLocal(logFolder);
                logger.Error("Error message");
                
                string logFile = Path.Combine(logFolder, $"shoemanager_{DateTime.Now:yyyyMMdd}.log");
                string content = File.ReadAllText(logFile);
                Assert.Contains("ERROR", content);
                Assert.Contains("Error message", content);
            }
            finally
            {
                if (Directory.Exists(logFolder)) Directory.Delete(logFolder, true);
            }
        }

        // ============================================================
        // TESTS: MOTOR DE FUSIÓN
        // ============================================================

        [Fact]
        public void MotorFusion_ResultadoFusion_TienePropiedades()
        {
            var resultado = new ResultadoFusion
            {
                Exitoso = true,
                SaldosNuevos = 3,
                SaldosActualizados = 2,
                ClientesNuevos = 1,
                ClientesActualizados = 0,
                PedidosNuevos = 4,
                ImagenesSincronizadas = 7,
                Errores = new List<string>()
            };

            Assert.True(resultado.Exitoso);
            Assert.Equal(3, resultado.SaldosNuevos);
            Assert.Empty(resultado.Errores);
            Assert.NotNull(resultado.Resumen);
        }

        // ============================================================
        // TESTS: ESTADOS DE PEDIDO
        // ============================================================

        [Fact]
        public void Pedido_TransicionEstado_PendienteACreado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            Assert.Equal(EstadosPedido.Pendiente, p.Estado);
            p.CambiarEstado(EstadosPedido.Creado);
            Assert.Equal(EstadosPedido.Creado, p.Estado);
        }

        [Fact]
        public void Pedido_TransicionEstado_CreadoAConfirmado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            Assert.Equal(EstadosPedido.Confirmado, p.Estado);
        }

        [Fact]
        public void Pedido_TransicionEstado_ConfirmadoAEnviado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            p.CambiarEstado(EstadosPedido.Enviado);
            Assert.Equal(EstadosPedido.Enviado, p.Estado);
        }

        [Fact]
        public void Pedido_TransicionEstado_EnviadoAEntregado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            p.CambiarEstado(EstadosPedido.Enviado);
            p.CambiarEstado(EstadosPedido.Entregado);
            Assert.Equal(EstadosPedido.Entregado, p.Estado);
        }

        [Fact]
        public void Pedido_TransicionEstado_Cancelado_DesdePendiente()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            Assert.Equal(EstadosPedido.Pendiente, p.Estado);
            p.CambiarEstado(EstadosPedido.Cancelado);
            Assert.Equal(EstadosPedido.Cancelado, p.Estado);
        }

        [Fact]
        public void Pedido_TransicionEstado_NoPermiteEntregadoACancelado()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 1, PrecioPactado = 10m }
            };
            var p = new Pedido("+123456", items);
            p.CambiarEstado(EstadosPedido.Creado);
            p.CambiarEstado(EstadosPedido.Confirmado);
            p.CambiarEstado(EstadosPedido.Enviado);
            p.CambiarEstado(EstadosPedido.Entregado);
            Assert.Throws<InvalidOperationException>(() => p.CambiarEstado(EstadosPedido.Cancelado));
        }

        // ============================================================
        // TESTS: VALIDACIONES DE SALDO
        // ============================================================

        [Fact]
        public void Saldo_ObtenerStock_RetornaCeroSiNoExiste()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
            };
            Assert.Equal(0, saldo.ObtenerStock("99"));
        }

        [Fact]
        public void Saldo_ReservarStock_ReduceStockCorrectamente()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 10 } }
            };
            saldo.ReservarStock("42", 3);
            Assert.Equal(7, saldo.StockPorTalla["42"]);
        }

        [Fact]
        public void Saldo_TieneStock_TrueCuandoDisponible()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
            };
            Assert.True(saldo.TieneStock("42", 3));
        }

        [Fact]
        public void Saldo_TieneStock_FalseCuandoInsuficiente()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 2 } }
            };
            Assert.False(saldo.TieneStock("42", 5));
        }

        [Fact]
        public void Saldo_TieneStock_FalseCuandoTallaNoExiste()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
            };
            Assert.False(saldo.TieneStock("99", 1));
        }

        [Fact]
        public void Saldo_ReintegrarStock_IncrementaStock()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
            };
            saldo.ReintegrarStock("42", 3);
            Assert.Equal(8, saldo.StockPorTalla["42"]);
        }

        [Fact]
        public void Saldo_ReintegrarStock_NuevaTalla()
        {
            var saldo = new Saldo
            {
                StockPorTalla = new Dictionary<string, int> { { "42", 5 } }
            };
            saldo.ReintegrarStock("43", 10);
            Assert.Equal(10, saldo.StockPorTalla["43"]);
        }

        [Fact]
        public void Saldo_RutaFotoFrontal_SeGuardaCorrectamente()
        {
            var saldo = new Saldo
            {
                RutaFotoFrontal = "C:\\fotos\\producto.jpg"
            };
            Assert.Equal("C:\\fotos\\producto.jpg", saldo.RutaFotoFrontal);
        }

        [Fact]
        public void Saldo_EsActivo_ValorPorDefectoTrue()
        {
            var saldo = new Saldo();
            Assert.True(saldo.EsActivo);
        }

        [Fact]
        public void Pedido_CalcularMontoTotal_Correcto()
        {
            var items = new List<DetallePedido> {
                new DetallePedido { SaldoID = "s1", Talla = "42", Cantidad = 3, PrecioPactado = 25m }
            };
            var p = new Pedido("+123456", items);
            Assert.Equal(75m, p.MontoTotal);
        }
    }
}