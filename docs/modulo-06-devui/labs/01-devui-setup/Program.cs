// ============================================================================
// Archivo: Program.cs
// Descripción: Aplicación de demostración con DevUI para debugging de agentes
// Módulo: 6 - DevUI
// Lab: 01-devui-setup
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using DevUIExample;

// ===== Configuración del Host con DevUI =====
var builder = Host.CreateApplicationBuilder(args);

// Cargar configuración desde appsettings.json y user secrets
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

var configuration = builder.Configuration;

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado. Usa: dotnet user-secrets set 'AzureOpenAI:ApiKey' 'tu-key'");

// ===== Configurar DevUI =====
// DevUI se integra con el sistema de hosting de .NET para exponer un endpoint
// donde la herramienta DevUI puede conectarse y visualizar la actividad del agente
var devUIEnabled = configuration.GetValue<bool>("DevUI:Enabled", true);
var devUIPort = configuration.GetValue<int>("DevUI:Port", 5100);

if (devUIEnabled)
{
    // Agregar servicios de DevUI al contenedor de DI
    builder.Services.AddDevUI(options =>
    {
        options.Port = devUIPort;
        options.EnableDetailedLogging = true;
    });
    
    Console.WriteLine($"🔧 DevUI habilitado en puerto {devUIPort}");
}

// ===== Crear Kernel con Function Tools =====
var kernelBuilder = Kernel.CreateBuilder();

// Agregar servicio de Azure OpenAI
kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

// Registrar todas las funciones de demostración como plugin
// DevUI mostrará cada una de estas funciones en su panel de inspección
kernelBuilder.Plugins.AddFromType<DemoFunctions>();

var kernel = kernelBuilder.Build();

// Registrar el kernel en el contenedor de DI para que DevUI lo pueda instrumentar
builder.Services.AddSingleton(kernel);

// ===== Crear Agente con DevUI Instrumentation =====
var agent = new ChatCompletionAgent()
{
    Name = "AgenteMultiFuncion",
    Instructions = """
        Eres un asistente personal muy capaz llamado AgenteMultiFuncion.
        Puedes ayudar con:
        
        🌤️ **Clima**: Consultar el tiempo en ciudades españolas
        📅 **Calendario**: Ver y crear eventos en la agenda
        🔢 **Cálculos**: Realizar operaciones matemáticas
        🔄 **Conversiones**: Convertir unidades (distancia, temperatura, moneda)
        
        Siempre responde en español de forma amigable.
        Cuando uses una función, explica brevemente qué estás haciendo.
        Si el usuario no especifica detalles necesarios, pregunta amablemente.
        """,
    Kernel = kernel,
    // Habilitar function calling automático para que DevUI pueda mostrar las invocaciones
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

// Registrar el agente para instrumentación de DevUI
builder.Services.AddSingleton(agent);

// ===== Iniciar Host (inicia DevUI en background) =====
var host = builder.Build();

// Iniciar el host en background para que DevUI esté disponible
_ = host.StartAsync();

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🔧  DevUI Demo - Agente Multi-Función");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine();
Console.WriteLine("📋 Funciones disponibles:");
Console.WriteLine("   • get_weather - Clima de ciudades");
Console.WriteLine("   • get_calendar_events - Ver agenda");
Console.WriteLine("   • create_calendar_event - Crear evento");
Console.WriteLine("   • calculate - Cálculos matemáticos");
Console.WriteLine("   • convert_units - Conversiones");
Console.WriteLine();

if (devUIEnabled)
{
    Console.WriteLine("============================================");
    Console.WriteLine("🔧 DevUI está activo");
    Console.WriteLine($"   1. Abre otra terminal y ejecuta: devui start");
    Console.WriteLine($"   2. O abre en navegador: http://localhost:{devUIPort}");
    Console.WriteLine("============================================");
}

Console.WriteLine();
Console.WriteLine("💡 Prueba preguntar:");
Console.WriteLine("   - ¿Cómo está el clima en Madrid?");
Console.WriteLine("   - ¿Qué reuniones tengo hoy?");
Console.WriteLine("   - Crea una cita para almorzar a las 12:30");
Console.WriteLine("   - ¿Cuánto es 150 euros en dólares?");
Console.WriteLine("   - ¿Cuánto es 25 por 4?");
Console.WriteLine();
Console.WriteLine("Escribe 'salir' para terminar");
Console.WriteLine("============================================\n");

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🔧 AgenteMultiFuncion: ¡Hasta pronto! Fue un placer ayudarte. 👋\n");
        break;
    }
    
    // Agregar mensaje del usuario al historial
    chatHistory.AddUserMessage(userInput);
    
    Console.Write("🔧 AgenteMultiFuncion: ");
    
    try
    {
        // Invocar el agente - DevUI capturará esta invocación
        // y mostrará el flujo de conversación y las llamadas a funciones
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
    
    // Gestión de historial - mantener últimos 20 mensajes
    if (chatHistory.Count > 20)
    {
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 20).ToList();
        chatHistory.Clear();
        foreach (var msg in messagesToKeep)
        {
            chatHistory.Add(msg);
        }
    }
}

// Detener el host al salir
await host.StopAsync();
