// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de composición de agentes (agent-as-tool) con MAF
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// ===== Configuración =====
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var endpoint = configuration["AzureOpenAI:Endpoint"] 
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
var deploymentName = configuration["AzureOpenAI:DeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:DeploymentName no configurado");

// ===== Crear cliente de Azure OpenAI =====
var chatClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();

// ===== Crear Agente Especializado: Calculadora =====
// Este agente se dedicará exclusivamente a operaciones matemáticas
AIAgent calculatorAgent = chatClient.CreateAIAgent(
    name: "CalculadoraExperta",
    instructions: """
        Eres un experto matemático llamado CalculadoraExperta.
        Tu único propósito es resolver problemas matemáticos.
        
        Reglas:
        1. Solo respondes preguntas matemáticas
        2. Siempre muestras el proceso paso a paso
        3. Usas notación matemática clara
        4. Respondes en español
        5. Si no es una pregunta matemática, indica que solo puedes hacer cálculos
        
        Ejemplos de lo que puedes hacer:
        - Operaciones básicas (suma, resta, multiplicación, división)
        - Porcentajes y proporciones
        - Ecuaciones simples
        - Conversiones de unidades
        - Estadísticas básicas (promedio, mediana)
        """
);

Console.WriteLine("✓ CalculadoraExperta creada - Agente especializado en matemáticas");

// ===== Crear Función que Invoca al Agente Calculadora (Agent-as-Tool) =====
// Usamos AsAIFunction para exponer el agente como una herramienta
var calculateFunction = calculatorAgent.AsAIFunction(
    new AIFunctionFactoryOptions
    {
        Name = "calculate",
        Description = "Resuelve problemas matemáticos complejos. Usa esta función cuando el usuario tenga preguntas sobre cálculos, matemáticas, porcentajes, ecuaciones o estadísticas."
    }
);

// ===== Crear Agente Principal =====
// Este agente usa la función calculate para delegar tareas matemáticas
AIAgent mainAgent = chatClient.CreateAIAgent(
    name: "AsistenteGeneral",
    instructions: """
        Eres un asistente general llamado AsistenteGeneral.
        Puedes ayudar con muchas tareas, pero tienes acceso a un experto matemático.
        
        REGLAS IMPORTANTES:
        1. Para preguntas de matemáticas, cálculos, porcentajes o estadísticas:
           → USA la función 'calculate' para delegarlas al experto
        2. Para otras preguntas (conversación general, información, consejos):
           → Responde tú directamente
        
        Siempre responde en español de forma amigable.
        Cuando delegues a la calculadora, presenta los resultados de forma clara.
        """,
    tools: [calculateFunction]
);

Console.WriteLine("✓ AsistenteGeneral creado - Agente coordinador con delegación");

// ===== Crear Thread para la conversación =====
var thread = mainAgent.GetNewThread();

// ===== Interfaz de Usuario =====
Console.WriteLine("\n============================================");
Console.WriteLine("🤖 Composición de Agentes (Agent-as-Tool)");
Console.WriteLine("============================================");
Console.WriteLine($"Agente Principal: {mainAgent.Name}");
Console.WriteLine($"Agente Especializado: {calculatorAgent.Name}");
Console.WriteLine("============================================");
Console.WriteLine();
Console.WriteLine("💡 Prueba estas preguntas:");
Console.WriteLine("   📊 Matemáticas: '¿Cuánto es 15% de 850?'");
Console.WriteLine("   📊 Matemáticas: 'Calcula el promedio de 85, 92, 78, 95'");
Console.WriteLine("   💬 General: '¿Cuál es la capital de España?'");
Console.WriteLine("   💬 General: 'Dame consejos para aprender programación'");
Console.WriteLine();
Console.WriteLine("Escribe 'salir' para terminar");
Console.WriteLine("============================================\n");

// ===== Bucle de Conversación =====
while (true)
{
    Console.Write("👤 Tú: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n🤖 AsistenteGeneral: ¡Hasta pronto! 👋\n");
        break;
    }
    
    Console.Write($"🤖 {mainAgent.Name}: ");
    
    try
    {
        // Invocar el agente - automáticamente decidirá si llamar a CalculadoraExperta
        await foreach (var update in mainAgent.RunStreamingAsync(userInput, thread))
        {
            Console.Write(update);
        }
        Console.WriteLine("\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n❌ Error de conexión: {ex.Message}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}\n");
    }
}
