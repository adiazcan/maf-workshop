// ============================================================================
// Archivo: Program.cs
// Descripción: Primer agente conversacional con Microsoft Agent Framework
// Módulo: 1 - Fundamentos
// Lab: 01-hello-agent
// ============================================================================

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

// ===== Configuración =====
// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()  // Cargar credenciales desde user secrets
    .Build();

// Obtener valores de configuración
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");

// ===== Crear Agente =====
// Usamos AzureOpenAIClient con DefaultAzureCredential para autenticación
// Esto usa az login, managed identity, o variables de entorno automáticamente
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "AsistenteGeneral",
        instructions: """
            Eres un asistente útil y amigable llamado AsistenteGeneral.
            Respondes siempre en español de forma clara y concisa.
            Eres cortés y profesional en todas tus interacciones.
            """
    );

// ===== Bucle de Conversación =====
Console.WriteLine("============================================");
Console.WriteLine("🤖 Hello Agent - Tu Primer Agente MAF");
Console.WriteLine("============================================");
Console.WriteLine("Agente: AsistenteGeneral");
Console.WriteLine("Escribe 'salir' para terminar la conversación");
Console.WriteLine("============================================\n");

// Lista para mantener el historial de mensajes
var messages = new List<ChatMessage>();

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
    
    // Agregar el mensaje del usuario al historial
    messages.Add(new UserChatMessage(userInput));
    
    // Invocar el agente y obtener respuesta
    Console.Write("🤖 AsistenteGeneral: ");
    
    try
    {
        // RunStreamingAsync permite mostrar la respuesta progresivamente
        var responseText = "";
        await foreach (var update in agent.RunStreamingAsync(messages))
        {
            // Mostrar cada fragmento de la respuesta
            Console.Write(update);
            responseText += update;
        }
        
        Console.WriteLine("\n");
        
        // Agregar la respuesta del asistente al historial para mantener contexto
        messages.Add(new AssistantChatMessage(responseText));
        
        // Gestión de historial: truncar si supera 10 mensajes
        // Esto previene exceder el límite de tokens del modelo
        if (messages.Count > 10)
        {
            messages = messages.Skip(messages.Count - 10).ToList();
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}");
        Console.WriteLine("Verifica que tu endpoint y credenciales sean correctos.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error inesperado: {ex.Message}\n");
    }
}
