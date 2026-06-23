using System;
using System.Collections.Generic;

namespace ShoeManager.Core
{
    /// <summary>
    /// Procesador Semántico de mensajes de WhatsApp.
    /// Extrae datos estructurados (talla, dirección, cliente, horario) del texto normalizado.
    /// </summary>
    public class ResultadoProcesamiento
    {
        public string TextoOriginal { get; set; } = string.Empty;
        public string TextoNormalizado { get; set; } = string.Empty;
        public string? TallaDetectada { get; set; }
        public int? CantidadDetectada { get; set; }
        public string? DireccionDetectada { get; set; }
        public string? NombreClienteDetectado { get; set; }
        public string? HorarioDetectado { get; set; }
        public bool EsConsultaStock { get; set; }
        public bool TieneDatosEntrega { get; set; }
    }

    public static class ProcesadorSemantico
    {
        /// <summary>
        /// Procesa un mensaje completo y extrae toda la información posible.
        /// </summary>
        public static ResultadoProcesamiento Procesar(string textoOriginal)
        {
            var resultado = new ResultadoProcesamiento
            {
                TextoOriginal = textoOriginal,
                TextoNormalizado = NormalizadorLinguistico.Normalizar(textoOriginal)
            };

            string normalizado = resultado.TextoNormalizado;

            // Detectar si es consulta de stock
            resultado.EsConsultaStock = DiccionarioKeywords.ContienePalabraClave(normalizado, DiccionarioKeywords.Stock);

            // Detectar talla
            resultado.TallaDetectada = DetectarTalla(normalizado);

            // Detectar cantidad (numeros pequeños, 1-99)
            resultado.CantidadDetectada = DetectarCantidad(normalizado);

            // Detectar dirección
            resultado.DireccionDetectada = ExtraerDireccion(normalizado);

            // Detectar nombre de cliente
            resultado.NombreClienteDetectado = ExtraerNombreCliente(normalizado, textoOriginal);

            // Detectar horario
            resultado.HorarioDetectado = ExtraerHorario(normalizado);

            // Determinar si tiene datos de entrega
            resultado.TieneDatosEntrega = !string.IsNullOrWhiteSpace(resultado.DireccionDetectada) ||
                                          !string.IsNullOrWhiteSpace(resultado.NombreClienteDetectado) ||
                                          !string.IsNullOrWhiteSpace(resultado.HorarioDetectado);

            return resultado;
        }

        /// <summary>
        /// Detecta una talla de calzado en el texto normalizado (34-45).
        /// </summary>
        private static string? DetectarTalla(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            // Buscar patrones de talla: número seguido de contexto de calzado
            var palabras = texto.Split(' ');
            foreach (var palabra in palabras)
            {
                if (NormalizadorLinguistico.EsTallaValida(palabra))
                {
                    return palabra;
                }
            }

            // Buscar número simple en rango de tallas
            var numeros = System.Text.RegularExpressions.Regex.Matches(texto, @"\d+[\.\,]?\d*");
            foreach (System.Text.RegularExpressions.Match match in numeros)
            {
                string val = match.Value.Replace(',', '.');
                if (double.TryParse(val, out double num))
                {
                    if (num >= 34 && num <= 45)
                        return val;
                }
            }

            return null;
        }

        /// <summary>
        /// Detecta una cantidad (1-99) en el texto.
        /// </summary>
        private static int? DetectarCantidad(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            var matches = System.Text.RegularExpressions.Regex.Matches(texto, @"\b(\d+)\b");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (int.TryParse(match.Value, out int num))
                {
                    if (num >= 1 && num <= 99 && (num < 34 || num > 45)) // No confundir con tallas
                        return num;
                }
            }

            return null;
        }

        /// <summary>
        /// Extrae la dirección completa después de una palabra clave de dirección.
        /// </summary>
        private static string? ExtraerDireccion(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            int pos = DiccionarioKeywords.EncontrarPosicionKeyword(texto, DiccionarioKeywords.Direccion);
            if (pos < 0) return null;

            // Capturar desde la palabra clave hasta 100 caracteres o hasta el final
            int start = pos;
            int length = Math.Min(100, texto.Length - start);
            string direccion = texto.Substring(start, length).Trim();

            return string.IsNullOrWhiteSpace(direccion) ? null : direccion;
        }

        /// <summary>
        /// Extrae el nombre del cliente después de palabras clave de presentación.
        /// </summary>
        private static string? ExtraerNombreCliente(string texto, string textoOriginal)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            int pos = DiccionarioKeywords.EncontrarPosicionKeyword(texto, DiccionarioKeywords.Cliente);
            if (pos < 0) return null;

            // Obtener la palabra clave encontrada
            string despues = texto.Substring(pos);
            int length = Math.Min(60, despues.Length);
            string nombre = despues.Substring(0, length).Trim();

            // Limpiar la palabra clave del inicio
            foreach (var kw in DiccionarioKeywords.Cliente)
            {
                if (nombre.StartsWith(kw))
                {
                    nombre = nombre.Substring(kw.Length).Trim();
                    break;
                }
            }

            // Limitar al primer número o palabra vacía
            int endPos = nombre.IndexOfAny(new[] { ' ', ',' });
            if (endPos > 0)
                nombre = nombre.Substring(0, endPos).Trim();

            return string.IsNullOrWhiteSpace(nombre) ? null : nombre;
        }

        /// <summary>
        /// Extrae el horario de entrega después de palabras clave de horario.
        /// </summary>
        private static string? ExtraerHorario(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            int pos = DiccionarioKeywords.EncontrarPosicionKeyword(texto, DiccionarioKeywords.Horario);
            if (pos < 0) return null;

            int length = Math.Min(50, texto.Length - pos);
            string horario = texto.Substring(pos, length).Trim();

            return string.IsNullOrWhiteSpace(horario) ? null : horario;
        }
    }
}