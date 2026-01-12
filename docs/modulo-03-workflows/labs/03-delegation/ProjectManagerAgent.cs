// =============================================================================
// ProjectManagerAgent.cs - Agente coordinador con routing inteligente
// =============================================================================
// Descripción: El ProjectManagerAgent analiza las tareas y las delega al 
// especialista apropiado (Designer, Developer, o QA) usando function calling.
// =============================================================================

using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Chat;
using Microsoft.SemanticKernel;

namespace DelegationWorkflow;

/// <summary>
/// Agente coordinador que actúa como Project Manager.
/// Analiza tareas entrantes y las delega al especialista correcto.
/// </summary>
public class ProjectManagerAgent
{
    private readonly ChatCompletionAgent _pmAgent;
    private readonly ChatCompletionAgent _designerAgent;
    private readonly ChatCompletionAgent _developerAgent;
    private readonly ChatCompletionAgent _qaAgent;
    private readonly Kernel _kernel;

    /// <summary>
    /// Resultado de la delegación incluyendo agente seleccionado y respuesta
    /// </summary>
    public record DelegationResult(
        string SelectedAgent,
        string Reasoning,
        string SpecialistResponse
    );

    public ProjectManagerAgent(string endpoint, string deploymentName, string apiKey)
    {
        // Crear los agentes especialistas
        _designerAgent = SpecialistAgents.CreateDesignerAgent(endpoint, deploymentName, apiKey);
        _developerAgent = SpecialistAgents.CreateDeveloperAgent(endpoint, deploymentName, apiKey);
        _qaAgent = SpecialistAgents.CreateQAAgent(endpoint, deploymentName, apiKey);

        // Crear el kernel para function calling
        _kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey)
            .Build();

        // Registrar la función de routing como plugin
        _kernel.Plugins.AddFromObject(new RoutingPlugin(), "Routing");

        // Crear el agente PM con capacidad de function calling
        _pmAgent = new ChatCompletionAgent(
            name: "ProjectManagerAgent",
            instructions: """
                Eres un Project Manager experto que coordina un equipo de 3 especialistas:
                
                👥 TU EQUIPO:
                1. **DesignerAgent** - Experto en UI/UX, wireframes, diseño visual
                2. **DeveloperAgent** - Experto en código, arquitectura, APIs
                3. **QAAgent** - Experto en testing, casos de prueba, automatización
                
                🎯 TU TRABAJO:
                1. Analizar cada tarea que recibas
                2. Determinar qué especialista es el más apropiado
                3. Usar la función 'route_task' para asignar la tarea
                
                📋 CRITERIOS DE ROUTING:
                - Tareas de diseño, UI, UX, colores, wireframes → DesignerAgent
                - Tareas de código, APIs, arquitectura, implementación → DeveloperAgent
                - Tareas de testing, QA, bugs, casos de prueba → QAAgent
                
                SIEMPRE usa la función route_task para delegar. No intentes resolver tú mismo.
                """,
            kernel: _kernel,
            endpoint: new Uri(endpoint),
            modelId: deploymentName,
            apiKey: apiKey
        );
    }

    /// <summary>
    /// Procesa una tarea y la delega al especialista apropiado
    /// </summary>
    public async Task<DelegationResult> ProcessTaskAsync(string task)
    {
        Console.WriteLine($"📋 PM recibió tarea: \"{task}\"");
        Console.WriteLine();

        // Paso 1: El PM analiza y decide a quién delegar
        var pmChat = new ChatHistory();
        pmChat.AddUserMessage($"""
            Analiza la siguiente tarea y usa la función route_task para asignarla al especialista correcto.
            
            TAREA: {task}
            
            Recuerda: DEBES usar la función route_task con el especialista apropiado (designer, developer, o qa).
            """);

        string selectedAgent = "";
        string reasoning = "";

        // El PM usa function calling para decidir
        var invocationOptions = new AgentInvocationOptions
        {
            KernelArguments = new KernelArguments
            {
                ["task"] = task
            }
        };

        await foreach (var message in _pmAgent.InvokeAsync(pmChat, invocationOptions))
        {
            // Capturar el razonamiento del PM
            if (!string.IsNullOrEmpty(message.Content))
            {
                reasoning += message.Content;
            }

            // Detectar la función llamada
            if (message.Items != null)
            {
                foreach (var item in message.Items)
                {
                    if (item is Microsoft.Agents.AI.Abstractions.FunctionResultContent functionResult)
                    {
                        selectedAgent = functionResult.Result?.ToString() ?? "";
                    }
                }
            }
        }

        // Si no se detectó el agente, usar heurística simple
        if (string.IsNullOrEmpty(selectedAgent))
        {
            selectedAgent = DetermineAgentFromTask(task);
            reasoning = $"Análisis heurístico: la tarea parece ser de tipo {selectedAgent}";
        }

        Console.WriteLine($"🎯 PM decidió delegar a: {selectedAgent}");
        Console.WriteLine($"💭 Razonamiento: {reasoning}");
        Console.WriteLine();

        // Paso 2: Invocar al especialista seleccionado
        var specialist = selectedAgent.ToLower() switch
        {
            "designer" or "designeragent" => _designerAgent,
            "developer" or "developeragent" => _developerAgent,
            "qa" or "qaagent" => _qaAgent,
            _ => _developerAgent // Default
        };

        Console.WriteLine($"📤 Delegando tarea a {specialist.Name}...");
        Console.WriteLine();

        var specialistChat = new ChatHistory();
        specialistChat.AddUserMessage(task);

        string specialistResponse = "";
        await foreach (var message in specialist.InvokeAsync(specialistChat))
        {
            specialistResponse += message.Content;
        }

        return new DelegationResult(
            SelectedAgent: specialist.Name ?? selectedAgent,
            Reasoning: reasoning,
            SpecialistResponse: specialistResponse
        );
    }

    /// <summary>
    /// Heurística simple para determinar el agente basado en palabras clave
    /// </summary>
    private static string DetermineAgentFromTask(string task)
    {
        var taskLower = task.ToLower();

        // Keywords de diseño
        if (taskLower.Contains("diseño") || taskLower.Contains("ui") || taskLower.Contains("ux") ||
            taskLower.Contains("interfaz") || taskLower.Contains("wireframe") || taskLower.Contains("mockup") ||
            taskLower.Contains("color") || taskLower.Contains("botón") || taskLower.Contains("pantalla"))
        {
            return "designer";
        }

        // Keywords de QA
        if (taskLower.Contains("test") || taskLower.Contains("prueba") || taskLower.Contains("qa") ||
            taskLower.Contains("bug") || taskLower.Contains("validar") || taskLower.Contains("verificar") ||
            taskLower.Contains("calidad") || taskLower.Contains("error"))
        {
            return "qa";
        }

        // Default: desarrollo
        return "developer";
    }
}

/// <summary>
/// Plugin con la función de routing que el PM puede llamar
/// </summary>
public class RoutingPlugin
{
    [KernelFunction("route_task")]
    [Description("Asigna una tarea al especialista apropiado: designer (UI/UX), developer (código), o qa (testing)")]
    public string RouteTask(
        [Description("El especialista seleccionado: 'designer', 'developer', o 'qa'")] string specialist,
        [Description("Breve explicación de por qué se eligió este especialista")] string reason)
    {
        Console.WriteLine($"   🔀 Function call: route_task(specialist=\"{specialist}\", reason=\"{reason}\")");
        return specialist;
    }
}
