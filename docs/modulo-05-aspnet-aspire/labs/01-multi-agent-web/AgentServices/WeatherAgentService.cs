// =============================================================================
// WeatherAgentService - Servicio de Agente para Consultas de Clima
// =============================================================================
// Este servicio encapsula un agente especializado en proporcionar información
// meteorológica de manera amigable y conversacional.
// =============================================================================

using Microsoft.AI.Agents.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentServices;

/// <summary>
/// Servicio que encapsula un agente especializado en consultas meteorológicas.
/// Utiliza ChatCompletionAgent de Microsoft Agent Framework para generar
/// respuestas conversacionales sobre el clima.
/// </summary>
public class WeatherAgentService
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger<WeatherAgentService> _logger;
    
    /// <summary>
    /// Instrucciones del sistema que definen el comportamiento del agente.
    /// El agente responde en español con información del clima de manera amigable.
    /// </summary>
    private const string SystemInstructions = """
        Eres un asistente meteorológico amigable y profesional. Tu función es proporcionar 
        información del clima de manera clara y útil.
        
        Directrices:
        - Responde siempre en español
        - Incluye temperatura, condiciones y recomendaciones cuando sea apropiado
        - Sé conciso pero informativo
        - Si no tienes información real del clima, proporciona una respuesta simulada
          realista basada en la ubicación y época del año
        - Sugiere qué ropa usar o actividades apropiadas para el clima
        
        Formato de respuesta:
        🌡️ **Temperatura**: [valor]
        🌤️ **Condiciones**: [descripción]
        💡 **Recomendación**: [sugerencia]
        """;
    
    /// <summary>
    /// Constructor que configura el agente con el Kernel proporcionado.
    /// </summary>
    /// <param name="kernel">Kernel de Semantic Kernel configurado con Azure OpenAI</param>
    /// <param name="logger">Logger para registro de eventos</param>
    public WeatherAgentService(Kernel kernel, ILogger<WeatherAgentService> logger)
    {
        _logger = logger;
        
        // Crear el agente con configuración específica
        _agent = new ChatCompletionAgent()
        {
            Name = "WeatherAgent",
            Instructions = SystemInstructions,
            Kernel = kernel
        };
        
        _logger.LogInformation("WeatherAgentService inicializado correctamente");
    }
    
    /// <summary>
    /// Consulta al agente sobre el clima en una ciudad específica.
    /// </summary>
    /// <param name="city">Nombre de la ciudad a consultar</param>
    /// <returns>Respuesta del agente con información del clima</returns>
    public async Task<string> GetWeatherAsync(string city)
    {
        _logger.LogDebug("Consultando clima para ciudad: {City}", city);
        
        // Crear historial de chat con la consulta del usuario
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage($"¿Cómo está el clima en {city}?");
        
        // Invocar al agente y obtener respuesta
        var responses = new List<string>();
        await foreach (var response in _agent.InvokeAsync(chatHistory))
        {
            if (response.Content is not null)
            {
                responses.Add(response.Content);
            }
        }
        
        var result = string.Join("", responses);
        _logger.LogDebug("Respuesta generada para {City}: {Length} caracteres", city, result.Length);
        
        return result;
    }
    
    /// <summary>
    /// Consulta al agente con una pregunta personalizada sobre el clima.
    /// </summary>
    /// <param name="question">Pregunta en lenguaje natural</param>
    /// <returns>Respuesta del agente</returns>
    public async Task<string> AskAsync(string question)
    {
        _logger.LogDebug("Procesando pregunta personalizada: {Question}", question);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(question);
        
        var responses = new List<string>();
        await foreach (var response in _agent.InvokeAsync(chatHistory))
        {
            if (response.Content is not null)
            {
                responses.Add(response.Content);
            }
        }
        
        return string.Join("", responses);
    }
}
