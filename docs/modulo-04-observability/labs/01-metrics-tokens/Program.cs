// =============================================================================
// Lab 01: Métricas de Tokens y Latencia
// Módulo 4: Observabilidad - Workshop Microsoft Agent Framework
// =============================================================================
// 
// Este laboratorio demuestra cómo implementar métricas personalizadas para
// monitorear el comportamiento de agentes de IA, incluyendo:
// - Latencia de invocaciones
// - Conteo de errores
// - Uso de tokens (prompt y completion)
// - Exportación a consola y formato Prometheus
//
// =============================================================================

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                     CONFIGURACIÓN DEL HOST                                 ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     Lab 01: Métricas de Tokens y Latencia con OpenTelemetry      ║");
Console.WriteLine("║     Módulo 4: Observabilidad - Microsoft Agent Framework         ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var builder = Host.CreateApplicationBuilder(args);

// Cargar configuración desde appsettings.json y user secrets
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true);

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                 CONFIGURACIÓN DE OPENTELEMETRY                            ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

// Definir el recurso que identifica nuestra aplicación
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(
        serviceName: "workshop-maf-metrics-lab",
        serviceVersion: "1.0.0")
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "workshop",
        ["lab.module"] = "04-observability",
        ["lab.number"] = "01"
    });

// Configurar OpenTelemetry para métricas
var meterProviderBuilder = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("workshop-maf-metrics-lab", "1.0.0"))
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(resourceBuilder)
            // Registrar nuestro Meter personalizado para métricas de agentes
            .AddMeter("Workshop.MAF.Agents")
            // Agregar métricas del runtime de .NET (GC, threads, etc.)
            .AddRuntimeInstrumentation()
            // Agregar métricas de cliente HTTP (para llamadas a Azure OpenAI)
            .AddHttpClientInstrumentation()
            // Exportar a consola para visualización inmediata
            .AddConsoleExporter();

        // Nota: Para producción, podrías agregar Prometheus:
        // .AddPrometheusExporter();
    });

// Registrar el servicio de métricas personalizado
builder.Services.AddSingleton<AgentMetricsService>();

// Configurar logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                     DEMOSTRACIÓN DE MÉTRICAS                               ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

// Obtener servicios
var configuration = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var metricsService = host.Services.GetRequiredService<AgentMetricsService>();

// Obtener configuración de Azure OpenAI
var endpoint = configuration["AzureOpenAI:Endpoint"];
var deploymentName = configuration["AzureOpenAI:DeploymentName"];
var apiKey = configuration["AzureOpenAI:ApiKey"];

// Validar configuración
if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Error: Configuración de Azure OpenAI no encontrada.");
    Console.WriteLine();
    Console.WriteLine("Por favor, configure las credenciales usando user-secrets:");
    Console.WriteLine();
    Console.WriteLine("  dotnet user-secrets set \"AzureOpenAI:ApiKey\" \"tu-api-key\"");
    Console.WriteLine("  dotnet user-secrets set \"AzureOpenAI:Endpoint\" \"https://tu-recurso.openai.azure.com/\"");
    Console.ResetColor();
    return;
}

Console.WriteLine("📊 Iniciando demostración de métricas...");
Console.WriteLine($"   Endpoint: {endpoint}");
Console.WriteLine($"   Modelo: {deploymentName}");
Console.WriteLine();

// Crear cliente de Azure OpenAI
var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey));

var chatClient = client.GetChatClient(deploymentName);

// Preguntas de ejemplo para generar métricas
var questions = new[]
{
    "¿Cuál es la capital de España?",
    "Dame un dato curioso sobre inteligencia artificial.",
    "¿Qué es el patrón Observer en programación?",
    "Resume en una frase qué es OpenTelemetry.",
    "¿Cuánto es 2 + 2?"
};

Console.WriteLine($"📝 Ejecutando {questions.Length} consultas para generar métricas...");
Console.WriteLine(new string('─', 60));
Console.WriteLine();

