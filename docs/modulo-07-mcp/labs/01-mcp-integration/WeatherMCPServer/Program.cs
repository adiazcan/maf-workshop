// ============================================================================
// Archivo: WeatherMCPServer/Program.cs
// Descripción: Servidor MCP para servicios de clima
// Módulo: 7 - Model Context Protocol
// ============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

// Configurar el servidor MCP con transporte stdio
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WeatherTools>();

// Configurar logging para stderr (requerido por MCP)
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

await builder.Build().RunAsync();

// ===== Definición de herramientas =====

[McpServerToolType]
file class WeatherTools
{
    private static readonly Dictionary<string, WeatherData> CityWeather = new()
    {
        ["madrid"] = new("Madrid", "Soleado", 22, 45),
        ["barcelona"] = new("Barcelona", "Parcialmente nublado", 24, 55),
        ["sevilla"] = new("Sevilla", "Muy caluroso", 35, 30),
        ["bilbao"] = new("Bilbao", "Lluvioso", 15, 80),
        ["valencia"] = new("Valencia", "Templado", 20, 50),
        ["paris"] = new("París", "Nublado", 14, 70),
        ["london"] = new("Londres", "Lluvioso", 12, 85),
        ["new york"] = new("Nueva York", "Soleado", 18, 40)
    };

    [McpServerTool(Name = "get_weather")]
    [Description("Obtiene el clima actual para una ciudad específica")]
    public static string GetWeather(
        [Description("Nombre de la ciudad (ej: Madrid, Barcelona, Paris)")] string city)
    {
        city = city.ToLower();

        if (CityWeather.TryGetValue(city, out var weather))
        {
            return $"🌤️ Clima en {weather.City}:\n" +
                   $"   Condición: {weather.Condition}\n" +
                   $"   Temperatura: {weather.Temperature}°C\n" +
                   $"   Humedad: {weather.Humidity}%";
        }

        return $"Ciudad no encontrada: {city}. Ciudades disponibles: {string.Join(", ", CityWeather.Keys)}";
    }

    [McpServerTool(Name = "get_forecast")]
    [Description("Obtiene el pronóstico del clima para los próximos días")]
    public static string GetForecast(
        [Description("Nombre de la ciudad")] string city,
        [Description("Número de días (1-7)")] int days = 3)
    {
        city = city.ToLower();
        days = Math.Clamp(days, 1, 7);

        if (!CityWeather.TryGetValue(city, out var weather))
        {
            return $"Ciudad no encontrada: {city}";
        }

        var forecast = new System.Text.StringBuilder();
        forecast.AppendLine($"📅 Pronóstico para {weather.City} ({days} días):\n");

        var random = new Random(city.GetHashCode());
        var baseTemp = weather.Temperature;
        var conditions = new[] { "Soleado", "Parcialmente nublado", "Nublado", "Lluvioso" };

        for (int i = 1; i <= days; i++)
        {
            var date = DateTime.Now.AddDays(i);
            var temp = baseTemp + random.Next(-3, 4);
            var condition = conditions[random.Next(conditions.Length)];

            forecast.AppendLine($"   {date:dddd, dd MMM}: {temp}°C - {condition}");
        }

        return forecast.ToString();
    }

    [McpServerTool(Name = "convert_temperature")]
    [Description("Convierte temperatura entre Celsius y Fahrenheit")]
    public static string ConvertTemperature(
        [Description("Valor de temperatura a convertir")] double value,
        [Description("Unidad de origen: C para Celsius, F para Fahrenheit")] string from_unit = "C")
    {
        var fromUnit = from_unit.ToUpperInvariant();

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
}

file record WeatherData(string City, string Condition, int Temperature, int Humidity);

