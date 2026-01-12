// =============================================================================
// Lab 03: Integración con Azure Monitor
// Módulo 4: Observabilidad - Workshop Microsoft Agent Framework
// =============================================================================
// 
// Este laboratorio demuestra cómo exportar telemetría a Azure Monitor
// Application Insights, incluyendo:
// - Configuración del exportador de Azure Monitor
// - Logging estructurado con redacción de PII
// - Métricas y trazas enviadas a Application Insights
// - Preparación para dashboards y alertas
//
// =============================================================================

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                     CONFIGURACIÓN DEL HOST                                 ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║        Lab 03: Integración con Azure Monitor                     ║");
Console.WriteLine("║        Módulo 4: Observabilidad - Microsoft Agent Framework      ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var builder = Host.CreateApplicationBuilder(args);

// Cargar configuración
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true);

// Obtener connection string de Application Insights
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
var useAzureMonitor = !string.IsNullOrEmpty(appInsightsConnectionString) && 
                       !appInsightsConnectionString.Contains("YOUR-KEY");

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║           CONFIGURACIÓN DE OPENTELEMETRY CON AZURE MONITOR                 ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

// Definir el recurso que identifica nuestra aplicación
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(
        serviceName: "workshop-maf-azure-monitor",
        serviceVersion: "1.0.0",
        serviceInstanceId: Environment.MachineName)
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "workshop",
        ["lab.module"] = "04-observability",
        ["lab.number"] = "03",
        ["cloud.provider"] = "azure"
    });

// Configurar OpenTelemetry
var otelBuilder = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("workshop-maf-azure-monitor", "1.0.0"));

// Configurar Métricas
otelBuilder.WithMetrics(metrics =>
{
    metrics
        .SetResourceBuilder(resourceBuilder)
        .AddMeter("Workshop.MAF.Agents")
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation();
    
    if (useAzureMonitor)
    {
        // Exportar a Azure Monitor
        metrics.AddAzureMonitorMetricExporter(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
        Console.WriteLine("📊 Métricas: Exportando a Azure Monitor");
    }
    else
    {
        // Fallback a consola para desarrollo local
        metrics.AddConsoleExporter();
        Console.WriteLine("📊 Métricas: Exportando a consola (modo desarrollo)");
    }
});

// Configurar Trazas
otelBuilder.WithTracing(tracing =>
{
    tracing
        .SetResourceBuilder(resourceBuilder)
        .AddSource("Workshop.MAF.Agents")
        .AddHttpClientInstrumentation();
    
    if (useAzureMonitor)
    {
        tracing.AddAzureMonitorTraceExporter(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
        Console.WriteLine("🔗 Trazas: Exportando a Azure Monitor");
    }
    else
    {
        tracing.AddConsoleExporter();
        Console.WriteLine("🔗 Trazas: Exportando a consola (modo desarrollo)");
    }
});

// Configurar Logging con OpenTelemetry
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    
    if (useAzureMonitor)
    {
        logging.AddAzureMonitorLogExporter(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
        Console.WriteLine("📝 Logs: Exportando a Azure Monitor");
    }
    else
    {
        logging.AddConsoleExporter();
        Console.WriteLine("📝 Logs: Exportando a consola (modo desarrollo)");
    }
});

// Registrar servicios
builder.Services.AddSingleton<AgentMetrics>();
builder.Services.AddSingleton<PiiRedactor>();
builder.Services.AddSingleton<ObservableAgentService>();

var host = builder.Build();

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                    DEMOSTRACIÓN CON AZURE MONITOR                          ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

var configuration = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var metrics = host.Services.GetRequiredService<AgentMetrics>();
var piiRedactor = host.Services.GetRequiredService<PiiRedactor>();
var agentService = host.Services.GetRequiredService<ObservableAgentService>();

Console.WriteLine();
Console.WriteLine(new string('═', 60));
Console.WriteLine();

// Validar configuración de Azure OpenAI
var endpoint = configuration["AzureOpenAI:Endpoint"];
var apiKey = configuration["AzureOpenAI:ApiKey"];

if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Error: Configuración de Azure OpenAI no encontrada.");
    Console.WriteLine();
    Console.WriteLine("Por favor, configure las credenciales:");
    Console.WriteLine("  dotnet user-secrets set \"AzureOpenAI:ApiKey\" \"tu-api-key\"");
    Console.WriteLine("  dotnet user-secrets set \"AzureOpenAI:Endpoint\" \"https://tu-recurso.openai.azure.com/\"");
    Console.ResetColor();
    return;
}

