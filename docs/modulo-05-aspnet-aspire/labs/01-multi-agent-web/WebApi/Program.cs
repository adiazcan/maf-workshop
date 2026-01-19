// =============================================================================
// WebApi - API REST para Agentes Inteligentes
// =============================================================================
// Este archivo implementa una API REST usando ASP.NET Core Minimal APIs
// que expone servicios de agentes de Microsoft Agent Framework.
// 
// Endpoints disponibles:
// - POST /api/chat/weather: Consultas sobre el clima
// - POST /api/chat/summary: Resúmenes de texto
// =============================================================================

using AgentServices;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Configuración de OpenTelemetry para Observabilidad
// -----------------------------------------------------------------------------
// Aspire recolecta automáticamente esta telemetría y la muestra en el dashboard.

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("webapi", serviceVersion: "1.0.0");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.AddOtlpExporter();
});

// -----------------------------------------------------------------------------
// Configuración del Chat Client de Microsoft Agent Framework
// -----------------------------------------------------------------------------
// El IChatClient es el componente central que conecta con Azure OpenAI.
// Se registra como Singleton para compartir la conexión entre servicios.

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<Program>>();
    
    // Obtener configuración de Azure OpenAI
    var endpoint = configuration["AzureOpenAI:Endpoint"] 
        ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
    var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";
    
    // Intentar obtener API key de diferentes fuentes
    var apiKey = configuration["AzureOpenAI:ApiKey"]
        ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
        ?? throw new InvalidOperationException(
            "API key no encontrada. Configure AzureOpenAI:ApiKey o AZURE_OPENAI_API_KEY");
    
    logger.LogInformation(
        "Configurando Chat Client con endpoint: {Endpoint}, deployment: {Deployment}",
        endpoint, deploymentName);
    
    // Crear el cliente de Azure OpenAI y obtener el chat client
    var azureClient = new AzureOpenAIClient(
        endpoint: new Uri(endpoint),
        credential: new AzureKeyCredential(apiKey)
    );
    
    var chatClient = azureClient.GetChatClient(deploymentName);
    
    // Convertir a IChatClient de Microsoft.Extensions.AI
    return chatClient.AsIChatClient();
});

// -----------------------------------------------------------------------------
// Registro de Servicios de Agentes
// -----------------------------------------------------------------------------
// Los servicios de agentes encapsulan la lógica de cada agente especializado.
// Se registran como Singleton para reutilizar las instancias.

builder.Services.AddSingleton<WeatherAgentService>();
builder.Services.AddSingleton<SummaryAgentService>();

// -----------------------------------------------------------------------------
// Configuración de OpenAPI/Swagger
// -----------------------------------------------------------------------------
// Swagger proporciona documentación interactiva de la API.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Multi-Agent Web API",
        Version = "v1",
        Description = "API REST para interactuar con agentes inteligentes de Microsoft Agent Framework"
    });
});

// -----------------------------------------------------------------------------
// Configuración de CORS
// -----------------------------------------------------------------------------
// Permite que aplicaciones frontend accedan a la API desde otros orígenes.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// -----------------------------------------------------------------------------
// Middleware Pipeline
// -----------------------------------------------------------------------------

// Habilitar CORS
app.UseCors();

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi-Agent API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

// -----------------------------------------------------------------------------
// Endpoint: Health Check
// -----------------------------------------------------------------------------
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("System")
   .WithDescription("Verifica que la API está funcionando correctamente");

// -----------------------------------------------------------------------------
// Endpoint: POST /api/chat/weather
// -----------------------------------------------------------------------------
// Consulta al agente de clima sobre condiciones meteorológicas.

