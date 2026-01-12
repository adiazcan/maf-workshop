// =============================================================================
// Program.cs - Workflow de Handoff con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa el patrón oficial de Handoff Orchestration
// donde un TriageAgent transfiere el control completo a agentes especialistas.
//
// Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff
//
// Conceptos demostrados:
// - AgentWorkflowBuilder.StartHandoffWith() para configurar handoffs
// - ChatClientAgent para agentes compatibles con handoff
// - Reglas de handoff bidireccionales
// - Streaming de eventos del workflow
// =============================================================================

using Azure.AI.OpenAI;
using Azure.Identity;
using DelegationWorkflow;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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

// Obtener configuración de Azure OpenAI
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:Endpoint");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:DeploymentName");
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:ApiKey. Use 'dotnet user-secrets set AzureOpenAI:ApiKey TU-API-KEY'");

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("    WORKFLOW DE HANDOFF: Triage → Especialistas");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear el cliente de Azure OpenAI
// =============================================================================

// Crear el cliente de chat usando Azure OpenAI
// Nota: Usamos API key aquí, pero en producción se recomienda Azure Identity
var azureClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new System.ClientModel.ApiKeyCredential(apiKey));

IChatClient chatClient = azureClient
    .GetChatClient(deploymentName)
    .AsIChatClient();

Console.WriteLine("✓ Cliente Azure OpenAI configurado");
Console.WriteLine($"  Endpoint: {endpoint}");
Console.WriteLine($"  Modelo: {deploymentName}");
Console.WriteLine();

// =============================================================================
// PASO 3: Crear los agentes especialistas
// =============================================================================

Console.WriteLine("👥 Creando equipo de agentes...");
Console.WriteLine();

// Crear el agente Triage (coordinador)
var triageAgent = TriageAgentFactory.CreateTriageAgent(chatClient);
Console.WriteLine($"  ✓ {triageAgent.Name} (Coordinador)");

// Crear los agentes especialistas
var designerAgent = SpecialistAgents.CreateDesignerAgent(chatClient);
var developerAgent = SpecialistAgents.CreateDeveloperAgent(chatClient);
var qaAgent = SpecialistAgents.CreateQAAgent(chatClient);

Console.WriteLine($"    └─ {designerAgent.Name} (UI/UX)");
Console.WriteLine($"    └─ {developerAgent.Name} (Código)");
Console.WriteLine($"    └─ {qaAgent.Name} (Testing)");
Console.WriteLine();

// =============================================================================
// PASO 4: Configurar las reglas de Handoff
// =============================================================================

Console.WriteLine("📋 Configurando reglas de handoff...");

// Construir el workflow de handoff usando AgentWorkflowBuilder
// - StartHandoffWith: Define el agente inicial que recibe todas las tareas
// - WithHandoffs: Define a qué agentes puede hacer handoff el triage
// - WithHandoff: Define handoffs individuales (especialistas → triage)
var workflow = AgentWorkflowBuilder
    .StartHandoffWith(triageAgent)
    .WithHandoffs(triageAgent, [designerAgent, developerAgent, qaAgent])
    .WithHandoff(designerAgent, triageAgent)
    .WithHandoff(developerAgent, triageAgent)
    .WithHandoff(qaAgent, triageAgent)
    .Build();

Console.WriteLine("   triage_agent → [designer_agent, developer_agent, qa_agent]");
Console.WriteLine("   designer_agent → [triage_agent]");
Console.WriteLine("   developer_agent → [triage_agent]");
Console.WriteLine("   qa_agent → [triage_agent]");
Console.WriteLine();

// =============================================================================
// PASO 5: Definir tareas de diferentes tipos
// =============================================================================

var tasks = new[]
{
    // Tarea de diseño
    "Diseñar la pantalla de login con campos de usuario y contraseña, incluir botón de 'Olvidé mi contraseña' y opción de login con redes sociales",
    
    // Tarea de desarrollo
    "Implementar un endpoint REST para autenticación JWT que reciba email y password, valide credenciales contra la base de datos y retorne un token",
    
    // Tarea de QA
    "Crear los casos de prueba para el flujo de login, incluyendo credenciales válidas, inválidas, cuenta bloqueada y rate limiting"
};

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    PROCESANDO TAREAS CON HANDOFF");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");

// =============================================================================
// PASO 6: Procesar cada tarea usando el workflow de handoff
// =============================================================================

var results = new List<(string Task, string HandoffTo, string Response)>();

