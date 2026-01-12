// =============================================================================
// Program.cs - Workflow Concurrente con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa ejecución concurrente donde múltiples 
// agentes trabajan en la misma tarea simultáneamente usando 
// AgentWorkflowBuilder.BuildConcurrent().
//
// Referencia oficial:
// https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/concurrent
//
// Conceptos demostrados:
// - ChatClientAgent: Agentes especializados con Microsoft Agent Framework
// - AgentWorkflowBuilder.BuildConcurrent(): Orquestación paralela nativa de MAF
// - InProcessExecution.StreamAsync(): Ejecución en proceso con streaming
// - WorkflowEvent: Eventos de progreso del workflow
// - AgentRunUpdateEvent: Actualizaciones en tiempo real de cada agente
// - WorkflowOutputEvent: Resultado agregado final
// =============================================================================

using System.Diagnostics;
using Azure.AI.OpenAI;
using Azure.Identity;
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

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("    WORKFLOW CONCURRENTE: Múltiples Perspectivas Simultáneas");
Console.WriteLine("        Usando AgentWorkflowBuilder.BuildConcurrent()");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear cliente de Azure OpenAI con Microsoft.Extensions.AI
// =============================================================================

// Usar DefaultAzureCredential para autenticación (recomendado para desarrollo)
// AsIChatClient() convierte el cliente a la interfaz IChatClient de Extensions.AI
var client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();

Console.WriteLine("✓ Cliente Azure OpenAI configurado con DefaultAzureCredential");
Console.WriteLine($"  Endpoint: {endpoint}");
Console.WriteLine($"  Modelo: {deploymentName}");
Console.WriteLine();

// =============================================================================
// PASO 3: Definir agentes especializados para ejecución concurrente
// =============================================================================
// Cada agente representa una perspectiva diferente analizando el mismo problema.
// Usamos ChatClientAgent de Microsoft.Agents.AI.Workflows para crear los agentes.
// =============================================================================

// Método helper para crear agentes con diferentes perspectivas
static ChatClientAgent CreateExpertAgent(IChatClient chatClient, string name, string instructions) =>
    new(chatClient, instructions);

// Crear los tres agentes especializados
var researcherAgent = CreateExpertAgent(
    client,
    name: "Investigador",
    instructions: """
        Eres un experto investigador de mercado y productos. Dado un tema o prompt:
        1. Proporciona insights concisos y basados en hechos
        2. Identifica oportunidades de mercado
        3. Señala riesgos potenciales
        4. Usa datos y tendencias actuales
        
        Responde en español, de forma estructurada y profesional.
        Máximo 150 palabras.
        """);

var marketerAgent = CreateExpertAgent(
    client,
    name: "Marketing",
    instructions: """
        Eres un estratega de marketing creativo. Dado un tema o prompt:
        1. Crea propuestas de valor convincentes
        2. Define mensajes para el público objetivo
        3. Sugiere canales de comunicación
        4. Incluye un slogan o tagline
        
        Responde en español, de forma creativa y orientada a la acción.
        Máximo 150 palabras.
        """);

var legalAgent = CreateExpertAgent(
    client,
    name: "Legal",
    instructions: """
        Eres un asesor legal y de cumplimiento cauteloso. Dado un tema o prompt:
        1. Identifica restricciones regulatorias
        2. Señala posibles riesgos legales
        3. Recomienda disclaimers necesarios
        4. Menciona certificaciones requeridas
        
        Responde en español, de forma precisa y orientada al cumplimiento.
        Máximo 150 palabras.
        """);

Console.WriteLine("✓ 3 agentes especializados creados:");
Console.WriteLine("   • 🔍 Investigador - Análisis de mercado y oportunidades");
Console.WriteLine("   • 📣 Marketing - Estrategia creativa y mensajes");
Console.WriteLine("   • ⚖️ Legal - Cumplimiento y regulaciones");
Console.WriteLine();

// =============================================================================
// PASO 4: Construir workflow concurrente con AgentWorkflowBuilder
// =============================================================================
// AgentWorkflowBuilder.BuildConcurrent() crea un workflow que ejecuta
// todos los agentes en paralelo y agrega sus resultados automáticamente.
// =============================================================================

