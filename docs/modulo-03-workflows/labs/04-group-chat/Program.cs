// =============================================================================
// Program.cs - Group Chat con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa un AgentGroupChat donde 3 agentes 
// colaboran en una discusión: BrainstormAgent, CriticAgent, SynthesizerAgent.
// La conversación continúa hasta alcanzar consenso o máximo de turnos.
//
// Conceptos demostrados:
// - AgentGroupChat para conversaciones multi-agente
// - Estrategias de terminación (MaxTurns, Keyword)
// - Agentes con roles complementarios
// - Historial de chat compartido
// =============================================================================

using Microsoft.AI.Agents;
using Microsoft.AI.Agents.Chat;
using Microsoft.AI.Agents.Chat.Termination;
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
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("Falta configuración: AzureOpenAI:ApiKey. Use 'dotnet user-secrets set AzureOpenAI:ApiKey TU-API-KEY'");

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("         GROUP CHAT: Brainstorm ↔ Critic ↔ Synthesizer");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear agentes colaborativos
// =============================================================================

// Agente 1: Brainstormer - Genera ideas creativas
var brainstormAgent = new ChatCompletionAgent(
    name: "BrainstormAgent",
    instructions: """
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
        
        🏁 CONSENSO:
        Si crees que el grupo ha llegado a una buena solución, di exactamente:
        "CONSENSO ALCANZADO: [resumen de la solución]"
        
        Responde en español.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

// Agente 2: Critic - Evalúa y cuestiona ideas
var criticAgent = new ChatCompletionAgent(
    name: "CriticAgent",
    instructions: """
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
        
        🏁 CONSENSO:
        Si crees que una idea ya aborda tus preocupaciones, di:
        "CONSENSO ALCANZADO: [resumen de la solución]"
        
        Responde en español.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

// Agente 3: Synthesizer - Combina y resume ideas
var synthesizerAgent = new ChatCompletionAgent(
    name: "SynthesizerAgent",
    instructions: """
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
        
        🏁 CONSENSO:
        Cuando veas que el grupo converge en una solución, declara:
        "CONSENSO ALCANZADO: [propuesta final detallada]"
        
        Responde en español.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

Console.WriteLine("✓ BrainstormAgent creado - Generador de ideas");
Console.WriteLine("✓ CriticAgent creado - Evaluador constructivo");
Console.WriteLine("✓ SynthesizerAgent creado - Integrador de propuestas");
Console.WriteLine();

// =============================================================================
// PASO 3: Crear el AgentGroupChat con estrategia de terminación
// =============================================================================

Console.WriteLine("Configurando AgentGroupChat...");

// Crear condición de terminación combinada:
// 1. Máximo 10 turnos (fallback de seguridad)
// 2. Keyword "CONSENSO ALCANZADO" (terminación por consenso)
var terminationCondition = new AggregatedTerminationCondition(
    new MaxTurnsTerminationCondition(10),
    new KeywordTerminationCondition("CONSENSO ALCANZADO")
);

// Crear el grupo de chat con los 3 agentes
var groupChat = new AgentGroupChat(brainstormAgent, criticAgent, synthesizerAgent)
{
    TerminationCondition = terminationCondition,
    // Estrategia de selección: round-robin entre agentes
    SelectionStrategy = new RoundRobinSelectionStrategy()
};

Console.WriteLine("✓ AgentGroupChat configurado");
Console.WriteLine("  └─ Estrategia de terminación: MaxTurns(10) OR Keyword('CONSENSO ALCANZADO')");
Console.WriteLine("  └─ Selección de agente: Round-robin");
Console.WriteLine();

// =============================================================================
// PASO 4: Definir el problema a discutir
// =============================================================================

var problem = """
    ¿Cómo podemos mejorar la experiencia de onboarding de nuevos usuarios 
    en nuestra aplicación móvil de fitness? Actualmente el 60% abandona 
    antes de completar el registro.
    """;

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    PROBLEMA A RESOLVER");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine(problem);
Console.WriteLine();

// =============================================================================
// PASO 5: Ejecutar el Group Chat
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    CONVERSACIÓN DEL GRUPO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// Agregar el problema inicial al chat
groupChat.AddChatMessage(new ChatMessageContent(
    role: AuthorRole.User,
    content: problem
));

// Contador de turnos para visualización
int turnCount = 0;
string? consensusMessage = null;

// Ejecutar la conversación
await foreach (var message in groupChat.InvokeAsync())
{
    turnCount++;
    
    // Mostrar el mensaje con formato
    var agentName = message.AuthorName ?? "Unknown";
    var emoji = agentName switch
    {
        "BrainstormAgent" => "💡",
        "CriticAgent" => "🔍",
        "SynthesizerAgent" => "🎯",
        _ => "👤"
    };

    Console.WriteLine($"┌─── Turno {turnCount}: {emoji} {agentName} ───");
    Console.WriteLine("│");
    foreach (var line in (message.Content ?? "").Split('\n'))
    {
        Console.WriteLine($"│ {line}");
    }
    Console.WriteLine("│");
    Console.WriteLine("└────────────────────────────────────────────────────────────────");
    Console.WriteLine();

    // Verificar si se alcanzó consenso
    if (message.Content?.Contains("CONSENSO ALCANZADO") == true)
    {
        consensusMessage = message.Content;
    }
}

// =============================================================================
// PASO 6: Resumen de la conversación
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    RESUMEN DE LA SESIÓN");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

Console.WriteLine($"📊 Total de turnos: {turnCount}");
Console.WriteLine($"👥 Participantes: BrainstormAgent, CriticAgent, SynthesizerAgent");
Console.WriteLine();

if (consensusMessage != null)
{
    Console.WriteLine("✅ RESULTADO: Consenso alcanzado");
    Console.WriteLine();
    Console.WriteLine("📋 PROPUESTA FINAL:");
    Console.WriteLine("─────────────────────────────────────────────────────────────────");
    
    // Extraer solo la parte del consenso
    var consensusStart = consensusMessage.IndexOf("CONSENSO ALCANZADO:");
    if (consensusStart >= 0)
    {
        Console.WriteLine(consensusMessage.Substring(consensusStart));
    }
    else
    {
        Console.WriteLine(consensusMessage);
    }
}
else
{
    Console.WriteLine("⚠️ RESULTADO: Máximo de turnos alcanzado sin consenso explícito");
    Console.WriteLine("   El grupo discutió el problema pero no declaró un consenso formal.");
}

Console.WriteLine();

// =============================================================================
// PASO 7: Validación del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ AgentGroupChat ejecutó conversación multi-agente");
Console.WriteLine("✓ Cada agente participó según su rol (brainstorm, critic, synthesize)");
Console.WriteLine($"✓ Terminación: {(consensusMessage != null ? "Por consenso" : "Por máximo de turnos")}");
Console.WriteLine("✓ Round-robin aseguró participación equitativa");
Console.WriteLine();
