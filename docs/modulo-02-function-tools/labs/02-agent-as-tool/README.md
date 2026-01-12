# Lab 02: Agent-as-Tool - Composición de Agentes

**Duración**: 25 minutos  
**Complejidad**: Standard  
**Objetivo**: Usar un agente especializado como herramienta (function tool) de otro agente

## Objetivo

En este laboratorio, aprenderás a componer agentes donde un agente principal **delega tareas especializadas** a otros agentes. Este patrón es fundamental para:

- **Modularidad**: Cada agente tiene una responsabilidad clara
- **Especialización**: Agentes expertos en dominios específicos
- **Reutilización**: El mismo agente especializado puede ser usado por múltiples coordinadores
- **Escalabilidad**: Agregar nuevas capacidades sin modificar el agente principal

Al finalizar, tendrás un sistema donde el agente principal automáticamente delega preguntas matemáticas a un agente calculadora especializado.

---

## Prerequisitos

### Software Requerido
- ✅ .NET 10 SDK instalado
- ✅ Visual Studio Code o Visual Studio 2025

### Conocimientos Previos
- ✅ Completar Lab 01 (Custom Function Tool)
- Entender `[KernelFunction]` y registro de plugins

### Configuración de Azure
- ✅ Azure OpenAI Service con deployment de `gpt-5.2`
- ✅ Endpoint y API Key disponibles

---

## Paso 1: Preparación del Proyecto

### 1.1 Crear Proyecto

```bash
mkdir AgentComposition
cd AgentComposition
dotnet new console -n AgentComposition
cd AgentComposition
```

### 1.2 Instalar Paquetes NuGet

```bash
dotnet add package Microsoft.AI.Agents --version 1.0.0-preview.260108.1
dotnet add package Microsoft.AI.Agents.Abstractions --version 1.0.0-preview.260108.1
dotnet add package Azure.AI.OpenAI --version 2.0.0
dotnet add package Microsoft.Extensions.Configuration --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.UserSecrets --version 10.0.0
```

---

## Paso 2: Configuración

### 2.1 Crear appsettings.json

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://TU-RECURSO-NOMBRE.openai.azure.com/",
    "DeploymentName": "gpt-5.2",
    "MaxTokens": 2000,
    "Temperature": 0.7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 2.2 Configurar User Secrets

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "TU-API-KEY-AQUI"
```

### 2.3 Actualizar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>maf-workshop-agent-composition-02</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <!-- Microsoft Agent Framework packages -->
    <PackageReference Include="Microsoft.AI.Agents" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.AI.Agents.Abstractions" Version="1.0.0-preview.260108.1" />
    
    <!-- Azure OpenAI -->
    <PackageReference Include="Azure.AI.OpenAI" Version="2.0.0" />
    
    <!-- Configuration -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

---

## Paso 3: Crear el Agente Especializado

### 3.1 Crear CalculatorAgent.cs

El agente calculadora es un **agente completo** con sus propias instrucciones, especializado en resolver problemas matemáticos.

```csharp
// ============================================================================
// Archivo: CalculatorAgent.cs
// Descripción: Agente especializado en cálculos matemáticos
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using Microsoft.AI.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentComposition;

