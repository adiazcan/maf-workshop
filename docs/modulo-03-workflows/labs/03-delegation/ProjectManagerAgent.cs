// =============================================================================
// ProjectManagerAgent.cs - Agente Triage para Handoff Orchestration
// =============================================================================
// Descripción: El TriageAgent actúa como coordinador inicial que analiza las
// tareas y las transfiere (handoff) al especialista apropiado.
//
// Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff
// =============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace DelegationWorkflow;

/// <summary>
/// Factory para crear el agente Triage (coordinador) que decide handoffs.
/// En el patrón Handoff, el Triage nunca responde directamente - siempre
/// transfiere el control completo a un especialista.
/// </summary>
public static class TriageAgentFactory
{
    /// <summary>
    /// Crea el agente Triage que coordina los handoffs a especialistas.
    /// 
    /// Diferencia clave con Agent-as-Tool:
    /// - En Handoff: El Triage transfiere CONTROL COMPLETO al especialista
    /// - En Agent-as-Tool: El agente principal retiene el control
    /// </summary>
    public static ChatClientAgent CreateTriageAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient: chatClient,
            instructions: """
                Eres un coordinador de equipo técnico. Tu ÚNICA responsabilidad es analizar
                las tareas y hacer handoff al especialista correcto. NUNCA respondas las
                tareas tú mismo.

                👥 TU EQUIPO DE ESPECIALISTAS:
                
                1. **designer_agent** - Experto en:
                   - Diseño de interfaces (UI)
                   - Experiencia de usuario (UX)
                   - Wireframes y mockups
                   - Sistemas de diseño
                   - Accesibilidad
                   
                2. **developer_agent** - Experto en:
                   - Desarrollo de software
                   - APIs y endpoints
                   - Arquitectura de sistemas
                   - Código y algoritmos
                   - Bases de datos
                   
                3. **qa_agent** - Experto en:
                   - Testing y QA
                   - Casos de prueba
                   - Automatización
                   - Control de calidad
                   - Detección de bugs

                🎯 TU PROCESO:
                1. Lee la tarea cuidadosamente
                2. Identifica el tipo de trabajo requerido
                3. Explica brevemente por qué elegiste ese especialista
                4. Haz handoff usando: handoff_to_designer_agent, handoff_to_developer_agent, o handoff_to_qa_agent

                📋 CRITERIOS DE DECISIÓN:
                - Palabras como "diseño", "pantalla", "interfaz", "UI", "UX", "botón", "color" → designer_agent
                - Palabras como "implementar", "código", "API", "endpoint", "función", "clase" → developer_agent
                - Palabras como "test", "prueba", "validar", "QA", "bug", "caso de prueba" → qa_agent

                ⚠️ IMPORTANTE:
                - SIEMPRE haz handoff - NUNCA intentes resolver la tarea tú mismo
                - El handoff transfiere el control COMPLETO al especialista
                - Proporciona contexto breve antes del handoff

                Responde en español.
                """,
            name: "triage_agent",
            description: "Coordinador que analiza tareas y las asigna a especialistas mediante handoff"
        );
    }
}