// Inicializar el servicio del agente
agentService.Initialize(
    endpoint, 
    apiKey, 
    configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4");

Console.WriteLine("🚀 Ejecutando demostración con telemetría completa...");
Console.WriteLine();

// Simular interacciones del usuario
var userInteractions = new[]
{
    ("user-001", "¿Cuál es la capital de Francia?"),
    ("user-002", "Mi email es juan.perez@example.com y necesito ayuda"),
    ("user-001", "Dame información sobre el clima"),
    ("user-003", "Explica qué es la inteligencia artificial"),
    ("invalid-query", ""), // Esto generará un error para demostrar manejo
};

foreach (var (userId, query) in userInteractions)
{
    Console.WriteLine($"👤 Usuario: {userId}");
    
    // Redactar PII del query para logging seguro
    var safeQuery = piiRedactor.RedactPii(query);
    Console.WriteLine($"📝 Query (redactado): {safeQuery}");
    
    // Log estructurado con contexto
    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["UserId"] = userId,
        ["QueryHash"] = piiRedactor.HashString(query),
        ["SessionId"] = Guid.NewGuid().ToString("N")[..8]
    }))
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query vacía no permitida");
            }
            
            var result = await agentService.ProcessQueryAsync(userId, query);
            
            // Truncar para mostrar
            var displayResult = result.Length > 100 ? result[..100] + "..." : result;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Respuesta: {displayResult}");
            Console.ResetColor();
            
            logger.LogInformation(
                "Query procesada exitosamente para usuario {UserId}",
                userId);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
            
            // Log de error estructurado
            logger.LogError(ex,
                "Error procesando query para usuario {UserId}: {ErrorType}",
                userId, ex.GetType().Name);
            
            metrics.RecordError();
        }
    }
    
    Console.WriteLine();
    await Task.Delay(1000);
}

// Mostrar resumen
Console.WriteLine(new string('═', 60));
Console.WriteLine();
Console.WriteLine("📊 Resumen de Telemetría Generada:");
Console.WriteLine($"   • Total invocaciones: {metrics.TotalInvocations}");
Console.WriteLine($"   • Total errores: {metrics.TotalErrors}");
Console.WriteLine($"   • Tokens consumidos: {metrics.TotalTokens}");
Console.WriteLine();

if (useAzureMonitor)
{
    Console.WriteLine("🔍 Para ver los datos en Azure Monitor:");
    Console.WriteLine("   1. Abre el portal de Azure (portal.azure.com)");
    Console.WriteLine("   2. Navega a tu recurso de Application Insights");
    Console.WriteLine("   3. Ve a 'Logs' para ejecutar queries KQL");
    Console.WriteLine("   4. Ve a 'Métricas' para ver gráficos");
    Console.WriteLine("   5. Ve a 'Mapa de aplicación' para ver trazas");
    Console.WriteLine();
    Console.WriteLine("📋 Queries KQL de ejemplo:");
    Console.WriteLine();
    Console.WriteLine("   // Latencia por usuario");
    Console.WriteLine("   customMetrics");
    Console.WriteLine("   | where name == 'agent.latency'");
    Console.WriteLine("   | summarize avg(value) by tostring(customDimensions.user_id)");
    Console.WriteLine();
    Console.WriteLine("   // Errores en la última hora");
    Console.WriteLine("   traces");
    Console.WriteLine("   | where severityLevel >= 3");
    Console.WriteLine("   | where timestamp > ago(1h)");
    Console.WriteLine("   | summarize count() by bin(timestamp, 5m)");
}
else
{
    Console.WriteLine("💡 Para exportar a Azure Monitor:");
    Console.WriteLine("   1. Crea un recurso Application Insights en Azure");
    Console.WriteLine("   2. Copia el Connection String");
    Console.WriteLine("   3. Configura:");
    Console.WriteLine("      dotnet user-secrets set \"ApplicationInsights:ConnectionString\" \"tu-connection-string\"");
}

Console.WriteLine();
Console.WriteLine("⏳ Esperando exportación de telemetría (5 segundos)...");
await Task.Delay(5000);

Console.WriteLine();
Console.WriteLine("✅ Laboratorio completado.");

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                    CLASES DE SOPORTE                                       ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Servicio de métricas para agentes con etiquetas enriquecidas.
/// </summary>
public class AgentMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _invocationsCounter;
    private readonly Counter<long> _errorsCounter;
    private readonly Counter<long> _tokensCounter;
    private readonly Histogram<double> _latencyHistogram;
    
    public long TotalInvocations { get; private set; }
    public long TotalErrors { get; private set; }
    public long TotalTokens { get; private set; }
    
    public AgentMetrics()
    {
        _meter = new Meter("Workshop.MAF.Agents", "1.0.0");
        
        _invocationsCounter = _meter.CreateCounter<long>(
            "agent.invocations.total",
            description: "Total de invocaciones del agente");
        
        _errorsCounter = _meter.CreateCounter<long>(
            "agent.invocations.errors",
            description: "Total de errores en invocaciones");
        
        _tokensCounter = _meter.CreateCounter<long>(
            "agent.tokens.total",
            unit: "tokens",
            description: "Total de tokens consumidos");
        
        _latencyHistogram = _meter.CreateHistogram<double>(
            "agent.latency",
            unit: "ms",
            description: "Latencia de invocaciones del agente");
    }
    
    public void RecordInvocation(string userId, string agentName)
    {
        _invocationsCounter.Add(1,
            new KeyValuePair<string, object?>("user_id", userId),
            new KeyValuePair<string, object?>("agent_name", agentName));
        TotalInvocations++;
    }
    
    public void RecordError()
    {
        _errorsCounter.Add(1);
        TotalErrors++;
    }
    
    public void RecordLatency(double milliseconds, string userId)
    {
        _latencyHistogram.Record(milliseconds,
            new KeyValuePair<string, object?>("user_id", userId));
    }
    
    public void RecordTokens(long tokens, string model)
    {
        _tokensCounter.Add(tokens,
            new KeyValuePair<string, object?>("model", model));
        TotalTokens += tokens;
    }
}

