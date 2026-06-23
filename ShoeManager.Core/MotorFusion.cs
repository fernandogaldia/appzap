using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ShoeManager.Core
{
    /// <summary>
    /// Algoritmo de Fusión Incremental de Datos (Merge bidireccional por USB).
    /// Paso 1-6 del Plan Maestro Fase 5.
    /// </summary>
    public class ResultadoFusion
    {
        public bool Exitoso { get; set; }
        public int SaldosActualizados { get; set; }
        public int SaldosNuevos { get; set; }
        public int ClientesActualizados { get; set; }
        public int ClientesNuevos { get; set; }
        public int PedidosNuevos { get; set; }
        public int ImagenesSincronizadas { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
        public DateTime TimestampFusion { get; set; } = DateTime.Now;
        public string Resumen => $"✅ Fusión completada: {SaldosNuevos} saldos nuevos, " +
                                 $"{SaldosActualizados} actualizados, " +
                                 $"{ClientesNuevos} clientes nuevos, " +
                                 $"{PedidosNuevos} pedidos, " +
                                 $"{ImagenesSincronizadas} imágenes";
    }

    public static class MotorFusion
    {
        /// <summary>
        /// Ejecuta el merge bidireccional completo entre dos archivos de base de datos.
        /// </summary>
        public static ResultadoFusion Fusionar(string rutaBasePC, string rutaBaseCelular, string rutaCarpetaImagenesPC, string rutaCarpetaImagenesCelular)
        {
            var resultado = new ResultadoFusion();

            try
            {
                // Paso 2: Carga paralela en RAM
                BaseMaestra basePC, baseCel;
                try
                {
                    basePC = BaseMaestra.Cargar(rutaBasePC);
                }
                catch
                {
                    throw new InvalidOperationException($"No se pudo cargar la base de PC: {rutaBasePC}");
                }

                try
                {
                    baseCel = BaseMaestra.Cargar(rutaBaseCelular);
                }
                catch
                {
                    throw new InvalidOperationException($"No se pudo cargar la base del celular: {rutaBaseCelular}");
                }

                // Paso 3 y 4: Fusión de Saldos
                FusionarColeccionSaldos(basePC, baseCel, resultado);

                // Fusión de Clientes
                FusionarColeccionClientes(basePC, baseCel, resultado);

                // Fusión de Pedidos (solo nuevos, no se modifican existentes)
                FusionarColeccionPedidos(basePC, baseCel, resultado);

                // Paso 5: Escritura atómica
                EscribirFusion(basePC, rutaBasePC, rutaBaseCelular);

                // Paso 6: Sincronización binaria de imágenes
                resultado.ImagenesSincronizadas = SincronizarImagenes(
                    rutaCarpetaImagenesPC, rutaCarpetaImagenesCelular);

                resultado.Exitoso = true;
                basePC.UltimaSincronizacion = DateTime.Now;
                basePC.Guardar(rutaBasePC);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Errores.Add($"Error crítico: {ex.Message}");
            }

            return resultado;
        }

        private static void FusionarColeccionSaldos(BaseMaestra destino, BaseMaestra fuente, ResultadoFusion resultado)
        {
            foreach (var saldoFuente in fuente.Saldos)
            {
                var existente = destino.Saldos.Find(s =>
                    s.ID == saldoFuente.ID || s.UPC == saldoFuente.UPC);

                if (existente != null)
                {
                    // Conflicto: resolver por timestamp (el más reciente gana)
                    if (saldoFuente.FechaModificacion > existente.FechaModificacion)
                    {
                        CopiarSaldo(saldoFuente, existente);
                        resultado.SaldosActualizados++;
                    }
                }
                else
                {
                    // Registro huérfano: insertar
                    destino.Saldos.Add(new Saldo
                    {
                        ID = saldoFuente.ID,
                        UPC = saldoFuente.UPC,
                        Marca = saldoFuente.Marca,
                        Modelo = saldoFuente.Modelo,
                        Precio = saldoFuente.Precio,
                        StockPorTalla = new Dictionary<string, int>(saldoFuente.StockPorTalla),
                        RutaFotoFrontal = saldoFuente.RutaFotoFrontal,
                        RutaFotoPerfil = saldoFuente.RutaFotoPerfil,
                        EsActivo = saldoFuente.EsActivo,
                        FechaLiquidacion = saldoFuente.FechaLiquidacion,
                        FechaCreacion = saldoFuente.FechaCreacion,
                        FechaModificacion = saldoFuente.FechaModificacion
                    });
                    resultado.SaldosNuevos++;
                }
            }
        }

        private static void CopiarSaldo(Saldo fuente, Saldo destino)
        {
            destino.Marca = fuente.Marca;
            destino.Modelo = fuente.Modelo;
            destino.Precio = fuente.Precio;
            destino.StockPorTalla = new Dictionary<string, int>(fuente.StockPorTalla);
            destino.RutaFotoFrontal = fuente.RutaFotoFrontal;
            destino.RutaFotoPerfil = fuente.RutaFotoPerfil;
            destino.EsActivo = fuente.EsActivo;
            destino.FechaLiquidacion = fuente.FechaLiquidacion;
            destino.FechaModificacion = fuente.FechaModificacion;
        }

        private static void FusionarColeccionClientes(BaseMaestra destino, BaseMaestra fuente, ResultadoFusion resultado)
        {
            foreach (var clienteFuente in fuente.Clientes)
            {
                var existente = destino.Clientes.Find(c => c.Telefono == clienteFuente.Telefono);
                if (existente != null)
                {
                    existente.Nombre ??= clienteFuente.Nombre;
                    existente.Direccion ??= clienteFuente.Direccion;
                    existente.PreferenciaHoraria ??= clienteFuente.PreferenciaHoraria;
                    resultado.ClientesActualizados++;
                }
                else
                {
                    destino.Clientes.Add(new Cliente(clienteFuente.Telefono, clienteFuente.Nombre, clienteFuente.Direccion));
                    resultado.ClientesNuevos++;
                }
            }
        }

        private static void FusionarColeccionPedidos(BaseMaestra destino, BaseMaestra fuente, ResultadoFusion resultado)
        {
            foreach (var pedidoFuente in fuente.Pedidos)
            {
                var existente = destino.Pedidos.Find(p => p.ID == pedidoFuente.ID);
                if (existente == null)
                {
                    destino.Pedidos.Add(pedidoFuente);
                    resultado.PedidosNuevos++;
                }
            }
        }

        private static void EscribirFusion(BaseMaestra baseUnificada, string rutaPC, string rutaCelular)
        {
            // Backup antes de escribir
            string backupPC = rutaPC + ".bak";
            if (File.Exists(rutaPC))
                File.Copy(rutaPC, backupPC, true);

            // Escritura atómica: primero PC
            baseUnificada.Guardar(rutaPC);

            // Luego celular (si la ruta es accesible)
            try
            {
                string? dirCel = Path.GetDirectoryName(rutaCelular);
                if (!string.IsNullOrEmpty(dirCel)) Directory.CreateDirectory(dirCel);
                baseUnificada.Guardar(rutaCelular);
            }
            catch
            {
                // Si falla la escritura al celular, continuar
                throw new InvalidOperationException(
                    "La base de PC se actualizó correctamente, pero no se pudo escribir en el celular. " +
                    "Verifica la conexión USB.");
            }
        }

        /// <summary>
        /// Sincroniza imágenes entre PC y Celular usando hash MD5.
        /// </summary>
        private static int SincronizarImagenes(string rutaPC, string rutaCelular)
        {
            int sincronizadas = 0;

            if (!Directory.Exists(rutaPC) || !Directory.Exists(rutaCelular))
                return 0;

            var imagenesPC = Directory.GetFiles(rutaPC, "*.jpg", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(rutaPC, "*.png", SearchOption.AllDirectories))
                .ToDictionary(f => Path.GetFileName(f), f => f);

            var imagenesCel = Directory.GetFiles(rutaCelular, "*.jpg", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(rutaCelular, "*.png", SearchOption.AllDirectories))
                .ToDictionary(f => Path.GetFileName(f), f => f);

            // Sincronizar de Celular a PC
            foreach (var kvp in imagenesCel)
            {
                if (!imagenesPC.ContainsKey(kvp.Key))
                {
                    string destinoPC = Path.Combine(rutaPC, kvp.Key);
                    File.Copy(kvp.Value, destinoPC);
                    sincronizadas++;
                }
                else if (CalcularHashMD5(kvp.Value) != CalcularHashMD5(imagenesPC[kvp.Key]))
                {
                    // Si el hash es diferente, la más reciente gana
                    FileInfo infoCel = new FileInfo(kvp.Value);
                    FileInfo infoPC = new FileInfo(imagenesPC[kvp.Key]);

                    if (infoCel.LastWriteTime > infoPC.LastWriteTime)
                    {
                        File.Copy(kvp.Value, imagenesPC[kvp.Key], true);
                        sincronizadas++;
                    }
                    else
                    {
                        File.Copy(imagenesPC[kvp.Key], kvp.Value, true);
                        sincronizadas++;
                    }
                }
            }

            // Sincronizar de PC a Celular (las que no existen en celular)
            foreach (var kvp in imagenesPC)
            {
                if (!imagenesCel.ContainsKey(kvp.Key))
                {
                    string destinoCel = Path.Combine(rutaCelular, kvp.Key);
                    File.Copy(kvp.Value, destinoCel);
                    sincronizadas++;
                }
            }

            return sincronizadas;
        }

        private static string CalcularHashMD5(string rutaArchivo)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(rutaArchivo);
            byte[] hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}