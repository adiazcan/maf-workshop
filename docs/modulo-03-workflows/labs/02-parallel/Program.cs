// =============================================================================
// Program.cs - Workflow Paralelo con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa ejecución paralela de 3 agentes 
// independientes: NewsAgent, WeatherAgent, StocksAgent. Los resultados 
// se agregan al final en una respuesta unificada.
//
// Conceptos demostrados:
// - Ejecución paralela con Task.WhenAll
// - Agentes independientes que no comparten estado
// - Agregación de resultados de múltiples fuentes
// - Medición de tiempo de ejecución paralela vs secuencial
// =============================================================================

using System.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Chat;
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
Console.WriteLine("           WORKFLOW PARALELO: News ║ Weather ║ Stocks");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear agentes especializados independientes
// =============================================================================

// Agente 1: Noticias - Obtiene titulares relevantes
var newsAgent = new ChatCompletionAgent(
    name: "NewsAgent",
    instructions: """
        Eres un agente de noticias. Tu trabajo es:
        1. Simular obtener los 3 titulares más relevantes del día
        2. Incluir noticias de tecnología, negocios y mundo
        3. Cada titular debe tener: título breve + resumen de 1 línea
        4. Usar formato de lista con viñetas
        
        Responde en español, de forma concisa.
        Simula datos realistas basados en tendencias actuales.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

// Agente 2: Clima - Obtiene información meteorológica
var weatherAgent = new ChatCompletionAgent(
    name: "WeatherAgent",
    instructions: """
        Eres un agente meteorológico. Tu trabajo es:
        1. Proporcionar el pronóstico actual para la ciudad solicitada
        2. Incluir: temperatura, condición (soleado/nublado/lluvia), humedad
        3. Agregar pronóstico para las próximas 24 horas
        4. Usar formato estructurado
        
        Responde en español, de forma concisa.
        Simula datos realistas basados en la época del año.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

// Agente 3: Acciones - Obtiene información bursátil
var stocksAgent = new ChatCompletionAgent(
    name: "StocksAgent",
    instructions: """
        Eres un agente de mercados financieros. Tu trabajo es:
        1. Proporcionar precio actual simulado del símbolo solicitado
        2. Incluir: precio, cambio del día (%), volumen
        3. Agregar un breve análisis de tendencia (alcista/bajista/neutral)
        4. Usar formato estructurado con emojis (📈 📉)
        
        Responde en español, de forma concisa.
        Simula datos realistas de mercado.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

Console.WriteLine("✓ NewsAgent creado - Especialista en noticias");
Console.WriteLine("✓ WeatherAgent creado - Especialista en clima");
Console.WriteLine("✓ StocksAgent creado - Especialista en mercados");
Console.WriteLine();

// =============================================================================
// PASO 3: Función auxiliar para invocar un agente
// =============================================================================

// Función helper que invoca un agente y retorna su resultado
async Task<(string AgentName, string Result, long ElapsedMs)> InvokeAgentAsync(
    ChatCompletionAgent agent, 
    string prompt)
{
    var stopwatch = Stopwatch.StartNew();
    var chat = new ChatHistory();
    chat.AddUserMessage(prompt);
    
    string result = "";
    await foreach (var message in agent.InvokeAsync(chat))
    {
        result += message.Content;
    }
    
    stopwatch.Stop();
    return (agent.Name ?? "Unknown", result, stopwatch.ElapsedMilliseconds);
}

// =============================================================================
// PASO 4: Ejecutar workflow PARALELO
// =============================================================================

Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ EJECUCIÓN PARALELA: Todos los agentes al mismo tiempo          │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
Console.WriteLine();

var parallelStopwatch = Stopwatch.StartNew();

// Crear tareas para ejecución paralela
var newsTask = InvokeAgentAsync(newsAgent, "Dame los 3 titulares más importantes de hoy");
var weatherTask = InvokeAgentAsync(weatherAgent, "Dame el pronóstico del clima para Madrid, España");
var stocksTask = InvokeAgentAsync(stocksAgent, "Dame información de la acción MSFT (Microsoft)");

// Esperar a que TODAS las tareas terminen en paralelo
// Task.WhenAll ejecuta las 3 tareas simultáneamente
var results = await Task.WhenAll(newsTask, weatherTask, stocksTask);

parallelStopwatch.Stop();

// =============================================================================
// PASO 5: Mostrar resultados individuales
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    RESULTADOS INDIVIDUALES");
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
// PASO 6: Agregar resultados en respuesta unificada
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("               RESPUESTA AGREGADA (BRIEFING DIARIO)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// Construir briefing agregado
var briefing = $"""
    📰 **NOTICIAS DEL DÍA**
    {results[0].Result}
    
    ☀️ **CLIMA EN MADRID**
    {results[1].Result}
    
    📊 **MERCADOS - MICROSOFT (MSFT)**
    {results[2].Result}
    """;

Console.WriteLine(briefing);

// =============================================================================
// PASO 7: Comparación de tiempos
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
var speedup = (double)totalSequentialTime / parallelTime;

Console.WriteLine("📊 Tiempos individuales de cada agente:");
foreach (var (agentName, _, elapsedMs) in results)
{
    Console.WriteLine($"   • {agentName}: {elapsedMs}ms");
}

Console.WriteLine();
Console.WriteLine($"⏱️  Tiempo PARALELO (real):        {parallelTime}ms");
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
Console.WriteLine("✓ Los 3 agentes ejecutaron en PARALELO usando Task.WhenAll");
Console.WriteLine("✓ Cada agente procesó su consulta de forma independiente");
Console.WriteLine("✓ Los resultados se agregaron en un briefing unificado");
Console.WriteLine($"✓ Ejecución paralela fue ~{speedup:F1}x más rápida que secuencial");
Console.WriteLine();
