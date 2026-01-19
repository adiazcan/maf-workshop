# 📘 Workshop Microsoft Agent Framework - Material del Participante

**Versión**: 1.0  
**Duración**: 7 horas  
**Framework**: Microsoft Agent Framework 1.0.0  
**Target**: .NET 10

---

## 📋 Tabla de Contenidos

1. [Información General](#información-general)
2. [Módulo 1: Fundamentos de MAF](#módulo-1-fundamentos-de-maf)
3. [Módulo 2: Function Tools y Composición](#módulo-2-function-tools-y-composición)
4. [Módulo 3: Workflows Multi-Agente](#módulo-3-workflows-multi-agente)
5. [Módulo 4: Observabilidad](#módulo-4-observabilidad)
6. [Módulo 5: ASP.NET Core y Aspire](#módulo-5-aspnet-core-y-aspire)
7. [Módulo 6: DevUI](#módulo-6-devui)
8. [Módulo 7: Model Context Protocol](#módulo-7-model-context-protocol)
9. [Apéndices](#apéndices)

---

## Información General

### Objetivos del Workshop

Al finalizar este workshop, serás capaz de:

✅ Construir agentes conversacionales usando Microsoft Agent Framework  
✅ Extender agentes con function tools personalizadas  
✅ Orquestar múltiples agentes en workflows complejos  
✅ Implementar observabilidad con OpenTelemetry y Azure Monitor  
✅ Desarrollar aplicaciones web multi-agente con ASP.NET Core  
✅ Usar .NET Aspire para orquestar servicios distribuidos  
✅ Debugging de agentes con DevUI  
✅ Comprender la interoperabilidad con Model Context Protocol

### Prerequisitos

- C# y .NET (nivel intermedio)
- Visual Studio Code o Visual Studio 2022
- .NET 10 SDK instalado
- Cuenta de Azure con acceso a Azure OpenAI Service

### Estructura de Horario

| Hora | Módulo | Duración |
|------|--------|----------|
| 09:00-09:30 | Setup + Bienvenida | 30 min |
| 09:30-10:30 | Módulo 1: Fundamentos | 60 min |
| 10:30-10:45 | BREAK | 15 min |
| 10:45-12:00 | Módulo 2: Function Tools | 75 min |
| 12:00-13:00 | LUNCH | 60 min |
| 13:00-14:30 | Módulo 3: Workflows | 90 min |
| 14:30-14:45 | BREAK | 15 min |
| 14:45-15:45 | Módulo 4: Observabilidad | 60 min |
| 15:45-16:00 | BREAK | 15 min |
| 16:00-17:00 | Módulo 5: ASP.NET + Aspire | 60 min |
| 17:00-17:15 | Módulo 6: DevUI | 15 min |
| 17:15-17:30 | Módulo 7: MCP | 15 min |

### Checkpoints de Validación

Cada módulo tiene checkpoints para validar tu progreso:

- **M1**: 90% de participantes con "Hello Agent" funcionando
- **M2.1**: 85% con function tool ejecutándose
- **M2.2**: 85% con composición de agentes
- **M3.1**: 80% con workflow secuencial
- **M3.2**: 80% con workflow paralelo
- **M4.1**: 75% con métricas capturadas
- **M4.2**: 75% con traces en Azure Monitor
- **M5.1**: 70% con endpoint API funcionando
- **M5.2**: 70% con Aspire Dashboard activo
- **M6**: 100% demo (sin lab)
- **M7**: 80% comprensión conceptual

---

<div style="page-break-after: always;"></div>

## Módulo 1: Fundamentos de MAF

**Duración**: 60 minutos  
**Objetivo**: Construir tu primer agente conversacional

### Teoría (20 minutos)

#### ¿Qué es Microsoft Agent Framework?

Microsoft Agent Framework (MAF) es un framework nativo de .NET para construir **agentes de IA inteligentes** que pueden:

- Mantener conversaciones naturales
- Usar herramientas (function calling)
- Colaborar con otros agentes
- Ejecutar tareas complejas de forma autónoma

**Características clave**:
- 🔹 Nativo de .NET con APIs idiomáticas
- 🔹 Integración con Azure OpenAI Service
- 🔹 Soporte para múltiples LLMs
- 🔹 Observabilidad integrada con OpenTelemetry
- 🔹 Workflows multi-agente (secuencial, paralelo, delegación)

#### Tipos de Agentes

**1. ChatCompletionAgent**
- Para conversaciones multi-turn
- Basado en modelos de chat (gpt-5.2, gpt-5.2-chat)
- Control completo sobre mensajes y contexto

**2. OpenAI Assistants Agent**
- Usa Azure OpenAI Assistants API
- State management automático
- Herramientas integradas (code interpreter, file search)

#### Arquitectura de un Agente

```
┌─────────────────────────────────────────────┐
│           Tu Aplicación                     │
│                                             │
│  ┌────────────────────────────────────┐    │
│  │  ChatCompletionAgent               │    │
│  │  - Model: "gpt-5.2"                │    │
│  │  - Instructions: "..."             │    │
│  │  - Tools: [...]                    │    │
│  └────────────┬───────────────────────┘    │
│               │                             │
│               │ InvokeAsync(messages)       │
│               ▼                             │
│  ┌────────────────────────────────────┐    │
│  │  Microsoft.Extensions.AI.Agents    │    │
│  │  - Message orchestration           │    │
│  │  - Tool invocation                 │    │
│  │  - State management                │    │
│  └────────────┬───────────────────────┘    │
│               │                             │
└───────────────┼─────────────────────────────┘
                │
                │ REST API
                ▼
┌─────────────────────────────────────────────┐
│      Azure OpenAI Service                   │
│      - Model: gpt-5.2                       │
│      - Endpoint: *.openai.azure.com         │
└─────────────────────────────────────────────┘
```

### Lab 1.1: Hello Agent (40 minutos)

#### Objetivo
Crear tu primer agente conversacional que responde preguntas sobre .NET.

#### Pasos

**1. Crear proyecto**

```bash
mkdir HelloAgent
cd HelloAgent
dotnet new console
dotnet add package Microsoft.Extensions.AI.Agents --version 1.0.0-preview.260108.1
dotnet add package Azure.AI.OpenAI --version 2.5.0
```

**2. Configurar user secrets**

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://[tu-recurso].openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "[tu-api-key]"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-5.2"
```

**3. Código del agente (Program.cs)**

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI.Agents;

// 1. Leer configuración de user secrets
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var endpoint = config["AzureOpenAI:Endpoint"]!;
var apiKey = config["AzureOpenAI:ApiKey"]!;
var deploymentName = config["AzureOpenAI:DeploymentName"]!;

// 2. Crear cliente de Azure OpenAI
var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey)
);

var chatClient = client.GetChatClient(deploymentName);

// 3. Configurar agente
var agent = new ChatCompletionAgent
{
    Name = "DotNetExpert",
    Instructions = @"
        Eres un experto en .NET y C#.
        
        Tus responsabilidades:
        - Responder preguntas sobre .NET, C#, ASP.NET Core
        - Proporcionar ejemplos de código cuando sea apropiado
        - Usar un tono profesional pero amigable
        - Responder siempre en español
        
        Limitaciones:
        - No proporcionar información sobre otros lenguajes de programación
        - Si no sabes algo, admítelo honestamente
    ",
    ChatClient = chatClient
};

// 4. Loop de conversación
Console.WriteLine("=== DotNet Expert Agent ===");
Console.WriteLine("Escribe 'salir' para terminar\n");

var messages = new List<ChatMessage>();

while (true)
{
    Console.Write("Usuario: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "salir")
        break;
    
    // Agregar mensaje del usuario
    messages.Add(new ChatMessage(ChatRole.User, userInput));
    
    // Invocar agente
    var response = await agent.InvokeAsync(messages);
    
    // Agregar respuesta al historial
    messages.Add(new ChatMessage(ChatRole.Assistant, response));
    
    // Mostrar respuesta
    Console.WriteLine($"\nAgente: {response}\n");
}

Console.WriteLine("¡Hasta luego!");
```

**4. Ejecutar**

```bash
dotnet run
```

**Output esperado**:

```
=== DotNet Expert Agent ===
Escribe 'salir' para terminar

Usuario: ¿Qué es .NET 10?

Agente: .NET 10 es la última versión del framework .NET de Microsoft, lanzada en noviembre de 2025.
Es una versión LTS (Long Term Support) con soporte hasta noviembre de 2028.

Mejoras clave:
- Performance mejorado en JIT y GC
- Nuevas APIs en ASP.NET Core
- Soporte nativo para IA con Microsoft.Extensions.AI
- ...

Usuario: salir
¡Hasta luego!
```

#### Checkpoint de Validación

✅ Tu código compila sin errores  
✅ El agente responde a tus preguntas  
✅ Las respuestas están en español  
✅ Puedes tener una conversación multi-turn (el agente recuerda contexto)

#### Troubleshooting

| Problema | Solución |
|----------|----------|
| "401 Unauthorized" | Verificar API key y endpoint en user secrets |
| "Deployment not found" | Verificar que deployment name coincide con Azure Portal |
| "Respuesta vacía" | Agregar `Console.WriteLine($"DEBUG: {response}")` |

---

<div style="page-break-after: always;"></div>

## Módulo 2: Function Tools y Composición

**Duración**: 75 minutos  
**Objetivo**: Extender agentes con herramientas personalizadas y composición

### Teoría (15 minutos)

#### ¿Qué son Function Tools?

Las **function tools** permiten a los agentes ejecutar código C# para:

- Consultar bases de datos
- Llamar APIs externas
- Realizar cálculos
- Interactuar con sistemas empresariales

**Flujo de ejecución**:

```
Usuario: "¿Cuál es el clima en Madrid?"
          ↓
Agente (LLM): "Necesito llamar a GetClima('Madrid')"
          ↓
MAF Framework: Ejecuta función GetClima('Madrid')
          ↓
Function: Retorna "25°C, soleado"
          ↓
Agente (LLM): "El clima en Madrid es 25°C y soleado"
          ↓
Usuario: Recibe respuesta final
```

#### Anatomía de una Function Tool

```csharp
public class ClimaPlugin
{
    [KernelFunction]  // ← Marca la función como tool
    [Description("Obtiene el clima actual de una ciudad")]  // ← LLM usa esto
    public async Task<string> GetClima(
        [Description("Nombre de la ciudad")] string ciudad  // ← Descripción de parámetro
    )
    {
        // Lógica real de la función
        var clima = await _apiClima.GetAsync(ciudad);
        return $"{clima.Temperatura}°C, {clima.Condicion}";
    }
}
```

**Elementos críticos**:
- `[KernelFunction]`: Marca la función como invocable por el agente
- `[Description]`: Descripción que el LLM usa para decidir cuándo llamarla
- Parámetros con `[Description]`: Ayuda al LLM a pasar los argumentos correctos

#### Composición de Agentes

Un agente puede usar **otro agente como tool**:

```csharp
var agenteEspecializado = new ChatCompletionAgent { ... };
var agentePrincipal = new ChatCompletionAgent
{
    Tools = [agenteEspecializado]  // ← Agente como herramienta
};
```

**Casos de uso**:
- Agente principal → Delega a agente experto en SQL
- Agente orquestador → Coordina múltiples agentes especializados

### Lab 2.1: Weather Tool (25 minutos)

#### Objetivo
Crear un agente con function tool para consultar el clima.

#### Código

**WeatherPlugin.cs**:

```csharp
using System.ComponentModel;
using Microsoft.Extensions.AI.Agents;

public class WeatherPlugin
{
    [KernelFunction]
    [Description("Obtiene el clima actual de una ciudad específica")]
    public async Task<string> GetClima(
        [Description("Nombre de la ciudad (ej: Madrid, Barcelona)")] string ciudad
    )
    {
        Console.WriteLine($"[TOOL CALL] GetClima('{ciudad}')");
        
        // Simulación (en producción, llamarías a una API real)
        await Task.Delay(500); // Simular latencia de API
        
        var random = new Random();
        var temperatura = random.Next(15, 35);
        var condiciones = new[] { "soleado", "nublado", "lluvioso", "ventoso" };
        var condicion = condiciones[random.Next(condiciones.Length)];
        
        return $"El clima en {ciudad} es {temperatura}°C y está {condicion}.";
    }
    
    [KernelFunction]
    [Description("Obtiene el pronóstico del clima para los próximos N días")]
    public async Task<string> GetPronostico(
        [Description("Nombre de la ciudad")] string ciudad,
        [Description("Número de días (1-7)")] int dias
    )
    {
        Console.WriteLine($"[TOOL CALL] GetPronostico('{ciudad}', {dias})");
        
        await Task.Delay(500);
        
        var pronostico = new StringBuilder();
        pronostico.AppendLine($"Pronóstico para {ciudad} ({dias} días):");
        
        for (int i = 1; i <= dias; i++)
        {
            var temp = new Random().Next(15, 35);
            pronostico.AppendLine($"- Día {i}: {temp}°C");
        }
        
        return pronostico.ToString();
    }
}
```

**Program.cs**:

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI.Agents;

// Configuración (igual que Lab 1.1)
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var endpoint = config["AzureOpenAI:Endpoint"]!;
var apiKey = config["AzureOpenAI:ApiKey"]!;
var deploymentName = config["AzureOpenAI:DeploymentName"]!;

var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey)
);

var chatClient = client.GetChatClient(deploymentName);

// Crear plugin
var weatherPlugin = new WeatherPlugin();

// Crear agente con tool
var agent = new ChatCompletionAgent
{
    Name = "WeatherAssistant",
    Instructions = @"
        Eres un asistente meteorológico.
        
        Puedes:
        - Consultar el clima actual de ciudades
        - Proporcionar pronósticos extendidos
        
        Cuando el usuario pregunte sobre el clima, DEBES usar tus herramientas.
        Responde siempre en español de forma amigable.
    ",
    ChatClient = chatClient,
    Tools = [weatherPlugin]  // ← Registrar plugin
};

// Loop de conversación
Console.WriteLine("=== Weather Assistant ===");
Console.WriteLine("Pregunta sobre el clima de cualquier ciudad\n");

var messages = new List<ChatMessage>();

while (true)
{
    Console.Write("Usuario: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "salir")
        break;
    
    messages.Add(new ChatMessage(ChatRole.User, userInput));
    
    var response = await agent.InvokeAsync(messages);
    
    messages.Add(new ChatMessage(ChatRole.Assistant, response));
    
    Console.WriteLine($"\nAgente: {response}\n");
}
```

#### Checkpoint de Validación

✅ Ves `[TOOL CALL] GetClima('...')` en consola cuando preguntas sobre el clima  
✅ El agente usa la función automáticamente (no solo describe que debería usarla)  
✅ Funciona con múltiples ciudades

**Test rápido**:
```
Usuario: ¿Qué clima hace en Madrid?
[TOOL CALL] GetClima('Madrid')  ← Debes ver esto
Agente: El clima en Madrid es 28°C y está soleado.
```

### Lab 2.2: Agent Composition (25 minutos)

#### Objetivo
Crear un agente principal que delega a un agente especializado en SQL.

#### Código

**SqlExpertAgent.cs**:

```csharp
public static class SqlExpertAgent
{
    public static ChatCompletionAgent Create(IChatClient chatClient)
    {
        return new ChatCompletionAgent
        {
            Name = "SqlExpert",
            Instructions = @"
                Eres un experto en SQL y bases de datos.
                
                Tu responsabilidad:
                - Generar queries SQL optimizadas
                - Explicar conceptos de bases de datos
                - Sugerir índices y optimizaciones
                
                Responde SOLO sobre SQL - si te preguntan sobre otros temas, 
                indica que no es tu área de expertise.
            ",
            ChatClient = chatClient
        };
    }
}
```

**Program.cs**:

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI.Agents;

// Configuración
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var endpoint = config["AzureOpenAI:Endpoint"]!;
var apiKey = config["AzureOpenAI:ApiKey"]!;
var deploymentName = config["AzureOpenAI:DeploymentName"]!;

var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey)
);

var chatClient = client.GetChatClient(deploymentName);

// Crear agente especializado en SQL
var sqlAgent = SqlExpertAgent.Create(chatClient);

// Crear agente principal que usa al agente SQL como tool
var mainAgent = new ChatCompletionAgent
{
    Name = "GeneralAssistant",
    Instructions = @"
        Eres un asistente general para desarrolladores.
        
        Para preguntas sobre SQL o bases de datos, DEBES delegar al SqlExpert.
        Para otros temas, responde directamente.
        
        Responde en español.
    ",
    ChatClient = chatClient,
    Tools = [sqlAgent]  // ← Agente como herramienta
};

// Loop de conversación
Console.WriteLine("=== General Assistant (con SqlExpert) ===\n");

var messages = new List<ChatMessage>();

while (true)
{
    Console.Write("Usuario: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "salir")
        break;
    
    messages.Add(new ChatMessage(ChatRole.User, userInput));
    
    Console.WriteLine("[Procesando...]");
    var response = await mainAgent.InvokeAsync(messages);
    
    messages.Add(new ChatMessage(ChatRole.Assistant, response));
    
    Console.WriteLine($"\nAgente: {response}\n");
}
```

#### Checkpoint de Validación

✅ Preguntas sobre SQL son manejadas por el agente especializado  
✅ Preguntas generales son manejadas por el agente principal  
✅ La delegación es transparente para el usuario

**Test rápido**:
```
Usuario: Dame un query SQL para obtener usuarios activos
[El agente principal delega a SqlExpert]
Agente: SELECT * FROM Users WHERE IsActive = 1;

Usuario: ¿Qué es .NET?
[El agente principal responde directamente]
Agente: .NET es un framework de desarrollo...
```

### Lab 2.3: Human-in-the-Loop (25 minutos)

#### Objetivo
Implementar aprobación humana antes de ejecutar acciones sensibles.

#### Código

**BankingPlugin.cs**:

```csharp
public class BankingPlugin
{
    [KernelFunction]
    [Description("Transfiere dinero entre cuentas - REQUIERE APROBACIÓN HUMANA")]
    public async Task<string> TransferirDinero(
        [Description("Cuenta origen")] string cuentaOrigen,
        [Description("Cuenta destino")] string cuentaDestino,
        [Description("Monto a transferir")] decimal monto
    )
    {
        Console.WriteLine($"\n⚠️  SOLICITUD DE APROBACIÓN");
        Console.WriteLine($"Acción: Transferir ${monto:N2}");
        Console.WriteLine($"Desde: {cuentaOrigen}");
        Console.WriteLine($"Hacia: {cuentaDestino}");
        Console.Write("¿Aprobar? (s/n): ");
        
        var aprobacion = Console.ReadLine()?.ToLower();
        
        if (aprobacion == "s" || aprobacion == "si" || aprobacion == "sí")
        {
            // Simular transferencia
            await Task.Delay(1000);
            return $"✅ Transferencia aprobada y completada: ${monto:N2} de {cuentaOrigen} a {cuentaDestino}";
        }
        else
        {
            return $"❌ Transferencia rechazada por el usuario.";
        }
    }
}
```

**Program.cs**:

```csharp
// Configuración (omitida para brevedad)

var bankingPlugin = new BankingPlugin();

var agent = new ChatCompletionAgent
{
    Name = "BankingAssistant",
    Instructions = @"
        Eres un asistente bancario.
        
        Puedes ayudar con transferencias de dinero usando tu herramienta TransferirDinero.
        
        SIEMPRE confirma los detalles con el usuario antes de proceder.
    ",
    ChatClient = chatClient,
    Tools = [bankingPlugin]
};

// Loop de conversación (similar a labs anteriores)
```

#### Checkpoint de Validación

✅ Ves el prompt de aprobación antes de ejecutar la transferencia  
✅ Puedes rechazar acciones sensibles  
✅ El agente maneja apropiadamente las aprobaciones y rechazos

---

<div style="page-break-after: always;"></div>

## Módulo 3: Workflows Multi-Agente

**Duración**: 90 minutos  
**Objetivo**: Orquestar múltiples agentes en patrones de workflow

### Teoría (20 minutos)

#### Patrones de Workflow

**1. Sequential Workflow**
```
Agente A → Agente B → Agente C → Resultado
```
- Cada agente procesa el output del anterior
- Ejemplo: Research → Análisis → Resumen

**2. Parallel Workflow**
```
         ┌─ Agente A ─┐
Input ───┼─ Agente B ─┼─→ Agregador → Resultado
         └─ Agente C ─┘
```
- Múltiples agentes procesan en paralelo
- Ejemplo: Traducción a múltiples idiomas simultáneamente

**3. Delegation Workflow**
```
Agente Principal
  ├─→ Delega a Agente SQL
  ├─→ Delega a Agente Email
  └─→ Agrega resultados
```
- Agente orquestador delega a especialistas
- Ejemplo: Asistente virtual con múltiples capacidades

**4. Group Chat Workflow**
```
Agente 1 ←→ Agente 2
    ↕          ↕
Agente 3 ←→ Agente 4
```
- Múltiples agentes colaboran en una conversación
- Ejemplo: Panel de expertos debatiendo una solución

#### Azure AI Agent Service

Para workflows con estado persistente (pausar/reanudar):

```csharp
var agentService = new AzureAIAgentService(connectionString);

var workflow = await agentService.CreateWorkflowAsync(new WorkflowConfig
{
    Name = "CustomerSupport",
    Agents = [agent1, agent2, agent3],
    Type = WorkflowType.Sequential
});

// El workflow puede pausarse y reanudarse en el futuro
var threadId = await workflow.StartAsync(input);
// ... (pausar/continuar más tarde) ...
var result = await workflow.ResumeAsync(threadId);
```

### Lab 3.1: Sequential Workflow (20 minutos)

#### Objetivo
Crear pipeline de research → análisis → resumen

#### Código

```csharp
// 1. Agente de Research
var researchAgent = new ChatCompletionAgent
{
    Name = "Researcher",
    Instructions = "Investiga el tema proporcionado. Proporciona datos y hechos clave.",
    ChatClient = chatClient
};

// 2. Agente de Análisis
var analysisAgent = new ChatCompletionAgent
{
    Name = "Analyst",
    Instructions = "Analiza la información proporcionada. Identifica patrones e insights.",
    ChatClient = chatClient
};

// 3. Agente de Resumen
var summaryAgent = new ChatCompletionAgent
{
    Name = "Summarizer",
    Instructions = "Crea un resumen ejecutivo conciso de 3-5 puntos clave.",
    ChatClient = chatClient
};

// 4. Ejecutar workflow secuencial
Console.WriteLine("=== Sequential Workflow ===\n");
Console.Write("Tema a investigar: ");
var topic = Console.ReadLine();

var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, $"Investiga: {topic}")
};

// Step 1: Research
Console.WriteLine("\n[Step 1/3] Research...");
var researchResult = await researchAgent.InvokeAsync(messages);
Console.WriteLine($"Research output:\n{researchResult}\n");

// Step 2: Analysis
Console.WriteLine("[Step 2/3] Analysis...");
messages.Add(new ChatMessage(ChatRole.Assistant, researchResult));
messages.Add(new ChatMessage(ChatRole.User, "Analiza esta información"));
var analysisResult = await analysisAgent.InvokeAsync(messages);
Console.WriteLine($"Analysis output:\n{analysisResult}\n");

// Step 3: Summary
Console.WriteLine("[Step 3/3] Summary...");
messages.Add(new ChatMessage(ChatRole.Assistant, analysisResult));
messages.Add(new ChatMessage(ChatRole.User, "Crea un resumen ejecutivo"));
var summaryResult = await summaryAgent.InvokeAsync(messages);
Console.WriteLine($"\n=== RESULTADO FINAL ===\n{summaryResult}");
```

#### Checkpoint de Validación

✅ Ves 3 pasos ejecutándose secuencialmente  
✅ Cada agente procesa el output del anterior  
✅ El resultado final es un resumen coherente

### Lab 3.2: Parallel Workflow (20 minutos)

#### Objetivo
Traducir texto a múltiples idiomas en paralelo

#### Código

```csharp
// Crear agentes de traducción
var spanishAgent = new ChatCompletionAgent
{
    Name = "SpanishTranslator",
    Instructions = "Traduce texto al español. Solo output el texto traducido.",
    ChatClient = chatClient
};

var frenchAgent = new ChatCompletionAgent
{
    Name = "FrenchTranslator",
    Instructions = "Traduce texto al francés. Solo output el texto traducido.",
    ChatClient = chatClient
};

var germanAgent = new ChatCompletionAgent
{
    Name = "GermanTranslator",
    Instructions = "Traduce texto al alemán. Solo output el texto traducido.",
    ChatClient = chatClient
};

// Input
Console.WriteLine("=== Parallel Translation Workflow ===\n");
Console.Write("Texto a traducir (en inglés): ");
var textToTranslate = Console.ReadLine();

var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, textToTranslate)
};

// Ejecutar traducciones en paralelo
Console.WriteLine("\n[Traduciendo en paralelo...]");

var tasks = new[]
{
    Task.Run(async () => ("Español", await spanishAgent.InvokeAsync(messages))),
    Task.Run(async () => ("Francés", await frenchAgent.InvokeAsync(messages))),
    Task.Run(async () => ("Alemán", await germanAgent.InvokeAsync(messages)))
};

var results = await Task.WhenAll(tasks);

// Mostrar resultados
Console.WriteLine("\n=== RESULTADOS ===");
foreach (var (language, translation) in results)
{
    Console.WriteLine($"\n{language}:");
    Console.WriteLine(translation);
}
```

#### Checkpoint de Validación

✅ Las 3 traducciones ocurren simultáneamente (no secuencial)  
✅ Todas las traducciones completan exitosamente  
✅ El tiempo total es similar al de una sola traducción

### Lab 3.3: Delegation Workflow (15 minutos)

#### Objetivo
Agente orquestador que delega a especialistas

#### Código

```csharp
// Especialistas
var sqlExpert = new ChatCompletionAgent
{
    Name = "SqlExpert",
    Instructions = "Experto en SQL. Solo responde preguntas sobre bases de datos.",
    ChatClient = chatClient
};

var emailExpert = new ChatCompletionAgent
{
    Name = "EmailExpert",
    Instructions = "Experto en email marketing. Solo responde sobre emails.",
    ChatClient = chatClient
};

// Orquestador
var orchestrator = new ChatCompletionAgent
{
    Name = "Orchestrator",
    Instructions = @"
        Eres un orquestador que delega tareas a expertos.
        
        - Para preguntas de SQL/DB → delega a SqlExpert
        - Para preguntas de email → delega a EmailExpert
        - Para todo lo demás → responde directamente
    ",
    ChatClient = chatClient,
    Tools = [sqlExpert, emailExpert]
};

// Loop de conversación
var messages = new List<ChatMessage>();

while (true)
{
    Console.Write("\nUsuario: ");
    var input = Console.ReadLine();
    if (input?.ToLower() == "salir") break;
    
    messages.Add(new ChatMessage(ChatRole.User, input));
    var response = await orchestrator.InvokeAsync(messages);
    messages.Add(new ChatMessage(ChatRole.Assistant, response));
    
    Console.WriteLine($"Agente: {response}");
}
```

### Lab 3.4: Group Chat (15 minutos)

#### Objetivo
Múltiples agentes colaboran en una discusión

**Código resumido** (ver repositorio completo):

```csharp
var productManager = new ChatCompletionAgent { Name = "PM", ... };
var developer = new ChatCompletionAgent { Name = "Dev", ... };
var designer = new ChatCompletionAgent { Name = "Designer", ... };

var groupChat = new AgentGroupChat([productManager, developer, designer]);

await groupChat.InvokeAsync("Diseñemos una feature de notificaciones push");
// Los agentes debaten y colaboran automáticamente
```

---

<div style="page-break-after: always;"></div>

## Módulo 4: Observabilidad

**Duración**: 60 minutos  
**Objetivo**: Implementar telemetría con OpenTelemetry y Azure Monitor

### Teoría (15 minutos)

#### Los 3 Pilares de Observabilidad

**1. Métricas (Metrics)**
- Contadores: Número de invocaciones de agente
- Gauges: Tokens consumidos por segundo
- Histogramas: Latencia de respuestas

**2. Traces (Trazas)**
- Seguimiento de solicitudes end-to-end
- Identificar cuellos de botella
- Relación entre componentes

**3. Logs**
- Eventos discretos
- Errores y warnings
- Debugging contextual

#### OpenTelemetry

Framework estándar para instrumentación:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Microsoft.Extensions.AI.Agents");
        metrics.AddAzureMonitorMetricExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource("Microsoft.Extensions.AI.Agents");
        tracing.AddAzureMonitorTraceExporter();
    });
```

### Lab 4.1: Metrics con OpenTelemetry (20 minutos)

#### Código

```csharp
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

var services = new ServiceCollection();

// Configurar OpenTelemetry
services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("MyAgent.Metrics");
        metrics.AddConsoleExporter(); // Para ver en consola localmente
        
        // Opcional: Azure Monitor
        // metrics.AddAzureMonitorMetricExporter(options =>
        // {
        //     options.ConnectionString = "InstrumentationKey=...";
        // });
    });

var serviceProvider = services.BuildServiceProvider();

// Crear meter personalizado
var meterFactory = serviceProvider.GetRequiredService<IMeterFactory>();
var meter = meterFactory.Create("MyAgent.Metrics");

// Definir métricas
var invocationCounter = meter.CreateCounter<int>("agent.invocations", "Count of agent invocations");
var tokenCounter = meter.CreateCounter<int>("agent.tokens", "Tokens consumed");
var latencyHistogram = meter.CreateHistogram<double>("agent.latency_ms", "Latency in milliseconds");

// Agente con instrumentación
var agent = new ChatCompletionAgent
{
    Name = "InstrumentedAgent",
    Instructions = "Eres un asistente útil.",
    ChatClient = chatClient
};

// Loop con métricas
var messages = new List<ChatMessage>();

while (true)
{
    Console.Write("Usuario: ");
    var input = Console.ReadLine();
    if (input?.ToLower() == "salir") break;
    
    messages.Add(new ChatMessage(ChatRole.User, input));
    
    // Medir latencia
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    var response = await agent.InvokeAsync(messages);
    
    sw.Stop();
    
    // Registrar métricas
    invocationCounter.Add(1);
    tokenCounter.Add(response.Length / 4); // Aproximación de tokens
    latencyHistogram.Record(sw.ElapsedMilliseconds);
    
    messages.Add(new ChatMessage(ChatRole.Assistant, response));
    Console.WriteLine($"Agente: {response}");
    Console.WriteLine($"[Métrica] Latencia: {sw.ElapsedMilliseconds}ms");
}
```

#### Checkpoint de Validación

✅ Ves métricas de latencia en consola  
✅ Métricas se exportan correctamente  
✅ Puedes conectar a Azure Monitor (opcional)

### Lab 4.2: Distributed Tracing (25 minutos)

#### Código

```csharp
using OpenTelemetry.Trace;

var services = new ServiceCollection();

services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("MyAgent.Tracing");
        tracing.AddConsoleExporter();
        
        // Azure Monitor
        // tracing.AddAzureMonitorTraceExporter(options =>
        // {
        //     options.ConnectionString = "...";
        // });
    });

var serviceProvider = services.BuildServiceProvider();

// Crear tracer
var tracerProvider = serviceProvider.GetRequiredService<TracerProvider>();
var tracerFactory = serviceProvider.GetRequiredService<ITracerFactory>();
var tracer = tracerFactory.Create("MyAgent.Tracing");

// Workflow con tracing
using var rootSpan = tracer.StartActiveSpan("sequential-workflow");

// Step 1
using (var researchSpan = tracer.StartActiveSpan("research"))
{
    var researchResult = await researchAgent.InvokeAsync(messages);
    researchSpan.SetAttribute("output.length", researchResult.Length);
}

// Step 2
using (var analysisSpan = tracer.StartActiveSpan("analysis"))
{
    var analysisResult = await analysisAgent.InvokeAsync(messages);
    analysisSpan.SetAttribute("output.length", analysisResult.Length);
}

// Step 3
using (var summarySpan = tracer.StartActiveSpan("summary"))
{
    var summaryResult = await summaryAgent.InvokeAsync(messages);
    summarySpan.SetAttribute("output.length", summaryResult.Length);
}

rootSpan.SetAttribute("workflow.status", "completed");
```

#### Checkpoint de Validación

✅ Ves traces jerárquicos en consola  
✅ Cada step del workflow tiene su propio span  
✅ Atributos personalizados se capturan correctamente

---

<div style="page-break-after: always;"></div>

## Módulo 5: ASP.NET Core y Aspire

**Duración**: 60 minutos  
**Objetivo**: Construir aplicación web multi-agente con orquestación Aspire

### Teoría (10 minutos)

#### .NET Aspire

Plataforma de orquestación para aplicaciones distribuidas:

- Dashboard unificado para servicios
- Service discovery automático
- Configuración centralizada
- Deployment simplificado con `azd`

#### Arquitectura

```
┌─────────────────────────────────────────┐
│  AppHost (Aspire Orchestrator)          │
│  - Configura servicios                  │
│  - Dashboard en localhost:15001         │
└────────────┬────────────────────────────┘
             │
       ┌─────┴─────┐
       │           │
┌──────▼─────┐  ┌──▼──────────┐
│  WebApi    │  │ AgentService│
│  (ASP.NET) │  │ (Workers)   │
└────────────┘  └─────────────┘
```

### Lab 5.1: REST API con Agente (25 minutos)

#### Estructura de proyecto

```bash
mkdir AgentWebApi
cd AgentWebApi
dotnet new webapi
dotnet add package Microsoft.Extensions.AI.Agents --version 1.0.0-preview.260108.1
```

#### Código - Program.cs

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI.Agents;

var builder = WebApplication.CreateBuilder(args);

// Configurar Azure OpenAI
var endpoint = builder.Configuration["AzureOpenAI:Endpoint"]!;
var apiKey = builder.Configuration["AzureOpenAI:ApiKey"]!;
var deploymentName = builder.Configuration["AzureOpenAI:DeploymentName"]!;

var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey)
);

var chatClient = client.GetChatClient(deploymentName);

// Registrar agente como singleton
builder.Services.AddSingleton<ChatCompletionAgent>(sp =>
{
    return new ChatCompletionAgent
    {
        Name = "ApiAgent",
        Instructions = "Eres un asistente útil. Responde en español.",
        ChatClient = chatClient
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint de chat
app.MapPost("/api/chat", async (ChatRequest request, ChatCompletionAgent agent) =>
{
    var messages = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.User, request.Message)
    };
    
    var response = await agent.InvokeAsync(messages);
    
    return Results.Ok(new ChatResponse
    {
        Message = response,
        Timestamp = DateTime.UtcNow
    });
})
.WithName("Chat")
.WithOpenApi();

app.Run();

// DTOs
record ChatRequest(string Message);
record ChatResponse(string Message, DateTime Timestamp);
```

#### Probar API

```bash
# Iniciar API
dotnet run

# En otra terminal, probar endpoint
curl -X POST http://localhost:5000/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"Hola, ¿cómo estás?"}'
```

#### Checkpoint de Validación

✅ API responde en `http://localhost:5000`  
✅ Swagger UI accesible en `/swagger`  
✅ Endpoint `/api/chat` retorna respuestas del agente

### Lab 5.2: Orquestación con Aspire (25 minutos)

#### Crear solución Aspire

```bash
dotnet new aspire-starter -n AgentWorkshop
cd AgentWorkshop
```

Estructura generada:
```
AgentWorkshop/
├── AgentWorkshop.AppHost/      ← Orquestador
├── AgentWorkshop.ServiceDefaults/
└── AgentWorkshop.Web/          ← Frontend (Blazor)
```

#### Agregar proyecto de API

```bash
cd AgentWorkshop
dotnet new webapi -n AgentWorkshop.Api
dotnet sln add AgentWorkshop.Api
```

#### Configurar AppHost - Program.cs

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Registrar API
var apiService = builder.AddProject<Projects.AgentWorkshop_Api>("apiservice");

// Registrar Web (Frontend)
builder.AddProject<Projects.AgentWorkshop_Web>("webfrontend")
    .WithReference(apiService);  // Frontend conoce a API

builder.Build().Run();
```

#### Ejecutar con Aspire

```bash
cd AgentWorkshop.AppHost
dotnet run
```

**Output esperado**:
```
Aspire Dashboard: http://localhost:15001
```

Abre el dashboard y verás:
- **Resources**: webfrontend, apiservice
- **Logs**: Logs de todos los servicios
- **Traces**: Distributed tracing entre servicios
- **Metrics**: CPU, memoria, requests

#### Checkpoint de Validación

✅ Aspire Dashboard abre en `http://localhost:15001`  
✅ Ves ambos servicios (webfrontend, apiservice) en estado "Running"  
✅ Puedes ver logs en tiempo real desde el dashboard

---

<div style="page-break-after: always;"></div>

## Módulo 6: DevUI

**Duración**: 15 minutos (Demo)  
**Objetivo**: Conocer herramientas de debugging para agentes

### Qué es DevUI

**DevUI** es una herramienta de debugging visual para agentes que muestra:

- **Conversaciones**: Historial de mensajes user ↔ agent
- **Function Calls**: Qué herramientas se llamaron y con qué parámetros
- **State Inspection**: Estado interno del agente en cada turno
- **Performance**: Latencia de cada operación

### Instalación

```bash
dotnet tool install --global Microsoft.Extensions.AI.DevUI
```

### Configuración en el agente

```csharp
using Microsoft.Extensions.AI.Agents.DevUI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAgentInstrumentation(options =>
{
    options.EnableDevUI = true;
    options.DevUIPort = 5002;
});

var agent = new ChatCompletionAgent
{
    // ... configuración normal ...
    Instrumentation = new InstrumentationOptions
    {
        Endpoint = "http://localhost:5002",
        EnableTracing = true
    }
};
```

### Iniciar DevUI

```bash
ai-devui
```

Abre http://localhost:5002 y verás el panel de debugging.

### Demo en vivo

El instructor mostrará:
1. Conversación con agente en consola
2. DevUI mostrando mensajes en tiempo real
3. Inspector de function calls
4. State debugging

**No hay lab práctico** - este módulo es demo-only.

---

<div style="page-break-after: always;"></div>

## Módulo 7: Model Context Protocol

**Duración**: 15 minutos (Teoría)  
**Objetivo**: Comprender MCP y su interoperabilidad

### ¿Qué es MCP?

**Model Context Protocol** es un protocolo estándar para que aplicaciones cliente (como IDEs) se comuniquen con **servidores de contexto** que proporcionan:

- Acceso a datos (bases de datos, APIs)
- Herramientas (funciones)
- Prompts predefinidos

### Arquitectura

```
┌──────────────────┐
│  Claude Desktop  │
│  (MCP Client)    │
└────────┬─────────┘
         │ MCP Protocol
         ▼
┌──────────────────┐
│  MCP Server      │
│  - Tools         │
│  - Resources     │
│  - Prompts       │
└──────────────────┘
```

### MAF como MCP Client

Microsoft Agent Framework puede actuar como **cliente MCP**:

```csharp
using Microsoft.Extensions.AI.Agents.MCP;

var mcpServer = await McpClient.ConnectAsync("http://localhost:3000");

var agent = new ChatCompletionAgent
{
    Name = "McpAgent",
    Instructions = "Eres un agente con acceso a herramientas MCP.",
    ChatClient = chatClient,
    McpServers = [mcpServer]  // ← Conectar a servidor MCP
};

// El agente ahora puede usar tools del servidor MCP
```

### Beneficios de MCP

1. **Interoperabilidad**: Mismas herramientas en múltiples clientes (Claude, VS Code, MAF)
2. **Reutilización**: Escribe herramientas una vez, úsalas en cualquier cliente
3. **Ecosistema**: Servidores MCP públicos disponibles (GitHub, Notion, etc.)

### Casos de Uso

- **Corporativo**: Exponer APIs internas vía MCP → consumir desde MAF
- **IDE Integration**: Agentes con acceso a filesystem, Git, debugger vía MCP
- **Multi-LLM**: Mismas herramientas para Claude, GPT, Gemini

**No hay lab práctico** - este es un módulo conceptual.

---

<div style="page-break-after: always;"></div>

## Apéndices

### A. Referencia Rápida de APIs

#### Crear Agente

```csharp
var agent = new ChatCompletionAgent
{
    Name = "MyAgent",
    Instructions = "...",
    ChatClient = chatClient,
    Tools = [plugin1, plugin2]
};
```

#### Invocar Agente

```csharp
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "Hola")
};

var response = await agent.InvokeAsync(messages);
```

#### Definir Function Tool

```csharp
public class MyPlugin
{
    [KernelFunction]
    [Description("Descripción de la función")]
    public string MyFunction(
        [Description("Descripción del parámetro")] string param)
    {
        return "resultado";
    }
}
```

### B. Configuración de Azure OpenAI

#### Portal Azure

1. Crear recurso "Azure OpenAI"
2. Deployment → Desplegar modelo "gpt-5.2"
3. Keys and Endpoint → Copiar API key
4. Configurar en user secrets:

```bash
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://[recurso].openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "[tu-api-key]"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-5.2"
```

### C. Troubleshooting Común

| Error | Solución |
|-------|----------|
| 401 Unauthorized | Verificar API key y endpoint |
| 429 Rate Limit | Reducir frecuencia de solicitudes |
| Function no se llama | Agregar `[Description]` detallado |
| Puerto en uso | Cambiar puerto: `dotnet run --urls http://localhost:5001` |

### D. Recursos Adicionales

- **Documentación MAF**: https://learn.microsoft.com/microsoft-agent-framework
- **Azure OpenAI Docs**: https://learn.microsoft.com/azure/ai-services/openai/
- **.NET Aspire Docs**: https://learn.microsoft.com/dotnet/aspire/
- **OpenTelemetry .NET**: https://opentelemetry.io/docs/languages/net/

### E. Glosario

| Término | Definición |
|---------|------------|
| **Agent** | Entidad de IA que puede razonar y ejecutar acciones |
| **ChatCompletionAgent** | Tipo de agente basado en modelos de chat |
| **Function Tool** | Función C# que el agente puede llamar |
| **Workflow** | Secuencia de ejecución de múltiples agentes |
| **Instrumentation** | Telemetría y observabilidad del agente |
| **MCP** | Model Context Protocol, estándar de interoperabilidad |

### F. Próximos Pasos

Después del workshop, te recomendamos:

1. **Construir un proyecto personal**: Aplicar lo aprendido en un caso de uso real
2. **Explorar Azure AI Agent Service**: Para workflows con estado persistente
3. **Contribuir a la comunidad**: Compartir tus plugins y patrones
4. **Seguir aprendiendo**:
   - Azure AI Studio para fine-tuning de modelos
   - Prompt engineering avanzado
   - Multi-modal agents (visión, audio)

---

## 📝 Notas Personales

Usa este espacio para tus anotaciones durante el workshop:

<div style="min-height: 300px; border: 1px solid #ccc; padding: 20px;">
<!-- Espacio para notas del participante -->
</div>

---

## ✅ Checklist de Completion

Marca los módulos que completaste exitosamente:

- [ ] Módulo 1: Hello Agent funcionando
- [ ] Módulo 2.1: Function tool ejecutándose
- [ ] Módulo 2.2: Composición de agentes
- [ ] Módulo 2.3: Human-in-the-loop
- [ ] Módulo 3.1: Sequential workflow
- [ ] Módulo 3.2: Parallel workflow
- [ ] Módulo 3.3: Delegation workflow
- [ ] Módulo 3.4: Group chat
- [ ] Módulo 4.1: Métricas capturadas
- [ ] Módulo 4.2: Distributed tracing
- [ ] Módulo 5.1: REST API funcionando
- [ ] Módulo 5.2: Aspire Dashboard activo
- [ ] Módulo 6: DevUI demo vista
- [ ] Módulo 7: MCP conceptos comprendidos

---

## 🎓 Certificación

Para obtener el certificado de completion:

1. Completar mínimo **10 de 14** labs prácticos
2. Participar activamente en al menos **5 checkpoints**
3. Enviar proyecto final (opcional pero recomendado)

---

**Fin del Material del Participante**

---

**Información de contacto**:  
Instructor: [Nombre]  
Email: [email]  
GitHub: [repositorio]  

**Workshop desarrollado con** ❤️ **por [Organización]**  
**Versión**: 1.0 | **Fecha**: [Fecha] | **Framework**: MAF 1.0.0-preview.260108.1