/// <summary>
/// Agente especializado en operaciones matemáticas.
/// Este agente será utilizado como "herramienta" por el agente principal
/// para delegar cualquier pregunta relacionada con matemáticas.
/// </summary>
public class CalculatorAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly Kernel _kernel;
    
    public CalculatorAgent(Kernel kernel)
    {
        _kernel = kernel;
        
        // Crear agente especializado en matemáticas
        _agent = new ChatCompletionAgent()
        {
            Name = "CalculadoraExperta",
            Instructions = """
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
                """,
            Kernel = kernel
        };
    }
    
    /// <summary>
    /// Nombre del agente para identificación
    /// </summary>
    public string Name => _agent.Name;
    
    /// <summary>
    /// Procesa una pregunta matemática y devuelve la respuesta.
    /// Este método será expuesto como función al agente principal.
    /// </summary>
    /// <param name="question">Pregunta o problema matemático</param>
    /// <returns>Solución con explicación paso a paso</returns>
    public async Task<string> SolveMathProblemAsync(string question)
    {
        Console.WriteLine($"\n   📊 [CalculadoraExperta recibió]: {question}");
        
        // Crear historial temporal para esta consulta
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(question);
        
        // Invocar el agente especializado
        var response = new System.Text.StringBuilder();
        
        await foreach (var message in _agent.InvokeStreamingAsync(chatHistory))
        {
            response.Append(message.Content);
        }
        
        var result = response.ToString();
        Console.WriteLine($"   📊 [CalculadoraExperta respondió]: {result.Substring(0, Math.Min(50, result.Length))}...\n");
        
        return result;
    }
}
```

### 3.2 Puntos Clave del Agente Especializado

**Propósito único**: El agente tiene instrucciones muy específicas para matemáticas.

**Método expuesto**: `SolveMathProblemAsync` será convertido en una función.

**Logs de delegación**: Los `Console.WriteLine` muestran cuando se delega al agente.

---

## Paso 4: Crear el Agente Coordinador

### 4.1 Crear MainAgent.cs

El agente principal **usa al calculador como herramienta**, decidiendo automáticamente cuándo delegarle.

```csharp
// ============================================================================
// Archivo: MainAgent.cs
// Descripción: Agente principal que usa otros agentes como herramientas
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using System.ComponentModel;
using Microsoft.AI.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AgentComposition;

