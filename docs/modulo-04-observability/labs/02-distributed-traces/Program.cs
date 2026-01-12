// =============================================================================
// Lab 02: Trazas Distribuidas (Distributed Tracing)
// Módulo 4: Observabilidad - Workshop Microsoft Agent Framework
// =============================================================================
// 
// Este laboratorio demuestra cómo implementar trazas distribuidas para
// visualizar el flujo de solicitudes a través de múltiples agentes:
// - Crear spans padre e hijo para representar jerarquías de llamadas
// - Propagar contexto entre agentes
// - Añadir atributos personalizados a las trazas
// - Visualizar la estructura de trazas en consola
//
// =============================================================================

using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                     CONFIGURACIÓN DEL HOST                                 ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║       Lab 02: Trazas Distribuidas con OpenTelemetry              ║");
Console.WriteLine("║       Módulo 4: Observabilidad - Microsoft Agent Framework       ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var builder = Host.CreateApplicationBuilder(args);

// Cargar configuración
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true);

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║              CONFIGURACIÓN DE OPENTELEMETRY TRACING                        ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

// Definir el recurso que identifica nuestra aplicación
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(
        serviceName: "workshop-maf-tracing-lab",
        serviceVersion: "1.0.0")
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "workshop",
        ["lab.module"] = "04-observability",
        ["lab.number"] = "02"
    });

// Configurar OpenTelemetry para trazas
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("workshop-maf-tracing-lab", "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(resourceBuilder)
            // Registrar nuestro ActivitySource para trazas de agentes
            .AddSource("Workshop.MAF.Agents")
            // Registrar ActivitySource para el workflow
            .AddSource("Workshop.MAF.Workflow")
            // Auto-instrumentación de llamadas HTTP (Azure OpenAI)
            .AddHttpClientInstrumentation(options =>
            {
                // Enriquecer spans HTTP con información adicional
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    activity.SetTag("http.request.host", request.RequestUri?.Host);
                };
                options.EnrichWithHttpResponseMessage = (activity, response) =>
                {
                    activity.SetTag("http.response.status_code", (int)response.StatusCode);
                };
            })
            // Exportar a consola para visualización
            .AddConsoleExporter();
    });

// Registrar servicios
builder.Services.AddSingleton<WeatherAgentService>();
builder.Services.AddSingleton<NewsAgentService>();
builder.Services.AddSingleton<WorkflowOrchestrator>();

builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                    DEMOSTRACIÓN DE TRAZAS                                  ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

var configuration = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var orchestrator = host.Services.GetRequiredService<WorkflowOrchestrator>();

// Validar configuración
var endpoint = configuration["AzureOpenAI:Endpoint"];
var apiKey = configuration["AzureOpenAI:ApiKey"];

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

