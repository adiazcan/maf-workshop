// ============================================================================
// Archivo: Program.cs
// Descripción: Agente con Function Tools usando Microsoft Agent Framework
// Módulo: 2 - Function Tools
// Lab: 01-custom-tool
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Agents.AI.Chat;
using Microsoft.Extensions.Configuration;
using WeatherAgent;

// ===== Configuración =====
// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado. Usa: dotnet user-secrets set 'AzureOpenAI:ApiKey' 'tu-key'");

// ===== Crear Function Tools =====
// Usamos AIFunctionFactory.Create para definir funciones invocables por el agente

// Función para obtener el clima actual
var getWeatherFunction = AIFunctionFactory.Create(
    (string city, string country) => WeatherService.GetWeather(city, country ?? "ES"),
    name: "get_weather",
    description: "Obtiene el clima actual para una ubicación específica. Úsala cuando el usuario pregunte sobre el clima, temperatura o condiciones meteorológicas de una ciudad."
);

// Función para obtener el pronóstico
var getForecastFunction = AIFunctionFactory.Create(
    (string city, int days) => WeatherService.GetForecast(city, days > 0 ? days : 3),
    name: "get_forecast",
    description: "Obtiene el pronóstico del clima para los próximos días. Úsala cuando el usuario pregunte sobre el clima futuro o pronóstico de una ciudad."
);

// ===== Crear Agente con Function Tools =====
var agent = new ChatCompletionAgent(
    name: "AgenteDelClima",
    instructions: """
        Eres un asistente experto en clima llamado AgenteDelClima.
        Puedes proporcionar información del clima actual y pronósticos.
        
        Cuando el usuario pregunte sobre el clima de una ciudad:
        1. Usa la función get_weather para obtener el clima actual
        2. Usa la función get_forecast si preguntan por el pronóstico
        
        Siempre responde en español de forma amigable y útil.
        Si el usuario no especifica una ciudad, pregunta amablemente cuál ciudad le interesa.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey,
    tools: new AIFunction[] { getWeatherFunction, getForecastFunction }
);

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🌤️  Agente del Clima con Function Tools (MAF)");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Funciones disponibles: get_weather, get_forecast");
Console.WriteLine("Escribe 'salir' para terminar");
Console.WriteLine("============================================\n");

Console.WriteLine("💡 Prueba preguntar:");
Console.WriteLine("   - ¿Cómo está el clima en Madrid?");
Console.WriteLine("   - ¿Cuál es el pronóstico para Barcelona?");
Console.WriteLine("   - ¿Qué temperatura hace en México?\n");

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🌤️ AgenteDelClima: ¡Hasta pronto! Que tengas un buen día. ☀️\n");
        break;
    }
    
    // Agregar mensaje del usuario al historial
    chatHistory.AddUserMessage(userInput);
    
    Console.Write("🌤️ AgenteDelClima: ");
    
    try
    {
        // Invocar el agente - automáticamente decidirá si llamar funciones
        await foreach (var message in agent.InvokeAsync(chatHistory))
        {
            Console.Write(message.Content);
        }
        Console.WriteLine("\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}");
        Console.WriteLine("Verifica tu endpoint y API key.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
    
    // Gestión de historial - mantener últimos 10 mensajes
    if (chatHistory.Count > 10)
    {
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 10).ToList();
        chatHistory.Clear();
        foreach (var msg in messagesToKeep)
        {
            chatHistory.Add(msg);
        }
    }
}
