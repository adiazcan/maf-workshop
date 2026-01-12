// =============================================================================
// Program.cs - Workflow Concurrente con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa orquestación concurrente usando
// AgentWorkflowBuilder.BuildConcurrent() donde múltiples agentes trabajan
// en la misma tarea simultáneamente.
//
// Referencia oficial:
// https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/concurrent
//
// Conceptos demostrados:
// - Orquestación concurrente con AgentWorkflowBuilder.BuildConcurrent()
// - Múltiples agentes procesando la misma entrada en paralelo
// - Agregación automática de resultados
// - Streaming de eventos para monitorear progreso en tiempo real
// =============================================================================

using System.Diagnostics;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Chat;
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
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear cliente de Azure OpenAI con IChatClient
// =============================================================================

// Usar AzureCliCredential para autenticación (recomendado para desarrollo)
// En producción, usar DefaultAzureCredential o ManagedIdentityCredential
var chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();

Console.WriteLine("✓ Cliente Azure OpenAI configurado con AzureCliCredential");
Console.WriteLine($"  Endpoint: {endpoint}");
Console.WriteLine($"  Modelo: {deploymentName}");
Console.WriteLine();

// =============================================================================
// PASO 3: Definir agentes especializados para ejecución concurrente
// =============================================================================

// Agente 1: Analista de Mercado - Perspectiva de investigación
var researcherAgent = new ChatClientAgent(chatClient,
    """
    Eres un experto investigador de mercado y productos. Dado un tema o prompt:
    1. Proporciona insights concisos y basados en hechos
    2. Identifica oportunidades de mercado
    3. Señala riesgos potenciales
    4. Usa datos y tendencias actuales
    
    Responde en español, de forma estructurada y profesional.
    Máximo 150 palabras.
    """);

// Agente 2: Estratega de Marketing - Perspectiva creativa
var marketerAgent = new ChatClientAgent(chatClient,
    """
    Eres un estratega de marketing creativo. Dado un tema o prompt:
    1. Crea propuestas de valor convincentes
    2. Define mensajes para el público objetivo
    3. Sugiere canales de comunicación
    4. Incluye un slogan o tagline
    
    Responde en español, de forma creativa y orientada a la acción.
    Máximo 150 palabras.
    """);

// Agente 3: Asesor Legal - Perspectiva de cumplimiento
var legalAgent = new ChatClientAgent(chatClient,
    """
    Eres un asesor legal y de cumplimiento cauteloso. Dado un tema o prompt:
    1. Identifica restricciones regulatorias
    2. Señala posibles riesgos legales
    3. Recomienda disclaimers necesarios
    4. Menciona certificaciones requeridas
    
    Responde en español, de forma precisa y orientada al cumplimiento.
    Máximo 150 palabras.
    """);

var agents = new[] { researcherAgent, marketerAgent, legalAgent };

Console.WriteLine("✓ 3 agentes especializados creados:");
Console.WriteLine("   • Investigador - Análisis de mercado y oportunidades");
Console.WriteLine("   • Marketing - Estrategia creativa y mensajes");
Console.WriteLine("   • Legal - Cumplimiento y regulaciones");
Console.WriteLine();

// =============================================================================
// PASO 4: Construir workflow concurrente con AgentWorkflowBuilder
// =============================================================================

// AgentWorkflowBuilder.BuildConcurrent() crea un workflow donde todos los
// agentes procesan la misma entrada simultáneamente
var workflow = AgentWorkflowBuilder.BuildConcurrent(agents);

Console.WriteLine("✓ Workflow concurrente construido con AgentWorkflowBuilder.BuildConcurrent()");
Console.WriteLine();

// =============================================================================
// PASO 5: Ejecutar workflow y monitorear eventos
// =============================================================================

Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ EJECUCIÓN CONCURRENTE: Todos los agentes al mismo tiempo       │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
Console.WriteLine();

// Definir el prompt que todos los agentes procesarán
var userPrompt = "Estamos lanzando una nueva bicicleta eléctrica económica para commuters urbanos en México.";

Console.WriteLine($"📝 Prompt: \"{userPrompt}\"");
Console.WriteLine();

// Crear lista de mensajes inicial
var messages = new List<ChatMessage> { new(ChatRole.User, userPrompt) };

// Medir tiempo de ejecución
var stopwatch = Stopwatch.StartNew();

// Ejecutar workflow con streaming de eventos
StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

// Recolectar resultados mientras se procesan los eventos
List<ChatMessage> results = new();
var agentUpdates = new List<string>();

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("              PROGRESO EN TIEMPO REAL (Streaming)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    // AgentRunUpdateEvent: Actualización de un agente individual
    if (evt is AgentRunUpdateEvent e)
    {
        // Mostrar progreso del agente
        var agentId = e.ExecutorId ?? "Unknown";
        Console.WriteLine($"⚡ [{agentId}]: {e.Data}");
        agentUpdates.Add($"{agentId}: {e.Data}");
    }
    // WorkflowOutputEvent: Resultado final agregado
    else if (evt is WorkflowOutputEvent outputEvt)
    {
        results = (List<ChatMessage>)outputEvt.Data!;
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

var agentNames = new[] { "🔍 Investigador", "📣 Marketing", "⚖️ Legal" };
var agentIndex = 0;

foreach (var message in results)
{
    if (message.Role == ChatRole.Assistant)
    {
        var agentName = agentIndex < agentNames.Length ? agentNames[agentIndex] : $"Agente {agentIndex + 1}";
        
        Console.WriteLine();
        Console.WriteLine($"┌─── {agentName} ───");
        Console.WriteLine("│");
        
        var content = message.Text ?? "(sin contenido)";
        foreach (var line in content.Split('\n'))
        {
            Console.WriteLine($"│ {line}");
        }
        
        Console.WriteLine("│");
        Console.WriteLine("└────────────────────────────────────────────────────────────────");
        
        agentIndex++;
    }
}

// =============================================================================
// PASO 7: Análisis de rendimiento
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    ANÁLISIS DE RENDIMIENTO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

var totalTime = stopwatch.ElapsedMilliseconds;
var estimatedSequentialTime = totalTime * 3; // Estimación: si fueran secuenciales

Console.WriteLine($"⏱️  Tiempo CONCURRENTE (real):     {totalTime}ms");
Console.WriteLine($"⏱️  Tiempo SECUENCIAL (estimado): {estimatedSequentialTime}ms");
Console.WriteLine($"💨 Tiempo ahorrado (estimado):    {estimatedSequentialTime - totalTime}ms");
Console.WriteLine($"🚀 Factor de aceleración:         ~3x más rápido");
Console.WriteLine();

// =============================================================================
// PASO 8: Resumen del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ Los 3 agentes ejecutaron CONCURRENTEMENTE usando AgentWorkflowBuilder.BuildConcurrent()");
Console.WriteLine("✓ Cada agente aportó su perspectiva única al mismo problema");
Console.WriteLine("✓ Los resultados se agregaron automáticamente por el workflow");
Console.WriteLine("✓ Streaming de eventos permitió monitorear el progreso en tiempo real");
Console.WriteLine();
Console.WriteLine("📚 Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/concurrent");
Console.WriteLine();
