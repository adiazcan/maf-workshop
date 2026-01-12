// ============================================================================
// Archivo: MCPWeatherServer.cs
// Descripción: Implementación de servidor MCP simple para demostración
// Módulo: 7
// Lab: 01-mcp-integration
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCPIntegration;

/// <summary>
/// Servidor MCP simple que expone herramientas de clima.
/// Este es un servidor de demostración que simula un MCP Server real.
/// </summary>
public class MCPWeatherServer
{
    // Datos de clima simulados para demostración
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

    // Noticias simuladas para demostración
    private static readonly List<NewsItem> Headlines = new()
    {
        new("Avances en IA generativa transforman industria tecnológica", "Tecnología", DateTime.Now.AddHours(-2)),
        new("Nuevo tratado climático firmado por 50 países", "Internacional", DateTime.Now.AddHours(-5)),
        new("Innovación en energías renovables reduce costos 30%", "Economía", DateTime.Now.AddHours(-8)),
        new("Startup española lidera desarrollo de agentes IA", "Negocios", DateTime.Now.AddDays(-1))
    };

    /// <summary>
    /// Obtiene la lista de herramientas disponibles en este servidor MCP.
    /// Implementa el endpoint MCP tools/list.
    /// </summary>
    public MCPToolsListResponse ListTools()
    {
        return new MCPToolsListResponse
        {
            Tools = new List<MCPTool>
            {
                new()
                {
                    Name = "get_weather",
                    Description = "Obtiene el clima actual para una ciudad específica",
                    InputSchema = new MCPInputSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, MCPPropertySchema>
                        {
                            ["city"] = new()
                            {
                                Type = "string",
                                Description = "Nombre de la ciudad (ej: Madrid, Barcelona, Paris)"
                            }
                        },
                        Required = new[] { "city" }
                    }
                },
                new()
                {
                    Name = "get_forecast",
                    Description = "Obtiene el pronóstico del clima para los próximos días",
                    InputSchema = new MCPInputSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, MCPPropertySchema>
                        {
                            ["city"] = new()
                            {
                                Type = "string",
                                Description = "Nombre de la ciudad"
                            },
                            ["days"] = new()
                            {
                                Type = "integer",
                                Description = "Número de días (1-7)"
                            }
                        },
                        Required = new[] { "city" }
                    }
                },
                new()
                {
                    Name = "get_headlines",
                    Description = "Obtiene los titulares de noticias recientes",
                    InputSchema = new MCPInputSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, MCPPropertySchema>
                        {
                            ["category"] = new()
                            {
                                Type = "string",
                                Description = "Categoría de noticias: Tecnología, Internacional, Economía, Negocios (opcional)"
                            },
                            ["count"] = new()
                            {
                                Type = "integer",
                                Description = "Número de titulares (1-10)"
                            }
                        },
                        Required = Array.Empty<string>()
                    }
                }
            }
        };
    }

    /// <summary>
    /// Obtiene la lista de recursos disponibles en este servidor MCP.
    /// Implementa el endpoint MCP resources/list.
    /// </summary>
    public MCPResourcesListResponse ListResources()
    {
        return new MCPResourcesListResponse
        {
            Resources = CityWeather.Keys.Select(city => new MCPResource
            {
                Uri = $"weather://{city}",
                Name = $"Clima en {CityWeather[city].City}",
                Description = $"Datos meteorológicos actuales para {CityWeather[city].City}",
                MimeType = "application/json"
            }).ToList()
        };
    }

    /// <summary>
    /// Ejecuta una herramienta MCP con los argumentos proporcionados.
    /// Implementa el endpoint MCP tools/call.
    /// </summary>
    public MCPToolCallResponse CallTool(string toolName, JsonElement arguments)
    {
        return toolName switch
        {
            "get_weather" => ExecuteGetWeather(arguments),
            "get_forecast" => ExecuteGetForecast(arguments),
            "get_headlines" => ExecuteGetHeadlines(arguments),
            _ => new MCPToolCallResponse
            {
                IsError = true,
                Content = new[] { new MCPContent { Type = "text", Text = $"Herramienta desconocida: {toolName}" } }
            }
        };
    }

    /// <summary>
    /// Lee un recurso MCP por su URI.
    /// Implementa el endpoint MCP resources/read.
    /// </summary>
    public MCPResourceReadResponse ReadResource(string uri)
    {
        // Parsear URI formato: weather://ciudad
        if (uri.StartsWith("weather://"))
        {
            var city = uri.Replace("weather://", "").ToLower();
            if (CityWeather.TryGetValue(city, out var weather))
            {
                return new MCPResourceReadResponse
                {
                    Contents = new[]
                    {
                        new MCPResourceContent
                        {
                            Uri = uri,
                            MimeType = "application/json",
                            Text = JsonSerializer.Serialize(weather, new JsonSerializerOptions { WriteIndented = true })
                        }
                    }
                };
            }
        }

        return new MCPResourceReadResponse
        {
            Contents = new[]
            {
                new MCPResourceContent
                {
                    Uri = uri,
                    MimeType = "text/plain",
                    Text = $"Recurso no encontrado: {uri}"
                }
            }
        };
    }

    // ===== Implementación de Herramientas =====

    private MCPToolCallResponse ExecuteGetWeather(JsonElement arguments)
    {
        var city = arguments.TryGetProperty("city", out var cityProp) 
            ? cityProp.GetString()?.ToLower() ?? "" 
            : "";

        if (CityWeather.TryGetValue(city, out var weather))
        {
            var result = $"🌤️ Clima en {weather.City}:\n" +
                        $"   Condición: {weather.Condition}\n" +
                        $"   Temperatura: {weather.Temperature}°C\n" +
                        $"   Humedad: {weather.Humidity}%";

            return new MCPToolCallResponse
            {
                Content = new[] { new MCPContent { Type = "text", Text = result } }
            };
        }

        return new MCPToolCallResponse
        {
            Content = new[] { new MCPContent { Type = "text", Text = $"Ciudad no encontrada: {city}. Ciudades disponibles: {string.Join(", ", CityWeather.Keys)}" } }
        };
    }

    private MCPToolCallResponse ExecuteGetForecast(JsonElement arguments)
    {
        var city = arguments.TryGetProperty("city", out var cityProp) 
            ? cityProp.GetString()?.ToLower() ?? "" 
            : "";
        var days = arguments.TryGetProperty("days", out var daysProp) 
            ? daysProp.GetInt32() 
            : 3;

        days = Math.Clamp(days, 1, 7);

        if (CityWeather.TryGetValue(city, out var weather))
        {
            var forecast = new System.Text.StringBuilder();
            forecast.AppendLine($"📅 Pronóstico para {weather.City} ({days} días):\n");

            var random = new Random(city.GetHashCode());
            var baseTemp = weather.Temperature;

            for (int i = 1; i <= days; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var temp = baseTemp + random.Next(-3, 4);
                var conditions = new[] { "Soleado", "Parcialmente nublado", "Nublado", "Lluvioso" };
                var condition = conditions[random.Next(conditions.Length)];

                forecast.AppendLine($"   {date:dddd, dd MMM}: {temp}°C - {condition}");
            }

            return new MCPToolCallResponse
            {
                Content = new[] { new MCPContent { Type = "text", Text = forecast.ToString() } }
            };
        }

        return new MCPToolCallResponse
        {
            Content = new[] { new MCPContent { Type = "text", Text = $"Ciudad no encontrada: {city}" } }
        };
    }

    private MCPToolCallResponse ExecuteGetHeadlines(JsonElement arguments)
    {
        var category = arguments.TryGetProperty("category", out var catProp) 
            ? catProp.GetString() 
            : null;
        var count = arguments.TryGetProperty("count", out var countProp) 
            ? countProp.GetInt32() 
            : 4;

        count = Math.Clamp(count, 1, 10);

        var headlines = string.IsNullOrEmpty(category)
            ? Headlines.Take(count)
            : Headlines.Where(h => h.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).Take(count);

        var result = new System.Text.StringBuilder();
        result.AppendLine("📰 Titulares de Noticias:\n");

        foreach (var headline in headlines)
        {
            result.AppendLine($"   [{headline.Category}] {headline.Title}");
            result.AppendLine($"      Publicado: {headline.PublishedAt:g}\n");
        }

        return new MCPToolCallResponse
        {
            Content = new[] { new MCPContent { Type = "text", Text = result.ToString() } }
        };
    }
}

