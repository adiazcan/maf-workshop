// =============================================================================
// Program.cs - Workflow Secuencial con Microsoft Agent Framework
// =============================================================================
// Descripción: Pipeline de 3 pasos donde cada agente procesa el resultado
// del agente anterior: ResearchAgent → WritingAgent → ReviewAgent
//
// Conceptos de Microsoft Agent Framework (MAF) demostrados:
// - AIAgent: Agente que usa modelos de chat para completar tareas
// - ChatMessage: Historial de conversación para cada agente
// - RunStreamingAsync: Invocación asíncrona con streaming de respuestas
// =============================================================================

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

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
Console.WriteLine("         WORKFLOW SECUENCIAL: Research → Write → Review");
Console.WriteLine("           Usando Microsoft Agent Framework (MAF)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear agentes especializados con Microsoft Agent Framework
// =============================================================================
// AIAgent es el tipo principal de agente en MAF.
// Se crea usando el patrón: AzureOpenAIClient.GetChatClient().CreateAIAgent()
// Cada agente tiene:
// - name: Identificador único del agente
// - instructions: Prompt del sistema que define su comportamiento
// =============================================================================

// Crear el cliente de Azure OpenAI (reutilizado para todos los agentes)
var openAIClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential());

// Agente 1: Investigador - Recopila información sobre un tema
var researchAgent = openAIClient
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "ResearchAgent",
        instructions: """
            Eres un investigador experto. Tu trabajo es:
            1. Analizar el tema solicitado
            2. Identificar 3-5 puntos clave importantes
            3. Proporcionar datos concretos y ejemplos relevantes
            4. Mantener un formato estructurado con viñetas
            
            Responde en español, de forma concisa pero informativa.
            Enfócate en información práctica y actualizada.
            """
    );

Console.WriteLine("✓ ResearchAgent creado - Especialista en investigación");

// Agente 2: Escritor - Transforma la investigación en un artículo
var writingAgent = openAIClient
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "WritingAgent",
        instructions: """
            Eres un escritor profesional de contenido técnico. Tu trabajo es:
            1. Tomar la información de investigación proporcionada
            2. Transformarla en un artículo bien estructurado
            3. Agregar una introducción atractiva y conclusión clara
            4. Usar un tono profesional pero accesible
            5. Incluir títulos y subtítulos apropiados
            
            Responde en español. El artículo debe tener:
            - Introducción (1 párrafo)
            - Cuerpo (2-3 secciones con subtítulos)
            - Conclusión (1 párrafo)
            """
    );

Console.WriteLine("✓ WritingAgent creado - Especialista en redacción");

// Agente 3: Revisor - Mejora y valida el artículo final
var reviewAgent = openAIClient
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "ReviewAgent",
        instructions: """
            Eres un editor profesional con experiencia en contenido técnico. Tu trabajo es:
            1. Revisar el artículo proporcionado
            2. Mejorar claridad y fluidez de lectura
            3. Corregir errores gramaticales o de estilo
            4. Agregar sugerencias de mejora al final
            5. Proporcionar la versión final pulida
            
            Responde en español. Proporciona:
            - El artículo final mejorado
            - Un breve resumen de cambios realizados (máximo 3 puntos)
            """
    );

Console.WriteLine("✓ ReviewAgent creado - Especialista en edición");
Console.WriteLine();

// =============================================================================
// PASO 3: Definir el tema a procesar
// =============================================================================

var topic = "El impacto de la inteligencia artificial generativa en el desarrollo de software moderno";

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine($"TEMA: {topic}");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 4: Ejecutar workflow secuencial con MAF
// =============================================================================
// En Microsoft Agent Framework, cada agente se invoca con RunStreamingAsync()
// pasando una lista de ChatMessage. El workflow secuencial pasa el resultado 
// de un agente al siguiente incluyéndolo en el prompt del siguiente mensaje.
// =============================================================================

// --- PASO 4.1: Research ---
Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ PASO 1/3: ResearchAgent - Investigando tema...                  │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");

// Lista de mensajes para el agente de investigación
var researchMessages = new List<ChatMessage>
{
    new UserChatMessage($"Investiga el siguiente tema: {topic}")
};

// RunStreamingAsync retorna un IAsyncEnumerable para streaming de respuestas
string researchResult = "";
await foreach (var update in researchAgent.RunStreamingAsync(researchMessages))
{
    researchResult += update;
}

Console.WriteLine();
Console.WriteLine("📊 RESULTADO DE INVESTIGACIÓN:");
Console.WriteLine("─────────────────────────────────────────────────────────────────");
Console.WriteLine(researchResult);
Console.WriteLine();

// --- PASO 4.2: Writing ---
Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ PASO 2/3: WritingAgent - Escribiendo artículo...                │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");

// Nueva lista de mensajes para el escritor - pasamos el resultado anterior en el prompt
// Este patrón de "inyección de contexto" es fundamental en workflows secuenciales
var writingMessages = new List<ChatMessage>
{
    new UserChatMessage($"""
        Basándote en la siguiente investigación, escribe un artículo completo:
        
        --- INVESTIGACIÓN ---
        {researchResult}
        --- FIN INVESTIGACIÓN ---
        
        Escribe el artículo ahora.
        """)
};

// Invocar al agente de escritura usando MAF
string writingResult = "";
await foreach (var update in writingAgent.RunStreamingAsync(writingMessages))
{
    writingResult += update;
}

Console.WriteLine();
Console.WriteLine("📝 BORRADOR DEL ARTÍCULO:");
Console.WriteLine("─────────────────────────────────────────────────────────────────");
Console.WriteLine(writingResult);
Console.WriteLine();

// --- PASO 4.3: Review ---
Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ PASO 3/3: ReviewAgent - Revisando y mejorando...                │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");

// Nueva lista de mensajes para el revisor - recibe el artículo del paso anterior
var reviewMessages = new List<ChatMessage>
{
    new UserChatMessage($"""
        Revisa y mejora el siguiente artículo:
        
        --- ARTÍCULO ---
        {writingResult}
        --- FIN ARTÍCULO ---
        
        Proporciona la versión final mejorada y un resumen de cambios.
        """)
};

// Invocar al agente de revisión usando MAF
string reviewResult = "";
await foreach (var update in reviewAgent.RunStreamingAsync(reviewMessages))
{
    reviewResult += update;
}

Console.WriteLine();
Console.WriteLine("✅ ARTÍCULO FINAL (REVISADO):");
Console.WriteLine("─────────────────────────────────────────────────────────────────");
Console.WriteLine(reviewResult);
Console.WriteLine();

// =============================================================================
// PASO 5: Resumen del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("📋 Resumen de ejecución:");
Console.WriteLine($"   • Paso 1 (Research): {researchResult.Length} caracteres generados");
Console.WriteLine($"   • Paso 2 (Writing):  {writingResult.Length} caracteres generados");
Console.WriteLine($"   • Paso 3 (Review):   {reviewResult.Length} caracteres generados");
Console.WriteLine();
Console.WriteLine("✓ El workflow secuencial ejecutó los 3 pasos en orden correcto");
Console.WriteLine("✓ Cada agente recibió el output del agente anterior como input");
Console.WriteLine();