// Inicializar el orquestador
orchestrator.Initialize(endpoint, apiKey, configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4");

Console.WriteLine("🔗 Iniciando demostración de trazas distribuidas...");
Console.WriteLine();
Console.WriteLine("📋 Este laboratorio simula un workflow multi-agente:");
Console.WriteLine("   1. Usuario hace una consulta compleja");
Console.WriteLine("   2. Orquestador crea un span padre");
Console.WriteLine("   3. WeatherAgent y NewsAgent crean spans hijos");
Console.WriteLine("   4. Cada llamada a Azure OpenAI crea sub-spans");
Console.WriteLine();
Console.WriteLine(new string('═', 60));
Console.WriteLine();

// Ejecutar el workflow
var query = "Dame el clima y las noticias destacadas de Madrid";
Console.WriteLine($"💬 Consulta del usuario: \"{query}\"");
Console.WriteLine();

try
{
    var result = await orchestrator.ProcessQueryAsync(query);
    
    Console.WriteLine();
    Console.WriteLine(new string('─', 60));
    Console.WriteLine("📊 Resultado del Workflow:");
    Console.WriteLine(new string('─', 60));
    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ Error en el workflow: {ex.Message}");
    Console.ResetColor();
}

Console.WriteLine();
Console.WriteLine(new string('═', 60));
Console.WriteLine();
Console.WriteLine("⏳ Esperando exportación de trazas a consola...");
Console.WriteLine("   (Las trazas se muestran en formato jerárquico)");
Console.WriteLine();

// Esperar para que las trazas se exporten
await Task.Delay(3000);

Console.WriteLine("✅ Laboratorio completado.");
Console.WriteLine();
Console.WriteLine("📝 Observa en la salida de trazas:");
Console.WriteLine("   • Activity.TraceId: Identificador único de la traza completa");
Console.WriteLine("   • Activity.SpanId: Identificador único de cada span");
Console.WriteLine("   • Activity.ParentId: Referencia al span padre (jerarquía)");
Console.WriteLine("   • Activity.Tags: Atributos personalizados que añadimos");
Console.WriteLine();
Console.WriteLine("🔍 Estructura esperada de la traza:");
Console.WriteLine("   └─ Workflow.ProcessQuery (span padre)");
Console.WriteLine("      ├─ WeatherAgent.GetWeather (span hijo)");
Console.WriteLine("      │  └─ HTTP GET (auto-instrumentado)");
Console.WriteLine("      └─ NewsAgent.GetNews (span hijo)");
Console.WriteLine("         └─ HTTP GET (auto-instrumentado)");

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                    ORQUESTADOR DEL WORKFLOW                                ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Orquestador que coordina múltiples agentes para procesar consultas complejas.
/// Demuestra cómo crear spans padre que contienen spans hijos de otros agentes.
/// </summary>
public class WorkflowOrchestrator
{
    // ActivitySource para trazas del workflow
    private static readonly ActivitySource ActivitySource = 
        new("Workshop.MAF.Workflow", "1.0.0");
    
    private readonly WeatherAgentService _weatherAgent;
    private readonly NewsAgentService _newsAgent;
    private readonly ILogger<WorkflowOrchestrator> _logger;
    
    private string _endpoint = "";
    private string _apiKey = "";
    private string _deploymentName = "";
    
    public WorkflowOrchestrator(
        WeatherAgentService weatherAgent,
        NewsAgentService newsAgent,
        ILogger<WorkflowOrchestrator> logger)
    {
        _weatherAgent = weatherAgent;
        _newsAgent = newsAgent;
        _logger = logger;
    }
    
    public void Initialize(string endpoint, string apiKey, string deploymentName)
    {
        _endpoint = endpoint;
        _apiKey = apiKey;
        _deploymentName = deploymentName;
        
        _weatherAgent.Initialize(endpoint, apiKey, deploymentName);
        _newsAgent.Initialize(endpoint, apiKey, deploymentName);
    }
    
    /// <summary>
    /// Procesa una consulta del usuario coordinando múltiples agentes.
    /// </summary>
    public async Task<string> ProcessQueryAsync(string query)
    {
        // Crear span padre para todo el workflow
        // Este span contendrá todos los spans hijos de los agentes
        using var activity = ActivitySource.StartActivity(
            name: "Workflow.ProcessQuery",
            kind: ActivityKind.Server);
        
        // Generar un ID de correlación para tracking
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        
        // Añadir atributos al span padre
        activity?.SetTag("workflow.correlation_id", correlationId);
        activity?.SetTag("workflow.query_length", query.Length);
        activity?.SetTag("workflow.agents_count", 2);
        
        // Usar baggage para propagar contexto a spans hijos
        activity?.SetBaggage("correlation_id", correlationId);
        
        _logger.LogInformation(
            "Iniciando workflow {CorrelationId} con query de {Length} caracteres",
            correlationId, query.Length);
        
        try
        {
            // Ejecutar agentes en paralelo (ambos generan spans hijos)
            Console.WriteLine("⚡ Ejecutando agentes en paralelo...");
            Console.WriteLine();
            
            var weatherTask = _weatherAgent.GetWeatherAsync("Madrid");
            var newsTask = _newsAgent.GetNewsAsync("Madrid");
            
            // Esperar a que ambos completen
            await Task.WhenAll(weatherTask, newsTask);
            
            var weatherResult = await weatherTask;
            var newsResult = await newsTask;
            
            // Combinar resultados
            var combinedResult = $"""
                🌤️ CLIMA EN MADRID:
                {weatherResult}
                
                📰 NOTICIAS DE MADRID:
                {newsResult}
                """;
            
            // Marcar el span como exitoso
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("workflow.status", "success");
            
            _logger.LogInformation(
                "Workflow {CorrelationId} completado exitosamente",
                correlationId);
            
            return combinedResult;
        }
        catch (Exception ex)
        {
            // Marcar el span con error
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("workflow.status", "error");
            activity?.SetTag("error.type", ex.GetType().Name);
            
            _logger.LogError(ex, 
                "Error en workflow {CorrelationId}: {Message}",
                correlationId, ex.Message);
            
            throw;
        }
    }
}

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                      AGENTE DE CLIMA                                       ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Agente especializado en obtener información del clima.
/// Crea spans hijos dentro del span del workflow padre.
/// </summary>
public class WeatherAgentService
{
    private static readonly ActivitySource ActivitySource = 
        new("Workshop.MAF.Agents", "1.0.0");
    
    private ChatClient? _chatClient;
    private readonly ILogger<WeatherAgentService> _logger;
    
    public WeatherAgentService(ILogger<WeatherAgentService> logger)
    {
        _logger = logger;
    }
    
    public void Initialize(string endpoint, string apiKey, string deploymentName)
    {
        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deploymentName);
    }
    
    public async Task<string> GetWeatherAsync(string city)
    {
        // Crear span hijo - automáticamente se enlaza al span padre activo
        using var activity = ActivitySource.StartActivity(
            name: "WeatherAgent.GetWeather",
            kind: ActivityKind.Client);
        
        // Obtener correlation_id del baggage propagado
        var correlationId = Activity.Current?.GetBaggageItem("correlation_id") ?? "unknown";
        
        // Añadir atributos específicos del agente de clima
        activity?.SetTag("agent.name", "WeatherAgent");
        activity?.SetTag("agent.operation", "get_weather");
        activity?.SetTag("weather.city", city);
        activity?.SetTag("correlation_id", correlationId);
        
        Console.WriteLine($"   🌤️ WeatherAgent: Consultando clima para {city}...");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var prompt = $"Describe brevemente el clima actual en {city}. " +
                        "Responde en máximo 2 oraciones en español.";
            
            // La llamada HTTP generará automáticamente un sub-span
            var response = await _chatClient!.CompleteChatAsync(prompt);
            
            stopwatch.Stop();
            
            // Registrar métricas en el span
            activity?.SetTag("llm.prompt_tokens", response.Value.Usage.InputTokenCount);
            activity?.SetTag("llm.completion_tokens", response.Value.Usage.OutputTokenCount);
            activity?.SetTag("agent.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            var result = response.Value.Content[0].Text;
            Console.WriteLine($"   ✅ WeatherAgent completado en {stopwatch.ElapsedMilliseconds}ms");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.message", ex.Message);
            
            _logger.LogError(ex, "Error en WeatherAgent para {City}", city);
            throw;
        }
    }
}