foreach (var question in questions)
{
    Console.WriteLine($"💬 Pregunta: {question}");
    
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        // Registrar inicio de invocación
        metricsService.RecordInvocation();
        
        // Realizar llamada a Azure OpenAI
        var response = await chatClient.CompleteChatAsync(question);
        
        stopwatch.Stop();
        
        // Registrar métricas de éxito
        metricsService.RecordLatency(stopwatch.Elapsed.TotalMilliseconds);
        
        // Registrar uso de tokens
        var usage = response.Value.Usage;
        metricsService.RecordTokens(
            promptTokens: usage.InputTokenCount,
            completionTokens: usage.OutputTokenCount);
        
        // Mostrar respuesta resumida
        var responseText = response.Value.Content[0].Text;
        var truncatedResponse = responseText.Length > 100 
            ? responseText[..100] + "..." 
            : responseText;
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Respuesta ({stopwatch.ElapsedMilliseconds}ms): {truncatedResponse}");
        Console.ResetColor();
        Console.WriteLine($"   📈 Tokens - Entrada: {usage.InputTokenCount}, " +
                          $"Salida: {usage.OutputTokenCount}, " +
                          $"Total: {usage.TotalTokenCount}");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        
        // Registrar error
        metricsService.RecordError();
        metricsService.RecordLatency(stopwatch.Elapsed.TotalMilliseconds);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.ResetColor();
    }
    
    Console.WriteLine();
    
    // Pequeña pausa entre consultas
    await Task.Delay(500);
}

Console.WriteLine(new string('─', 60));
Console.WriteLine();
Console.WriteLine("📊 Resumen de métricas generadas:");
Console.WriteLine($"   • Total invocaciones: {metricsService.TotalInvocations}");
Console.WriteLine($"   • Total errores: {metricsService.TotalErrors}");
Console.WriteLine($"   • Tokens de entrada: {metricsService.TotalPromptTokens}");
Console.WriteLine($"   • Tokens de salida: {metricsService.TotalCompletionTokens}");
Console.WriteLine($"   • Tokens totales: {metricsService.TotalPromptTokens + metricsService.TotalCompletionTokens}");
Console.WriteLine();

Console.WriteLine("⏳ Forzando exportación de métricas a la consola...");
Console.WriteLine();

// Forzar el flush de las métricas antes de terminar
// Obtener el MeterProvider y hacer flush
var meterProvider = host.Services.GetService<MeterProvider>();
if (meterProvider != null)
{
    meterProvider.ForceFlush();
    await Task.Delay(2000); // Dar tiempo para que se impriman
}

// Mostrar métricas manualmente como alternativa
Console.WriteLine("📊 MÉTRICAS DE OPENTELEMETRY (Vista Resumida):");
Console.WriteLine(new string('═', 70));
Console.WriteLine();

Console.WriteLine("Export agent.invocations.total, Meter: Workshop.MAF.Agents/1.0.0");
Console.WriteLine($"Value: {metricsService.TotalInvocations}");
Console.WriteLine();

Console.WriteLine("Export agent.invocations.errors, Meter: Workshop.MAF.Agents/1.0.0");
Console.WriteLine($"Value: {metricsService.TotalErrors}");
Console.WriteLine();

Console.WriteLine("Export agent.latency, Meter: Workshop.MAF.Agents/1.0.0");
Console.WriteLine("Histogram (valores individuales registrados - ver logs de OTel para detalles)");
Console.WriteLine();

Console.WriteLine("Export agent.tokens.prompt, Meter: Workshop.MAF.Agents/1.0.0");
Console.WriteLine($"Value: {metricsService.TotalPromptTokens}");
Console.WriteLine();

Console.WriteLine("Export agent.tokens.completion, Meter: Workshop.MAF.Agents/1.0.0");
Console.WriteLine($"Value: {metricsService.TotalCompletionTokens}");
Console.WriteLine();

Console.WriteLine(new string('═', 70));

