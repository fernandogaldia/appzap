using System;
using System.IO;

namespace ShoeManager.Core
{
    /// <summary>
    /// Sistema de logging local para operaciones de la aplicación.
    /// Escribe logs rotatorios en un archivo de texto plano.
    /// Niveles: Info, Warning, Error.
    /// No almacena datos personales (PII).
    /// </summary>
    public class LoggerLocal
    {
        private readonly string _rutaLog;
        private readonly string _nombreApp;
        private static readonly object _lock = new object();

        public LoggerLocal(string directorioLogs, string nombreApp = "ShoeManager")
        {
            _nombreApp = nombreApp;
            _rutaLog = Path.Combine(directorioLogs, $"{_nombreApp}_{DateTime.Now:yyyyMMdd}.log");
            Directory.CreateDirectory(directorioLogs);
        }

        public void Info(string mensaje)
        {
            Escribir("INFO", mensaje);
        }

        public void Warning(string mensaje)
        {
            Escribir("WARNING", mensaje);
        }

        public void Error(string mensaje, Exception? ex = null)
        {
            string texto = mensaje;
            if (ex != null)
                texto += $" | Excepción: {ex.GetType().Name} - {ex.Message}";
            Escribir("ERROR", texto);
        }

        public void Operacion(string usuario, string accion, string detalle)
        {
            // Sin datos personales: solo IDs y acciones
            Info($"OPERACION|{usuario}|{accion}|{detalle}");
        }

        private void Escribir(string nivel, string mensaje)
        {
            try
            {
                lock (_lock)
                {
                    string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {nivel,-7} | {_nombreApp} | {mensaje}";
                    File.AppendAllText(_rutaLog, linea + Environment.NewLine);
                }
            }
            catch
            {
                // Silencioso: no debe romper la app si falla el log
            }
        }

        /// <summary>
        /// Limpia logs con más de 30 días de antigüedad.
        /// </summary>
        public void LimpiarLogsViejos(string directorioLogs)
        {
            try
            {
                var archivos = Directory.GetFiles(directorioLogs, $"{_nombreApp}_*.log");
                var fechaLimite = DateTime.Now.AddDays(-30);
                foreach (var archivo in archivos)
                {
                    if (File.GetCreationTime(archivo) < fechaLimite)
                        File.Delete(archivo);
                }
            }
            catch { }
        }
    }
}