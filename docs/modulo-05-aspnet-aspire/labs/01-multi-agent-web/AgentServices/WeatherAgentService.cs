// =============================================================================
// WeatherAgentService - Servicio de Agente para Consultas de Clima
// =============================================================================
// Este servicio encapsula un agente especializado en proporcionar información
// meteorológica de manera amigable y conversacional.
// =============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgentServices;

/// <summary>
/// Servicio que encapsula un agente especializado en consultas meteorológicas.
/// Utiliza AIAgent de Microsoft Agent Framework para generar
/// respuestas conversacionales sobre el clima.
/// </summary>
public class WeatherAgentService
{
    private readonly AIAgent _agent;
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
    /// Constructor que configura el agente con el cliente de chat proporcionado.
    /// </summary>
    /// <param name="chatClient">Cliente de chat configurado con Azure OpenAI</param>
    /// <param name="logger">Logger para registro de eventos</param>
    public WeatherAgentService(IChatClient chatClient, ILogger<WeatherAgentService> logger)
    {
        _logger = logger;
        
        // Crear el agente usando el método de extensión CreateAIAgent del IChatClient
        // Este patrón es el recomendado por Microsoft Agent Framework
        _agent = chatClient.CreateAIAgent(
            instructions: SystemInstructions,
            name: "WeatherAgent");
        
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
        
        // Invocar al agente usando RunAsync con string input
        // El agente maneja internamente la conversión a mensajes
        var response = await _agent.RunAsync($"¿Cómo está el clima en {city}?");
        var result = response.Text;
        
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
        
        // Usar RunAsync directamente con el texto de la pregunta
        var response = await _agent.RunAsync(question);
        
        return response.Text;
    }
}