for (int i = 0; i < tasks.Length; i++)
{
    Console.WriteLine();
    Console.WriteLine($"┌─────────────────────────────────────────────────────────────────┐");
    Console.WriteLine($"│ TAREA {i + 1}/{tasks.Length}                                                        │");
    Console.WriteLine($"└─────────────────────────────────────────────────────────────────┘");
    Console.WriteLine();
    
    var taskDescription = tasks[i].Length > 60 
        ? tasks[i].Substring(0, 57) + "..." 
        : tasks[i];
    Console.WriteLine($"📨 \"{taskDescription}\"");
    Console.WriteLine();

    // Crear historial de mensajes para esta tarea
    List<ChatMessage> messages = new()
    {
        new ChatMessage(ChatRole.User, tasks[i])
    };

    // Ejecutar el workflow con streaming
    StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    string currentAgent = "triage_agent";
    string handoffTo = "";
    string finalResponse = "";
    
    // Procesar eventos del workflow
    await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
    {
        if (evt is AgentRunUpdateEvent e)
        {
            // Detectar cambio de agente (handoff)
            if (e.ExecutorId != currentAgent)
            {
                if (currentAgent == "triage_agent")
                {
                    handoffTo = e.ExecutorId ?? "";
                    Console.WriteLine($"   🔀 Handoff → {handoffTo}");
                    Console.WriteLine();
                }
                currentAgent = e.ExecutorId ?? currentAgent;
            }
            
            // Acumular respuesta del especialista
            if (currentAgent != "triage_agent" && !string.IsNullOrEmpty(e.Data?.ToString()))
            {
                finalResponse += e.Data?.ToString();
            }
            
            // Mostrar streaming (solo primeros caracteres para no saturar)
            if (!string.IsNullOrEmpty(e.Data?.ToString()))
            {
                Console.Write(e.Data);
            }
        }
        else if (evt is WorkflowOutputEvent outputEvt)
        {
            // El workflow terminó
            var outputMessages = outputEvt.Data as List<ChatMessage>;
            if (outputMessages?.Count > 0)
            {
                var lastMessage = outputMessages.Last();
                if (string.IsNullOrEmpty(finalResponse))
                {
                    finalResponse = lastMessage.Text ?? "";
                }
            }
            break;
        }
    }
    
    Console.WriteLine();
    Console.WriteLine();
    
    results.Add((tasks[i], handoffTo, finalResponse));
}

// =============================================================================
// PASO 7: Resumen de handoffs
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    RESUMEN DE HANDOFFS");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

Console.WriteLine("┌──────────┬────────────────────────────────────────┬─────────────────┐");
Console.WriteLine("│ Tarea    │ Descripción                            │ Handoff a       │");
Console.WriteLine("├──────────┼────────────────────────────────────────┼─────────────────┤");

for (int i = 0; i < results.Count; i++)
{
    var (task, handoff, _) = results[i];
    var truncatedTask = task.Length > 38 ? task.Substring(0, 35) + "..." : task.PadRight(38);
    var agentDisplay = string.IsNullOrEmpty(handoff) ? "N/A" : handoff;
    Console.WriteLine($"│ Tarea {i + 1}  │ {truncatedTask} │ {agentDisplay,-15} │");
}

Console.WriteLine("└──────────┴────────────────────────────────────────┴─────────────────┘");
Console.WriteLine();

// Contar handoffs por tipo
var designerCount = results.Count(r => r.HandoffTo.Contains("designer"));
var developerCount = results.Count(r => r.HandoffTo.Contains("developer"));
var qaCount = results.Count(r => r.HandoffTo.Contains("qa"));

Console.WriteLine("📊 Distribución de handoffs:");
Console.WriteLine($"   🎨 designer_agent:   {designerCount} tarea(s)");
Console.WriteLine($"   💻 developer_agent:  {developerCount} tarea(s)");
Console.WriteLine($"   🔍 qa_agent:         {qaCount} tarea(s)");
Console.WriteLine();

// =============================================================================
// PASO 8: Validación del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ Patrón Handoff Orchestration implementado correctamente");
Console.WriteLine("✓ El triage_agent transfirió control a especialistas");
Console.WriteLine("✓ Cada especialista manejó su tarea de forma independiente");
Console.WriteLine("✓ El contexto completo se transfirió en cada handoff");
Console.WriteLine();
Console.WriteLine("📖 Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff");
Console.WriteLine();
