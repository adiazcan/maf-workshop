// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de integración MCP con Microsoft Agent Framework
// Módulo: 7
// Lab: 01-mcp-integration
// ============================================================================

using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using MCPIntegration;

// ===== Configuración =====
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║   🔌 Lab: Integración MCP con Microsoft Agent Framework    ║");
Console.WriteLine("║   Módulo 7 - Model Context Protocol                        ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .Build();

// Obtener configuración de Azure OpenAI
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:Endpoint");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:DeploymentName");
var apiKey = configuration["AzureOpenAI:ApiKey"];

Console.WriteLine("📋 Configuración:");
Console.WriteLine($"   Endpoint: {endpoint}");
Console.WriteLine($"   Deployment: {deploymentName}\n");

// ===== Crear servidor MCP local para demostración =====
Console.WriteLine("🔌 Iniciando servidor MCP local de demostración...");
var mcpServer = new MCPWeatherServer();

// Descubrir herramientas disponibles en el servidor MCP
var mcpTools = mcpServer.ListTools();
Console.WriteLine($"   ✅ Servidor MCP listo con {mcpTools.Tools.Count} herramientas:");
foreach (var tool in mcpTools.Tools)
{
    Console.WriteLine($"      • {tool.Name}: {tool.Description}");
}

// Descubrir recursos disponibles
var mcpResources = mcpServer.ListResources();
Console.WriteLine($"\n   📦 {mcpResources.Resources.Count} recursos disponibles:");
foreach (var resource in mcpResources.Resources.Take(3))
{
    Console.WriteLine($"      • {resource.Uri}: {resource.Name}");
}
if (mcpResources.Resources.Count > 3)
{
    Console.WriteLine($"      • ... y {mcpResources.Resources.Count - 3} más");
}

// ===== Construir el Kernel con Azure OpenAI =====
Console.WriteLine("\n🔧 Configurando Semantic Kernel con Azure OpenAI...");

var kernelBuilder = Kernel.CreateBuilder();

// Agregar servicio de chat de Azure OpenAI
if (!string.IsNullOrEmpty(apiKey))
{
    // Usar API Key si está disponible
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        apiKey: apiKey);
}
else
{
    // Usar DefaultAzureCredential para autenticación sin clave
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        credentials: new DefaultAzureCredential());
}

var kernel = kernelBuilder.Build();
Console.WriteLine("   ✅ Kernel configurado con Azure OpenAI");

// ===== Registrar herramientas nativas =====
Console.WriteLine("\n🔧 Registrando herramientas nativas (function tools locales)...");
var nativeFunctions = new NativeFunctions();
kernel.Plugins.AddFromObject(nativeFunctions, "native");
Console.WriteLine("   ✅ Herramientas nativas registradas: convert_temperature, get_datetime, calculate, get_capabilities");

// ===== Registrar herramientas MCP como plugins =====
Console.WriteLine("\n🔌 Registrando herramientas MCP en el kernel...");

// Crear funciones wrapper para las herramientas MCP
// En producción, MAF tiene MCPClient que hace esto automáticamente
var mcpPlugin = new MCPToolsPlugin(mcpServer);
kernel.Plugins.AddFromObject(mcpPlugin, "mcp");
Console.WriteLine("   ✅ Herramientas MCP registradas: get_weather, get_forecast, get_headlines");

// ===== Configurar el agente con todas las herramientas =====
Console.WriteLine("\n🤖 Creando agente con herramientas híbridas (MCP + nativas)...");

var chatService = kernel.GetRequiredService<IChatCompletionService>();

// Instrucciones del agente en español
var systemPrompt = """
    Eres un asistente útil que puede proporcionar información sobre:
    
    📡 HERRAMIENTAS MCP (fuente externa):
    - Clima actual en diferentes ciudades (get_weather)
    - Pronóstico del tiempo (get_forecast)  
    - Titulares de noticias (get_headlines)
    
    🔧 HERRAMIENTAS NATIVAS (locales):
    - Conversión de temperatura Celsius/Fahrenheit (convert_temperature)
    - Fecha y hora actual (get_datetime)
    - Cálculos matemáticos (calculate)
    - Lista de capacidades (get_capabilities)
    
    Siempre responde en español. Cuando necesites información sobre clima o noticias,
    usa las herramientas MCP. Para conversiones y cálculos, usa las herramientas nativas.
    
    Explica brevemente qué tipo de herramienta usaste (MCP o nativa) cuando corresponda.
    """;

var chatHistory = new ChatHistory(systemPrompt);

Console.WriteLine("   ✅ Agente listo con herramientas híbridas\n");

