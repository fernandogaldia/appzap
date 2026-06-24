using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ShoeManager.Core
{
    /// <summary>
    /// Motor de Normalización Lingüística para mensajes de WhatsApp.
    /// Transforma texto en un formato estándar: MAYÚSCULAS, sin acentos, sin emoticonos.
    /// </summary>
    public static class NormalizadorLinguistico
    {
        /// <summary>
        /// Normaliza un texto completo: mayúsculas, sin acentos, sin emoticonos, espacios unificados.
        /// </summary>
        public static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string resultado = texto.ToUpperInvariant();

            // Remover acentos
            resultado = resultado
                .Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I')
                .Replace('Ó', 'O').Replace('Ú', 'U')
                .Replace('À', 'A').Replace('È', 'E').Replace('Ì', 'I')
                .Replace('Ò', 'O').Replace('Ù', 'U')
                .Replace('Ä', 'A').Replace('Ë', 'E').Replace('Ï', 'I')
                .Replace('Ö', 'O').Replace('Ü', 'U')
                .Replace('Ñ', 'N');

            // Remover acentos de letras minúsculas
            resultado = resultado
                .Replace('á', 'A').Replace('é', 'E').Replace('í', 'I')
                .Replace('ó', 'O').Replace('ú', 'U')
                .Replace('à', 'A').Replace('è', 'E').Replace('ì', 'I')
                .Replace('ò', 'O').Replace('ù', 'U')
                .Replace('ä', 'A').Replace('ë', 'E').Replace('ï', 'I')
                .Replace('ö', 'O').Replace('ü', 'U')
                .Replace('ñ', 'N');

            // Remover emoticonos y símbolos gráficos (rango Unicode de emojis)
            resultado = Regex.Replace(resultado, @"[\u1F00-\u1FFF\u2600-\u27BF\uD800-\uDBFF\uDC00-\uDFFF]", "");

            // Remover símbolos no alfanuméricos (mantener letras, números, espacios, /, -, ., #)
            resultado = Regex.Replace(resultado, @"[^A-Z0-9\s\/\.#\-+_(),]", " ");

            // Unificar espacios
            resultado = Regex.Replace(resultado, @"\s+", " ").Trim();

            return resultado;
        }

        /// <summary>
        /// Extrae números de un texto (para detectar tallas, cantidades, precios).
        /// </summary>
        public static string ExtraerNumeros(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var match = Regex.Match(texto, @"\d+[\.\,]?\d*");
            return match.Success ? match.Value.Replace(',', '.') : string.Empty;
        }

        /// <summary>
        /// Verifica si un texto contiene un número de calzado válido (34 al 45, incluyendo medios).
        /// </summary>
        public static bool EsTallaValida(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;
            // Buscar patrones como "42", "42.5", "42,5"
            var match = Regex.Match(texto, @"(\d+)[\.\,]?(\d*)?");
            if (!match.Success) return false;

            string entero = match.Groups[1].Value;
            string decimalPart = match.Groups[2].Value;

            if (int.TryParse(entero, out int numEntero))
            {
                if (!string.IsNullOrEmpty(decimalPart))
                {
                    return numEntero >= 34 && numEntero <= 45;
                }
                return numEntero >= 34 && numEntero <= 45;
            }
            return false;
        }
    }
}