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
- ✅ Endpoint configurado
- ✅ Credenciales de Azure configuradas (para `DefaultAzureCredential`)

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
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.260108.1
dotnet add package Azure.AI.OpenAI --version 2.1.0
dotnet add package Azure.Identity --version 1.13.0
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

### 2.2 Configurar Autenticación de Azure

Este lab usa `DefaultAzureCredential` para autenticación. Asegúrate de tener una de estas opciones configuradas:

```bash
# Opción 1: Azure CLI (recomendado para desarrollo)
az login

# Opción 2: Variables de entorno
export AZURE_CLIENT_ID="tu-client-id"
export AZURE_CLIENT_SECRET="tu-client-secret"
export AZURE_TENANT_ID="tu-tenant-id"
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
    <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.260108.1" />
    <PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview.260108.1" />
    
    <!-- Azure OpenAI SDK -->
    <PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />
    
    <!-- Azure Identity para autenticación -->
    <PackageReference Include="Azure.Identity" Version="1.13.0" />
    
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

El enfoque con Microsoft Agent Framework utiliza el patrón `AsAIFunction` para exponer agentes como herramientas:

```csharp
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
```

### 3.2 La Técnica Clave: AsAIFunction

```csharp
// El método AsAIFunction convierte un agente completo en una función invocable
var calculateFunction = calculatorAgent.AsAIFunction(
    new AIFunctionFactoryOptions
    {
        Name = "calculate",
        Description = "Resuelve problemas matemáticos..."
    }
);
```

**Esto convierte un agente completo en una función invocable**:
- El agente principal puede llamar a `calculate`
- La función invoca internamente a `calculatorAgent`
- El resultado se devuelve al agente principal
- MAF maneja automáticamente la serialización y el contexto

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
🤖 AsistenteGeneral: El 15% de 850 es 127.5. 

El cálculo es: 850 × (15/100) = 850 × 0.15 = 127.5
```

**Observa**: El agente principal delegó automáticamente a CalculadoraExperta usando la función `calculate`.

**Prueba 2: Pregunta general (NO debe delegar)**
```
👤 Tú: ¿Cuál es la capital de España?
🤖 AsistenteGeneral: La capital de España es Madrid.
```

**Observa**: El agente principal respondió directamente sin usar la función `calculate`.

---

## Paso 5: Validación

### ✅ Checkpoint: Verificación de Agent-as-Tool

- [ ] ✅ El programa muestra ambos agentes al iniciar
- [ ] ✅ Preguntas matemáticas son procesadas por CalculadoraExperta (delegación vía `AsAIFunction`)
- [ ] ✅ Preguntas generales son respondidas directamente por AsistenteGeneral
- [ ] ✅ El AsistenteGeneral presenta los resultados matemáticos de forma clara
- [ ] ✅ Ambos agentes responden en español

---

## Solución de Problemas

### El agente principal no delega nunca

**Síntoma**: Las preguntas matemáticas no activan al agente calculadora.

**Causa**: La descripción de la función no es suficientemente clara.

**Solución**: Mejora la descripción para ser más específica:
```csharp
var calculateFunction = calculatorAgent.AsAIFunction(
    new AIFunctionFactoryOptions
    {
        Name = "calculate",
        Description = "OBLIGATORIO usar para CUALQUIER cálculo numérico, porcentaje, promedio, suma, resta, multiplicación, división, o problema matemático de cualquier tipo."
    }
);
```

### Error de autenticación

**Síntoma**: Error de credenciales o 401 Unauthorized.

**Solución**: Verifica que `az login` esté activo o las variables de entorno estén configuradas:
```bash
az login
az account show  # Verificar que estás logueado
```

### Error de conexión

**Síntoma**: Error de conexión o timeout.

**Solución**: Verifica endpoint y deployment name en appsettings.json.

---

## Resumen

En este laboratorio aprendiste:

✅ **Crear agentes especializados** con `CreateAIAgent`  
✅ **Convertir agentes en funciones** con `AsAIFunction`  
✅ **Componer sistemas multi-agente** donde el coordinador delega automáticamente  
✅ **El poder del patrón agent-as-tool** para modularidad y escalabilidad
✅ **Usar `DefaultAzureCredential`** para autenticación segura

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