/// <summary>
/// Agente coordinador que delega tareas especializadas a otros agentes.
/// Demuestra el patrón "agent-as-tool" donde agentes completos se exponen
/// como funciones para ser invocados por un agente principal.
/// </summary>
public class MainAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly CalculatorAgent _calculatorAgent;
    
    public MainAgent(Kernel kernel, CalculatorAgent calculatorAgent)
    {
        _calculatorAgent = calculatorAgent;
        
        // ===== Crear función que invoca al agente calculadora =====
        // Esto convierte el agente especializado en una "function tool"
        var calculateFunction = KernelFunctionFactory.CreateFromMethod(
            method: async (string mathQuestion) => 
            {
                return await _calculatorAgent.SolveMathProblemAsync(mathQuestion);
            },
            functionName: "calculate",
            description: "Resuelve problemas matemáticos complejos. Usa esta función cuando el usuario tenga preguntas sobre cálculos, matemáticas, porcentajes, ecuaciones o estadísticas."
        );
        
        // Registrar la función en el kernel
        kernel.Plugins.AddFromFunctions(
            pluginName: "AgentesEspecializados",
            description: "Agentes especializados para tareas específicas",
            functions: new[] { calculateFunction }
        );
        
        // ===== Crear agente principal =====
        _agent = new ChatCompletionAgent()
        {
            Name = "AsistenteGeneral",
            Instructions = """
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
            Kernel = kernel,
            Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                }
            )
        };
    }
    
    /// <summary>
    /// Nombre del agente principal
    /// </summary>
    public string Name => _agent.Name;
    
    /// <summary>
    /// Procesa un mensaje del usuario, delegando a agentes especializados cuando sea necesario.
    /// </summary>
    public async IAsyncEnumerable<string> ProcessMessageAsync(ChatHistory chatHistory)
    {
        await foreach (var message in _agent.InvokeStreamingAsync(chatHistory))
        {
            yield return message.Content ?? "";
        }
    }
}
```

### 4.2 La Técnica Clave: KernelFunctionFactory

```csharp
var calculateFunction = KernelFunctionFactory.CreateFromMethod(
    method: async (string mathQuestion) => 
    {
        return await _calculatorAgent.SolveMathProblemAsync(mathQuestion);
    },
    functionName: "calculate",
    description: "Resuelve problemas matemáticos..."
);
```

**Esto convierte cualquier método en una función invocable por agentes**:
- `method`: La función a ejecutar (puede ser async)
- `functionName`: Identificador para el modelo
- `description`: Guía para cuándo usarla

---

## Paso 5: Implementar el Programa Principal

### 5.1 Reemplazar Program.cs

```csharp
// ============================================================================
// Archivo: Program.cs
// Descripción: Demostración de composición de agentes (agent-as-tool)
// Módulo: 2 - Function Tools
// Lab: 02-agent-as-tool
// ============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AgentComposition;

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
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

// ===== Crear Kernel Base =====
// Este kernel se compartirá entre agentes para optimizar recursos
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

var kernel = builder.Build();

// ===== Crear Agente Especializado =====
// El CalculatorAgent es un agente completo dedicado a matemáticas
var calculatorAgent = new CalculatorAgent(kernel);

// ===== Crear Agente Principal =====
// MainAgent usa al CalculatorAgent como una "herramienta"
// El kernel debe ser clonado para agregar plugins específicos del MainAgent
var mainKernel = kernel.Clone();
var mainAgent = new MainAgent(mainKernel, calculatorAgent);

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

// ===== Interfaz de Usuario =====
Console.WriteLine("============================================");
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
    
    chatHistory.AddUserMessage(userInput);
    
    Console.Write($"🤖 {mainAgent.Name}: ");
    
    try
    {
        await foreach (var content in mainAgent.ProcessMessageAsync(chatHistory))
        {
            Console.Write(content);
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
    
    // Gestión de historial
    if (chatHistory.Count > 10)
    {
        var messagesToKeep = chatHistory.Skip(chatHistory.Count - 10).ToList();
        chatHistory.Clear();
        foreach (var msg in messagesToKeep)
        {
            chatHistory.Add(msg);
        }
    }
}
```

---

## Paso 6: Ejecución

### 6.1 Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

### 6.2 Salida Esperada

```
============================================
🤖 Composición de Agentes (Agent-as-Tool)
============================================
Agente Principal: AsistenteGeneral
Agente Especializado: CalculadoraExperta
============================================

💡 Prueba estas preguntas:
   📊 Matemáticas: '¿Cuánto es 15% de 850?'
   📊 Matemáticas: 'Calcula el promedio de 85, 92, 78, 95'
   💬 General: '¿Cuál es la capital de España?'
   💬 General: 'Dame consejos para aprender programación'

Escribe 'salir' para terminar
============================================

👤 Tú: _
```

### 6.3 Probar la Delegación

**Prueba 1: Pregunta matemática (DEBE delegar)**
```
👤 Tú: ¿Cuánto es 15% de 850?

   📊 [CalculadoraExperta recibió]: ¿Cuánto es 15% de 850?
   📊 [CalculadoraExperta respondió]: Para calcular el 15% de 850:...

🤖 AsistenteGeneral: El 15% de 850 es 127.5. 

El cálculo es: 850 × (15/100) = 850 × 0.15 = 127.5
```

**Observa**: Aparece el log `[CalculadoraExperta recibió]` - ¡la delegación funcionó!

**Prueba 2: Pregunta general (NO debe delegar)**
```
👤 Tú: ¿Cuál es la capital de España?
🤖 AsistenteGeneral: La capital de España es Madrid.
```

**Observa**: No aparece ningún log de CalculadoraExperta - el agente principal respondió directamente.

**Prueba 3: Estadísticas (DEBE delegar)**
```
👤 Tú: Calcula el promedio de 85, 92, 78, 95

   📊 [CalculadoraExperta recibió]: Calcula el promedio de 85, 92, 78, 95
   📊 [CalculadoraExperta respondió]: El promedio se calcula sumando...

🤖 AsistenteGeneral: El promedio de 85, 92, 78 y 95 es 87.5.

Proceso: (85 + 92 + 78 + 95) / 4 = 350 / 4 = 87.5
```

---

## Paso 7: Validación

### ✅ Checkpoint: Verificación de Agent-as-Tool

- [ ] ✅ El programa muestra ambos agentes al iniciar
- [ ] ✅ Preguntas matemáticas activan el log `[CalculadoraExperta recibió]`
- [ ] ✅ Preguntas generales NO activan el log de CalculadoraExperta
- [ ] ✅ El AsistenteGeneral presenta los resultados matemáticos de forma clara
- [ ] ✅ Ambos agentes responden en español

**Prueba de validación definitiva**:
```
👤 Tú: Primero dime la capital de Francia, y luego calcula 20% de 500
```

El agente debe:
1. Responder "París" directamente
2. Delegar el cálculo a CalculadoraExperta
3. Presentar ambas respuestas

---

## Experimentación (Opcional)

### Experimento 1: Agregar Agente Traductor

Crea un `TranslatorAgent` y exponlo como función:

```csharp
// En TranslatorAgent.cs
public async Task<string> TranslateAsync(string text, string targetLanguage)
{
    // Similar a CalculatorAgent pero para traducciones
}

// En MainAgent.cs
var translateFunction = KernelFunctionFactory.CreateFromMethod(
    method: async (string text, string language) => 
        await _translatorAgent.TranslateAsync(text, language),
    functionName: "translate",
    description: "Traduce texto a otro idioma"
);
```

### Experimento 2: Cadena de Agentes

Crea un flujo donde el resultado de un agente pasa al siguiente:
- Usuario pregunta: "Calcula 15% de 1000 y tradúcelo al inglés"
- CalculatorAgent calcula → TranslatorAgent traduce

---

## Solución de Problemas

### El agente principal no delega nunca

**Síntoma**: Las preguntas matemáticas no activan al agente calculadora.

**Causa**: La descripción de la función no es suficientemente clara.

**Solución**: Mejora la descripción para ser más específica:
```csharp
description: "OBLIGATORIO usar para CUALQUIER cálculo numérico, porcentaje, promedio, suma, resta, multiplicación, división, o problema matemático de cualquier tipo."
```

---

### Error: "Kernel already has plugin"

**Síntoma**: Error al agregar plugins repetidamente.

**Causa**: El kernel se reutiliza y ya tiene el plugin.

**Solución**: Usar `kernel.Clone()` para crear una copia limpia:
```csharp
var mainKernel = kernel.Clone();
var mainAgent = new MainAgent(mainKernel, calculatorAgent);
```

---

### El agente especializado no responde correctamente

**Síntoma**: CalculadoraExperta da respuestas incompletas o incorrectas.

**Causa**: Las instrucciones del agente especializado no son claras.

**Solución**: Revisa y mejora las instrucciones de CalculatorAgent.

---

## Resumen

En este laboratorio aprendiste:

✅ **Crear agentes especializados** con propósitos específicos  
✅ **Convertir agentes en funciones** con `KernelFunctionFactory.CreateFromMethod`  
✅ **Componer sistemas multi-agente** donde el coordinador delega automáticamente  
✅ **El poder del patrón agent-as-tool** para modularidad y escalabilidad

### Conceptos Clave

| Concepto | Descripción |
|----------|-------------|
| **Agent-as-Tool** | Patrón donde un agente completo se expone como función |
| **KernelFunctionFactory** | Factory para crear funciones desde métodos arbitrarios |
| **Agente Coordinador** | Agente principal que orquesta a otros agentes |
| **Agente Especializado** | Agente experto en un dominio específico |

### Diagrama del Flujo

```
Usuario → AsistenteGeneral → (¿Es matemáticas?) 
                                   ↓ Sí
                            CalculadoraExperta
                                   ↓
                            Resultado al Usuario
```

---

## Próximos Pasos

Continúa con [Lab 03: Human Approval](../03-human-approval/) donde aprenderás a implementar aprobación humana para acciones sensibles.

---

## Referencias

- [Semantic Kernel Function Factory](https://learn.microsoft.com/semantic-kernel/concepts/kernel-functions)
- [Agent Composition Patterns](https://learn.microsoft.com/microsoft-agent-framework/patterns/composition)
- [Multi-Agent Systems](https://learn.microsoft.com/azure/ai-services/openai/concepts/multi-agent)

---

**Tiempo completado**: ~25 minutos  
**¡Felicitaciones!** 🎉 Has implementado tu primer sistema de composición de agentes.
