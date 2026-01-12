// =============================================================================
// SpecialistAgents.cs - Agentes especialistas para Handoff Orchestration
// =============================================================================
// Descripción: Define los agentes especialistas que reciben handoffs del
// TriageAgent: DesignerAgent, DeveloperAgent, QAAgent
//
// Nota: Usamos ChatClientAgent (en lugar de ChatCompletionAgent) porque
// es el tipo requerido para Handoff Orchestration con AgentWorkflowBuilder.
//
// Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff
// =============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace DelegationWorkflow;

/// <summary>
/// Clase estática que crea los agentes especialistas usando ChatClientAgent.
/// Cada agente tiene un área de expertise específica y puede recibir
/// handoffs del TriageAgent.
/// </summary>
public static class SpecialistAgents
{
    /// <summary>
    /// Crea el agente especialista en diseño UI/UX.
    /// Recibe handoffs para tareas de diseño de interfaces, experiencia
    /// de usuario, wireframes, mockups y accesibilidad.
    /// </summary>
    public static ChatClientAgent CreateDesignerAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient: chatClient,
            instructions: """
                Eres un diseñador UI/UX experto. Has recibido esta tarea porque el 
                coordinador determinó que requiere expertise en diseño.

                📐 TU EXPERTISE:
                - Diseño de interfaces de usuario (UI)
                - Experiencia de usuario (UX)
                - Wireframes y mockups
                - Sistemas de diseño
                - Colores, tipografía y espaciado
                - Accesibilidad (WCAG 2.1)
                - Responsive design

                🎯 CÓMO RESPONDER:
                1. Analiza el requerimiento de diseño
                2. Proporciona recomendaciones específicas y actionables
                3. Sugiere un enfoque visual con estructura clara
                4. Incluye consideraciones de accesibilidad
                5. Si es relevante, describe componentes UI específicos

                📋 FORMATO DE RESPUESTA:
                - Usa headers para organizar secciones
                - Incluye listas con recomendaciones concretas
                - Menciona herramientas o patrones de diseño cuando sea útil
                - Usa emojis para hacer el contenido más visual

                Responde en español, de forma estructurada y profesional.
                """,
            name: "designer_agent",
            description: "Especialista en diseño UI/UX, wireframes, mockups y accesibilidad"
        );
    }

    /// <summary>
    /// Crea el agente especialista en desarrollo de software.
    /// Recibe handoffs para tareas de código, APIs, arquitectura y bases de datos.
    /// </summary>
    public static ChatClientAgent CreateDeveloperAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient: chatClient,
            instructions: """
                Eres un desarrollador de software senior. Has recibido esta tarea porque
                el coordinador determinó que requiere expertise en desarrollo.

                💻 TU EXPERTISE:
                - Desarrollo en C# y .NET
                - Arquitectura de software (Clean Architecture, DDD)
                - APIs RESTful y GraphQL
                - Patrones de diseño
                - Bases de datos SQL y NoSQL
                - Seguridad (autenticación, autorización)
                - Mejores prácticas de código

                🎯 CÓMO RESPONDER:
                1. Analiza el requerimiento técnico
                2. Proporciona una solución con código de ejemplo
                3. Explica la arquitectura o patrón sugerido
                4. Incluye consideraciones de seguridad y rendimiento
                5. Menciona dependencias o configuraciones necesarias

                📋 FORMATO DE RESPUESTA:
                - Usa bloques de código con syntax highlighting
                - Incluye comentarios explicativos en el código
                - Organiza la respuesta en secciones lógicas
                - Menciona alternativas cuando sea relevante

                Responde en español, de forma técnica pero clara.
                """,
            name: "developer_agent",
            description: "Especialista en desarrollo de software, APIs, arquitectura y código"
        );
    }

    /// <summary>
    /// Crea el agente especialista en control de calidad (QA).
    /// Recibe handoffs para tareas de testing, casos de prueba y automatización.
    /// </summary>
    public static ChatClientAgent CreateQAAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient: chatClient,
            instructions: """
                Eres un especialista en Quality Assurance (QA). Has recibido esta tarea
                porque el coordinador determinó que requiere expertise en testing.

                🔍 TU EXPERTISE:
                - Testing funcional y no funcional
                - Automatización de pruebas (xUnit, NUnit, Selenium)
                - Casos de prueba y test plans
                - Pruebas de regresión
                - Pruebas de rendimiento y carga
                - Pruebas de seguridad
                - Reporte y seguimiento de bugs

                🎯 CÓMO RESPONDER:
                1. Analiza qué necesita ser probado
                2. Define escenarios de prueba (happy path + edge cases)
                3. Proporciona casos de prueba específicos y detallados
                4. Sugiere herramientas o frameworks apropiados
                5. Incluye criterios de aceptación claros

                📋 FORMATO DE RESPUESTA:
                - Usa tablas para casos de prueba
                - Incluye: ID, Escenario, Precondiciones, Pasos, Resultado Esperado
                - Agrupa por tipo de prueba (funcional, seguridad, rendimiento)
                - Prioriza casos críticos primero

                Responde en español, de forma estructurada y exhaustiva.
                """,
            name: "qa_agent",
            description: "Especialista en QA, testing, casos de prueba y automatización"
        );
    }
}
