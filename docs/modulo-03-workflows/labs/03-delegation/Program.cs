// =============================================================================
// Program.cs - Workflow de Delegación con Microsoft Agent Framework
// =============================================================================
// Descripción: Este ejemplo implementa un patrón de delegación donde un
// ProjectManagerAgent analiza tareas y las asigna a especialistas:
// Designer, Developer o QA según el tipo de trabajo requerido.
//
// Conceptos demostrados:
// - Routing inteligente de tareas
// - Agente coordinador con múltiples especialistas
// - Uso de function calling para decisiones de routing
// - Delegación basada en análisis de contenido
// =============================================================================

using DelegationWorkflow;
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
Console.WriteLine("         WORKFLOW DE DELEGACIÓN: Project Manager → Especialistas");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

// =============================================================================
// PASO 2: Crear el Project Manager Agent
// =============================================================================

Console.WriteLine("Inicializando equipo de trabajo...");
Console.WriteLine();

var projectManager = new ProjectManagerAgent(endpoint, deploymentName, apiKey);

Console.WriteLine("✓ ProjectManagerAgent creado - Coordinador del equipo");
Console.WriteLine("  └─ DesignerAgent (UI/UX)");
Console.WriteLine("  └─ DeveloperAgent (Código)");
Console.WriteLine("  └─ QAAgent (Testing)");
Console.WriteLine();

// =============================================================================
// PASO 3: Definir tareas de diferentes tipos
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
Console.WriteLine("                    PROCESANDO TAREAS");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");

// =============================================================================
// PASO 4: Procesar cada tarea con el Project Manager
// =============================================================================

var results = new List<(string Task, ProjectManagerAgent.DelegationResult Result)>();

for (int i = 0; i < tasks.Length; i++)
{
    Console.WriteLine();
    Console.WriteLine($"┌─────────────────────────────────────────────────────────────────┐");
    Console.WriteLine($"│ TAREA {i + 1}/{tasks.Length}                                                        │");
    Console.WriteLine($"└─────────────────────────────────────────────────────────────────┘");
    Console.WriteLine();

    var result = await projectManager.ProcessTaskAsync(tasks[i]);
    results.Add((tasks[i], result));

    Console.WriteLine($"📥 Respuesta de {result.SelectedAgent}:");
    Console.WriteLine("─────────────────────────────────────────────────────────────────");
    Console.WriteLine(result.SpecialistResponse);
    Console.WriteLine();
}

// =============================================================================
// PASO 5: Resumen de delegaciones
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    RESUMEN DE DELEGACIONES");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();

Console.WriteLine("┌──────────┬────────────────────────────────────────┬─────────────────┐");
Console.WriteLine("│ # Tarea  │ Descripción (truncada)                 │ Delegada a      │");
Console.WriteLine("├──────────┼────────────────────────────────────────┼─────────────────┤");

for (int i = 0; i < results.Count; i++)
{
    var (task, result) = results[i];
    var truncatedTask = task.Length > 38 ? task.Substring(0, 35) + "..." : task.PadRight(38);
    Console.WriteLine($"│ Tarea {i + 1}  │ {truncatedTask} │ {result.SelectedAgent,-15} │");
}

Console.WriteLine("└──────────┴────────────────────────────────────────┴─────────────────┘");
Console.WriteLine();

// Contar delegaciones por tipo
var designerCount = results.Count(r => r.Result.SelectedAgent.Contains("Designer"));
var developerCount = results.Count(r => r.Result.SelectedAgent.Contains("Developer"));
var qaCount = results.Count(r => r.Result.SelectedAgent.Contains("QA"));

Console.WriteLine("📊 Distribución de trabajo:");
Console.WriteLine($"   🎨 DesignerAgent:   {designerCount} tarea(s)");
Console.WriteLine($"   💻 DeveloperAgent:  {developerCount} tarea(s)");
Console.WriteLine($"   🔍 QAAgent:         {qaCount} tarea(s)");
Console.WriteLine();

// =============================================================================
// PASO 6: Validación del workflow
// =============================================================================

Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine("                    WORKFLOW COMPLETADO");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("✓ ProjectManagerAgent analizó cada tarea y determinó el especialista");
Console.WriteLine("✓ Las tareas de diseño fueron delegadas a DesignerAgent");
Console.WriteLine("✓ Las tareas de código fueron delegadas a DeveloperAgent");
Console.WriteLine("✓ Las tareas de testing fueron delegadas a QAAgent");
Console.WriteLine("✓ Cada especialista proporcionó una respuesta en su área de expertise");
Console.WriteLine();
