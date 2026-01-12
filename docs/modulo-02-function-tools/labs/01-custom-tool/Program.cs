// ============================================================================
// Archivo: Program.cs
// Descripción: Agente con Function Tool personalizada para consultar el clima
// Módulo: 2 - Function Tools
// Lab: 01-custom-tool
// ============================================================================

using Microsoft.AI.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
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

// ===== Crear Kernel con Function Tools =====
var builder = Kernel.CreateBuilder();

// Agregar servicio de Azure OpenAI
builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

// ===== CLAVE: Registrar el servicio como plugin =====
// Esto hace que las funciones decoradas con [KernelFunction] estén disponibles
// para que el agente las invoque automáticamente
builder.Plugins.AddFromType<WeatherService>();

var kernel = builder.Build();

// ===== Crear Agente con Function Calling Habilitado =====
var agent = new ChatCompletionAgent()
{
    Name = "AgenteDelClima",
    Instructions = """
        Eres un asistente experto en clima llamado AgenteDelClima.
        Puedes proporcionar información del clima actual y pronósticos.
        
        Cuando el usuario pregunte sobre el clima de una ciudad:
        1. Usa la función get_weather para obtener el clima actual
        2. Usa la función get_forecast si preguntan por el pronóstico
        
        Siempre responde en español de forma amigable y útil.
        Si el usuario no especifica una ciudad, pregunta amablemente cuál ciudad le interesa.
        """,
    Kernel = kernel,
    // Configurar para que el agente pueda llamar funciones automáticamente
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🌤️  Agente del Clima con Function Tools");
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
        await foreach (var message in agent.InvokeStreamingAsync(chatHistory))
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
    
    // Gestión de historial
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
