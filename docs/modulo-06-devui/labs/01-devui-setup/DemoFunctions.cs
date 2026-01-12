// ============================================================================
// Archivo: DemoFunctions.cs
// Descripción: Funciones de demostración para debugging con DevUI
// Módulo: 6 - DevUI
// Lab: 01-devui-setup
// ============================================================================

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace DevUIExample;

/// <summary>
/// Colección de function tools para demostración de debugging con DevUI.
/// Incluye funciones de clima, calendario y calculadora para mostrar
/// diferentes tipos de invocaciones y parámetros.
/// </summary>
public class DemoFunctions
{
    // ===== Datos simulados =====
    private static readonly Dictionary<string, (string Condition, int Temp)> _weather = new()
    {
        ["madrid"] = ("Soleado", 22),
        ["barcelona"] = ("Parcialmente nublado", 24),
        ["sevilla"] = ("Muy soleado", 30),
        ["valencia"] = ("Soleado", 26),
        ["bilbao"] = ("Nublado", 18),
    };

    private static readonly List<CalendarEvent> _calendar = new()
    {
        new("Reunión de equipo", DateTime.Today.AddHours(10), 60),
        new("Almuerzo", DateTime.Today.AddHours(13), 45),
        new("Revisión de proyecto", DateTime.Today.AddHours(15), 90),
        new("Stand-up diario", DateTime.Today.AddDays(1).AddHours(9), 15),
    };

    // ===== Function Tools de Clima =====

    /// <summary>
    /// Obtiene el clima actual para una ciudad española.
    /// DevUI mostrará esta invocación con los parámetros recibidos.
    /// </summary>
    [KernelFunction("get_weather")]
    [Description("Obtiene el clima actual para una ciudad española. Úsala cuando pregunten por el tiempo o temperatura.")]
    public string GetWeather(
        [Description("Nombre de la ciudad española (Madrid, Barcelona, Sevilla, Valencia, Bilbao)")] string city)
    {
        var cityKey = city.ToLowerInvariant().Trim();
        
        if (_weather.TryGetValue(cityKey, out var data))
        {
            return $"""
                🌤️ Clima en {city}:
                • Condición: {data.Condition}
                • Temperatura: {data.Temp}°C
                """;
        }
        
        return $"⚠️ No tengo datos del clima para '{city}'. Ciudades disponibles: Madrid, Barcelona, Sevilla, Valencia, Bilbao.";
    }

    // ===== Function Tools de Calendario =====

    /// <summary>
    /// Lista los eventos del calendario para hoy o mañana.
    /// DevUI mostrará los parámetros y resultado de esta función.
    /// </summary>
    [KernelFunction("get_calendar_events")]
    [Description("Obtiene los eventos del calendario. Úsala cuando pregunten por reuniones, citas o agenda.")]
    public string GetCalendarEvents(
        [Description("Día a consultar: 'hoy' o 'mañana'")] string day = "hoy")
    {
        var targetDate = day.ToLowerInvariant() == "mañana" 
            ? DateTime.Today.AddDays(1) 
            : DateTime.Today;
        
        var events = _calendar
            .Where(e => e.StartTime.Date == targetDate)
            .OrderBy(e => e.StartTime)
            .ToList();
        
        if (!events.Any())
        {
            return $"📅 No tienes eventos programados para {day}.";
        }

        var result = $"📅 Eventos para {day} ({targetDate:dd/MM/yyyy}):\n";
        foreach (var evt in events)
        {
            result += $"  • {evt.StartTime:HH:mm} - {evt.Title} ({evt.DurationMinutes} min)\n";
        }
        
        return result;
    }

    /// <summary>
    /// Crea un nuevo evento en el calendario.
    /// DevUI mostrará todos los parámetros recibidos.
    /// </summary>
    [KernelFunction("create_calendar_event")]
    [Description("Crea un nuevo evento en el calendario. Úsala cuando el usuario quiera agendar algo.")]
    public string CreateCalendarEvent(
        [Description("Título del evento")] string title,
        [Description("Hora en formato HH:mm (24 horas)")] string time,
        [Description("Duración en minutos")] int durationMinutes = 30)
    {
        // Simular creación del evento
        var timeSpan = TimeSpan.Parse(time);
        var eventTime = DateTime.Today.Add(timeSpan);
        
        return $"""
            ✅ Evento creado exitosamente:
            • Título: {title}
            • Fecha: {eventTime:dd/MM/yyyy HH:mm}
            • Duración: {durationMinutes} minutos
            """;
    }

    // ===== Function Tools de Calculadora =====

    /// <summary>
    /// Realiza una operación matemática básica.
    /// DevUI mostrará el flujo de invocación con parámetros numéricos.
    /// </summary>
    [KernelFunction("calculate")]
    [Description("Realiza cálculos matemáticos básicos (sumar, restar, multiplicar, dividir). Úsala para operaciones numéricas.")]
    public string Calculate(
        [Description("Primer número")] double a,
        [Description("Segundo número")] double b,
        [Description("Operación: 'sumar', 'restar', 'multiplicar', 'dividir'")] string operation)
    {
        var result = operation.ToLowerInvariant() switch
        {
            "sumar" or "suma" or "+" => (a + b, $"{a} + {b}"),
            "restar" or "resta" or "-" => (a - b, $"{a} - {b}"),
            "multiplicar" or "multiplicacion" or "*" => (a * b, $"{a} × {b}"),
            "dividir" or "division" or "/" when b != 0 => (a / b, $"{a} ÷ {b}"),
            "dividir" or "division" or "/" => (double.NaN, "Error: división por cero"),
            _ => (double.NaN, $"Operación '{operation}' no reconocida")
        };
        
        if (double.IsNaN(result.Item1))
        {
            return $"❌ {result.Item2}";
        }
        
        return $"🔢 {result.Item2} = {result.Item1:N2}";
    }

    /// <summary>
    /// Convierte una cantidad entre diferentes unidades.
    /// </summary>
    [KernelFunction("convert_units")]
    [Description("Convierte entre unidades (km a millas, celsius a fahrenheit, euros a dólares). Úsala para conversiones.")]
    public string ConvertUnits(
        [Description("Valor a convertir")] double value,
        [Description("Tipo de conversión: 'km_millas', 'millas_km', 'celsius_fahrenheit', 'fahrenheit_celsius', 'euros_dolares', 'dolares_euros'")] string conversionType)
    {
        var (result, formula) = conversionType.ToLowerInvariant() switch
        {
            "km_millas" => (value * 0.621371, $"{value:N2} km = {value * 0.621371:N2} millas"),
            "millas_km" => (value * 1.60934, $"{value:N2} millas = {value * 1.60934:N2} km"),
            "celsius_fahrenheit" => ((value * 9/5) + 32, $"{value:N1}°C = {(value * 9/5) + 32:N1}°F"),
            "fahrenheit_celsius" => ((value - 32) * 5/9, $"{value:N1}°F = {(value - 32) * 5/9:N1}°C"),
            "euros_dolares" => (value * 1.08, $"€{value:N2} = ${value * 1.08:N2} USD"),
            "dolares_euros" => (value * 0.93, $"${value:N2} USD = €{value * 0.93:N2}"),
            _ => (double.NaN, $"Conversión '{conversionType}' no reconocida")
        };
        
        if (double.IsNaN(result))
        {
            return $"❌ {formula}";
        }
        
        return $"🔄 {formula}";
    }
}

/// <summary>
/// Estructura para eventos del calendario
/// </summary>
internal record CalendarEvent(string Title, DateTime StartTime, int DurationMinutes);
