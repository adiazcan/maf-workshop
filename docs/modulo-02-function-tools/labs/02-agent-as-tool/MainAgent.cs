// ============================================================================
// Archivo: MainAgent.cs
// Descripción: Agente principal que usa otros agentes como herramientas
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using System.ComponentModel;
using Microsoft.AI.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AgentComposition;

/// <summary>
/// Agente coordinador que delega tareas especializadas a otros agentes.
/// Demuestra el patrón "agent-as-tool" donde agentes completos se exponen
/// como funciones para ser invocados por un agente principal.
/// </summary>
public class MainAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly CalculatorAgent _calculatorAgent;
    
    public MainAgent(Kernel kernel, CalculatorAgent calculatorAgent)
    {
        _calculatorAgent = calculatorAgent;
        
        // ===== Crear función que invoca al agente calculadora =====
        // Esto convierte el agente especializado en una "function tool"
        var calculateFunction = KernelFunctionFactory.CreateFromMethod(
            method: async (string mathQuestion) => 
            {
                return await _calculatorAgent.SolveMathProblemAsync(mathQuestion);
            },
            functionName: "calculate",
            description: "Resuelve problemas matemáticos complejos. Usa esta función cuando el usuario tenga preguntas sobre cálculos, matemáticas, porcentajes, ecuaciones o estadísticas."
        );
        
        // Registrar la función en el kernel
        kernel.Plugins.AddFromFunctions(
            pluginName: "AgentesEspecializados",
            description: "Agentes especializados para tareas específicas",
            functions: new[] { calculateFunction }
        );
        
        // ===== Crear agente principal =====
        _agent = new ChatCompletionAgent()
        {
            Name = "AsistenteGeneral",
            Instructions = """
                Eres un asistente general llamado AsistenteGeneral.
                Puedes ayudar con muchas tareas, pero tienes acceso a un experto matemático.
                
                REGLAS IMPORTANTES:
                1. Para preguntas de matemáticas, cálculos, porcentajes o estadísticas:
                   → USA la función 'calculate' para delegarlas al experto
                2. Para otras preguntas (conversación general, información, consejos):
                   → Responde tú directamente
                
                Siempre responde en español de forma amigable.
                Cuando delegues a la calculadora, presenta los resultados de forma clara.
                """,
            Kernel = kernel,
            Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }
            )
        };
    }
    
    /// <summary>
    /// Nombre del agente principal
    /// </summary>
    public string Name => _agent.Name;
    
    /// <summary>
    /// Procesa un mensaje del usuario, delegando a agentes especializados cuando sea necesario.
    /// </summary>
    public async IAsyncEnumerable<string> ProcessMessageAsync(ChatHistory chatHistory)
    {
        await foreach (var message in _agent.InvokeStreamingAsync(chatHistory))
        {
            yield return message.Content ?? "";
        }
    }
}
