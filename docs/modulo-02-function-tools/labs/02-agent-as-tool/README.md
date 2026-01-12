# Lab 02: Agent-as-Tool - Composición de Agentes

**Duración**: 25 minutos  
**Complejidad**: Standard  
**Objetivo**: Usar un agente especializado como herramienta (function tool) de otro agente

## Objetivo

En este laboratorio, aprenderás a componer agentes donde un agente principal **delega tareas especializadas** a otros agentes usando Microsoft Agent Framework. Este patrón es fundamental para:

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
- Entender `AIFunctionFactory.Create` y registro de tools

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
dotnet add package Microsoft.Agents.AI --version 1.0.0-preview.260108.1
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
    "DeploymentName": "gpt-5.2"
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
    <!-- Microsoft Agent Framework package -->
    <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.260108.1" />
    
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

## Paso 3: Implementar el Sistema Multi-Agente

### 3.1 Reemplazar Program.cs

El enfoque con Microsoft Agent Framework es más directo - todo se define en un solo archivo:

```csharp
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;
using Microsoft.Agents.AI.Chat;
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
var apiKey = configuration["AzureOpenAI:ApiKey"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

// ===== Crear Agente Especializado: Calculadora =====
// Este agente se dedicará exclusivamente a operaciones matemáticas
var calculatorAgent = new ChatCompletionAgent(
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
        """,
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey
);

Console.WriteLine("✓ CalculadoraExperta creada - Agente especializado en matemáticas");

// ===== Crear Función que Invoca al Agente Calculadora =====
// Esta función será usada por el agente principal para delegar tareas matemáticas
var calculateFunction = AIFunctionFactory.Create(
    async (string mathQuestion) =>
    {
        Console.WriteLine($"\n   📊 [Delegando a CalculadoraExperta]: {mathQuestion}");
        
        // Crear historial temporal para esta consulta
        var chat = new ChatHistory();
        chat.AddUserMessage(mathQuestion);
        
        // Invocar el agente especializado
        string result = "";
        await foreach (var message in calculatorAgent.InvokeAsync(chat))
        {
            result += message.Content;
        }
        
        Console.WriteLine($"   📊 [CalculadoraExperta respondió]: {result.Substring(0, Math.Min(50, result.Length))}...\n");
        
        return result;
    },
    name: "calculate",
    description: "Resuelve problemas matemáticos complejos. Usa esta función cuando el usuario tenga preguntas sobre cálculos, matemáticas, porcentajes, ecuaciones o estadísticas."
);

// ===== Crear Agente Principal =====
// Este agente usa la función calculate para delegar tareas matemáticas
var mainAgent = new ChatCompletionAgent(
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
    endpoint: new Uri(endpoint),
    modelId: deploymentName,
    apiKey: apiKey,
    tools: new AIFunction[] { calculateFunction }
);

Console.WriteLine("✓ AsistenteGeneral creado - Agente coordinador con delegación");

// ===== Historial de Conversación =====
var chatHistory = new ChatHistory();

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
    
    chatHistory.AddUserMessage(userInput);
    
    Console.Write($"🤖 {mainAgent.Name}: ");
    
    try
    {
        await foreach (var message in mainAgent.InvokeAsync(chatHistory))
        {
            Console.Write(message.Content);
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

### 3.2 La Técnica Clave: AIFunctionFactory con Agentes

```csharp
var calculateFunction = AIFunctionFactory.Create(
    async (string mathQuestion) =>
    {
        var chat = new ChatHistory();
        chat.AddUserMessage(mathQuestion);
        
        string result = "";
        await foreach (var message in calculatorAgent.InvokeAsync(chat))
        {
            result += message.Content;
        }
        return result;
    },
    name: "calculate",
    description: "Resuelve problemas matemáticos..."
);
```

**Esto convierte un agente completo en una función invocable**:
- El agente principal puede llamar a `calculate`
- La función invoca internamente a `calculatorAgent`
- El resultado se devuelve al agente principal

---

## Paso 4: Ejecución

### 4.1 Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

### 4.2 Salida Esperada

```
✓ CalculadoraExperta creada - Agente especializado en matemáticas
✓ AsistenteGeneral creado - Agente coordinador con delegación

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

### 4.3 Probar la Delegación

**Prueba 1: Pregunta matemática (DEBE delegar)**
```
👤 Tú: ¿Cuánto es 15% de 850?

   📊 [Delegando a CalculadoraExperta]: ¿Cuánto es 15% de 850?
   📊 [CalculadoraExperta respondió]: Para calcular el 15% de 850:...

🤖 AsistenteGeneral: El 15% de 850 es 127.5. 

El cálculo es: 850 × (15/100) = 850 × 0.15 = 127.5
```

**Observa**: Aparece el log `[Delegando a CalculadoraExperta]` - ¡la delegación funcionó!

**Prueba 2: Pregunta general (NO debe delegar)**
```
👤 Tú: ¿Cuál es la capital de España?
🤖 AsistenteGeneral: La capital de España es Madrid.
```

**Observa**: No aparece ningún log de delegación - el agente principal respondió directamente.

---

## Paso 5: Validación

### ✅ Checkpoint: Verificación de Agent-as-Tool

- [ ] ✅ El programa muestra ambos agentes al iniciar
- [ ] ✅ Preguntas matemáticas activan el log `[Delegando a CalculadoraExperta]`
- [ ] ✅ Preguntas generales NO activan el log de delegación
- [ ] ✅ El AsistenteGeneral presenta los resultados matemáticos de forma clara
- [ ] ✅ Ambos agentes responden en español

---

## Solución de Problemas

### El agente principal no delega nunca

**Síntoma**: Las preguntas matemáticas no activan al agente calculadora.

**Causa**: La descripción de la función no es suficientemente clara.

**Solución**: Mejora la descripción para ser más específica:
```csharp
description: "OBLIGATORIO usar para CUALQUIER cálculo numérico, porcentaje, promedio, suma, resta, multiplicación, división, o problema matemático de cualquier tipo."
```

### Error de conexión

**Síntoma**: Error 401 o timeout.

**Solución**: Verifica endpoint, API key y deployment name.

---

## Resumen

En este laboratorio aprendiste:

✅ **Crear agentes especializados** con `ChatCompletionAgent`  
✅ **Convertir agentes en funciones** con `AIFunctionFactory.Create`  
✅ **Componer sistemas multi-agente** donde el coordinador delega automáticamente  
✅ **El poder del patrón agent-as-tool** para modularidad y escalabilidad

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

Continúa con el [Módulo 3: Workflows y Orquestación](../../modulo-03-workflows/) donde aprenderás a crear workflows secuenciales, paralelos y de delegación.

---

**Tiempo completado**: ~25 minutos  
**¡Felicitaciones!** 🎉 Has implementado tu primer sistema de composición de agentes con Microsoft Agent Framework.
