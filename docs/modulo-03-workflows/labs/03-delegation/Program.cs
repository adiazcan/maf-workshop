// =============================================================================
// Program.cs - Workflow de Handoff con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa el patrón de Handoff Orchestration donde
// un agente Triage transfiere el control completo a agentes especialistas según
// el tipo de trabajo requerido.
//
// Conceptos demostrados:
// - Patrón Handoff Orchestration (transferencia de control)
// - ChatClientAgent para agentes especializados
// - AgentWorkflowBuilder para configurar reglas de handoff
// - Eventos de workflow (AgentRunUpdateEvent, WorkflowOutputEvent)
//
// Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff
// =============================================================================

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// =============================================================================
// PASO 1: Configuración
// =============================================================================

// Cargar configuración desde appsettings.json y user secrets
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

// Obtener configuración de Azure OpenAI
var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:Endpoint");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:DeploymentName");

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("    WORKFLOW DE HANDOFF: Triage → Especialistas");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear el cliente de Azure OpenAI
// =============================================================================

// Crear el cliente de chat usando Azure OpenAI con DefaultAzureCredential
// AsIChatClient() convierte el cliente a la interfaz IChatClient de Extensions.AI
IChatClient chatClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();

Console.WriteLine("✓ Cliente Azure OpenAI configurado con DefaultAzureCredential");
Console.WriteLine($"  Endpoint: {endpoint}");
Console.WriteLine($"  Modelo: {deploymentName}");
Console.WriteLine();

// =============================================================================
// PASO 3: Crear los agentes especialistas usando ChatClientAgent
// =============================================================================
// En Handoff Orchestration, usamos ChatClientAgent que permite la transferencia
// de control entre agentes. Cada agente tiene un nombre único y descripción
// que el sistema usa para el routing automático.
// =============================================================================

Console.WriteLine("👥 Creando equipo de agentes especialistas...");
Console.WriteLine();

// Agente especialista en Diseño UI/UX
ChatClientAgent designerAgent = new(
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

        Responde en español, de forma estructurada y profesional.
        Máximo 200 palabras.
        """,
    name: "designer_agent",
    description: "Especialista en diseño UI/UX, wireframes, mockups y accesibilidad"
);
Console.WriteLine($"  ✓ {designerAgent.Name} (UI/UX)");

// Agente especialista en Desarrollo
ChatClientAgent developerAgent = new(
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
        2. Proporciona una solución con pseudocódigo o estructura
        3. Explica la arquitectura o patrón sugerido
        4. Incluye consideraciones de seguridad y rendimiento
        5. Menciona dependencias o configuraciones necesarias

        Responde en español, de forma técnica pero clara.
        Máximo 200 palabras.
        """,
    name: "developer_agent",
    description: "Especialista en desarrollo de software, APIs y arquitectura"
);
Console.WriteLine($"  ✓ {developerAgent.Name} (Código)");

