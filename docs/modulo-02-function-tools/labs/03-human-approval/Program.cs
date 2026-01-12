// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de Human-in-the-Loop con Microsoft Agent Framework
// Módulo: 2 - Function Tools
// Lab: 03-human-approval
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Agents.AI.Chat;
using Microsoft.Extensions.Configuration;
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

// ===== Crear Function Tools =====

// Función de SOLO LECTURA - No requiere aprobación
var listFilesFunction = AIFunctionFactory.Create(
    () => SensitiveOperations.ListFiles(),
    name: "list_files",
    description: "Lista los archivos disponibles en el sistema. Úsala cuando el usuario quiera ver qué archivos existen. NO requiere aprobación."
);

// Función de SOLO LECTURA - No requiere aprobación
var checkBalanceFunction = AIFunctionFactory.Create(
    () => SensitiveOperations.CheckBalance(),
    name: "check_balance",
    description: "Consulta el saldo disponible en la cuenta. Operación de solo lectura. NO requiere aprobación."
);

// Función SENSIBLE - REQUIERE aprobación humana
var deleteFileFunction = AIFunctionFactory.Create(
    (string filePath) => SensitiveOperations.DeleteFile(filePath),
    name: "delete_file",
    description: "Elimina un archivo del sistema. OPERACIÓN SENSIBLE: Requiere confirmación del usuario antes de ejecutar."
);

// Función SENSIBLE - REQUIERE aprobación humana
var sendEmailFunction = AIFunctionFactory.Create(
    (string recipient, string subject, string body) => SensitiveOperations.SendEmail(recipient, subject, body),
    name: "send_email",
    description: "Envía un correo electrónico. OPERACIÓN SENSIBLE: Requiere confirmación antes de enviar."
);

// Función SENSIBLE - REQUIERE aprobación humana
var transferFundsFunction = AIFunctionFactory.Create(
    (string destinationAccount, decimal amount, string concept) => SensitiveOperations.TransferFunds(destinationAccount, amount, concept),
    name: "transfer_funds",
    description: "Transfiere fondos a una cuenta. OPERACIÓN FINANCIERA SENSIBLE: Requiere aprobación obligatoria."
);

// ===== Crear Agente con todas las funciones =====
var agent = new ChatCompletionAgent(
    name: "AsistenteSeguro",
    instructions: """
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
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey,
    tools: new AIFunction[] 
    { 
        listFilesFunction, 
        checkBalanceFunction, 
        deleteFileFunction, 
        sendEmailFunction, 
        transferFundsFunction 
    }
);

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
        await foreach (var message in agent.InvokeAsync(chatHistory))
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
