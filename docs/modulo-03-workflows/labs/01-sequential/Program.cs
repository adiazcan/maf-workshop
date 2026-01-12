// =============================================================================
// Program.cs - Workflow Secuencial con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa un pipeline de 3 pasos donde cada agente
// procesa el resultado del agente anterior: ResearchAgent → WritingAgent → ReviewAgent
//
// Conceptos demostrados:
// - Creación de múltiples agentes especializados
// - Paso de resultados entre agentes en secuencia
// - Orquestación manual de flujo secuencial
// =============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Chat;
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
Console.WriteLine("         WORKFLOW SECUENCIAL: Research → Write → Review");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear agentes especializados
// =============================================================================

// Agente 1: Investigador - Recopila información sobre un tema
var researchAgent = new ChatCompletionAgent(
    name: "ResearchAgent",
    instructions: """
        Eres un investigador experto. Tu trabajo es:
        1. Analizar el tema solicitado
        2. Identificar 3-5 puntos clave importantes
        3. Proporcionar datos concretos y ejemplos relevantes
        4. Mantener un formato estructurado con viñetas
        
        Responde en español, de forma concisa pero informativa.
        Enfócate en información práctica y actualizada.
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

Console.WriteLine("✓ ResearchAgent creado - Especialista en investigación");

// Agente 2: Escritor - Transforma la investigación en un artículo
var writingAgent = new ChatCompletionAgent(
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
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

Console.WriteLine("✓ WritingAgent creado - Especialista en redacción");

// Agente 3: Revisor - Mejora y valida el artículo final
var reviewAgent = new ChatCompletionAgent(
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
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
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
// PASO 4: Ejecutar workflow secuencial
// =============================================================================

// --- PASO 4.1: Research ---
Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ PASO 1/3: ResearchAgent - Investigando tema...                  │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");

// Crear historial de chat para el investigador
var researchChat = new ChatHistory();
researchChat.AddUserMessage($"Investiga el siguiente tema: {topic}");

// Invocar al agente de investigación
string researchResult = "";
await foreach (var message in researchAgent.InvokeAsync(researchChat))
{
    researchResult += message.Content;
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

// Crear historial de chat para el escritor, pasando el resultado anterior
var writingChat = new ChatHistory();
writingChat.AddUserMessage($"""
    Basándote en la siguiente investigación, escribe un artículo completo:
    
    --- INVESTIGACIÓN ---
    {researchResult}
    --- FIN INVESTIGACIÓN ---
    
    Escribe el artículo ahora.
    """);

// Invocar al agente de escritura
string writingResult = "";
await foreach (var message in writingAgent.InvokeAsync(writingChat))
{
    writingResult += message.Content;
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

// Crear historial de chat para el revisor, pasando el artículo
var reviewChat = new ChatHistory();
reviewChat.AddUserMessage($"""
    Revisa y mejora el siguiente artículo:
    
    --- ARTÍCULO ---
    {writingResult}
    --- FIN ARTÍCULO ---
    
    Proporciona la versión final mejorada y un resumen de cambios.
    """);

// Invocar al agente de revisión
string reviewResult = "";
await foreach (var message in reviewAgent.InvokeAsync(reviewChat))
{
    reviewResult += message.Content;
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
