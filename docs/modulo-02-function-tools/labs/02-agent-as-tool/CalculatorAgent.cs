// ============================================================================
// Archivo: CalculatorAgent.cs
// Descripción: Agente especializado en cálculos matemáticos
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using Microsoft.AI.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentComposition;

/// <summary>
/// Agente especializado en operaciones matemáticas.
/// Este agente será utilizado como "herramienta" por el agente principal
/// para delegar cualquier pregunta relacionada con matemáticas.
/// </summary>
public class CalculatorAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly Kernel _kernel;
    
    public CalculatorAgent(Kernel kernel)
    {
        _kernel = kernel;
        
        // Crear agente especializado en matemáticas
        _agent = new ChatCompletionAgent()
        {
            Name = "CalculadoraExperta",
            Instructions = """
                Eres un experto matemático llamado CalculadoraExperta.
                Tu único propósito es resolver problemas matemáticos.
                
                Reglas:
                1. Solo respondes preguntas matemáticas
                2. Siempre muestras el proceso paso a paso
                3. Usas notación matemática clara
                4. Respondes en español
                5. Si no es una pregunta matemática, indica que solo puedes hacer cálculos
                
                Ejemplos de lo que puedes hacer:
                - Operaciones básicas (suma, resta, multiplicación, división)
                - Porcentajes y proporciones
                - Ecuaciones simples
                - Conversiones de unidades
                - Estadísticas básicas (promedio, mediana)
                """,
            Kernel = kernel
        };
    }
    
    /// <summary>
    /// Nombre del agente para identificación
    /// </summary>
    public string Name => _agent.Name;
    
    /// <summary>
    /// Procesa una pregunta matemática y devuelve la respuesta.
    /// Este método será expuesto como función al agente principal.
    /// </summary>
    /// <param name="question">Pregunta o problema matemático</param>
    /// <returns>Solución con explicación paso a paso</returns>
    public async Task<string> SolveMathProblemAsync(string question)
    {
        Console.WriteLine($"\n   📊 [CalculadoraExperta recibió]: {question}");
        
        // Crear historial temporal para esta consulta
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(question);
        
        // Invocar el agente especializado
        var response = new System.Text.StringBuilder();
        
        await foreach (var message in _agent.InvokeStreamingAsync(chatHistory))
        {
            response.Append(message.Content);
        }
        
        var result = response.ToString();
        Console.WriteLine($"   📊 [CalculadoraExperta respondió]: {result.Substring(0, Math.Min(50, result.Length))}...\n");
        
        return result;
    }
}