Console.WriteLine();
Console.WriteLine("✅ Laboratorio completado.");
Console.WriteLine();
Console.WriteLine("📚 Próximos pasos:");
Console.WriteLine("   • Revisa las métricas exportadas arriba");
Console.WriteLine("   • Las métricas están siendo enviadas a OpenTelemetry en segundo plano");
Console.WriteLine("   • Continúa con Lab 02 para aprender sobre trazas distribuidas");

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                    SERVICIO DE MÉTRICAS PERSONALIZADO                      ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Servicio que encapsula las métricas personalizadas para agentes de IA.
/// Utiliza System.Diagnostics.Metrics que es compatible con OpenTelemetry.
/// </summary>
public class AgentMetricsService
{
    // El Meter es el contenedor de métricas - similar a un namespace
    private readonly Meter _meter;
    
    // Contadores para valores que solo aumentan
    private readonly Counter<long> _invocationsCounter;
    private readonly Counter<long> _errorsCounter;
    private readonly Counter<long> _promptTokensCounter;
    private readonly Counter<long> _completionTokensCounter;
    
    // Histograma para distribución de valores (percentiles)
    private readonly Histogram<double> _latencyHistogram;
    
    // Variables para tracking local (solo para mostrar en consola)
    public long TotalInvocations { get; private set; }
    public long TotalErrors { get; private set; }
    public long TotalPromptTokens { get; private set; }
    public long TotalCompletionTokens { get; private set; }
    
    public AgentMetricsService()
    {
        // Crear el Meter con nombre único y versión
        // Este nombre debe coincidir con AddMeter() en la configuración de OTel
        _meter = new Meter("Workshop.MAF.Agents", "1.0.0");
        
        // Contador de invocaciones totales
        _invocationsCounter = _meter.CreateCounter<long>(
            name: "agent.invocations.total",
            unit: "invocations",
            description: "Número total de invocaciones del agente");
        
        // Contador de errores
        _errorsCounter = _meter.CreateCounter<long>(
            name: "agent.invocations.errors",
            unit: "errors",
            description: "Número de invocaciones que resultaron en error");
        
        // Histograma de latencia - permite calcular percentiles (p50, p95, p99)
        _latencyHistogram = _meter.CreateHistogram<double>(
            name: "agent.latency",
            unit: "ms",
            description: "Tiempo de respuesta del agente en milisegundos");
        
        // Contadores de tokens
        _promptTokensCounter = _meter.CreateCounter<long>(
            name: "agent.tokens.prompt",
            unit: "tokens",
            description: "Tokens de entrada (prompt) consumidos");
        
        _completionTokensCounter = _meter.CreateCounter<long>(
            name: "agent.tokens.completion",
            unit: "tokens",
            description: "Tokens de salida (completion) generados");
    }
    
    /// <summary>
    /// Registra una invocación del agente.
    /// </summary>
    public void RecordInvocation()
    {
        _invocationsCounter.Add(1);
        TotalInvocations++;
    }
    
    /// <summary>
    /// Registra un error en la invocación del agente.
    /// </summary>
    public void RecordError()
    {
        _errorsCounter.Add(1);
        TotalErrors++;
    }
    
    /// <summary>
    /// Registra la latencia de una invocación.
    /// </summary>
    /// <param name="milliseconds">Tiempo de respuesta en milisegundos.</param>
    public void RecordLatency(double milliseconds)
    {
        _latencyHistogram.Record(milliseconds);
    }
    
    /// <summary>
    /// Registra el uso de tokens de una invocación.
    /// </summary>
    /// <param name="promptTokens">Tokens de entrada consumidos.</param>
    /// <param name="completionTokens">Tokens de salida generados.</param>
    public void RecordTokens(long promptTokens, long completionTokens)
    {
        _promptTokensCounter.Add(promptTokens);
        _completionTokensCounter.Add(completionTokens);
        
        TotalPromptTokens += promptTokens;
        TotalCompletionTokens += completionTokens;
    }
}
