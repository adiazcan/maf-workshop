// =============================================================================
// Program.cs - Workflow Concurrente con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa ejecución concurrente donde múltiples 
// agentes trabajan en la misma tarea simultáneamente, cada uno aportando
// su perspectiva única.
//
// Referencia oficial:
// https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/concurrent
//
// Conceptos demostrados:
// - AIAgent: Agentes especializados con Microsoft Agent Framework
// - Task.WhenAll: Ejecución paralela de múltiples agentes
// - Agregación de resultados de perspectivas diversas
// - Medición de tiempo concurrente vs secuencial
// =============================================================================

using System.Diagnostics;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

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
Console.WriteLine("           Usando Microsoft Agent Framework (MAF)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear cliente de Azure OpenAI
// =============================================================================

// Usar DefaultAzureCredential para autenticación (recomendado para desarrollo)
// En producción, esto usará Managed Identity automáticamente
var openAIClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential());

Console.WriteLine("✓ Cliente Azure OpenAI configurado con DefaultAzureCredential");
Console.WriteLine($"  Endpoint: {endpoint}");
Console.WriteLine($"  Modelo: {deploymentName}");
Console.WriteLine();

// =============================================================================
// PASO 3: Definir agentes especializados para ejecución concurrente
// =============================================================================
// Cada agente representa una perspectiva diferente analizando el mismo problema.
// Usamos CreateAIAgent() de Microsoft.Agents.AI.OpenAI para crear agentes MAF.
// =============================================================================

// Agente 1: Analista de Mercado - Perspectiva de investigación
var researcherAgent = openAIClient
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "ResearcherAgent",
        instructions: """
            Eres un experto investigador de mercado y productos. Dado un tema o prompt:
            1. Proporciona insights concisos y basados en hechos
            2. Identifica oportunidades de mercado
            3. Señala riesgos potenciales
            4. Usa datos y tendencias actuales
            
            Responde en español, de forma estructurada y profesional.
            Máximo 150 palabras.
            """);

// Agente 2: Estratega de Marketing - Perspectiva creativa
var marketerAgent = openAIClient
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "MarketerAgent",
        instructions: """
            Eres un estratega de marketing creativo. Dado un tema o prompt:
            1. Crea propuestas de valor convincentes
            2. Define mensajes para el público objetivo
            3. Sugiere canales de comunicación
            4. Incluye un slogan o tagline
            
            Responde en español, de forma creativa y orientada a la acción.
            Máximo 150 palabras.
            """);

// Agente 3: Asesor Legal - Perspectiva de cumplimiento
var legalAgent = openAIClient
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "LegalAgent",
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
Console.WriteLine("   • ResearcherAgent - Análisis de mercado y oportunidades");
Console.WriteLine("   • MarketerAgent - Estrategia creativa y mensajes");
Console.WriteLine("   • LegalAgent - Cumplimiento y regulaciones");
Console.WriteLine();

// =============================================================================
// PASO 4: Definir función helper para invocar un agente
// =============================================================================
// Esta función encapsula la lógica de invocación y mide el tiempo de ejecución.
// =============================================================================

async Task<(string AgentName, string Result, long ElapsedMs)> InvokeAgentAsync(
    AIAgent agent,
    string agentName, 
    string prompt)
{
    var stopwatch = Stopwatch.StartNew();
    
    // Crear mensajes para el agente
    var messages = new List<ChatMessage>
    {
        new UserChatMessage(prompt)
    };
    
    // Invocar al agente usando RunStreamingAsync de MAF
    string result = "";
    await foreach (var update in agent.RunStreamingAsync(messages))
    {
        result += update;
    }
    
    stopwatch.Stop();
    return (agentName, result, stopwatch.ElapsedMilliseconds);
}

// =============================================================================
// PASO 5: Ejecutar workflow CONCURRENTE
// =============================================================================

Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ EJECUCIÓN CONCURRENTE: Todos los agentes al mismo tiempo       │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
Console.WriteLine();

// Definir el prompt que todos los agentes procesarán
var userPrompt = "Estamos lanzando una nueva bicicleta eléctrica económica para commuters urbanos en México.";

Console.WriteLine($"📝 Prompt: \"{userPrompt}\"");
Console.WriteLine();

// Medir tiempo de ejecución total
var parallelStopwatch = Stopwatch.StartNew();

// Crear tareas para ejecución CONCURRENTE
// Task.WhenAll ejecuta las 3 tareas simultáneamente
var researcherTask = InvokeAgentAsync(researcherAgent, "🔍 Investigador", userPrompt);
var marketerTask = InvokeAgentAsync(marketerAgent, "📣 Marketing", userPrompt);
var legalTask = InvokeAgentAsync(legalAgent, "⚖️ Legal", userPrompt);

// Esperar a que TODAS las tareas terminen en paralelo
var results = await Task.WhenAll(researcherTask, marketerTask, legalTask);

parallelStopwatch.Stop();

// =============================================================================
// PASO 6: Mostrar resultados de cada agente
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("            RESULTADOS DE TODOS LOS AGENTES");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");

foreach (var (agentName, result, elapsedMs) in results)
{
    Console.WriteLine();
    Console.WriteLine($"┌─── {agentName} ({elapsedMs}ms) ───");
    Console.WriteLine("│");
    foreach (var line in result.Split('\n'))
    {
        Console.WriteLine($"│ {line}");
    }
    Console.WriteLine("│");
    Console.WriteLine("└────────────────────────────────────────────────────────────────");
}

// =============================================================================
// PASO 7: Análisis de rendimiento
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    ANÁLISIS DE RENDIMIENTO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// Calcular tiempo que hubiera tomado ejecutar secuencialmente
var totalSequentialTime = results.Sum(r => r.ElapsedMs);
var parallelTime = parallelStopwatch.ElapsedMilliseconds;
var timeSaved = totalSequentialTime - parallelTime;
var speedup = totalSequentialTime > 0 ? (double)totalSequentialTime / parallelTime : 1;

Console.WriteLine("📊 Tiempos individuales de cada agente:");
foreach (var (agentName, _, elapsedMs) in results)
{
    Console.WriteLine($"   • {agentName}: {elapsedMs}ms");
}

Console.WriteLine();
Console.WriteLine($"⏱️  Tiempo CONCURRENTE (real):     {parallelTime}ms");
Console.WriteLine($"⏱️  Tiempo SECUENCIAL (estimado): {totalSequentialTime}ms");
Console.WriteLine($"💨 Tiempo ahorrado:               {timeSaved}ms");
Console.WriteLine($"🚀 Factor de aceleración:         {speedup:F2}x más rápido");
Console.WriteLine();

// =============================================================================
// PASO 8: Resumen del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ Los 3 agentes ejecutaron CONCURRENTEMENTE usando Task.WhenAll");
Console.WriteLine("✓ Cada agente aportó su perspectiva única al mismo problema");
Console.WriteLine("✓ Los resultados se recolectaron después de que todos terminaron");
Console.WriteLine($"✓ Ejecución concurrente fue ~{speedup:F1}x más rápida que secuencial");
Console.WriteLine();
Console.WriteLine("📚 Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/concurrent");
Console.WriteLine();
