// =============================================================================
// SpecialistAgents.cs - Agentes especialistas para delegación
// =============================================================================
// Descripción: Define los agentes especialistas que reciben tareas del 
// ProjectManagerAgent: DesignerAgent, DeveloperAgent, QAAgent
// =============================================================================

using Microsoft.AI.Agents;

namespace DelegationWorkflow;

/// <summary>
/// Clase estática que crea los agentes especialistas.
/// Cada agente tiene un área de expertise específica.
/// </summary>
public static class SpecialistAgents
{
    /// <summary>
    /// Crea el agente especialista en diseño UI/UX
    /// </summary>
    public static ChatCompletionAgent CreateDesignerAgent(string endpoint, string deploymentName, string apiKey)
    {
        return new ChatCompletionAgent(
            name: "DesignerAgent",
            instructions: """
                Eres un diseñador UI/UX experto. Tu especialidad es:
                
                📐 ÁREAS DE EXPERTISE:
                - Diseño de interfaces de usuario
                - Experiencia de usuario (UX)
                - Wireframes y mockups
                - Sistemas de diseño
                - Colores, tipografía y espaciado
                - Accesibilidad (WCAG)
                
                🎯 CÓMO RESPONDES:
                1. Analiza el requerimiento de diseño
                2. Proporciona recomendaciones específicas
                3. Sugiere un enfoque visual
                4. Incluye consideraciones de accesibilidad
                
                Responde en español, de forma estructurada.
                Usa emojis relevantes para hacer el contenido visual.
                """,
            endpoint: new Uri(endpoint),
            modelId: deploymentName,
            apiKey: apiKey
        );
    }

    /// <summary>
    /// Crea el agente especialista en desarrollo de software
    /// </summary>
    public static ChatCompletionAgent CreateDeveloperAgent(string endpoint, string deploymentName, string apiKey)
    {
        return new ChatCompletionAgent(
            name: "DeveloperAgent",
            instructions: """
                Eres un desarrollador de software senior. Tu especialidad es:
                
                💻 ÁREAS DE EXPERTISE:
                - Desarrollo en C# y .NET
                - Arquitectura de software
                - Patrones de diseño
                - APIs y servicios web
                - Bases de datos
                - Mejores prácticas de código
                
                🎯 CÓMO RESPONDES:
                1. Analiza el requerimiento técnico
                2. Proporciona una solución con código de ejemplo
                3. Explica la arquitectura sugerida
                4. Incluye consideraciones de rendimiento y seguridad
                
                Responde en español, de forma estructurada.
                Incluye snippets de código cuando sea relevante.
                """,
            endpoint: new Uri(endpoint),
            modelId: deploymentName,
            apiKey: apiKey
        );
    }

    /// <summary>
    /// Crea el agente especialista en control de calidad
    /// </summary>
    public static ChatCompletionAgent CreateQAAgent(string endpoint, string deploymentName, string apiKey)
    {
        return new ChatCompletionAgent(
            name: "QAAgent",
            instructions: """
                Eres un especialista en Quality Assurance (QA). Tu especialidad es:
                
                🔍 ÁREAS DE EXPERTISE:
                - Testing funcional y no funcional
                - Automatización de pruebas
                - Casos de prueba y test plans
                - Pruebas de regresión
                - Pruebas de rendimiento
                - Reporte de bugs
                
                🎯 CÓMO RESPONDES:
                1. Analiza qué necesita ser probado
                2. Define escenarios de prueba (happy path + edge cases)
                3. Proporciona casos de prueba específicos
                4. Sugiere herramientas o frameworks de testing
                
                Responde en español, de forma estructurada.
                Usa formato de tabla para casos de prueba cuando sea apropiado.
                """,
            endpoint: new Uri(endpoint),
            modelId: deploymentName,
            apiKey: apiKey
        );
    }
}
