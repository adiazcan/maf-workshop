// ============================================================================
// Archivo: WeatherService.cs
// Descripción: Servicio de clima con Function Tool para agentes MAF
// Módulo: 2 - Function Tools
// Lab: 01-custom-tool
// ============================================================================

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace WeatherAgent;

/// <summary>
/// Servicio que proporciona información del clima.
/// Los métodos decorados con [KernelFunction] pueden ser invocados automáticamente
/// por el agente cuando detecta que el usuario necesita esta información.
/// </summary>
public class WeatherService
{
    // Diccionario con datos de clima simulados para diferentes ciudades
    private static readonly Dictionary<string, WeatherData> _weatherDatabase = new()
    {
        ["madrid"] = new("Madrid", "ES", "Soleado", 22, 45),
        ["barcelona"] = new("Barcelona", "ES", "Parcialmente nublado", 24, 65),
        ["valencia"] = new("Valencia", "ES", "Soleado", 26, 55),
        ["sevilla"] = new("Sevilla", "ES", "Muy soleado", 30, 35),
        ["bilbao"] = new("Bilbao", "ES", "Nublado", 18, 75),
        ["mexico city"] = new("Ciudad de México", "MX", "Parcialmente nublado", 20, 50),
        ["buenos aires"] = new("Buenos Aires", "AR", "Templado", 18, 60),
        ["bogota"] = new("Bogotá", "CO", "Lluvioso", 15, 80),
        ["lima"] = new("Lima", "PE", "Nublado", 19, 85),
        ["santiago"] = new("Santiago", "CL", "Soleado", 25, 40),
    };

    /// <summary>
    /// Obtiene el clima actual para una ciudad específica.
    /// El agente llamará automáticamente este método cuando el usuario
    /// pregunte sobre el clima de una ubicación.
    /// </summary>
    /// <param name="city">Nombre de la ciudad (ej: Madrid, Barcelona, México City)</param>
    /// <param name="country">Código de país ISO 3166-1 alpha-2 (ej: ES, MX, AR). Por defecto: ES</param>
    /// <returns>Descripción del clima actual en español</returns>
    [KernelFunction("get_weather")]
    [Description("Obtiene el clima actual para una ubicación específica. Úsala cuando el usuario pregunte sobre el clima, temperatura o condiciones meteorológicas de una ciudad.")]
    public string GetWeather(
        [Description("El nombre de la ciudad para consultar el clima")] string city,
        [Description("Código de país ISO (ej: ES para España, MX para México)")] string country = "ES")
    {
        // Normalizar el nombre de la ciudad para búsqueda
        var cityKey = city.ToLowerInvariant().Trim();
        
        // Buscar en la base de datos simulada
        if (_weatherDatabase.TryGetValue(cityKey, out var weather))
        {
            return $"""
                📍 Clima en {weather.City}, {weather.Country}:
                🌡️ Temperatura: {weather.Temperature}°C
                ☁️ Condición: {weather.Condition}
                💧 Humedad: {weather.Humidity}%
                """;
        }
        
        // Ciudad no encontrada - retornar respuesta informativa
        return $"""
            ⚠️ No tengo datos del clima para '{city}' ({country}).
            Ciudades disponibles: Madrid, Barcelona, Valencia, Sevilla, Bilbao, 
            Ciudad de México, Buenos Aires, Bogotá, Lima, Santiago.
            """;
    }

    /// <summary>
    /// Obtiene el pronóstico del clima para los próximos días.
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="days">Número de días para el pronóstico (1-7)</param>
    /// <returns>Pronóstico del clima en español</returns>
    [KernelFunction("get_forecast")]
    [Description("Obtiene el pronóstico del clima para los próximos días. Úsala cuando el usuario pregunte sobre el clima futuro o pronóstico.")]
    public string GetForecast(
        [Description("El nombre de la ciudad")] string city,
        [Description("Número de días para el pronóstico (1-7)")] int days = 3)
    {
        // Validar rango de días
        days = Math.Clamp(days, 1, 7);
        
        var cityKey = city.ToLowerInvariant().Trim();
        
        if (!_weatherDatabase.TryGetValue(cityKey, out var currentWeather))
        {
            return $"⚠️ No tengo datos de pronóstico para '{city}'.";
        }

        // Generar pronóstico simulado
        var forecast = new System.Text.StringBuilder();
        forecast.AppendLine($"📅 Pronóstico para {currentWeather.City} ({days} días):");
        forecast.AppendLine();
        
        var random = new Random(city.GetHashCode()); // Seed para consistencia
        var conditions = new[] { "Soleado", "Parcialmente nublado", "Nublado", "Lluvioso" };
        
        for (int i = 1; i <= days; i++)
        {
            var date = DateTime.Now.AddDays(i).ToString("dddd dd/MM", new System.Globalization.CultureInfo("es-ES"));
            var temp = currentWeather.Temperature + random.Next(-3, 4);
            var condition = conditions[random.Next(conditions.Length)];
            
            forecast.AppendLine($"  📆 {date}: {temp}°C, {condition}");
        }
        
        return forecast.ToString();
    }
}

/// <summary>
/// Estructura para almacenar datos del clima
/// </summary>
internal record WeatherData(
    string City,
    string Country,
    string Condition,
    int Temperature,
    int Humidity
);
