// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de composición de agentes (agent-as-tool)
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AgentComposition;

// ===== Configuración =====
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

// ===== Crear Kernel Base =====
// Este kernel se compartirá entre agentes para optimizar recursos
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

var kernel = builder.Build();

// ===== Crear Agente Especializado =====
// El CalculatorAgent es un agente completo dedicado a matemáticas
var calculatorAgent = new CalculatorAgent(kernel);

// ===== Crear Agente Principal =====
// MainAgent usa al CalculatorAgent como una "herramienta"
// El kernel debe ser clonado para agregar plugins específicos del MainAgent
var mainKernel = kernel.Clone();
var mainAgent = new MainAgent(mainKernel, calculatorAgent);

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
Console.WriteLine("🤖 Composición de Agentes (Agent-as-Tool)");
Console.WriteLine("============================================");
Console.WriteLine($"Agente Principal: {mainAgent.Name}");
Console.WriteLine($"Agente Especializado: {calculatorAgent.Name}");
Console.WriteLine("============================================");
Console.WriteLine();
Console.WriteLine("💡 Prueba estas preguntas:");
Console.WriteLine("   📊 Matemáticas: '¿Cuánto es 15% de 850?'");
Console.WriteLine("   📊 Matemáticas: 'Calcula el promedio de 85, 92, 78, 95'");
Console.WriteLine("   💬 General: '¿Cuál es la capital de España?'");
Console.WriteLine("   💬 General: 'Dame consejos para aprender programación'");
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
        Console.WriteLine("\n🤖 AsistenteGeneral: ¡Hasta pronto! 👋\n");
        break;
    }
    
    chatHistory.AddUserMessage(userInput);
    
    Console.Write($"🤖 {mainAgent.Name}: ");
    
    try
    {
        await foreach (var content in mainAgent.ProcessMessageAsync(chatHistory))
        {
            Console.Write(content);
        }
        Console.WriteLine("\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}\n");
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
