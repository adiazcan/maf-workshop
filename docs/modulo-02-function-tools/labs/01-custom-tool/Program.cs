// ============================================================================
// Archivo: Program.cs
// Descripción: Agente con Function Tools usando Microsoft Agent Framework
// Módulo: 2 - Function Tools
// Lab: 01-custom-tool
// ============================================================================

using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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

// ===== Crear Function Tools =====
// Usamos AIFunctionFactory.Create para definir funciones invocables por el agente

// Función para obtener el clima actual
[Description("Obtiene el clima actual para una ubicación específica. Úsala cuando el usuario pregunte sobre el clima, temperatura o condiciones meteorológicas de una ciudad.")]
static string GetWeather(
    [Description("El nombre de la ciudad (ej: Madrid, Barcelona, México City)")] string city,
    [Description("Código de país ISO (ej: ES, MX, AR). Por defecto: ES")] string country = "ES")
{
    return WeatherService.GetWeather(city, country);
}

// Función para obtener el pronóstico
[Description("Obtiene el pronóstico del clima para los próximos días. Úsala cuando el usuario pregunte sobre el clima futuro o pronóstico de una ciudad.")]
static string GetForecast(
    [Description("El nombre de la ciudad")] string city,
    [Description("Número de días para el pronóstico (1-7). Por defecto: 3")] int days = 3)
{
    return WeatherService.GetForecast(city, days > 0 ? days : 3);
}

// Crear las herramientas del agente usando AIFunctionFactory
AITool[] tools = [
    AIFunctionFactory.Create(GetWeather),
    AIFunctionFactory.Create(GetForecast)
];

// ===== Crear Agente con Function Tools =====
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .CreateAIAgent(
        name: "AgenteDelClima",
        instructions: """
            Eres un asistente experto en clima llamado AgenteDelClima.
            Puedes proporcionar información del clima actual y pronósticos.
            
            Cuando el usuario pregunte sobre el clima de una ciudad:
            1. Usa la función GetWeather para obtener el clima actual
            2. Usa la función GetForecast si preguntan por el pronóstico
            
            Siempre responde en español de forma amigable y útil.
            Si el usuario no especifica una ciudad, pregunta amablemente cuál ciudad le interesa.
            """,
        tools: tools
    );

// ===== Crear Thread para la conversación =====
// El thread mantiene el historial de conversación
var thread = agent.GetNewThread();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🌤️  Agente del Clima con Function Tools (MAF)");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Funciones disponibles: GetWeather, GetForecast");
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
    
    Console.Write("🌤️ AgenteDelClima: ");
    
    try
    {
        // Invocar el agente - automáticamente decidirá si llamar funciones
        await foreach (var update in agent.RunStreamingAsync(userInput, thread))
        {
            Console.Write(update);
        }
        Console.WriteLine("\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}");
        Console.WriteLine("Verifica tu endpoint y credenciales.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
}
