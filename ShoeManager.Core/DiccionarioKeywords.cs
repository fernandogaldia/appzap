using System.Collections.Generic;

namespace ShoeManager.Core
{
    /// <summary>
    /// Diccionario de palabras clave para el procesamiento semántico de mensajes WhatsApp.
    /// Organizado por categorías: Stock, Dirección, Cliente, Horario.
    /// </summary>
    public static class DiccionarioKeywords
    {
        /// <summary>
        /// Palabras clave para detección de consultas de stock/disponibilidad.
        /// </summary>
        public static readonly HashSet<string> Stock = new HashSet<string>
        {
            "TALLA", "NUMERO", "CALCE", "MEDIDA", "STOCK", "HAY",
            "TIENES", "DISponible", "CUANTO", "PRECIO", "VALOR",
            "SALE", "COSTO", "CUAL ES EL PRECIO", "PRECIO DE",
            "TIENE", "EXISTE", "CONSIGUE"
        };

        /// <summary>
        /// Palabras clave para detección de direcciones de entrega.
        /// </summary>
        public static readonly HashSet<string> Direccion = new HashSet<string>
        {
            "AVENIDA", "AV.", "AV", "CALLE", "CL.", "CL",
            "JR.", "JR", "JIRON", "PASAJE", "PSJ.", "PSJ",
            "URBANIZACION", "URB.", "URB",
            "DEPARTAMENTO", "DEPTO", "DPTO",
            "CASA", "MANZANA", "MZ.", "MZ",
            "LOTE", "LT.", "LT", "PISO",
            "INTERIOR", "INT.", "INT",
            "SECTOR", "BARRIO", "RESIDENCIAL",
            "KM", "CARRETERA", "PANAMERICANA"
        };

        /// <summary>
        /// Palabras clave para detección de identidad del cliente.
        /// </summary>
        public static readonly HashSet<string> Cliente = new HashSet<string>
        {
            "MI NOMBRE ES", "NOMBRE ES", "ME LLAMO", "SOY",
            "A NOMBRE DE", "PARA", "YO SOY", "MI NOMBRE",
            "SE LLAMA", "DIRIGIDO A", "ATT", "ATT."
        };

        /// <summary>
        /// Palabras clave para detección de horarios de entrega.
        /// </summary>
        public static readonly HashSet<string> Horario = new HashSet<string>
        {
            "MAÑANA", "MANANA", "TARDE", "NOCHE", "MADRUGADA",
            "SABADO", "SÁBADO", "DOMINGO",
            "ENTRE LAS", "HORAS", "A PARTIR DE",
            "DESDE LAS", "HASTA LAS", "EN LA MAÑANA",
            "EN LA TARDE", "EN LA NOCHE", "POR LA MAÑANA",
            "POR LA TARDE", "POR LA NOCHE",
            "TEMPRANO", "TARDE", "AL MEDIODIA",
            "MEDIODIA", "12 DEL MEDIODIA", "1 PM",
            "2 PM", "3 PM", "4 PM", "5 PM", "6 PM",
            "7 PM", "8 PM", "9 PM", "10 AM", "11 AM"
        };

        /// <summary>
        /// Verifica si un texto contiene alguna palabra clave de la categoría especificada.
        /// </summary>
        public static bool ContienePalabraClave(string texto, HashSet<string> categoria)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;
            foreach (var keyword in categoria)
            {
                if (texto.Contains(keyword))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Encuentra la primera palabra clave de una categoría en el texto y devuelve su posición.
        /// </summary>
        public static int EncontrarPosicionKeyword(string texto, HashSet<string> categoria)
        {
            if (string.IsNullOrWhiteSpace(texto)) return -1;
            int minPos = int.MaxValue;
            foreach (var keyword in categoria)
            {
                int pos = texto.IndexOf(keyword);
                if (pos >= 0 && pos < minPos)
                    minPos = pos;
            }
            return minPos == int.MaxValue ? -1 : minPos;
        }
    }
}