// ============================================================================
// Archivo: NativeFunctions.cs
// Descripción: Function tools nativas para comparación con herramientas MCP
// Módulo: 7
// Lab: 01-mcp-integration
// ============================================================================

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MCPIntegration;

/// <summary>
/// Herramientas nativas del agente (function tools de MAF).
/// Estas funcionan localmente sin necesidad de MCP.
/// Se usan para demostrar la diferencia con herramientas MCP.
/// </summary>
public class NativeFunctions
{
    /// <summary>
    /// Calcula la conversión de temperatura entre Celsius y Fahrenheit.
    /// Esta es una herramienta NATIVA que no requiere MCP.
    /// </summary>
    /// <param name="value">Valor de temperatura a convertir</param>
    /// <param name="fromUnit">Unidad de origen: "C" para Celsius, "F" para Fahrenheit</param>
    /// <returns>Temperatura convertida con descripción</returns>
    [KernelFunction("convert_temperature")]
    [Description("Convierte temperatura entre Celsius y Fahrenheit")]
    public string ConvertTemperature(
        [Description("Valor de temperatura a convertir")] double value,
        [Description("Unidad de origen: C para Celsius, F para Fahrenheit")] string fromUnit = "C")
    {
        // Validar unidad de entrada
        fromUnit = fromUnit.ToUpperInvariant();
        
        if (fromUnit == "C")
        {
            var fahrenheit = (value * 9 / 5) + 32;
            return $"🌡️ {value}°C = {fahrenheit:F1}°F";
        }
        else if (fromUnit == "F")
        {
            var celsius = (value - 32) * 5 / 9;
            return $"🌡️ {value}°F = {celsius:F1}°C";
        }
        else
        {
            return $"Unidad no válida: {fromUnit}. Use 'C' para Celsius o 'F' para Fahrenheit.";
        }
    }

    /// <summary>
    /// Obtiene la fecha y hora actual.
    /// Esta es una herramienta NATIVA útil para contexto temporal.
    /// </summary>
    /// <param name="timezone">Zona horaria (por defecto: Europe/Madrid)</param>
    /// <returns>Fecha y hora formateada</returns>
    [KernelFunction("get_datetime")]
    [Description("Obtiene la fecha y hora actual")]
    public string GetDateTime(
        [Description("Zona horaria, por defecto Europe/Madrid")] string timezone = "Europe/Madrid")
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            
            return $"📅 Fecha y hora actual ({timezone}):\n" +
                   $"   {now:dddd, dd 'de' MMMM 'de' yyyy}\n" +
                   $"   🕐 {now:HH:mm:ss}";
        }
        catch (TimeZoneNotFoundException)
        {
            var now = DateTime.Now;
            return $"📅 Fecha y hora local:\n" +
                   $"   {now:dddd, dd 'de' MMMM 'de' yyyy}\n" +
                   $"   🕐 {now:HH:mm:ss}\n" +
                   $"   ⚠️ Zona horaria '{timezone}' no encontrada, usando hora local.";
        }
    }

    /// <summary>
    /// Realiza cálculos matemáticos básicos.
    /// Esta es una herramienta NATIVA para operaciones matemáticas.
    /// </summary>
    /// <param name="expression">Expresión matemática simple (ej: "15 + 27")</param>
    /// <returns>Resultado del cálculo</returns>
    [KernelFunction("calculate")]
    [Description("Realiza cálculos matemáticos básicos: suma, resta, multiplicación, división")]
    public string Calculate(
        [Description("Expresión matemática simple, ej: '15 + 27' o '100 / 4'")] string expression)
    {
        try
        {
            // Parser simple para operaciones básicas
            // En producción usaríamos un evaluador de expresiones más robusto
            var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length != 3)
            {
                return "Formato: 'número operador número' (ej: '15 + 27')";
            }

            if (!double.TryParse(parts[0], out var a))
            {
                return $"'{parts[0]}' no es un número válido";
            }

            if (!double.TryParse(parts[2], out var b))
            {
                return $"'{parts[2]}' no es un número válido";
            }

            var result = parts[1] switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "x" => a * b,
                "/" => b != 0 ? a / b : double.NaN,
                _ => double.NaN
            };

            if (double.IsNaN(result))
            {
                return parts[1] == "/" 
                    ? "Error: División por cero" 
                    : $"Operador no válido: {parts[1]}. Use +, -, *, /";
            }

            return $"🔢 {expression} = {result:G}";
        }
        catch (Exception ex)
        {
            return $"Error en el cálculo: {ex.Message}";
        }
    }

    /// <summary>
    /// Proporciona información sobre las capacidades del agente.
    /// </summary>
    [KernelFunction("get_capabilities")]
    [Description("Lista las herramientas y capacidades disponibles del agente")]
    public string GetCapabilities()
    {
        return """
            🤖 Capacidades del Agente MCP Demo:

            📡 HERRAMIENTAS MCP (desde servidor externo):
               • get_weather - Clima actual de ciudades
               • get_forecast - Pronóstico del tiempo
               • get_headlines - Titulares de noticias

            🔧 HERRAMIENTAS NATIVAS (locales):
               • convert_temperature - Conversión C↔F
               • get_datetime - Fecha y hora actual
               • calculate - Operaciones matemáticas

            ℹ️ Este agente demuestra cómo MAF puede combinar
               herramientas de fuentes MCP externas con
               function tools nativas definidas localmente.
            """;
    }
}
