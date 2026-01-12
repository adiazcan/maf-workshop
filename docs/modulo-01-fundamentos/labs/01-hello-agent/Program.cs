// ============================================================================
// Archivo: Program.cs
// Descripción: Primer agente conversacional con Microsoft Agent Framework
// Módulo: 1 - Fundamentos
// Lab: 01-hello-agent
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Chat;
using Microsoft.Extensions.Configuration;

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

// ===== Crear Agente =====
// ChatCompletionAgent es el tipo básico de agente conversacional en MAF
// Se configura directamente con el endpoint de Azure OpenAI
var agent = new ChatCompletionAgent(
    name: "AsistenteGeneral",
    instructions: """
        Eres un asistente útil y amigable llamado AsistenteGeneral.
        Respondes siempre en español de forma clara y concisa.
        Eres cortés y profesional en todas tus interacciones.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

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
        // InvokeAsync permite obtener la respuesta del agente
        // Iteramos sobre los mensajes de respuesta
        string response = "";
        await foreach (var message in agent.InvokeAsync(chatHistory))
        {
            // Mostrar cada fragmento de la respuesta
            Console.Write(message.Content);
            response += message.Content;
        }
        
        Console.WriteLine("\n");
        
        // Agregar la respuesta del agente al historial para mantener contexto
        chatHistory.AddAssistantMessage(response);
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
