// =============================================================================
// Program.cs - Workflow Persistente con Azure AI Agent Service
// =============================================================================
// Descripción: Este ejemplo demuestra cómo usar Azure AI Agent Service para
// crear workflows con estado persistente. El thread de conversación se guarda
// y puede recuperarse después de cerrar y reabrir la aplicación.
//
// Conceptos demostrados:
// - AIProjectClient para Azure AI Foundry
// - Creación de agentes con Azure AI Agent Service
// - Threads persistentes (conversaciones que sobreviven reinicios)
// - Runs para ejecución de agentes
// - Pausa y reanudación de workflows
// =============================================================================

using System.Text.Json;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

// =============================================================================
// PASO 1: Configuración
// =============================================================================

// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

// Obtener configuración de Azure AI Foundry
var projectEndpoint = configuration["AzureAIFoundry:ProjectEndpoint"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureAIFoundry:ProjectEndpoint");
var modelDeployment = configuration["AzureAIFoundry:ModelDeployment"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureAIFoundry:ModelDeployment");

// Archivo para persistir el ID del thread localmente
var threadStateFile = Path.Combine(Directory.GetCurrentDirectory(), "thread_state.json");

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("     WORKFLOW PERSISTENTE: Azure AI Agent Service");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear cliente de Azure AI Projects
// =============================================================================

Console.WriteLine("Conectando a Azure AI Foundry...");

// Usar DefaultAzureCredential para autenticación
// Funciona con: Azure CLI login, Visual Studio credentials, Managed Identity
var credential = new DefaultAzureCredential();

// Crear cliente del proyecto
var projectClient = new AIProjectClient(new Uri(projectEndpoint), credential);

// Obtener el cliente de agentes
var agentsClient = projectClient.GetAgentsClient();

Console.WriteLine($"✓ Conectado a proyecto: {projectEndpoint}");
Console.WriteLine();

// =============================================================================
// PASO 3: Verificar si existe un thread previo
// =============================================================================

string? existingThreadId = null;
string? existingAgentId = null;

if (File.Exists(threadStateFile))
{
    try
    {
        var stateJson = await File.ReadAllTextAsync(threadStateFile);
        var state = JsonSerializer.Deserialize<ThreadState>(stateJson);
        existingThreadId = state?.ThreadId;
        existingAgentId = state?.AgentId;
        
        Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 📁 ESTADO PREVIO ENCONTRADO                                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
        Console.WriteLine($"   Thread ID: {existingThreadId}");
        Console.WriteLine($"   Agent ID: {existingAgentId}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("⚠️ No se pudo leer el estado previo, iniciando nuevo workflow");
    }
}

// =============================================================================
// PASO 4: Crear o recuperar agente
// =============================================================================

Agent agent;

if (!string.IsNullOrEmpty(existingAgentId))
{
    try
    {
        // Intentar recuperar el agente existente
        agent = await agentsClient.GetAgentAsync(existingAgentId);
        Console.WriteLine($"✓ Agente recuperado: {agent.Name} (ID: {agent.Id})");
    }
    catch
    {
        // Si no existe, crear uno nuevo
        Console.WriteLine("⚠️ Agente previo no encontrado, creando nuevo...");
        agent = await CreateNewAgentAsync();
    }
}
else
{
    // Crear un nuevo agente
    agent = await CreateNewAgentAsync();
}

async Task<Agent> CreateNewAgentAsync()
{
    Console.WriteLine("Creando nuevo agente persistente...");
    
    var newAgent = await agentsClient.CreateAgentAsync(
        model: modelDeployment,
        name: "AsistenteAnalisisDatos",
        instructions: """
            Eres un asistente especializado en análisis de datos de ventas.
            Tu trabajo es:
            1. Ayudar a analizar datos de ventas
            2. Identificar tendencias y patrones
            3. Proporcionar recomendaciones basadas en datos
            4. Mantener el contexto de conversaciones anteriores
            
            Responde en español, de forma clara y estructurada.
            Si el usuario retoma una conversación, reconócelo y continúa donde quedaron.
            """
    );
    
    Console.WriteLine($"✓ Agente creado: {newAgent.Name} (ID: {newAgent.Id})");
    return newAgent;
}

Console.WriteLine();

// =============================================================================
// PASO 5: Crear o recuperar thread
// =============================================================================

AgentThread thread;

if (!string.IsNullOrEmpty(existingThreadId))
{
    try
    {
        // Intentar recuperar el thread existente
        thread = await agentsClient.GetThreadAsync(existingThreadId);
        Console.WriteLine($"✓ Thread recuperado (ID: {thread.Id})");
        
        // Mostrar mensajes anteriores
        Console.WriteLine();
        Console.WriteLine("📜 HISTORIAL DE CONVERSACIÓN:");
        Console.WriteLine("─────────────────────────────────────────────────────────────────");
        
        var messages = agentsClient.GetMessagesAsync(thread.Id);
        var messageList = new List<ThreadMessage>();
        await foreach (var msg in messages)
        {
            messageList.Add(msg);
        }
        
        // Mostrar en orden cronológico (los mensajes vienen en orden inverso)
        messageList.Reverse();
        foreach (var msg in messageList)
        {
            var role = msg.Role == MessageRole.User ? "👤 Usuario" : "🤖 Agente";
            var content = msg.ContentItems.FirstOrDefault()?.ToString() ?? "";
            // Truncar mensajes largos para visualización
            if (content.Length > 100)
            {
                content = content.Substring(0, 97) + "...";
            }
            Console.WriteLine($"   {role}: {content}");
        }
        Console.WriteLine("─────────────────────────────────────────────────────────────────");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("⚠️ Thread previo no encontrado, creando nuevo...");
        thread = await CreateNewThreadAsync();
    }
}
else
{
    thread = await CreateNewThreadAsync();
}

async Task<AgentThread> CreateNewThreadAsync()
{
    Console.WriteLine("Creando nuevo thread de conversación...");
    var newThread = await agentsClient.CreateThreadAsync();
    Console.WriteLine($"✓ Thread creado (ID: {newThread.Id})");
    return newThread;
}

// Guardar estado actual
await SaveStateAsync(thread.Id, agent.Id);

async Task SaveStateAsync(string threadId, string agentId)
{
    var state = new ThreadState { ThreadId = threadId, AgentId = agentId };
    var json = JsonSerializer.Serialize(state);
    await File.WriteAllTextAsync(threadStateFile, json);
}

Console.WriteLine();

// =============================================================================
// PASO 6: Loop de conversación
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    CONVERSACIÓN INTERACTIVA");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("Escribe tus mensajes. Comandos especiales:");
Console.WriteLine("  'salir' - Termina la sesión (el thread persiste para después)");
Console.WriteLine("  'nuevo' - Crea un nuevo thread (borra historial)");
Console.WriteLine("  'historial' - Muestra el historial completo");
Console.WriteLine();
Console.WriteLine("─────────────────────────────────────────────────────────────────");

while (true)
{
    Console.Write("\n👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }
    
    // Comandos especiales
    if (userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine();
        Console.WriteLine("💾 Estado guardado. Puedes continuar la conversación más tarde.");
        Console.WriteLine($"   Thread ID: {thread.Id}");
        break;
    }
    
    if (userInput.Equals("nuevo", StringComparison.OrdinalIgnoreCase))
    {
        thread = await CreateNewThreadAsync();
        await SaveStateAsync(thread.Id, agent.Id);
        Console.WriteLine("✓ Nuevo thread creado. Historial limpio.");
        continue;
    }
    
    if (userInput.Equals("historial", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n📜 HISTORIAL COMPLETO:");
        var messages = agentsClient.GetMessagesAsync(thread.Id);
        var messageList = new List<ThreadMessage>();
        await foreach (var msg in messages)
        {
            messageList.Add(msg);
        }
        messageList.Reverse();
        foreach (var msg in messageList)
        {
            var role = msg.Role == MessageRole.User ? "👤" : "🤖";
            foreach (var content in msg.ContentItems)
            {
                Console.WriteLine($"   {role} {content}");
            }
        }
        continue;
    }
    
    // Enviar mensaje al agente
    try
    {
        // Crear mensaje en el thread
        await agentsClient.CreateMessageAsync(
            thread.Id,
            MessageRole.User,
            userInput
        );
        
        // Crear un run para que el agente procese el mensaje
        var run = await agentsClient.CreateRunAsync(
            thread.Id,
            agent.Id
        );
        
        Console.Write("\n🤖 Agente: ");
        
        // Esperar a que el run complete
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
        {
            await Task.Delay(500);
            Console.Write(".");
            run = await agentsClient.GetRunAsync(thread.Id, run.Id);
        }
        
        Console.WriteLine();
        
        if (run.Status == RunStatus.Completed)
        {
            // Obtener el mensaje más reciente del agente
            var messages = agentsClient.GetMessagesAsync(thread.Id);
            await foreach (var msg in messages)
            {
                if (msg.Role == MessageRole.Agent)
                {
                    foreach (var content in msg.ContentItems)
                    {
                        Console.WriteLine($"   {content}");
                    }
                    break;
                }
            }
        }
        else
        {
            Console.WriteLine($"   ⚠️ El run terminó con estado: {run.Status}");
            if (!string.IsNullOrEmpty(run.LastError?.Message))
            {
                Console.WriteLine($"   Error: {run.LastError.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n⚠️ Error: {ex.Message}");
    }
}

// =============================================================================
// PASO 7: Resumen final
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    SESIÓN FINALIZADA");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("📋 Resumen:");
Console.WriteLine($"   • Agent ID: {agent.Id}");
Console.WriteLine($"   • Thread ID: {thread.Id}");
Console.WriteLine($"   • Estado guardado en: {threadStateFile}");
Console.WriteLine();
Console.WriteLine("✓ El thread persiste en Azure AI Agent Service");
Console.WriteLine("✓ Ejecuta 'dotnet run' de nuevo para continuar la conversación");
Console.WriteLine("✓ El historial completo estará disponible");
Console.WriteLine();

// =============================================================================
// Clase para persistir el estado
// =============================================================================

record ThreadState
{
    public string? ThreadId { get; init; }
    public string? AgentId { get; init; }
}