// ===== Demostración interactiva =====
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  💬 DEMOSTRACIÓN: Agente con herramientas MCP + Nativas");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Ejemplos de preguntas:");
Console.WriteLine("  • \"¿Qué clima hace en Madrid?\" (usa herramienta MCP)");
Console.WriteLine("  • \"Convierte 25 grados Celsius a Fahrenheit\" (usa nativa)");
Console.WriteLine("  • \"¿Qué hora es?\" (usa nativa)");
Console.WriteLine("  • \"Dame el pronóstico de Barcelona para 5 días\" (usa MCP)");
Console.WriteLine("  • \"¿Cuáles son las noticias de tecnología?\" (usa MCP)");
Console.WriteLine("  • \"¿Qué capacidades tienes?\" (usa nativa)");
Console.WriteLine("  • \"Escribe 'salir' para terminar\"\n");

// Configuración para auto-ejecución de funciones
var executionSettings = new AzureOpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Bucle de conversación
while (true)
{
    Console.Write("👤 Usuario: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput))
        continue;
        
    if (userInput.Equals("salir", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n👋 ¡Hasta luego! Gracias por probar la integración MCP.\n");
        break;
    }

    // Agregar mensaje del usuario al historial
    chatHistory.AddUserMessage(userInput);

    try
    {
        Console.Write("\n🤖 Agente: ");
        
        // Obtener respuesta del agente con ejecución automática de funciones
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel);

        Console.WriteLine(response.Content);
        
        // Agregar respuesta al historial
        chatHistory.AddAssistantMessage(response.Content ?? "");
        
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n⚠️ Error: {ex.Message}");
        Console.WriteLine("   Intenta con otra pregunta.\n");
    }
}

// ===== Demostración de lectura de recursos MCP =====
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  📦 BONUS: Lectura directa de recursos MCP");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine("Leyendo recurso 'weather://madrid' directamente del servidor MCP:");
var resourceData = mcpServer.ReadResource("weather://madrid");
Console.WriteLine(resourceData.Contents[0].Text);

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("  ✅ Lab completado: Integración MCP con MAF");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine("📝 Resumen de lo aprendido:");
Console.WriteLine("   1. Los servidores MCP exponen herramientas (tools) y recursos (resources)");
Console.WriteLine("   2. MAF puede consumir herramientas MCP como plugins del kernel");
Console.WriteLine("   3. Las herramientas MCP y nativas pueden coexistir en el mismo agente");
Console.WriteLine("   4. El agente decide automáticamente qué herramienta usar según el contexto");
Console.WriteLine("   5. MCP permite interoperabilidad entre diferentes frameworks de IA\n");

// ===== Clase helper para registrar herramientas MCP como plugins =====

/// <summary>
/// Plugin wrapper que expone las herramientas del servidor MCP como funciones del kernel.
/// En producción, MAF proporciona MCPClient que hace esto automáticamente.
/// </summary>
public class MCPToolsPlugin
{
    private readonly MCPWeatherServer _mcpServer;

    public MCPToolsPlugin(MCPWeatherServer mcpServer)
    {
        _mcpServer = mcpServer;
    }

    [Microsoft.SemanticKernel.KernelFunction("get_weather")]
    [System.ComponentModel.Description("Obtiene el clima actual para una ciudad (herramienta MCP)")]
    public string GetWeather(
        [System.ComponentModel.Description("Nombre de la ciudad")] string city)
    {
        Console.WriteLine($"\n   📡 [MCP] Llamando get_weather(city=\"{city}\")");
        
        var args = JsonSerializer.SerializeToElement(new { city });
        var result = _mcpServer.CallTool("get_weather", args);
        
        return result.Content.FirstOrDefault()?.Text ?? "Sin resultado";
    }

    [Microsoft.SemanticKernel.KernelFunction("get_forecast")]
    [System.ComponentModel.Description("Obtiene el pronóstico del tiempo para varios días (herramienta MCP)")]
    public string GetForecast(
        [System.ComponentModel.Description("Nombre de la ciudad")] string city,
        [System.ComponentModel.Description("Número de días (1-7)")] int days = 3)
    {
        Console.WriteLine($"\n   📡 [MCP] Llamando get_forecast(city=\"{city}\", days={days})");
        
        var args = JsonSerializer.SerializeToElement(new { city, days });
        var result = _mcpServer.CallTool("get_forecast", args);
        
        return result.Content.FirstOrDefault()?.Text ?? "Sin resultado";
    }

    [Microsoft.SemanticKernel.KernelFunction("get_headlines")]
    [System.ComponentModel.Description("Obtiene titulares de noticias recientes (herramienta MCP)")]
    public string GetHeadlines(
        [System.ComponentModel.Description("Categoría: Tecnología, Internacional, Economía, Negocios")] string? category = null,
        [System.ComponentModel.Description("Número de titulares (1-10)")] int count = 4)
    {
        Console.WriteLine($"\n   📡 [MCP] Llamando get_headlines(category=\"{category ?? "todas"}\", count={count})");
        
        var args = JsonSerializer.SerializeToElement(new { category, count });
        var result = _mcpServer.CallTool("get_headlines", args);
        
        return result.Content.FirstOrDefault()?.Text ?? "Sin resultado";
    }
}