// ===== Modelos de Datos =====

public record WeatherData(string City, string Condition, int Temperature, int Humidity);
public record NewsItem(string Title, string Category, DateTime PublishedAt);

// ===== Modelos MCP =====

public class MCPToolsListResponse
{
    [JsonPropertyName("tools")]
    public List<MCPTool> Tools { get; set; } = new();
}

public class MCPTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("inputSchema")]
    public MCPInputSchema InputSchema { get; set; } = new();
}

public class MCPInputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, MCPPropertySchema> Properties { get; set; } = new();

    [JsonPropertyName("required")]
    public string[] Required { get; set; } = Array.Empty<string>();
}

public class MCPPropertySchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
}

public class MCPResourcesListResponse
{
    [JsonPropertyName("resources")]
    public List<MCPResource> Resources { get; set; } = new();
}

public class MCPResource
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "";
}

public class MCPToolCallResponse
{
    [JsonPropertyName("content")]
    public MCPContent[] Content { get; set; } = Array.Empty<MCPContent>();

    [JsonPropertyName("isError")]
    public bool IsError { get; set; }
}

public class MCPContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}

public class MCPResourceReadResponse
{
    [JsonPropertyName("contents")]
    public MCPResourceContent[] Contents { get; set; } = Array.Empty<MCPResourceContent>();
}

public class MCPResourceContent
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = "";

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}