var agents = new[] { researcherAgent, marketerAgent, legalAgent };
var workflow = AgentWorkflowBuilder.BuildConcurrent(agents);

Console.WriteLine("✓ Workflow concurrente construido con AgentWorkflowBuilder.BuildConcurrent()");
Console.WriteLine();

// =============================================================================
// PASO 5: Ejecutar workflow concurrente con streaming
// =============================================================================

Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ EJECUCIÓN CONCURRENTE: Todos los agentes al mismo tiempo       │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
Console.WriteLine();

// Definir el prompt que todos los agentes procesarán
var userPrompt = "Estamos lanzando una nueva bicicleta eléctrica económica para commuters urbanos en México.";

Console.WriteLine($"📝 Prompt: \"{userPrompt}\"");
Console.WriteLine();
Console.WriteLine("🔄 Ejecutando agentes en paralelo...");
Console.WriteLine();

// Preparar mensajes de entrada
var messages = new List<ChatMessage> { new(ChatRole.User, userPrompt) };

// Medir tiempo de ejecución
var stopwatch = Stopwatch.StartNew();

// Ejecutar el workflow con streaming usando InProcessExecution
StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);

// Enviar mensaje para iniciar el flujo de eventos
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

// Procesar eventos del workflow
List<ChatMessage>? result = null;
var agentStartTimes = new Dictionary<string, DateTime>();

await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentRunUpdateEvent updateEvent)
    {
        // Mostrar actualizaciones en tiempo real de cada agente
        var agentId = updateEvent.ExecutorId ?? "Unknown";
        
        if (!agentStartTimes.ContainsKey(agentId))
        {
            agentStartTimes[agentId] = DateTime.Now;
            Console.WriteLine($"   → {agentId} comenzó a procesar...");
        }
    }
    else if (evt is WorkflowOutputEvent outputEvt)
    {
        // Recolectar resultado final agregado
        result = outputEvt.Data as List<ChatMessage>;
        break;
    }
}

stopwatch.Stop();

// =============================================================================
// PASO 6: Mostrar resultados agregados
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("            RESULTADOS AGREGADOS DE TODOS LOS AGENTES");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");

if (result != null)
{
    var assistantResponses = result.Where(m => m.Role == ChatRole.Assistant).ToList();
    var agentNames = new[] { "🔍 Investigador", "📣 Marketing", "⚖️ Legal" };
    var index = 0;
    
    foreach (var message in assistantResponses)
    {
        var agentName = index < agentNames.Length ? agentNames[index] : $"Agente {index + 1}";
        Console.WriteLine();
        Console.WriteLine($"┌─── {agentName} ───");
        Console.WriteLine("│");
        
        var content = message.Text ?? message.Contents?.FirstOrDefault()?.ToString() ?? "(sin contenido)";
        foreach (var line in content.Split('\n'))
        {
            Console.WriteLine($"│ {line}");
        }
        
        Console.WriteLine("│");
        Console.WriteLine("└────────────────────────────────────────────────────────────────");
        index++;
    }
}
else
{
    Console.WriteLine("⚠️ No se recibieron resultados del workflow");
}

// =============================================================================
// PASO 7: Análisis de rendimiento
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    ANÁLISIS DE RENDIMIENTO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

Console.WriteLine($"⏱️  Tiempo total de ejecución concurrente: {stopwatch.ElapsedMilliseconds}ms");
Console.WriteLine($"📊 Agentes ejecutados en paralelo: {agents.Length}");
Console.WriteLine();

// =============================================================================
// PASO 8: Resumen del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ Workflow concurrente ejecutado con AgentWorkflowBuilder.BuildConcurrent()");
Console.WriteLine("✓ Todos los agentes procesaron el mismo prompt en paralelo");
Console.WriteLine("✓ Resultados agregados automáticamente por el framework");
Console.WriteLine("✓ Eventos de streaming procesados en tiempo real");
Console.WriteLine();
Console.WriteLine("📚 Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/concurrent");
Console.WriteLine();
