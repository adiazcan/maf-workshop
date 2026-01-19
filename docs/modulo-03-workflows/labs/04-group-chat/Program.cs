// =============================================================================
// Program.cs - Group Chat con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa un Group Chat Workflow donde 3 agentes 
// colaboran en una discusión: BrainstormAgent, CriticAgent, SynthesizerAgent.
// La conversación continúa hasta alcanzar el máximo de iteraciones.
//
// Conceptos demostrados:
// - AgentWorkflowBuilder.CreateGroupChatBuilderWith() para workflows de grupo
// - RoundRobinGroupChatManager para coordinación de turnos
// - MaximumIterationCount para control de terminación
// - Streaming de eventos con WorkflowEvent
// - Agentes con roles complementarios
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

// Configurar Azure OpenAI client usando Azure CLI authentication
var chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("         GROUP CHAT: Brainstorm ↔ Critic ↔ Synthesizer");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear agentes colaborativos
// =============================================================================

// Agente 1: Brainstormer - Genera ideas creativas
ChatClientAgent brainstormAgent = new(chatClient,
    """
    Eres un agente creativo de brainstorming. Tu rol en el grupo es:
    
    💡 TU FUNCIÓN:
    - Generar ideas creativas e innovadoras
    - Proponer soluciones fuera de lo convencional
    - Expandir sobre las ideas de otros
    - Mantener la energía positiva del grupo
    
    📋 REGLAS:
    - Propón 2-3 ideas por turno
    - Sé breve y directo (máximo 100 palabras)
    - Construye sobre feedback recibido
    - No critiques, solo propón
    
    Responde en español.
    """,
    "CopyWriter",
    "Un agente creativo de generación de ideas"
);

// Agente 2: Critic - Evalúa y cuestiona ideas
ChatClientAgent criticAgent = new(chatClient,
    """
    Eres un agente crítico constructivo. Tu rol en el grupo es:
    
    🔍 TU FUNCIÓN:
    - Evaluar las ideas propuestas
    - Identificar debilidades y riesgos
    - Sugerir mejoras específicas
    - Mantener el realismo y viabilidad
    
    📋 REGLAS:
    - Sé constructivo, no destructivo
    - Ofrece alternativas cuando critiques
    - Sé breve (máximo 100 palabras)
    - Reconoce los puntos fuertes también
    
    Responde en español.
    """,
    "Reviewer",
    "Un agente de evaluación y mejora"
);

// Agente 3: Synthesizer - Combina y resume ideas
ChatClientAgent synthesizerAgent = new(chatClient,
    """
    Eres un agente sintetizador. Tu rol en el grupo es:
    
    🎯 TU FUNCIÓN:
    - Combinar las mejores ideas del grupo
    - Encontrar puntos en común
    - Crear propuestas unificadas
    - Facilitar el consenso
    
    📋 REGLAS:
    - Resume los puntos clave de la discusión
    - Propón síntesis que integren todas las perspectivas
    - Sé conciso (máximo 100 palabras)
    - Destaca áreas de acuerdo
    
    Responde en español.
    """,
    "Synthesizer",
    "Un agente de síntesis y consenso"
);

Console.WriteLine("✓ CopyWriter creado - Generador de ideas");
Console.WriteLine("✓ Reviewer creado - Evaluador constructivo");
Console.WriteLine("✓ Synthesizer creado - Integrador de propuestas");
Console.WriteLine();

// =============================================================================
// PASO 3: Construir el Group Chat Workflow
// =============================================================================

Console.WriteLine("Configurando Group Chat Workflow...");

// Construir el workflow con AgentWorkflowBuilder
// CreateGroupChatBuilderWith recibe una función factory que configura el manager
var workflow = AgentWorkflowBuilder
    .CreateGroupChatBuilderWith(agents => 
        new RoundRobinGroupChatManager(agents) 
        { 
            MaximumIterationCount = 5  // Máximo de 5 turnos (ajustado para demos)
        })
    .AddParticipants(brainstormAgent, criticAgent, synthesizerAgent)
    .Build();

Console.WriteLine("✓ Group Chat Workflow configurado");
Console.WriteLine("  └─ Manager: RoundRobinGroupChatManager");
Console.WriteLine("  └─ Máximo de iteraciones: 5");
Console.WriteLine("  └─ Participantes: 3 agentes (round-robin)");
Console.WriteLine();

// =============================================================================
// PASO 4: Definir el problema a discutir
// =============================================================================

var problem = "Crea un eslogan para un vehículo eléctrico ecológico.";

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    TAREA A RESOLVER");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine(problem);
Console.WriteLine();

// =============================================================================
// PASO 5: Ejecutar el Group Chat Workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    CONVERSACIÓN DEL GRUPO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// Crear lista de mensajes con el prompt inicial
var messages = new List<ChatMessage> { 
    new(ChatRole.User, problem) 
};

// Ejecutar el workflow como streaming
StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

// Contador de turnos para visualización
int turnCount = 0;
List<ChatMessage>? finalConversation = null;

// Procesar eventos del workflow
await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentRunUpdateEvent update)
    {
        // Procesar respuestas streaming de los agentes
        AgentRunResponse response = update.AsResponse();
        
        foreach (ChatMessage message in response.Messages)
        {
            // Detectar cambio de agente
            if (message.AuthorName != null)
            {
                turnCount++;
                var emoji = update.ExecutorId switch
                {
                    "CopyWriter" => "💡",
                    "Reviewer" => "🔍",
                    "Synthesizer" => "🎯",
                    _ => "👤"
                };
                
                Console.WriteLine($"\n┌─── Turno {turnCount}: {emoji} {update.ExecutorId} ───");
                Console.WriteLine("│");
            }
            
            // Mostrar el texto del mensaje
            if (message.Text != null)
            {
                foreach (var line in message.Text.Split('\n'))
                {
                    Console.WriteLine($"│ {line}");
                }
            }
        }
        
        if (response.Messages.Any())
        {
            Console.WriteLine("│");
            Console.WriteLine("└────────────────────────────────────────────────────────────────");
        }
    }
    else if (evt is WorkflowOutputEvent output)
    {
        // Workflow completado - guardar la conversación final
        finalConversation = output.As<List<ChatMessage>>();
        break;
    }
}

// =============================================================================
// PASO 6: Resumen de la conversación
// =============================================================================

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    CONVERSACIÓN FINAL");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");

if (finalConversation != null)
{
    Console.WriteLine();
    foreach (var message in finalConversation)
    {
        var author = message.AuthorName ?? "Usuario";
        var emoji = author switch
        {
            "CopyWriter" => "💡",
            "Reviewer" => "🔍",
            "Synthesizer" => "🎯",
            "Usuario" => "👤",
            _ => "👤"
        };
        
        Console.WriteLine($"{emoji} [{author}]");
        Console.WriteLine(message.Text ?? "(sin contenido)");
        Console.WriteLine("───────────────────────────────────────────────────────────────────");
    }
}

Console.WriteLine();
Console.WriteLine($"📊 Total de turnos: {turnCount}");
Console.WriteLine($"👥 Participantes: CopyWriter, Reviewer, Synthesizer");
Console.WriteLine();

// =============================================================================
// PASO 7: Validación del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ Group Chat Workflow ejecutó conversación multi-agente");
Console.WriteLine("✓ Cada agente participó según su rol (writer, reviewer, synthesizer)");
Console.WriteLine("✓ RoundRobinGroupChatManager coordinó los turnos");
Console.WriteLine("✓ Terminación por MaximumIterationCount");
Console.WriteLine("✓ Eventos procesados con streaming en tiempo real");
Console.WriteLine();
