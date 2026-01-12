// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de Human-in-the-Loop para operaciones sensibles
// Módulo: 2 - Function Tools
// Lab: 03-human-approval
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ApprovalWorkflow;

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

// ===== Crear Kernel =====
var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

// Registrar las operaciones sensibles como plugin
builder.Plugins.AddFromType<SensitiveOperations>();

var kernel = builder.Build();

// ===== Crear Agente =====
var agent = new ChatCompletionAgent()
{
    Name = "AsistenteSeguro",
    Instructions = """
        Eres un asistente administrativo llamado AsistenteSeguro.
        Tienes acceso a operaciones del sistema que pueden ser sensibles.
        
        OPERACIONES DISPONIBLES:
        1. list_files - Ver archivos (sin aprobación)
        2. delete_file - Eliminar archivos (REQUIERE APROBACIÓN)
        3. check_balance - Ver saldo (sin aprobación)
        4. transfer_funds - Transferir dinero (REQUIERE APROBACIÓN)
        5. send_email - Enviar correos (REQUIERE APROBACIÓN)
        
        REGLAS DE SEGURIDAD:
        - Antes de operaciones sensibles, informa al usuario que se pedirá confirmación
        - Si el usuario cancela, respeta su decisión y confirma la cancelación
        - Nunca intentes evadir las aprobaciones
        
        Responde siempre en español de forma profesional.
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        }
    )
};

// ===== Historial =====
var chatHistory = new ChatHistory();

// ===== Interfaz =====
Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
Console.WriteLine("║  🔒 Workflow con Aprobación Humana (Human-in-the-Loop)║");
Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
Console.WriteLine("║  Agente: AsistenteSeguro                              ║");
Console.WriteLine("║                                                       ║");
Console.WriteLine("║  Operaciones de SOLO LECTURA (sin aprobación):        ║");
Console.WriteLine("║    • Listar archivos                                  ║");
Console.WriteLine("║    • Ver saldo                                        ║");
Console.WriteLine("║                                                       ║");
Console.WriteLine("║  Operaciones SENSIBLES (requieren aprobación):        ║");
Console.WriteLine("║    • Eliminar archivos                                ║");
Console.WriteLine("║    • Enviar correos                                   ║");
Console.WriteLine("║    • Transferir fondos                                ║");
Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
Console.WriteLine("║  Escribe 'salir' para terminar                        ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
Console.WriteLine();

Console.WriteLine("💡 Prueba estas acciones:");
Console.WriteLine("   📋 'Lista los archivos del sistema'");
Console.WriteLine("   🗑️  'Elimina el archivo /temporal/cache.tmp'");
Console.WriteLine("   💳 'Muestra mi saldo'");
Console.WriteLine("   💸 'Transfiere 100 euros a ES1234567890'");
Console.WriteLine("   📧 'Envía un correo a juan@empresa.com'");
Console.WriteLine();

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🔒 AsistenteSeguro: ¡Hasta pronto! Tus operaciones están protegidas. 🛡️\n");
        break;
    }
    
    chatHistory.AddUserMessage(userInput);
    
    Console.Write("🔒 AsistenteSeguro: ");
    
    try
    {
        await foreach (var message in agent.InvokeStreamingAsync(chatHistory))
        {
            Console.Write(message.Content);
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
    if (chatHistory.Count > 12)
    {
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 12).ToList();
        chatHistory.Clear();
        foreach (var msg in messagesToKeep)
        {
            chatHistory.Add(msg);
        }
    }
}