// Agente especialista en QA
ChatClientAgent qaAgent = new(
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

        Responde en español, de forma estructurada y exhaustiva.
        Máximo 200 palabras.
        """,
    name: "qa_agent",
    description: "Especialista en QA, testing y automatización de pruebas"
);
Console.WriteLine($"  ✓ {qaAgent.Name} (Testing)");
Console.WriteLine();

// =============================================================================
// PASO 4: Crear el agente Triage (Coordinador)
// =============================================================================
// El Triage es el agente que recibe todas las tareas inicialmente y decide
// a qué especialista transferir el control. En Handoff, el Triage NUNCA
// responde directamente - siempre hace handoff a un especialista.
// =============================================================================

Console.WriteLine("👔 Creando Triage Agent (Coordinador)...");

ChatClientAgent triageAgent = new(
    chatClient: chatClient,
    instructions: """
        Eres un coordinador de equipo técnico. Tu ÚNICA responsabilidad es analizar
        las tareas y hacer handoff al especialista correcto. NUNCA respondas las
        tareas tú mismo - SIEMPRE transfiere el control a un especialista.

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
           - Validación

        🎯 TU PROCESO:
        1. Lee la tarea cuidadosamente
        2. Identifica el tipo de trabajo requerido
        3. Explica brevemente por qué elegiste ese especialista
        4. Haz handoff usando: handoff_to_designer_agent, handoff_to_developer_agent, o handoff_to_qa_agent

        📋 CRITERIOS DE DECISIÓN:
        - Palabras como "diseño", "pantalla", "interfaz", "UI", "UX", "botón", "color" → designer_agent
        - Palabras como "implementar", "código", "API", "endpoint", "función", "clase" → developer_agent
        - Palabras como "test", "prueba", "validar", "QA", "bug", "caso de prueba" → qa_agent

        ⚠️ IMPORTANTE: SIEMPRE haz handoff - NUNCA intentes resolver la tarea tú mismo.

        Responde en español.
        """,
    name: "triage_agent",
    description: "Coordinador que asigna tareas a especialistas mediante handoff"
);

Console.WriteLine($"  ✓ {triageAgent.Name} (Coordinador)");
Console.WriteLine($"    └─ {designerAgent.Name}");
Console.WriteLine($"    └─ {developerAgent.Name}");
Console.WriteLine($"    └─ {qaAgent.Name}");
Console.WriteLine();

// =============================================================================
// PASO 5: Configurar el Workflow de Handoff con AgentWorkflowBuilder
// =============================================================================
// AgentWorkflowBuilder es la API oficial para configurar Handoff Orchestration.
// - CreateHandoffBuilderWith(): Define el agente que inicia el workflow
// - WithHandoffs(): Configura qué agentes pueden recibir handoff desde un agente
// - WithHandoff(): Configura handoff de un agente a otro específico
// =============================================================================

Console.WriteLine("📋 Configurando reglas de handoff...");

var workflow = AgentWorkflowBuilder
    .CreateHandoffBuilderWith(triageAgent)                                  // Agente inicial
    .WithHandoffs(triageAgent, [designerAgent, developerAgent, qaAgent])    // Triage → Especialistas
    .WithHandoff(designerAgent, triageAgent)                                // Designer puede volver a Triage
    .WithHandoff(developerAgent, triageAgent)                               // Developer puede volver a Triage
    .WithHandoff(qaAgent, triageAgent)                                      // QA puede volver a Triage
    .Build();

Console.WriteLine("   triage_agent → [designer_agent, developer_agent, qa_agent]");
Console.WriteLine("   designer_agent → [triage_agent]");
Console.WriteLine("   developer_agent → [triage_agent]");
Console.WriteLine("   qa_agent → [triage_agent]");
Console.WriteLine();

// =============================================================================
// PASO 6: Definir tareas de diferentes tipos
// =============================================================================

var tasks = new[]
{
    // Tarea de diseño
    "Diseñar la pantalla de login con campos de usuario y contraseña, incluir botón de 'Olvidé mi contraseña' y opción de login con redes sociales",
    
    // Tarea de desarrollo
    "Implementar un endpoint REST para autenticación JWT que reciba email y password, valide credenciales contra la base de datos y retorne un token",
    
    // Tarea de QA
    "Crear los casos de prueba para el flujo de login, incluyendo credenciales válidas, inválidas, cuenta bloqueada y rate limiting"
};

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    PROCESANDO TAREAS CON HANDOFF");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");

// =============================================================================
// PASO 7: Procesar cada tarea usando el workflow de Handoff
// =============================================================================
// Para cada tarea:
// 1. Creamos un mensaje de usuario
// 2. Ejecutamos el workflow con InProcessExecution.StreamAsync()
// 3. Observamos los eventos del workflow para ver los handoffs
// 4. Capturamos la respuesta del especialista
// =============================================================================

var results = new List<(string Task, string HandoffTo, string Response)>();

for (int i = 0; i < tasks.Length; i++)
{
    Console.WriteLine();
    Console.WriteLine($"┌─────────────────────────────────────────────────────────────────┐");
    Console.WriteLine($"│ TAREA {i + 1}/{tasks.Length}                                                        │");
    Console.WriteLine($"└─────────────────────────────────────────────────────────────────┘");
    Console.WriteLine();
    
    var taskDescription = tasks[i].Length > 60 
        ? tasks[i].Substring(0, 57) + "..." 
        : tasks[i];
    Console.WriteLine($"📨 \"{taskDescription}\"");
    Console.WriteLine();

    // Crear mensajes para esta tarea
    List<ChatMessage> messages = [new ChatMessage(ChatRole.User, tasks[i])];

    string currentAgent = "triage_agent";
    string handoffTo = "";
    string fullResponse = "";
    
    try
    {
        // Ejecutar el workflow con streaming de eventos
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        // Procesar eventos del workflow
        await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
            if (evt is AgentRunUpdateEvent e)
            {
                // Detectar handoff (cambio de agente)
                if (e.ExecutorId != currentAgent)
                {
                    if (currentAgent == "triage_agent" && !string.IsNullOrEmpty(e.ExecutorId))
                    {
                        handoffTo = e.ExecutorId;
                        Console.WriteLine($"   🔀 Handoff: {currentAgent} → {handoffTo}");
                        Console.WriteLine();
                        Console.Write($"🤖 {handoffTo}: ");
                    }
                    currentAgent = e.ExecutorId ?? currentAgent;
                }
                
                // Mostrar respuesta en streaming (solo del especialista)
                if (!string.IsNullOrEmpty(e.Data?.ToString()) && currentAgent != "triage_agent")
                {
                    Console.Write(e.Data);
                    fullResponse += e.Data;
                }
            }
            else if (evt is WorkflowOutputEvent)
            {
                // El workflow ha terminado
                break;
            }
        }
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}");
        fullResponse = $"Error: {ex.Message}";
    }
    
    Console.WriteLine();
    
    results.Add((tasks[i], handoffTo, fullResponse));
}

// =============================================================================
// PASO 8: Resumen de Handoffs
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    RESUMEN DE HANDOFFS");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

Console.WriteLine("┌──────────┬────────────────────────────────────────┬─────────────────┐");
Console.WriteLine("│ Tarea    │ Descripción                            │ Handoff a       │");
Console.WriteLine("├──────────┼────────────────────────────────────────┼─────────────────┤");

for (int i = 0; i < results.Count; i++)
{
    var (task, handoff, _) = results[i];
    var truncatedTask = task.Length > 38 ? task.Substring(0, 35) + "..." : task.PadRight(38);
    var handoffDisplay = string.IsNullOrEmpty(handoff) ? "N/A" : handoff;
    Console.WriteLine($"│ Tarea {i + 1}  │ {truncatedTask} │ {handoffDisplay,-15} │");
}

Console.WriteLine("└──────────┴────────────────────────────────────────┴─────────────────┘");
Console.WriteLine();

// Contar handoffs por tipo
var designerCount = results.Count(r => r.HandoffTo.Contains("designer"));
var developerCount = results.Count(r => r.HandoffTo.Contains("developer"));
var qaCount = results.Count(r => r.HandoffTo.Contains("qa"));

Console.WriteLine("📊 Distribución de handoffs:");
Console.WriteLine($"   🎨 designer_agent:    {designerCount} tarea(s)");
Console.WriteLine($"   💻 developer_agent:   {developerCount} tarea(s)");
Console.WriteLine($"   🔍 qa_agent:          {qaCount} tarea(s)");
Console.WriteLine();

// =============================================================================
// PASO 9: Validación del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ Patrón Handoff Orchestration implementado correctamente");
Console.WriteLine("✓ El Triage analizó y transfirió control a especialistas");
Console.WriteLine("✓ Cada especialista procesó su tarea con control completo");
Console.WriteLine("✓ Los eventos del workflow mostraron las transiciones");
Console.WriteLine();
Console.WriteLine("📖 Conceptos demostrados:");
Console.WriteLine("   - Handoff: Transferencia completa de control entre agentes");
Console.WriteLine("   - AgentWorkflowBuilder: Configuración declarativa de handoffs");
Console.WriteLine("   - WorkflowEvents: Observación de transiciones en tiempo real");
Console.WriteLine();
Console.WriteLine("📚 Referencia: https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff");
Console.WriteLine();
