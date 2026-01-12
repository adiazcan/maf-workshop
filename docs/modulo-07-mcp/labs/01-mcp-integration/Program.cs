// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de integración MCP con Microsoft Agent Framework
// Módulo: 7
// Lab: 01-mcp-integration
// Usando el SDK oficial de Model Context Protocol
// ============================================================================

using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

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

Console.WriteLine("📋 Configuración:");
Console.WriteLine($"   Endpoint: {endpoint}");
Console.WriteLine($"   Deployment: {deploymentName}\n");

// ===== Conectar a servidor MCP usando el SDK oficial =====
Console.WriteLine("🔌 Conectando a servidor MCP de Weather Service...");
Console.WriteLine("   (Iniciando servidor MCP automáticamente)\n");

// Crear cliente MCP que se conecta al servidor Weather
await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "WeatherMCPServer",
    Command = "dotnet",
    Arguments = ["run", "--project", "WeatherMCPServer"],
    WorkingDirectory = Directory.GetCurrentDirectory()
}));

// Descubrir herramientas disponibles en el servidor MCP
Console.WriteLine("   ✅ Conectado al servidor MCP de Weather");
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
Console.WriteLine($"   📋 {mcpTools.Count} herramientas disponibles:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"      • {tool.Name}: {tool.Description}");
}

// ===== Crear agente MAF con herramientas MCP =====
Console.WriteLine("\n🤖 Creando agente MAF con herramientas MCP...");
Console.WriteLine("   📌 Registrando herramientas MCP como AITool...");

// Convertir herramientas MCP a AITool para Microsoft Agent Framework
var aiTools = new List<AITool>();

foreach (var mcpTool in mcpTools)
{
    var toolName = mcpTool.Name;
    var toolDescription = mcpTool.Description ?? "";
    
    // Crear la función que llama al servidor MCP
    var mcpFunction = async (IReadOnlyDictionary<string, object?> arguments) =>
    {
        var args = arguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var result = await mcpClient.CallToolAsync(toolName, args);
        var textContent = result.Content.FirstOrDefault() as ModelContextProtocol.Protocol.TextContentBlock;
        return textContent?.Text ?? "";
    };

    // Crear AIFunction
    var aiFunction = AIFunctionFactory.Create(
        method: mcpFunction,
        name: toolName,
        description: toolDescription);
    
    aiTools.Add(aiFunction);
}

Console.WriteLine($"   ✅ Convertidas {aiTools.Count} herramientas MCP a AITool\n");

// ===== Crear Agente MAF con herramientas MCP =====
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .CreateAIAgent(
        name: "AgenteMCP",
        instructions: """
            Eres un asistente útil que tiene acceso a herramientas de clima a través de MCP.
            
            Puedes ayudar con:
            • Obtener el clima actual de ciudades
            • Obtener pronósticos del tiempo
            • Convertir temperaturas entre Celsius y Fahrenheit
            
            Siempre responde en español de forma clara y concisa.
            """,
        tools: aiTools.ToArray()
    );

// Crear thread para mantener el contexto de la conversación
var thread = agent.GetNewThread();

// ===== Demostración interactiva =====
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  💬 DEMOSTRACIÓN: Agente MAF con herramientas MCP de Weather");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Ejemplos de preguntas:");
Console.WriteLine("  • \"¿Qué clima hace en Madrid?\"");
Console.WriteLine("  • \"Convierte 25 grados Celsius a Fahrenheit\"");
Console.WriteLine("  • \"Dame el pronóstico de Barcelona para 5 días\"");
Console.WriteLine("  • \"¿Qué temperatura hace en Sevilla en Fahrenheit?\"");
Console.WriteLine("  • \"Escribe 'salir' para terminar\"\n");

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

    try
    {
        Console.Write("\n🤖 AgenteMCP: ");
        
        // Ejecutar el agente con las herramientas MCP
        await foreach (var update in agent.RunStreamingAsync(userInput, thread))
        {
            Console.Write(update);
        }
        
        Console.WriteLine("\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n⚠️ Error: {ex.Message}");
        Console.WriteLine("   Intenta con otra pregunta.\n");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Detalles: {ex.InnerException.Message}\n");
        }
    }
}

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
Console.WriteLine("  ✅ Lab completado: Integración MCP con MAF");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine("📝 Resumen de lo aprendido:");
Console.WriteLine("   1. Creaste un servidor MCP personalizado con herramientas de clima");
Console.WriteLine("   2. MAF se conectó al servidor usando el SDK oficial de MCP");
Console.WriteLine("   3. Las herramientas MCP se convirtieron automáticamente en AITool");
Console.WriteLine("   4. El agente usó function calling para invocar las herramientas MCP");
Console.WriteLine("   5. MCP permite interoperabilidad entre diferentes frameworks de IA\n");
Console.WriteLine("🔗 Próximos pasos:");
Console.WriteLine("   • Crea tus propios servidores MCP para tus APIs");
Console.WriteLine("   • Explora servidores MCP públicos: @modelcontextprotocol/*");
Console.WriteLine("   • Combina múltiples servidores MCP en un solo agente\n");