app.MapPost("/api/chat/weather", async (
    WeatherRequest request,
    WeatherAgentService service,
    ILogger<Program> logger) =>
{
    // Validación de entrada
    if (string.IsNullOrWhiteSpace(request.City))
    {
        return Results.BadRequest(new ErrorResponse(
            "validation_error",
            "El campo 'city' es requerido y no puede estar vacío"));
    }
    
    logger.LogInformation("Procesando consulta de clima para: {City}", request.City);
    
    try
    {
        var result = await service.GetWeatherAsync(request.City);
        
        logger.LogInformation("Respuesta generada exitosamente para: {City}", request.City);
        
        return Results.Ok(new ChatResponse(
            Response: result,
            Agent: "WeatherAgent",
            Timestamp: DateTime.UtcNow));
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error de conectividad con Azure OpenAI");
        return Results.Json(
            new ErrorResponse(
                "service_unavailable",
                "No se pudo conectar con el servicio de Azure OpenAI. Intente de nuevo más tarde."),
            statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error procesando consulta de clima");
        return Results.Problem(
            title: "Error interno",
            detail: "Ocurrió un error procesando su solicitud.",
            statusCode: 500);
    }
})
.WithName("ChatWeather")
.WithTags("Chat")
.WithDescription("Consulta información del clima para una ciudad específica")
.Produces<ChatResponse>(StatusCodes.Status200OK)
.Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ErrorResponse>(StatusCodes.Status503ServiceUnavailable);

// -----------------------------------------------------------------------------
// Endpoint: POST /api/chat/summary
// -----------------------------------------------------------------------------
// Solicita al agente de resumen que procese un texto largo.

app.MapPost("/api/chat/summary", async (
    SummaryRequest request,
    SummaryAgentService service,
    ILogger<Program> logger) =>
{
    // Validación de entrada
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new ErrorResponse(
            "validation_error",
            "El campo 'text' es requerido y no puede estar vacío"));
    }
    
    // Validar longitud mínima para resumir
    if (request.Text.Length < 100)
    {
        return Results.BadRequest(new ErrorResponse(
            "validation_error",
            "El texto debe tener al menos 100 caracteres para generar un resumen útil"));
    }
    
    logger.LogInformation(
        "Procesando solicitud de resumen. Longitud del texto: {Length} caracteres",
        request.Text.Length);
    
    try
    {
        var result = await service.SummarizeAsync(request.Text, request.MaxLength);
        
        logger.LogInformation("Resumen generado exitosamente");
        
        return Results.Ok(new ChatResponse(
            Response: result,
            Agent: "SummaryAgent",
            Timestamp: DateTime.UtcNow));
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Error de conectividad con Azure OpenAI");
        return Results.Json(
            new ErrorResponse(
                "service_unavailable",
                "No se pudo conectar con el servicio de Azure OpenAI. Intente de nuevo más tarde."),
            statusCode: 503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error procesando solicitud de resumen");
        return Results.Problem(
            title: "Error interno",
            detail: "Ocurrió un error procesando su solicitud.",
            statusCode: 500);
    }
})
.WithName("ChatSummary")
.WithTags("Chat")
.WithDescription("Genera un resumen conciso de un texto largo")
.Produces<ChatResponse>(StatusCodes.Status200OK)
.Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
.Produces<ErrorResponse>(StatusCodes.Status503ServiceUnavailable);

// -----------------------------------------------------------------------------
// Iniciar la aplicación
// -----------------------------------------------------------------------------

app.Run();

// =============================================================================
// Modelos de Request/Response
// =============================================================================

/// <summary>
/// Request para consultas de clima.
/// </summary>
/// <param name="City">Nombre de la ciudad a consultar (requerido)</param>
record WeatherRequest(string City);

/// <summary>
/// Request para generar resúmenes.
/// </summary>
/// <param name="Text">Texto a resumir (requerido, mínimo 100 caracteres)</param>
/// <param name="MaxLength">Longitud máxima del resumen en palabras (opcional, default: 150)</param>
record SummaryRequest(string Text, int? MaxLength = 150);

/// <summary>
/// Respuesta exitosa de los endpoints de chat.
/// </summary>
/// <param name="Response">Respuesta generada por el agente</param>
/// <param name="Agent">Nombre del agente que generó la respuesta</param>
/// <param name="Timestamp">Momento en que se generó la respuesta</param>
record ChatResponse(string Response, string Agent, DateTime Timestamp);

/// <summary>
/// Respuesta de error estructurada.
/// </summary>
/// <param name="Code">Código de error para identificación programática</param>
/// <param name="Message">Mensaje descriptivo del error en español</param>
record ErrorResponse(string Code, string Message);
