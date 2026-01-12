// =============================================================================
// SummaryAgentService - Servicio de Agente para Resúmenes de Texto
// =============================================================================
// Este servicio encapsula un agente especializado en generar resúmenes concisos
// y bien estructurados de textos largos.
// =============================================================================

using Microsoft.AI.Agents.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentServices;

/// <summary>
/// Servicio que encapsula un agente especializado en generar resúmenes.
/// Utiliza ChatCompletionAgent de Microsoft Agent Framework para procesar
/// textos largos y generar resúmenes estructurados.
/// </summary>
public class SummaryAgentService
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger<SummaryAgentService> _logger;
    
    /// <summary>
    /// Instrucciones del sistema que definen el comportamiento del agente.
    /// </summary>
    private const string SystemInstructions = """
        Eres un asistente especializado en crear resúmenes claros y concisos.
        Tu objetivo es extraer la información más importante de un texto
        y presentarla de manera estructurada.
        
        Directrices:
        - Responde siempre en español
        - Identifica los puntos clave del texto
        - Mantén la esencia del mensaje original
        - Usa un lenguaje claro y profesional
        - Organiza la información de manera lógica
        - No agregues información que no esté en el texto original
        
        Formato de respuesta:
        📋 **Resumen**
        [Resumen principal en 2-3 oraciones]
        
        🔑 **Puntos clave**
        • [Punto 1]
        • [Punto 2]
        • [Punto 3]
        
        💡 **Conclusión**
        [Conclusión breve si aplica]
        """;
    
    /// <summary>
    /// Constructor que configura el agente con el Kernel proporcionado.
    /// </summary>
    /// <param name="kernel">Kernel de Semantic Kernel configurado con Azure OpenAI</param>
    /// <param name="logger">Logger para registro de eventos</param>
    public SummaryAgentService(Kernel kernel, ILogger<SummaryAgentService> logger)
    {
        _logger = logger;
        
        // Crear el agente con configuración específica
        _agent = new ChatCompletionAgent()
        {
            Name = "SummaryAgent",
            Instructions = SystemInstructions,
            Kernel = kernel
        };
        
        _logger.LogInformation("SummaryAgentService inicializado correctamente");
    }
    
    /// <summary>
    /// Genera un resumen del texto proporcionado.
    /// </summary>
    /// <param name="text">Texto a resumir</param>
    /// <param name="maxLength">Longitud máxima aproximada del resumen en palabras (opcional)</param>
    /// <returns>Resumen estructurado del texto</returns>
    public async Task<string> SummarizeAsync(string text, int? maxLength = null)
    {
        _logger.LogDebug(
            "Generando resumen. Longitud del texto: {TextLength}, Max palabras: {MaxLength}",
            text.Length,
            maxLength ?? 150);
        
        // Construir el prompt con restricción de longitud si se especifica
        var prompt = maxLength.HasValue
            ? $"Resume el siguiente texto en aproximadamente {maxLength} palabras:\n\n{text}"
            : $"Resume el siguiente texto:\n\n{text}";
        
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);
        
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
        _logger.LogDebug("Resumen generado: {Length} caracteres", result.Length);
        
        return result;
    }
    
    /// <summary>
    /// Genera un resumen en forma de lista de puntos clave.
    /// </summary>
    /// <param name="text">Texto a analizar</param>
    /// <param name="bulletPoints">Número de puntos clave a extraer</param>
    /// <returns>Lista de puntos clave del texto</returns>
    public async Task<string> ExtractKeyPointsAsync(string text, int bulletPoints = 5)
    {
        _logger.LogDebug(
            "Extrayendo {BulletPoints} puntos clave de texto de {Length} caracteres",
            bulletPoints,
            text.Length);
        
        var prompt = $"""
            Extrae exactamente {bulletPoints} puntos clave del siguiente texto.
            Presenta cada punto como un bullet point conciso.
            
            Texto:
            {text}
            """;
        
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);
        
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
