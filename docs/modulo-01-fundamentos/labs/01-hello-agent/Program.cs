// ============================================================================
// Archivo: Program.cs
// Descripción: Primer agente conversacional con Microsoft Agent Framework
// Módulo: 1 - Fundamentos
// Lab: 01-hello-agent
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

// ===== Configuración =====
// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()  // Cargar API key desde user secrets
    .Build();

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado. Usa: dotnet user-secrets set 'AzureOpenAI:ApiKey' 'tu-key'");

// ===== Crear Kernel =====
// El Kernel es el contenedor de dependencias de Microsoft Agent Framework
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

var kernel = builder.Build();

// ===== Crear Agente =====
// ChatCompletionAgent es el tipo básico de agente conversacional
var agent = new ChatCompletionAgent()
{
    Name = "AsistenteGeneral",
    Instructions = @"Eres un asistente útil y amigable llamado AsistenteGeneral.
Respondes siempre en español de forma clara y concisa.
Eres cortés y profesional en todas tus interacciones.",
    Kernel = kernel
};

// ===== Crear Historial de Conversación =====
// ChatHistory almacena el contexto de la conversación
var chatHistory = new ChatHistory();

// ===== Bucle de Conversación =====
Console.WriteLine("============================================");
Console.WriteLine("🤖 Hello Agent - Tu Primer Agente MAF");
Console.WriteLine("============================================");
Console.WriteLine($"Agente: {agent.Name}");
Console.WriteLine("Escribe 'salir' para terminar la conversación");
Console.WriteLine("============================================\n");

while (true)
{
    // Obtener entrada del usuario
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    // Verificar si el usuario quiere salir
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🤖 AsistenteGeneral: ¡Hasta pronto! 👋\n");
        break;
    }
    
    // Agregar mensaje del usuario al historial
    chatHistory.AddUserMessage(userInput);
    
    // Invocar el agente y obtener respuesta
    Console.Write("🤖 AsistenteGeneral: ");
    
    try
    {
        // InvokeStreamingAsync permite mostrar la respuesta progresivamente
        await foreach (var message in agent.InvokeStreamingAsync(chatHistory))
        {
            // Mostrar cada fragmento de la respuesta
            Console.Write(message.Content);
        }
        
        Console.WriteLine("\n");
        
        // Agregar la última respuesta del agente al historial
        // (El streaming no lo hace automáticamente)
        var lastMessage = chatHistory.Last();
        if (lastMessage.Role == Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User)
        {
            // Si el último mensaje es del usuario, necesitamos obtener la respuesta completa
            var response = await agent.InvokeAsync(chatHistory);
            chatHistory.Add(response);
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}");
        Console.WriteLine("Verifica que tu endpoint y API key sean correctos.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error inesperado: {ex.Message}\n");
    }
    
    // Gestión de historial: truncar si supera 10 mensajes
    // Esto previene exceder el límite de tokens del modelo
    if (chatHistory.Count > 10)
    {
        // Mantener solo los últimos 10 mensajes
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 10).ToList();
        chatHistory.Clear();
        foreach (var message in messagesToKeep)
        {
            chatHistory.Add(message);
        }
    }
}