// ╔═══════════════════════════════════════════════════════════════════════════╗
// ║                      AGENTE DE NOTICIAS                                    ║
// ╚═══════════════════════════════════════════════════════════════════════════╝

/// <summary>
/// Agente especializado en obtener noticias.
/// Crea spans hijos dentro del span del workflow padre.
/// </summary>
public class NewsAgentService
{
    private static readonly ActivitySource ActivitySource = 
        new("Workshop.MAF.Agents", "1.0.0");
    
    private ChatClient? _chatClient;
    private readonly ILogger<NewsAgentService> _logger;
    
    public NewsAgentService(ILogger<NewsAgentService> logger)
    {
        _logger = logger;
    }
    
    public void Initialize(string endpoint, string apiKey, string deploymentName)
    {
        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deploymentName);
    }
    
    public async Task<string> GetNewsAsync(string location)
    {
        // Crear span hijo
        using var activity = ActivitySource.StartActivity(
            name: "NewsAgent.GetNews",
            kind: ActivityKind.Client);
        
        var correlationId = Activity.Current?.GetBaggageItem("correlation_id") ?? "unknown";
        
        // Añadir atributos específicos del agente de noticias
        activity?.SetTag("agent.name", "NewsAgent");
        activity?.SetTag("agent.operation", "get_news");
        activity?.SetTag("news.location", location);
        activity?.SetTag("correlation_id", correlationId);
        
        Console.WriteLine($"   📰 NewsAgent: Buscando noticias de {location}...");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var prompt = $"Dame 2 titulares de noticias ficticias pero realistas sobre {location}. " +
                        "Responde en español con formato de lista.";
            
            var response = await _chatClient!.CompleteChatAsync(prompt);
            
            stopwatch.Stop();
            
            activity?.SetTag("llm.prompt_tokens", response.Value.Usage.InputTokenCount);
            activity?.SetTag("llm.completion_tokens", response.Value.Usage.OutputTokenCount);
            activity?.SetTag("agent.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            var result = response.Value.Content[0].Text;
            Console.WriteLine($"   ✅ NewsAgent completado en {stopwatch.ElapsedMilliseconds}ms");
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.message", ex.Message);
            
            _logger.LogError(ex, "Error en NewsAgent para {Location}", location);
            throw;
        }
    }
}