/// <summary>
/// Utilidad para redactar información personal identificable (PII).
/// </summary>
public class PiiRedactor
{
    // Patrones comunes de PII
    private static readonly System.Text.RegularExpressions.Regex EmailRegex = 
        new(@"\b[\w\.-]+@[\w\.-]+\.\w{2,}\b", System.Text.RegularExpressions.RegexOptions.Compiled);
    
    private static readonly System.Text.RegularExpressions.Regex PhoneRegex = 
        new(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", System.Text.RegularExpressions.RegexOptions.Compiled);
    
    private static readonly System.Text.RegularExpressions.Regex CreditCardRegex = 
        new(@"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", System.Text.RegularExpressions.RegexOptions.Compiled);
    
    /// <summary>
    /// Redacta PII común de un texto.
    /// </summary>
    public string RedactPii(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        var result = text;
        result = EmailRegex.Replace(result, "[EMAIL_REDACTED]");
        result = PhoneRegex.Replace(result, "[PHONE_REDACTED]");
        result = CreditCardRegex.Replace(result, "[CC_REDACTED]");
        
        return result;
    }
    
    /// <summary>
    /// Genera un hash seguro de un string para correlación sin exponer el contenido.
    /// </summary>
    public string HashString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "empty";
        
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }
}

/// <summary>
/// Servicio de agente con observabilidad completa.
/// </summary>
public class ObservableAgentService
{
    private static readonly ActivitySource ActivitySource = 
        new("Workshop.MAF.Agents", "1.0.0");
    
    private readonly AgentMetrics _metrics;
    private readonly PiiRedactor _piiRedactor;
    private readonly ILogger<ObservableAgentService> _logger;
    
    private ChatClient? _chatClient;
    private string _deploymentName = "";
    
    public ObservableAgentService(
        AgentMetrics metrics, 
        PiiRedactor piiRedactor,
        ILogger<ObservableAgentService> logger)
    {
        _metrics = metrics;
        _piiRedactor = piiRedactor;
        _logger = logger;
    }
    
    public void Initialize(string endpoint, string apiKey, string deploymentName)
    {
        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deploymentName);
        _deploymentName = deploymentName;
    }
    
    public async Task<string> ProcessQueryAsync(string userId, string query)
    {
        using var activity = ActivitySource.StartActivity(
            "Agent.ProcessQuery",
            ActivityKind.Server);
        
        activity?.SetTag("user.id", userId);
        activity?.SetTag("query.hash", _piiRedactor.HashString(query));
        activity?.SetTag("query.length", query.Length);
        activity?.SetTag("model", _deploymentName);
        
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation(
            "Procesando query para usuario {UserId}, longitud: {QueryLength}",
            userId, query.Length);
        
        try
        {
            _metrics.RecordInvocation(userId, "MainAgent");
            
            var response = await _chatClient!.CompleteChatAsync(query);
            
            stopwatch.Stop();
            
            var usage = response.Value.Usage;
            var totalTokens = usage.InputTokenCount + usage.OutputTokenCount;
            
            // Registrar métricas
            _metrics.RecordLatency(stopwatch.Elapsed.TotalMilliseconds, userId);
            _metrics.RecordTokens(totalTokens, _deploymentName);
            
            // Añadir info al span
            activity?.SetTag("llm.prompt_tokens", usage.InputTokenCount);
            activity?.SetTag("llm.completion_tokens", usage.OutputTokenCount);
            activity?.SetTag("llm.total_tokens", totalTokens);
            activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogInformation(
                "Query completada en {DurationMs}ms, tokens: {Tokens}",
                stopwatch.ElapsedMilliseconds, totalTokens);
            
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
            
            _logger.LogError(ex,
                "Error procesando query: {ErrorType}",
                ex.GetType().Name);
            
            throw;
        }
    }
}
